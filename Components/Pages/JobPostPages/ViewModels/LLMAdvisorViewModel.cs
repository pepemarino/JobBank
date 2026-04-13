using JobBank.Extensions;
using JobBank.Management;
using JobBank.Management.Abstraction;
using JobBank.ModelsDTO;
using JobBank.Services.Abstraction;
using System.Text.Json;

namespace JobBank.Components.Pages.JobPostPages.ViewModels
{
    public partial class LLMAdvisorViewModel : ILLMAdvisorViewModel
    {        
        private readonly CareerAssistant _careerAssistant;
        private readonly IIdentityService _identityService;
        private readonly IJobPostService _jobPostService;
        private readonly IAnalysisCacheService _analysisCacheService;
        private readonly ILogger<LLMAdvisorViewModel> _logger;
        private readonly ILLMManager _llmManager;

        public LLMAdvisorViewModel(            
            CareerAssistant careerAssistant,
            IIdentityService identityService,
            IJobPostService jobPostService,
            IAnalysisCacheService analysisCacheService,
            ILogger<LLMAdvisorViewModel> logger,
            ILLMManager llmManager)
        {            
            _careerAssistant = careerAssistant;
            _identityService = identityService;
            _jobPostService = jobPostService;
            _analysisCacheService = analysisCacheService;
            _logger = logger;
            _llmManager = llmManager;
        }

        public int JobPostId { get; set; }

        public string Title { get; set; } = "LLM Advisor";

        public bool Donate { get; set; }
        public bool CanDonate { get; set; }

        public event Action? OnRequestUIUpdate;

        public bool IsLoading { get; set; } = true;

        public bool IsError { get; set; } = false;

        public string ErrorDescription { get; set; } = string.Empty;

        public string JobTitle { get; set; } = string.Empty;

        public string[] InterviewQuestions { get; set; } = Array.Empty<string>();

        public string[] StudySubjects { get; set; } = Array.Empty<string>();

        public string[] EmployerQuestions { get; set; } = Array.Empty<string>();

        public async Task InitializeAsync()
        {
            // If called by ViewModelBase before JobPostId is set, do nothing
            if (JobPostId <= 0) return;

            IsLoading = true;

            try
            {
                var currentUser = await _identityService.GetCurrentUserDetailsAsync();
                if (currentUser == null)
                {
                    throw new InvalidOperationException("User is not authenticated. LLM analysis cannot be performed.");
                }

                var userId = currentUser.Id;
                if (string.IsNullOrEmpty(userId))
                {
                    throw new InvalidOperationException("User is not authenticated. LLM analysis cannot be performed.");
                }

                CanDonate = await _llmManager.UserHasValidPrivateKeyAsync(userId);

                // Get JobPost 
                var jobPost = await _jobPostService.GetJobPostByIdAsync(JobPostId);

                if (jobPost == null)
                {
                    var msg = $"Job Application with Id {JobPostId} not found.";
                    _logger.LogWarning("{msg} (This should not happen if navigation guards are working correctly.)", msg);
                    throw new InvalidOperationException(msg);
                }

                if (string.IsNullOrEmpty(jobPost.Description))
                {
                    throw new InvalidOperationException("Job Application description is empty. LLM analysis cannot be performed.");
                }

                var jobDescriptionHash = jobPost.Description!.ToCanonicalHash();
                JobTitle = jobPost?.Title ?? "Unknown Job Title"; // this should never be null because of database constraints,

                JobAnalysisCacheDTO? analysisCache = await _analysisCacheService
                    .GetAnalysisCache(currentUser, userId, jobDescriptionHash);

                if (analysisCache == null)
                {
                    LLMAnalysisResult analysisResult = await _careerAssistant.RunLLMAnalysis(jobPost.Description!, interviewPreparationQuestions, userId);
                    if (!string.IsNullOrEmpty(analysisResult.ErrorMessage))
                    {
                        throw new InvalidOperationException($"LLM analysis failed: {analysisResult.ErrorMessage}");
                    }

                    // Defaut for IsValid is false for doated.
                    analysisCache = new JobAnalysisCacheDTO
                    {
                        Hash = jobDescriptionHash,
                        UserId = userId,
                        IsLegacy = false,
                        IsPublic = !CanDonate,
                        SourceModelTier = CanDonate && currentUser.ForceMyKeyy 
                            ? TearModel.Paid.ToString() 
                            : TearModel.Free.ToString(),  // this is still in the works
                        JobPostDescription = jobPost.Description,
                        ModelUsed = analysisResult.Model,
                        PromptVersion = analysisResult.Version,
                        Result = analysisResult.Analysis
                    };

                    await _analysisCacheService.AddJobAnalysisCacheAsync(analysisCache);
                }

                CanTearDonate(analysisCache);

                UILoadCachedAnalysis(analysisCache);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading LLMAdvisorViewModel for JobPostId: {JobPostId}", JobPostId);

                IsError = true;
                UILoadError("API Error", ex.Message);
            }
            finally
            {
                IsLoading = false;                
                OnRequestUIUpdate?.Invoke();
            }
            
            void UILoadCachedAnalysis(JobAnalysisCacheDTO analysisCache)
            {
                if (analysisCache == null) return;

                // Load analysis result from cache
                Title = $"LLM Advisor - Cached Analysis (Model: {analysisCache.ModelUsed}, Prompt Version: {analysisCache.PromptVersion})";

                var analysis = JsonSerializer.Deserialize<Management.AnalysisResult>(analysisCache.Result!);
                InterviewQuestions = analysis?.InterviewQuestions.ToArray() ?? Array.Empty<string>();
                StudySubjects = analysis?.StudySubjects.ToArray() ?? Array.Empty<string>();
                EmployerQuestions = analysis?.EmployerQuestions.ToArray() ?? Array.Empty<string>();
            }

            /// <summary>
            /// Using this for now because the UI is blocking on the LLM response, 
            /// so if there is an error we want to show it immediately.  
            /// If we move to a more async UI in the future, we may want to change 
            /// this to show a notification instead of blocking the UI with an error message.
            /// </summary>  
            void UILoadError(string errorType, string errorMessage)
            {
                // Load analysis result from cache
                Title = $"LLM Advisor - {errorType}";
                ErrorDescription = errorMessage;
            }
        }

        private void CanTearDonate(JobAnalysisCacheDTO analysisCache)
        {
            var tearUsed = (TearModel)Enum.Parse(typeof(TearModel), analysisCache.SourceModelTier, true);
            CanDonate = tearUsed == TearModel.Paid;
        }
    }
}
