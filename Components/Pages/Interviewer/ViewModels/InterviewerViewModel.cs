using JobBank.Management.Abstraction;
using JobBank.Management.Interview;
using JobBank.ModelsDTO;
using JobBank.Services.Abstraction;
using JobBank.StartUpServices;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Text.Json;
using static JobBank.Management.Abstraction.IInterviewLLMService;

namespace JobBank.Components.Pages.Interviewer.ViewModels
{
    public partial class InterviewerViewModel : IInterviewerViewModel
    {
        private readonly IJobPostService _jobPostService;
        private readonly IProtectedLocalStoreService<InterviewState> _interviewStateStore;
        private readonly IProtectedLocalStoreService<List<ChatMessage>> _interviewMessagesStore;
        private readonly IInterviewLLMService _llmInterviewService;
        private readonly IInterviewService _interviewService;
        private readonly IConfiguration _appsettings;
        private readonly IIdentityService _identityService;
        private readonly ILogger<InterviewerViewModel> _logger;

        private readonly PrompService _prompService;

        public InterviewerViewModel(
            IJobPostService jobPostService,
            IJSRuntime js,
            IProtectedLocalStoreService<InterviewState> interviewStateStore,
            IInterviewLLMService llmInterviewService,
            PrompService prompService,
            IConfiguration appsettings,
            IInterviewService interviewService,
            IIdentityService identityService,
            IProtectedLocalStoreService<List<ChatMessage>> interviewMessagesStore,
            ILogger<InterviewerViewModel> logger)
        {
            _jobPostService = jobPostService;
            _interviewStateStore = interviewStateStore;
            _llmInterviewService = llmInterviewService;
            _appsettings = appsettings;
            maxQuestions = _appsettings.GetValue<int>("Interview:InterviewMaxQuestion", 3);

            _prompService = prompService;
            _interviewService = interviewService;
            _identityService = identityService;
            _interviewMessagesStore = interviewMessagesStore;
            _logger = logger;
        }

        #region Interview State Tracking
        private List<string> CoveredTopics { get; set; } = new();
        private List<string> WeakAreas { get; set; } = new();
        private List<EvaluationResult> Evaluations { get; set; } = new();

        #endregion Interview State Tracking

        #region View Model Properties

        /// <summary>
        /// List of all messages in the conversation, including both user answers and interviewer questions.
        /// </summary>
        public List<ChatMessage> History { get; set; } = new();

        public string Title { get; set; } = interviewAgentName;

        [SupplyParameterFromQuery]
        public int JobPostId { get; set; }
        public string InterviewAgentQuestion { get; set; } = string.Empty;

        [SupplyParameterFromForm]
        public string? InterviewAnswer { get; set; }
        public bool IsProcessing { get; set; }

        public bool IsJobDescriptionAvailable { get; set; } = false;

        public bool IsHydrated { get; set; } = false;

        public string? ResponseMessage { get; set; }

        public string CompanyName { get; set; } = string.Empty;

        public string JobTitle { get; set; } = string.Empty;
        public string JobDescription { get; set; } = string.Empty;

        public event Action? OnRequestUIUpdate;

        public int QuestionCount { get; set; } = 1;

        public int QuestionMax => maxQuestions;

        public string QuestionTopic { get; set; } = string.Empty;

        public bool IsInterviewCompleted => QuestionCount > maxQuestions;

        public bool RequestScrollToBottom { get; set; }

        public string QuestionProgressCounter
        {
            get
            {
                if (IsInterviewCompleted)
                {
                    InterviewAgentQuestion = string.Empty;
                    return $"{QuestionMax}/{QuestionMax} Interview is complete — Score " +
                        $"{Evaluations.Sum(e => e.Score * e.Weight).ToString("0.00")} of {Evaluations.Sum(e => e.Weight).ToString("0.00")}" +
                        $" — Duration (hh:mm:ss) {Duration.ToString(@"hh\:mm\:ss")}";
                }
                return $"{QuestionCount}/{QuestionMax}";
            }
        }

        #endregion View Model Properties

        #region Answer Processing

        public async Task ProcessAnswerAsync(MouseEventArgs args)
        {
            await AnswerProcessingAsync($"Answer submitted (button clicked) : {InterviewAnswer}");
        }

