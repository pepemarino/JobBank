using JobBank.ModelsDTO;
using JobBank.Services.Abstraction;
using JobBank.StartUpServices;

namespace JobBank.Management
{
    public class RejectionAnalysisWorker : BackgroundService
    {
        private readonly AnalysisChannel _analysisChannel;
        private readonly IServiceScopeFactory _scopeFactory;

        public RejectionAnalysisWorker(AnalysisChannel channel, IServiceScopeFactory scopeFactory)
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

                    var userSkills = await userSkillService.GetUserSkillsAsync(1); // we do not have users yet.  AFTER THIS!  THIS IS GROWING!!!  JOSE!!!  Stop BSin!
                    if (userSkills == null)
                        return;                                                    // No skills: Do not bother; keep going

                    var jobId = request.JobApplicationId;

                    // the query is showing the Rejection Guard pattern
                    var jobApplications = await jobPostService
                        .GetJobPostsByQueryAsync<AgentAnalysisDTO>(jp => jp.ApplicationDeclined && 
                                                       jp.JobRejectionAnalysis == null && 
                                                       jp.Id == jobId);
                        
                    var jobAppkication = jobApplications.FirstOrDefault();
                    if (jobAppkication == null) return;                           // this would be an error

                    // Again the growing problem.  Because Job Application is not related to user yet this is a 
                    // veggy stew
                    jobAppkication.UserSkillSet = userSkills.RawSkills;
                    jobAppkication = await careerAssistant.RunLLMAnalysis(jobAppkication, prompService.SkillGap);

                    var rejectedApplication = await jobPostService.GetJobPostByIdAsync(jobId);
                    if (rejectedApplication == null) return;                     // this is exception, but 
                    rejectedApplication.JobRejectionAnalysis = new JobRejectionAnalysisDTO
                    {
                        JobPostId = jobId,
                        Version = 1,
                        ApplicantSkills = jobAppkication.UserSkillSet,
                        Analisis = jobAppkication.AnalysisResult,
                        JobDescription = jobAppkication.Description,
                        IsProcessed = true,
                        ModelUsed = prompService.LLMModel,
                        PromptVersion = "v1"
                    };

                    await jobPostService.UpdateOrAddJobPostAsync(rejectedApplication);
                }
            }
        }
    }

}
