
using AutoMapper;
using JobBank.Models;
using JobBank.Services;
using JobBank.Services.Abstraction;
using JobBank.StartUpServices;
using static JobBank.Components.Pages.JobPostPages.ViewModels.SkillMacthAnalysisViewModel;

namespace JobBank.Components.Pages.JobPostPages.ViewModels
{
    public class SkillMacthAnalysisViewModel : ISkillMacthAnalysisViewModel
    {
        private readonly PrompService _prompService;
        private readonly IJobPostService _jobPostService;
        private readonly ISkillsService _skillsService;
        private readonly IMapper _mapper;

        public enum ExitType
        {
            IsError,
            IsWarning
        }

        public SkillMacthAnalysisViewModel(
            PrompService prompService, 
            IMapper mapper, 
            IJobPostService jobPostService, 
            ISkillsService skillsService)
        {
            _mapper = mapper;
            _prompService = prompService;
            _jobPostService = jobPostService;
            _skillsService = skillsService;
        }

        public int JobPostId { get; set; }
        public string Title { get; set; } = "Skill Set Match";
        public bool IsLoading { get; set; } = true;
        public bool IsErrorOrWarning { get; set; } = false;
        public string ErrorOrWarningDescription { get; set; } = string.Empty;
        public string ErrorOrWarningClass { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;

        public event Action? OnRequestUIUpdate;

        public async Task InitializeAsync()
        {
            if (JobPostId <= 0) return;

            IsLoading = true;
            var llmModel = _prompService.LLMModel;
            var apiKeyName = _prompService.ApiKeyName;

            try
            {
                var apiKey = Environment.GetEnvironmentVariable(apiKeyName);
                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    IsErrorOrWarning = true;
                    UILoadErrorOrWarning("Configuration Error",
                        $"API key not configured. Set environment variable '{apiKeyName}'.", ExitType.IsError);
                    return;
                }

                var jobPost = await _jobPostService.GetJobPostByIdAsync(JobPostId);

                if (jobPost == null)
                {
                    throw new InvalidOperationException($"JobPost with Id {JobPostId} not found.");
                }

                JobTitle = jobPost!.Title!;

                // if no skills fast exit
                var userSkills = await _skillsService.GetUserSkillsWithLazyPropsAsync(1); // we have no users yet. Next
                if (userSkills == null || string.IsNullOrEmpty(userSkills.RawSkills))
                {
                    IsErrorOrWarning = true;
                    ErrorOrWarningDescription = "Please add your professional skills to your profile to continue. " +
                        "List them separated by commas (e.g., Food Safety, Electrical Theory, Class 5 DL).";
                    UILoadErrorOrWarning("Skills Required for Analysis", ErrorOrWarningDescription, ExitType.IsWarning);
                }


            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., log the error, show a message to the user, etc.)
                Console.Error.WriteLine($"Error initializing LLMAdvisorViewModel: {ex.Message}");
                IsErrorOrWarning = true;
                UILoadErrorOrWarning("API Error", ex.Message, ExitType.IsError);
            }
            finally
            {
                IsLoading = false;
                OnRequestUIUpdate?.Invoke();
            }

            void UILoadCachedAnalysis(JobAnalysisCache analysisCache)
            {
                if (analysisCache == null) return;

                // Load analysis result from cache
                Title = $"Skill Analysis - Cached Analysis (Model: {analysisCache.ModelUsed}, Prompt Version: {analysisCache.PromptVersion})";


            }
 
            void UILoadErrorOrWarning(string errorType, string errorMessage, ExitType exitType)
            {
                // Load analysis result from cache
                Title = $"Skill Analysis - {errorType}";
                ErrorOrWarningDescription = errorMessage;
                ErrorOrWarningClass = exitType == ExitType.IsError 
                    ? "danger"
                    : "warning" ;
            }
        }
    }
}