        private async Task AnswerProcessingAsync(string message)
        {
            if (string.IsNullOrWhiteSpace(InterviewAnswer) || IsInterviewCompleted) return;

            History.Add(new ChatMessage(InterviewRole.Interviewer.ToString(), InterviewAgentQuestion, DateTime.Now));
            History.Add(new ChatMessage(InterviewRole.User.ToString(), InterviewAnswer, DateTime.Now));

            RequestScrollToBottom = true;
            var userId = await _identityService.GetUserIdAsync();

            var userResponse = new UserJobApplicantDTO
            {
                JobDescription = JobDescription,
                UserAnswer = InterviewAnswer,
                QuestionTopic = QuestionTopic,
                History = History,
                WeakAreas = WeakAreas,
                CoveredTopics = CoveredTopics,
                Evaluations = Evaluations,
                UserId = userId
            };

            InterviewAnswer = string.Empty;
            IsProcessing = true;
            QuestionCount++;

            try
            {
                var llmResponse = await _llmInterviewService.GetInterviewerAnalysisAsync(userResponse, systemPrompt);
                if(llmResponse == null)
                {
                    _logger.LogWarning("LLM service returned null response for JobPostId: {JobPostId}", JobPostId);
                    ResponseMessage = "Sorry, something went wrong with processing your answer. Please try again.";
                    return;
                }
                
                if (IsInterviewCompleted)
                {                    
                    ResponseMessage = "Interview completed. Thank you for your time!";
                    
                    var metadata = new InterviewMetadata
                    {
                        CoveredTopics = CoveredTopics,
                        WeakAreas = WeakAreas,
                        Evaluations = Evaluations
                    };

                    var interviewDto = new InterviewDTO
                    {
                        JobPostId = JobPostId,
                        UserId = userId,
                        Prompt = systemPrompt,
                        CreatedDateUtc = DateTime.UtcNow,
                        StartedAtUtc = History.Min(m => m.Timestamp),
                        CompletedAtUtc = History.Max(m => m.Timestamp),
                        ScoreMax = Evaluations.Sum(e => e.Weight),
                        ScoreTotal = (decimal)Evaluations.Sum(e => e.Score * e.Weight),
                        NumberOfQuestions = maxQuestions,
                        IsCompleted = IsInterviewCompleted,
                        Result = JsonSerializer.Serialize(metadata)
                    };

                    await _interviewService.AddInterviewAsync(interviewDto);

                    await _interviewStateStore.ClearAsync($"interview-state-{JobPostId}");
                    await _interviewMessagesStore.SaveAsync($"interview-messages-{interviewDto.Id}-{JobPostId}", History);

                    _logger.LogInformation("Interview completed for JobPostId: {JobPostId}, UserId: {UserId}, Score: {Score}/{MaxScore}",
                        JobPostId, userId, Evaluations.Sum(e => e.Score * e.Weight), Evaluations.Sum(e => e.Weight));

                    return;
                }

                InterviewAgentQuestion = llmResponse.AgentQuestion;
                QuestionTopic = llmResponse.QuestionTopic;
                CoveredTopics = llmResponse.CoveredTopics;
                WeakAreas = llmResponse.WeakAreas;

                if (llmResponse.Evaluation != null 
                    && !Evaluations.Any(e => e.Equals(llmResponse.Evaluation)))
                    Evaluations.Add(llmResponse.Evaluation!);
                
                await SaveToBrowserInterviewStateAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing answer for JobPostId: {JobPostId}, QuestionCount: {QuestionCount}", 
                    JobPostId, QuestionCount);
                ResponseMessage = "An unexpected error occurred. Please try again.";
            }
            finally
            {
                IsProcessing = false;
                OnRequestUIUpdate?.Invoke();                
            }
        }

        private TimeSpan Duration
        {
            get
            {
                if (History.Any())
                {
                    var first = History.Min(m => m.Timestamp);
                    var last = History.Max(m => m.Timestamp);

                    return last >= first ? last - first : TimeSpan.Zero;
                }
                else
                {
                    return TimeSpan.Zero;
                }
            }
        }

        #endregion Answer Processing

        #region Initialization and State Management

