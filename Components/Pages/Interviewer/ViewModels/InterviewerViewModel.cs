using JobBank.Extensions;
using JobBank.Services.Abstraction;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using static JobBank.Components.Pages.Interviewer.ViewModels.IInterviewerViewModel;

namespace JobBank.Components.Pages.Interviewer.ViewModels
{
    public partial class InterviewerViewModel : IInterviewerViewModel
    {
        private readonly IJobPostService _jobPostService;
        private readonly IJSRuntime _jsRuntime;
        private readonly IAnalysisCacheService _analysisCacheService;
        
        public InterviewerViewModel(IJobPostService jobPostService, IJSRuntime js, IAnalysisCacheService analysisCacheService)
        {
            _jobPostService = jobPostService;
            _jsRuntime = js;
            _analysisCacheService = analysisCacheService;
        }

        /// <summary>
        /// List of all messages in the conversation, including both user answers and interviewer questions.
        /// </summary>
        public List<ChatMessage> History { get; set; } = new ();

        public string Title { get; set; } = interviewAgentName;

        [SupplyParameterFromQuery]
        public int JobPostId { get; set; }
        public string InterviewAgentQuestion { get; set; } = string.Empty;  

        [SupplyParameterFromForm]
        public string? InterviewAnswer { get; set; }
        public bool IsProcessing { get; set; }

        public bool IsJobDescriptionAvailable { get; set; } = false;

        public string? ResponseMessage { get; set; }

        public string CompanyName { get; set; } = string.Empty;

        public string JobTitle { get; set; } = string.Empty;
        public string JobDescription { get; set; } = string.Empty;

        public event Action? OnRequestUIUpdate;

        public async Task ProcessAnswerAsync(MouseEventArgs args)
        {
            await AnswerProcessingAsync($"Answer submitted (button cliecked) : {InterviewAnswer}");
        }

        public bool ShouldPreventDefault { get; set; } = false;
        public int QuestionCount { get; set; } = 1;

        public int QuestionMax => maxQuestions;

        /// <summary>
        /// This is not working. Need to see if this is because S-SSR, but the intention is to allow the user to hit 
        /// enter to submit the answer, but it is not working
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public async Task SendAnswerAsync(KeyboardEventArgs e)
        {
            if (e.Key == "Enter" || e.Code == "NumpadEnter")
            {
                // ShouldPreventDefault = true; // Tell Blazor to stop the new line trying to
                                                // hit enter to submit the answer, but not working,
                                                // so leaving it off for now and just using the button
                                                // click to submit the answer

                await AnswerProcessingAsync($"Processing: {InterviewAnswer}");
            }
            else
            {
                //ShouldPreventDefault = false;
            }
        }

        private async Task AnswerProcessingAsync(string message)
        {
            if (string.IsNullOrWhiteSpace(InterviewAnswer)) return;
            
            History.Add(new ChatMessage("User", InterviewAnswer, DateTime.Now));

            var currentAnswer = InterviewAnswer;
            InterviewAnswer = string.Empty;

            IsProcessing = true;
            OnRequestUIUpdate?.Invoke();

            try
            {
                
                // Pass the 'History' list so the AI has the full context
                var response = "// await _interviewService.GetNextQuestionAsync(History)";

                // 4. Add AI's response to History
                History.Add(new ChatMessage("Interviewer", response, DateTime.Now));
                QuestionCount++;
            }
            finally
            {
                IsProcessing = false;
                OnRequestUIUpdate?.Invoke();
                await _jsRuntime.InvokeVoidAsync("scrollToBottom", "chat-container");  // fire the scrollong, on who?
                                                                                       // the chat container div, to scroll to the bottom after adding the new message
            }
        }

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
                    if(!IsJobDescriptionAvailable) return;
                    
                    var jobDescriptionHash = jobPost.Description!.ToCanonicalHash();



                    InterviewAgentQuestion = $"As an interviewer for {CompanyName}, what are the top 3 " +
                        $"qualities you look for in a candidate for the {JobTitle} position?";
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
    }
}
