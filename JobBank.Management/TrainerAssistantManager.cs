using JobBank.Extensions;
using JobBank.Management.Abstraction;
using JobBank.Management.Interview;
using JobBank.ModelsDTO;
using JobBank.Services.Abstraction;
using JobBank.Util;
using System.Text.Json;

namespace JobBank.Management
{
    public class TrainerAssistantManager : ITrainerAssistantManager
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ITrainerAssistantManager> _logger;
        public TrainerAssistantManager(IServiceScopeFactory scopeFactory, ILogger<ITrainerAssistantManager> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        /// <summary>
        /// Because a background worker is used to run the analysis, we need to create a new 
        /// scope for each analysis to ensure that we have a fresh set of services and to avoid 
        /// any potential issues with shared state or concurrency. By creating a new scope,
        /// we can safely resolve the necessary services for the analysis without worrying 
        /// about conflicts with other analyses that may be running concurrently. This approach 
        /// also allows us to manage the lifetime of the services more effectively, ensuring that 
        /// they are disposed of properly after the analysis is complete.
        /// For this reason I cannot inject the services directly into the constructor of the TrainerAssistantManager, 
        /// because they may have a different lifetime than the manager itself and may not be thread-safe. 
        /// Instead, I create a new scope for each analysis and resolve the services within that scope to 
        /// ensure that they are properly managed and disposed of after use.
        /// I sure hope you read this.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="interviewId"></param>
        /// <param name="prompt"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<int> AnalyzeInterviewAsync(string userId, int interviewId, string? prompt = null)
        {
            #region Services and Providers

            await using var scope = _scopeFactory.CreateAsyncScope();

            var jobDescriptionParser = scope.ServiceProvider.GetRequiredService<JobDescriptionParser>();
            var interviewService = scope.ServiceProvider.GetRequiredService<IInterviewService>();
            var jobPostService = scope.ServiceProvider.GetRequiredService<IJobPostService>();
            var userSkillService = scope.ServiceProvider.GetRequiredService<ISkillsService>();
            var trainerAssistant = scope.ServiceProvider.GetRequiredService<ITrainerAssistant>();
            var trainerService = scope.ServiceProvider.GetRequiredService<ITrainingService>();

            #endregion

            #region Existence Checks

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("TrainerAnalysisWorker: Received request with empty UserId. Skipping analysis.");
                throw new ArgumentException("UserId cannot be null or empty.", nameof(userId));
            }

            var userSkills = await userSkillService.GetUserSkillsAsync(userId);
            if (userSkills == null)
            {
                _logger.LogWarning("TrainerAnalysisWorker: No skills found for UserId {UserId}. Skipping analysis.", userId);
                throw new InvalidOperationException($"No skills found for UserId {userId}.");
            }

            var interview = await interviewService.GetInterviewByIdAsync(interviewId);
            if (interview == null)
            {
                _logger.LogWarning("TrainerAnalysisWorker: Interview with ID {InterviewId} not found. Skipping analysis.", interviewId);
                throw new InvalidOperationException($"Interview with ID {interviewId} not found.");
            }

            if (string.IsNullOrEmpty(interview.Result))
            {
                _logger.LogWarning("TrainerAnalysisWorker: Interview with ID {InterviewId} has empty result. Skipping analysis.", interviewId);
                throw new InvalidOperationException($"Interview with ID {interviewId} has empty result.");
            }

            var interviewResultIsValid =
                interview.Result.IsValidJson<InterviewMetadata>(
                     Common.StrictValidationOptions,
                     BusinessRules.InterviewTrainingRulesFunc());

            if (!interviewResultIsValid)
            {
                _logger.LogWarning("TrainerAnalysisWorker: Interview with ID {InterviewId} has invalid result format. Skipping analysis.", interviewId);
                throw new InvalidOperationException($"Interview with ID {interviewId} has invalid result format.");
            }

            var interviewMetadata = JsonSerializer.Deserialize<InterviewMetadata>(interview.Result)
                ?? throw new InvalidOperationException($"Failed to deserialize interview metadata for Interview ID {interviewId}.");

            var jobPost = await jobPostService.GetJobPostByIdAsync(interview.JobPostId);
            if (jobPost == null || string.IsNullOrEmpty(jobPost.Description))
            {
                _logger.LogWarning("TrainerAnalysisWorker: Job post with ID {JobPostId} not found for interview {InterviewId}. Skipping analysis.", interview.JobPostId, interviewId);
                throw new InvalidOperationException($"Job post with ID {interview.JobPostId} not found for interview {interviewId}.");
            }

            #endregion Existence Checks

            var jobDescriptionDictionarySkills = jobDescriptionParser.GetSections(jobPost.Description);
            if (jobDescriptionDictionarySkills == null ||
                !jobDescriptionDictionarySkills.Any() ||
                !jobDescriptionDictionarySkills.Values.All(value => !string.IsNullOrEmpty(value)))
            {
                _logger.LogWarning("TrainerAnalysisWorker: Failed to parse skills from job description for JobPost ID {JobPostId}. Skipping analysis for Interview ID {InterviewId}.", interview.JobPostId, interviewId);
                throw new InvalidOperationException($"Failed to parse skills from job description for JobPost ID {interview.JobPostId}. Skipping analysis for Interview ID {interviewId}.");
            }

            var trainerAnalysisDTO = new TrainerAnalysisMetadataDTO
            {
                JobDescriptionDictionarySkills = jobDescriptionDictionarySkills,
                CanonicalApplicantSkills = userSkills.RawSkills.ToDenormalized(),
                CoveredTopics = interviewMetadata.CoveredTopics,
                WeakAreas = interviewMetadata.WeakAreas,
                Evaluations = interviewMetadata.Evaluations
            };

            var training = await trainerAssistant.RunLLMAnalysis(trainerAnalysisDTO, userId: userId, prompt: prompt);

            if (!string.IsNullOrEmpty(training.ErrorMessage))
            {
                _logger.LogWarning("TrainerAnalysisWorker: LLM analysis failed for Interview ID {InterviewId}: {ErrorMessage}",
                    interviewId, training.ErrorMessage);
                throw new InvalidOperationException($"LLM analysis failed for Interview ID {interviewId}: {training.ErrorMessage}");
            }

            var trainerDto = new TrainingDTO
            {
                UserId = userId,
                InterviewId = interviewId,
                Model = training.Model,
                Prompt = training.Prompt,
                Result = JsonSerializer.Serialize<InterviewTrainingAnalysisResultDTO>(training)
            };

            await trainerService.AddTrainingAsync(trainerDto);

            _logger.LogInformation("TrainerAnalysisWorker: Successfully created training for Interview ID {InterviewId}, UserId {UserId}",
                interviewId, userId);

            return trainerDto.Id;
        }        
    }
}