        /// <summary>
        /// Note: Do not call this method in the constructor or from InitializeAsync, 
        /// because it needs to be called after the component has rendered at least once.
        /// It sounds nuts but it's because we need to ensure that the JS interop is ready
        /// </summary>
        /// <returns></returns>
        public async Task RestoreFromBrowserAsync()
        {
            var state = await _interviewStateStore.LoadAsync($"interview-state-{JobPostId}");
            if (state != null && !_prompService.DisableBrowserStorage)
            {
                await _interviewStateStore.ClearAsync($"interview-state-{JobPostId}");
                _logger.LogInformation("Cleared browser storage for JobPostId: {JobPostId} (DisableBrowserStorage is true)", JobPostId);
            }

            if (state is null) 
            {
                _logger.LogDebug("No persisted interview state found for JobPostId: {JobPostId}", JobPostId);
                return;
            }

            if (state.JobPostId != JobPostId) 
            {
                _logger.LogWarning("JobPostId mismatch in persisted state. Expected: {Expected}, Found: {Found}", JobPostId, state.JobPostId);
                return;
            }

            JobPostId = state.JobPostId;
            History = state.History;
            JobDescription = state.JobDescription;
            QuestionCount = state.QuestionCount;
            CoveredTopics = state.CoveredTopics;
            WeakAreas = state.WeakAreas;
            Evaluations = state.Evaluations;
            
            IsJobDescriptionAvailable = !string.IsNullOrEmpty(JobDescription);
            _logger.LogInformation("Restored interview state from browser storage for JobPostId: {JobPostId}, Question: {QuestionCount}/{MaxQuestions}",
                JobPostId, QuestionCount, maxQuestions);
        }

        private async Task SaveToBrowserInterviewStateAsync()
        {
            var state = new InterviewState
            {
                JobPostId = JobPostId,
                JobDescription = JobDescription,
                History = History,
                CoveredTopics = CoveredTopics,
                WeakAreas = WeakAreas,
                Evaluations = Evaluations,
                QuestionCount = QuestionCount,
                IsFinished = QuestionCount > maxQuestions
            };
            await _interviewStateStore.SaveAsync($"interview-state-{JobPostId}", state);
        }

        #endregion Initialization and State Management

        #region Component Initialization

        public async Task InitializeAsync()
        {
            if (JobPostId <= 0) 
            {
                _logger.LogWarning("InitializeAsync called with invalid JobPostId: {JobPostId}", JobPostId);
                return;
            }

            try
            {
                IsProcessing = true;
                ResponseMessage = string.Empty;
                var jobPost = await _jobPostService.GetJobPostByIdAsync(JobPostId);
                if (jobPost == null)
                {
                    _logger.LogWarning("Job post not found for JobPostId: {JobPostId}", JobPostId);
                    ResponseMessage = "Job post not found.";
                    return;
                }

                CompanyName = jobPost.Company ?? string.Empty;
                JobTitle = jobPost.Title ?? string.Empty;

                JobDescription = jobPost.Description ?? "No job description provided. Interview not available.";

                IsJobDescriptionAvailable = !string.IsNullOrEmpty(jobPost.Description);
                if (!IsJobDescriptionAvailable) 
                {
                    _logger.LogWarning("Job post has no description for JobPostId: {JobPostId}", JobPostId);
                    return;
                }

                var response = await _llmInterviewService.GetInterviewerAnalysisAsync(
                    new UserJobApplicantDTO
                    {
                        JobDescription = JobDescription,
                        UserAnswer = string.Empty,
                        QuestionTopic = string.Empty,
                        History = History,
                        IsInterviewComplated = false,
                        WeakAreas = WeakAreas,
                        CoveredTopics = CoveredTopics,
                        UserId = await _identityService.GetUserIdAsync()
                    }, systemPrompt);

                InterviewAgentQuestion = response.AgentQuestion;
                QuestionTopic = response.QuestionTopic;
                CoveredTopics = response.CoveredTopics;

                _logger.LogInformation("Interview initialized for JobPostId: {JobPostId}, Company: {CompanyName}, Title: {JobTitle}",
                    JobPostId, CompanyName, JobTitle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing interview for JobPostId: {JobPostId}", JobPostId);
                ResponseMessage = "An error occurred while loading the job post. Please try again.";
            }
            finally
            {
                IsProcessing = false;
                OnRequestUIUpdate?.Invoke();
            }
        }

        public void Reset()
        {
            History = new();
            CoveredTopics = new();
            WeakAreas = new();
            Evaluations = new();
            QuestionCount = 1;
            InterviewAgentQuestion = string.Empty;
            ResponseMessage = string.Empty;
            IsProcessing = false;
            IsJobDescriptionAvailable = false;
            JobDescription = string.Empty;

            _logger.LogInformation("Interview state reset for JobPostId: {JobPostId}", JobPostId);
        }

        #endregion Component Initialization
    }
}
