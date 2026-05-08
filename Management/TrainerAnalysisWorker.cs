using JobBank.Extensions;
using JobBank.Management.Interview;
using JobBank.ModelsDTO;
using JobBank.Services.Abstraction;
using System.Text.Json;

namespace JobBank.Management
{
    public partial class TrainerAnalysisWorker : BackgroundService
    {
        private readonly TrainerChannel _trainerChannel;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<TrainerAnalysisWorker> _logger;

        public TrainerAnalysisWorker(
            TrainerChannel trainerChannel,
            IServiceScopeFactory scopeFactory,
            ILogger<TrainerAnalysisWorker> logger
            )
        {
            _scopeFactory = scopeFactory;
            _trainerChannel = trainerChannel;
            _logger = logger;
        }

        /// <summary>
        /// It is beautiful
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (await _trainerChannel.Reader.WaitToReadAsync(stoppingToken))
            {
                while (_trainerChannel.Reader.TryRead(out var request))
                {
                    try
                    {
                        #region Services and Providers

                        await using var scope = _scopeFactory.CreateAsyncScope();

                        var jobDescriptionParser = scope.ServiceProvider.GetRequiredService<JobDescriptionParser>();
                        var interviewService = scope.ServiceProvider.GetRequiredService<IInterviewService>();
                        var jobPostService = scope.ServiceProvider.GetRequiredService<IJobPostService>();
                        var userSkillService = scope.ServiceProvider.GetRequiredService<ISkillsService>();
                        var trainerAssistant = scope.ServiceProvider.GetRequiredService<TrainerAssistant>();
                        var trainerService = scope.ServiceProvider.GetRequiredService<ITrainingService>();

                        #endregion

                        #region Existence Checks

                        if (string.IsNullOrEmpty(request.UserId))
                        {
                            _logger.LogWarning("TrainerAnalysisWorker: Received request with empty UserId. Skipping analysis.");
                            continue;
                        }

                        var userSkills = await userSkillService.GetUserSkillsAsync(request.UserId);
                        if (userSkills == null)
                        {
                            _logger.LogWarning("TrainerAnalysisWorker: No skills found for UserId {UserId}. Skipping analysis.", request.UserId);
                            continue;
                        }

                        var interview = await interviewService.GetInterviewByIdAsync(request.InterviewId);
                        if (interview == null)
                        {
                            _logger.LogWarning("TrainerAnalysisWorker: Interview with ID {InterviewId} not found. Skipping analysis.", request.InterviewId);
                            continue;
                        }

                        if (string.IsNullOrEmpty(interview.Result))
                        {
                            _logger.LogWarning("TrainerAnalysisWorker: Interview with ID {InterviewId} has empty result. Skipping analysis.", request.InterviewId);
                            continue;
                        }

                        var interviewMetadata = JsonSerializer.Deserialize<InterviewMetadata>(interview.Result);
                        if (interviewMetadata == null || !interviewMetadata.Evaluations.Any())
                        {
                            _logger.LogWarning("TrainerAnalysisWorker: Interview metadata for Interview ID {InterviewId} is null or has no evaluations. Skipping analysis.", request.InterviewId);
                            continue;
                        }

                        var jobPost = await jobPostService.GetJobPostByIdAsync(interview.JobPostId);
                        if (jobPost == null || string.IsNullOrEmpty(jobPost.Description))
                        {
                            _logger.LogWarning("TrainerAnalysisWorker: Job post with ID {JobPostId} not found for interview {InterviewId}. Skipping analysis.", interview.JobPostId, request.InterviewId);
                            continue;
                        }

                        #endregion Existence Checks

                        var trainerAnalysisDTO = new TrainerAnalysisMetadataDTO
                        {
                            JobDescriptionDictionarySkills = jobDescriptionParser.GetSections(jobPost.Description),
                            CanonicalApplicantSkills = userSkills.RawSkills.ToDenormalized(),
                            CoveredTopics = interviewMetadata.CoveredTopics,
                            WeakAreas = interviewMetadata.WeakAreas,
                            Evaluations = interviewMetadata.Evaluations
                        };

                        var training = await trainerAssistant.RunLLMAnalysis(trainerAnalysisDTO, TrainerPrompt, request.UserId);

                        if (!string.IsNullOrEmpty(training.ErrorMessage))
                        {
                            _logger.LogWarning("TrainerAnalysisWorker: LLM analysis failed for Interview ID {InterviewId}: {ErrorMessage}", 
                                request.InterviewId, training.ErrorMessage);
                            continue;
                        }

                        var trainerDto = new TrainingDTO
                        {
                            UserId = request.UserId,
                            InterviewId = request.InterviewId,
                            Model = training.Model,
                            Prompt = TrainerPrompt,
                            Result = JsonSerializer.Serialize<InterviewTrainingAnalysisResultDTO>(training)
                        };

                        await trainerService.AddTrainingAsync(trainerDto);
                        _logger.LogInformation("TrainerAnalysisWorker: Successfully created training for Interview ID {InterviewId}, UserId {UserId}", 
                            request.InterviewId, request.UserId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "TrainerAnalysisWorker: Unexpected error processing Interview ID {InterviewId}", 
                            request.InterviewId);
                    }
                }
            }
        }
    }
}
