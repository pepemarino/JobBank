using JobBank.Management.Abstraction;
using JobBank.ModelsDTO;
using JobBank.Services.Abstraction;
using JobBank.StartUpServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace JobBank.Management
{
    public sealed class RejectionAnalysisWorker : BackgroundService
    {
        private readonly AnalysisChannel _analysisChannel;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<RejectionAnalysisWorker> _logger;

        public RejectionAnalysisWorker(
            AnalysisChannel channel,
            IServiceScopeFactory scopeFactory,
            ILogger<RejectionAnalysisWorker> logger)
        {
            _analysisChannel = channel;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("RejectionAnalysisWorker started.");

            await foreach (var request in _analysisChannel.Reader.ReadAllAsync(stoppingToken))
            {
                try
                {
                    await ProcessRequestAsync(request, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("RejectionAnalysisWorker stopping due to cancellation.");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Unexpected error while processing JobId {JobId} for UserId {UserId}.",
                        request.JobApplicationId,
                        request.UserId);
                }
            }
        }

        private async Task ProcessRequestAsync(AnalysisRequest request, CancellationToken token)
        {
            if (string.IsNullOrWhiteSpace(request.UserId))
            {
                _logger.LogWarning("Skipping request with missing UserId.");
                return;
            }

            await using var scope = _scopeFactory.CreateAsyncScope();

            var jobPostService = scope.ServiceProvider.GetRequiredService<IJobPostService>();
            var promptService = scope.ServiceProvider.GetRequiredService<PrompService>();
            var careerAssistant = scope.ServiceProvider.GetRequiredService<ICareerAssistant>();
            var userSkillService = scope.ServiceProvider.GetRequiredService<ISkillsService>();

            var jobId = request.JobApplicationId;
            var userId = request.UserId;

            var userSkills = await userSkillService.GetUserSkillsAsync(userId);
            if (userSkills == null)
            {
                _logger.LogWarning("No skills found for UserId {UserId}. Skipping.", userId);
                return;
            }

            var jobApplications = await jobPostService
                .GetJobPostsByQueryAsync<JobApplicationAnalysisDTO>(jp =>
                    jp.ApplicationDeclined &&
                    jp.JobRejectionAnalysis == null &&
                    jp.Id == jobId);

            var jobApplication = jobApplications.FirstOrDefault();
            if (jobApplication == null || string.IsNullOrWhiteSpace(jobApplication.Description))
            {
                _logger.LogWarning("JobId {JobId} has no valid application or description.", jobId);
                return;
            }

            jobApplication.UserSkillSet = userSkills.RawSkills;

            // Run LLM analysis
            jobApplication = await careerAssistant.RunLLMAnalysis(
                jobApplication,
                promptService.SkillGap,
                userId);

            // Fetch the actual entity to update
            var rejectedApplication = await jobPostService.GetJobPostByIdAsync(jobId);
            if (rejectedApplication == null)
            {
                _logger.LogError("JobId {JobId} not found during update.", jobId);
                return;
            }

            rejectedApplication.JobRejectionAnalysis = new JobRejectionAnalysisDTO
            {
                JobPostId = jobId,
                Version = 1,
                ApplicantSkills = jobApplication.UserSkillSet,
                Analisis = jobApplication.AnalysisResult,
                JobDescription = jobApplication.Description,
                IsProcessed = true,
                ModelUsed = promptService.LLMModel,
                PromptVersion = "v1",
                UserId = userId
            };

            await jobPostService.UpdateOrAddJobPostAsync(rejectedApplication);

            _logger.LogInformation("Processed rejection analysis for JobId {JobId}.", jobId);
        }
    }
}
