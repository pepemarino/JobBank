using JobBank.Management.Abstraction;
using JobBank.Management.Interview;
using JobBank.Services.Abstraction;
using JobBank.StartUpServices;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using static JobBank.Management.Abstraction.IInterviewLLMService;

namespace JobBank.Components.Pages.Interviewer.ViewModels
{
    public partial class InterviewerViewModel : IInterviewerViewModel
    {
        private readonly IJobPostService _jobPostService;
        private readonly IJSRuntime _jsRuntime;
        private readonly IAnalysisCacheService _analysisCacheService;
        private readonly IInterviewStateStore _interviewStateStore;
        private readonly IInterviewLLMService _llmInterviewService;
        private readonly IInterviewService _interviewService;
        private readonly IConfiguration _appsettings;

        private readonly PrompService _prompService;

        public InterviewerViewModel(
            IJobPostService jobPostService,
            IJSRuntime js,
            IAnalysisCacheService analysisCacheService,
            IInterviewStateStore interviewStateStore,
            IInterviewLLMService llmInterviewService,
            PrompService prompService,
            IConfiguration appsettings,
            IInterviewService interviewService)
        {
            _jobPostService = jobPostService;
            _jsRuntime = js;
            _analysisCacheService = analysisCacheService;
            _interviewStateStore = interviewStateStore;
            _llmInterviewService = llmInterviewService; // This is the service that will call the LLM to get the next question and analyze the user's answer, etc.
            _appsettings = appsettings;
            maxQuestions = _appsettings.GetValue<int>("Interview:InterviewMaxQuestion", 3);

            _prompService = prompService;
            _interviewService = interviewService;  // DAO for managing interview data, like saving final results to the database, etc.
        }

        #region Interview State Tracking
        private List<string> CoveredTopics { get; set; } = new(); // This list will keep track of the topics that have
                                                                  // already been covered in the interview,
                                                                  // to help the Agent avoid repeating questions on the same topic.
        private List<string> WeakAreas { get; set; } = new(); // This list will keep track of the candidate's weak areas identified during the interview,
                                                              // which can be used to drive adaptive questioning by asking follow-up questions
                                                              // to probe deeper into those areas.
                                                              // The trainer will use this.
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
            await AnswerProcessingAsync($"Answer submitted (button cliecked) : {InterviewAnswer}");
        }

        private async Task AnswerProcessingAsync(string message)
        {
            if (string.IsNullOrWhiteSpace(InterviewAnswer) || IsInterviewCompleted) return;

            History.Add(new ChatMessage(InterviewRole.Interviewer.ToString(), InterviewAgentQuestion, DateTime.Now));
            History.Add(new ChatMessage(InterviewRole.User.ToString(), InterviewAnswer, DateTime.Now));

            // Set this flag to true to indicate that the UI should scroll to the bottom to show the latest question and answer.
            RequestScrollToBottom = true;

            var userResponse = new UserJobApplicantDTO
            {
                JobDescription = JobDescription,
                UserAnswer = InterviewAnswer,
                QuestionTopic = QuestionTopic,
                History = History,
                WeakAreas = WeakAreas,
                CoveredTopics = CoveredTopics,
                Evaluations = Evaluations
            };

            InterviewAnswer = string.Empty;
            IsProcessing = true;            

            try
            {
                var llmRresponse = await _llmInterviewService.GetInterviewerAnalysisAsync(userResponse, systemPrompt);
                if(llmRresponse == null)
                {
                    ResponseMessage = "Sorry, something went wrong with processing your answer. Please try again.";
                    return;
                }

                if (IsInterviewCompleted)
                {
                    QuestionCount = maxQuestions;
                    ResponseMessage = "Interview completed. Thank you for your time!";

                    // need to do management and cleanup here, like clearing the browser state,
                    // and maybe saving the final interview result to the server for future reference by the user and for training the model.

                    return;
                }

                InterviewAgentQuestion = llmRresponse.AgentQuestion;
                QuestionTopic = llmRresponse.QuestionTopic;
                CoveredTopics = llmRresponse.CoveredTopics;
                WeakAreas = llmRresponse.WeakAreas;

                if (llmRresponse.Evaluation != null 
                    && !Evaluations.Any(e => e.Equals(llmRresponse.Evaluation)))
                    Evaluations.Add(llmRresponse.Evaluation!);

                QuestionCount++;

                await SaveToBrowserInterviewStateAsync();
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
            var state = await _interviewStateStore.LoadAsync(JobPostId);
            if (state != null && _prompService.DisableBrowserStorage)
                await _interviewStateStore.ClearAsync(JobPostId);

            if (state is null) return;

            if (state.JobPostId != JobPostId) return;

            JobPostId = state.JobPostId;
            History = state.History;
            JobDescription = state.JobDescription;
            QuestionCount = state.QuestionCount;
            CoveredTopics = state.CoveredTopics;
            WeakAreas = state.WeakAreas;
            Evaluations = state.Evaluations;
            
            IsJobDescriptionAvailable = !string.IsNullOrEmpty(JobDescription);
        }

        private async Task<InterviewState?> LoadFromBrowserInterviewStateAsync(int jobPostId)
        {
            return await _interviewStateStore.LoadAsync(jobPostId);
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
            await _interviewStateStore.SaveAsync(JobPostId, state);
        }

        #endregion Initialization and State Management

        #region Component Initialization

        public async Task InitializeAsync()
        {
            if (JobPostId <= 0) return;

            try
            {
                IsProcessing = true;
                ResponseMessage = string.Empty;
                var jobPost = await _jobPostService.GetJobPostByIdAsync(JobPostId);
                if (jobPost == null)
                {
                    ResponseMessage = "Job post not found.";
                    return;
                }

                if (jobPost != null)
                {
                    CompanyName = jobPost.Company ?? string.Empty;
                    JobTitle = jobPost.Title ?? string.Empty;

                    // this is shown in the view if the job description is not available,
                    // so we want to set it to a message instead of leaving it blank
                    JobDescription = jobPost.Description ?? "No job description provided. Interview not available.";

                    IsJobDescriptionAvailable = !string.IsNullOrEmpty(jobPost.Description);
                    if (!IsJobDescriptionAvailable) return;

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
                        }, systemPrompt);

                    InterviewAgentQuestion = response.AgentQuestion;
                    QuestionTopic = response.QuestionTopic;
                    CoveredTopics = response.CoveredTopics;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);  // need to log this properly
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
        }

        #endregion Component Initialization
    }
}
