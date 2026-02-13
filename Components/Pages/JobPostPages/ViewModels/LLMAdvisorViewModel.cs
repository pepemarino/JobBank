using JobBank.Data;
using JobBank.Extensions;
using JobBank.Management;
using JobBank.Models;
using JobBank.StartUpServices;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace JobBank.Components.Pages.JobPostPages.ViewModels
{
    public class LLMAdvisorViewModel : ILLMAdvisorViewModel
    {
        private readonly string _errorString = "{\"interviewQuestions\":[\"Grrr! Operation Error - No INtergiew Questions\"],\"studySubjects\":[\"Oh, no!  Operation Error - No Study Subjects.\"]}";
        private readonly string _apiKeyName = "JOBBANK_OPENAI_API_KEY";
        private string _apiKey = string.Empty;
        private readonly string _llmModel = "gpt-4o-mini"; // Use GPT-4o or GPT-4o-mini for best reasoning on job descriptions

        private readonly PrompService _prompService;

        public LLMAdvisorViewModel(IDbContextFactory<EmploymentBankContext> DbFactory, PrompService prompService)
        {            
            Context = DbFactory.CreateDbContext();
            _prompService = prompService ?? throw new ArgumentNullException(nameof(prompService));
        }

        private PrompService PrompService
        {
            get
            {
                if (_prompService == null)
                {
                    throw new InvalidOperationException(
                        "PrompService is not initialized. Ensure it is registered in the DI container.");
                }
                return _prompService;
            }
        }

        public EmploymentBankContext Context { get; }
        public int JobPostId { get; set; }

        public string Title { get; set; } = "LLM Advisor";

        public event Action? OnRequestUIUpdate;

        public async ValueTask DisposeAsync() => await Context.DisposeAsync();

        public bool IsLoading { get; set; } = true;

        public bool IsError { get; set; } = false;

        public string ErrorDescription { get; set; } = string.Empty;

        public string JobTitle { get; set; } = string.Empty;

        public string[] InterviewQuestions { get; set; } = Array.Empty<string>();

        public string[] StudySubjects { get; set; } = Array.Empty<string>();

        public async Task InitializeAsync()
        {
            // If called by ViewModelBase before JobPostId is set, do nothing
            if (JobPostId <= 0) return;

            IsLoading = true;

            try
            {
                _apiKey = Environment.GetEnvironmentVariable(_apiKeyName);
                if (string.IsNullOrWhiteSpace(_apiKey))
                {
                    IsError = true;
                    UILoadError("Configuration Error",
                        $"API key not configured. Set environment variable '{_apiKeyName}'.");
                    return;
                }

                // Get JobPost 
                var jobPost = await Context
                    .JobPost.AsNoTracking()
                    .FirstOrDefaultAsync(jp => jp.Id == JobPostId);

                if (jobPost == null)
                {
                    // It should never happen because the page is only navigated to with a valid JobPostId,
                    // but we should still handle it just in case - Should we add loging here?
                    throw new InvalidOperationException($"JobPost with Id {JobPostId} not found.");
                }

                var jobDescriptionHash = jobPost.Description!.ToCanonicalHash();
                JobTitle = jobPost!.Title!;

                // get the JobAnalysisCache for the JobPost
                var analysisCache = await Context
                    .JobAnalysisCache
                    .AsNoTracking()
                    .FirstOrDefaultAsync(jac => jac.Hash == jobDescriptionHash);

                string analysisResult = string.Empty;
                if (analysisCache == null)
                {
                    var carreerAssistant = new CareerAssistant(_apiKey, _llmModel);

                    analysisResult = await carreerAssistant.RunLLMAnalysis(jobPost.Description!, PrompService.InterviewQuestions, PrompService.TimeoutSeconds);                    
                    analysisCache = await SaveAnalysisToCacheAsync(jobPost.Description!, jobDescriptionHash, _llmModel, "v1", analysisResult);
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

            async Task<JobAnalysisCache> SaveAnalysisToCacheAsync(string jobDescription, string jobDescriptionHash, string modelUsed, string promptVersion, string analysisResult)
            {
                var newCacheEntry = new JobAnalysisCache
                {
                    Hash = jobDescriptionHash,
                    JobPostDescription = jobDescription,
                    CreatedDate = DateTime.UtcNow,
                    ModelUsed = modelUsed,
                    PromptVersion = promptVersion,
                    Result = analysisResult
                };
                Context.JobAnalysisCache.Add(newCacheEntry);
                await Context.SaveChangesAsync();
                return newCacheEntry;
            }

            void UILoadCachedAnalysis(JobAnalysisCache analysisCache)
            {
                if (analysisCache == null) return;

                // Load analysis result from cache
                Title = $"LLM Advisor - Cached Analysis (Model: {analysisCache.ModelUsed}, Prompt Version: {analysisCache.PromptVersion})";

                var analysis = JsonSerializer.Deserialize<Management.AnalysisResult>(analysisCache.Result!);
                InterviewQuestions = analysis?.InterviewQuestions.ToArray() ?? Array.Empty<string>();
                StudySubjects = analysis?.StudySubjects.ToArray() ?? Array.Empty<string>();   
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

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
