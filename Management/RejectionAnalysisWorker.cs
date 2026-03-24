using JobBank.ModelsDTO;
using JobBank.Services.Abstraction;
using JobBank.StartUpServices;

namespace JobBank.Management
{
    public class RejectionAnalysisWorker : BackgroundService
    {
        private readonly AnalysisChannel _analysisChannel;
        private readonly IServiceScopeFactory _scopeFactory;

        public RejectionAnalysisWorker(
            AnalysisChannel channel,
            IServiceScopeFactory scopeFactory)
        {
            _analysisChannel = channel;
            _scopeFactory = scopeFactory;
        }

        /// <summary>
        /// Note that this is not happening on startup but when there is data in the channel.
        /// It needs to be changed to startup.  When this happens we need tp change this implementation.
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Wait until there is data to read in the channel
            while (await _analysisChannel.Reader.WaitToReadAsync(stoppingToken))
            {
                // Try to pull all available items from the channel
                while (_analysisChannel.Reader.TryRead(out var request))
                {
                    await using var scope = _scopeFactory.CreateAsyncScope();
                    var jobPostService = scope.ServiceProvider.GetRequiredService<IJobPostService>();
                    var prompService = scope.ServiceProvider.GetRequiredService<PrompService>();
                    var careerAssistant = scope.ServiceProvider.GetRequiredService<CareerAssistant>();
                    var userSkillService = scope.ServiceProvider.GetRequiredService<ISkillsService>();

                    // Use the UserId from the request instead of trying to get it from AuthenticationStateProvider
                    var currentUserId = request.UserId;

                    if (string.IsNullOrEmpty(currentUserId))
                        continue;

                    var userSkills = await userSkillService.GetUserSkillsAsync(currentUserId);
                    if (userSkills == null)
                        continue;

                    var jobId = request.JobApplicationId;

                    // the query is showing the Rejection Guard pattern
                    var jobApplications = await jobPostService
                        .GetJobPostsByQueryAsync<AgentAnalysisDTO>(jp => jp.ApplicationDeclined &&
                                                       jp.JobRejectionAnalysis == null &&
                                                       jp.Id == jobId);

                    var jobApplication = jobApplications.FirstOrDefault();
                    if (jobApplication == null || string.IsNullOrEmpty(jobApplication.Description)) continue;

                    jobApplication.UserSkillSet = userSkills.RawSkills;
                    
                    jobApplication = await careerAssistant.RunLLMAnalysis(jobApplication, prompService.SkillGap, currentUserId);

                    var rejectedApplication = await jobPostService.GetJobPostByIdAsync(jobId);
                    if (rejectedApplication == null) return;                     // this is exception, but 

                    rejectedApplication.JobRejectionAnalysis = new JobRejectionAnalysisDTO
                    {
                        JobPostId = jobId,
                        Version = 1,
                        ApplicantSkills = jobApplication.UserSkillSet,
                        Analisis = jobApplication.AnalysisResult,
                        JobDescription = jobApplication.Description,
                        IsProcessed = true,
                        ModelUsed = prompService.LLMModel,
                        PromptVersion = "v1",
                        UserId = currentUserId  // Include the user ID when queuing work
                    };

                    await jobPostService.UpdateOrAddJobPostAsync(rejectedApplication);
                }
            }
        }
    }

}
