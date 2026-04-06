using JobBank.Extensions;
using JobBank.Management;
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

        public LLMAdvisorViewModel(            
            CareerAssistant careerAssistant,
            IIdentityService identityService,
            IJobPostService jobPostService,
            IAnalysisCacheService analysisCacheService)
        {            
            _careerAssistant = careerAssistant;
            _identityService = identityService;
            _jobPostService = jobPostService;
            _analysisCacheService = analysisCacheService;   
        }

        public int JobPostId { get; set; }

        public string Title { get; set; } = "LLM Advisor";

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
                // Get JobPost 
                var jobPost = await _jobPostService.GetJobPostByIdAsync(JobPostId);

                if (jobPost == null)
                {
                    // It should never happen because the page is only navigated to with a valid JobPostId,
                    // but we should still handle it just in case - Should we add loging here?
                    throw new InvalidOperationException($"Job Application with Id {JobPostId} not found.");
                }

                if (string.IsNullOrEmpty(jobPost.Description))
                {
                    throw new InvalidOperationException("Job Application description is empty. LLM analysis cannot be performed.");
                }

                var jobDescriptionHash = jobPost.Description!.ToCanonicalHash();
                JobTitle = jobPost!.Title!;

                // get the JobAnalysisCache for the JobPost
                var analysisCache = await _analysisCacheService
                    .GetJobAnalysisCacheAsync(jobDescriptionHash);
                
                if (analysisCache == null)
                {
                    var userId = await _identityService.GetUserIdAsync();
                    LLMAnalysisResult analysisResult = await _careerAssistant.RunLLMAnalysis(jobPost.Description!, interviewPreparationQuestions, userId); 
                    if (!string.IsNullOrEmpty(analysisResult.ErrorMessage))
                    {
                        throw new InvalidOperationException($"LLM analysis failed: {analysisResult.ErrorMessage}");
                    }

                    analysisCache = new JobAnalysisCacheDTO
                    {
                        Hash = jobDescriptionHash,
                        JobPostDescription = jobPost.Description,
                        ModelUsed = analysisResult.Model,
                        PromptVersion = analysisResult.Version,
                        Result = analysisResult.Analysis
                    };
                        
                    await _analysisCacheService.AddJobAnalysisCacheAsync(analysisCache);
                }

                UILoadCachedAnalysis(analysisCache);
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., log the error, show a message to the user, etc.)
                Console.Error.WriteLine($"Error initializing LLMAdvisorViewModel: {ex.Message}");
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
    }
}
