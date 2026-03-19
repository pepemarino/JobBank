using JobBank.Services.Abstraction;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using static JobBank.Components.Pages.Interviewer.ViewModels.IInterviewerViewModel;

namespace JobBank.Components.Pages.Interviewer.ViewModels
{
    public class InterviewerViewModel : IInterviewerViewModel
    {
        private readonly IJobPostService _jobPostService;
        private readonly IJSRuntime _jsRuntime;
        public InterviewerViewModel(IJobPostService jobPostService, IJSRuntime js)
        {
            _jobPostService = jobPostService;
            _jsRuntime = js;
        }

        /// <summary>
        /// List of all messages in the conversation, including both user answers and interviewer questions.
        /// </summary>
        public List<ChatMessage> History { get; set; } = new ();

        public string Title { get; set; } = "Interviewer";

        [SupplyParameterFromQuery]
        public int JobPostId { get; set; }
        public string InterviewAgentQuestion { get; set; } = string.Empty;  

        [SupplyParameterFromForm]
        public string? InterviewAnswer { get; set; }
        public bool IsProcessing { get; set; }
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
                // 3. Call your Service (e.g., OpenAI/Semantic Kernel)
                // Pass the 'History' list so the AI has the full context
                var response = "// await _interviewService.GetNextQuestionAsync(History)";

                // 4. Add AI's response to History
                History.Add(new ChatMessage("Interviewer", response, DateTime.Now));
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
                    JobDescription = jobPost.Description ?? string.Empty;
                    InterviewAgentQuestion = $"As an interviewer for {CompanyName}, what are the top 3 " +
                        $"qualities you look for in a candidate for the {JobTitle} position?";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                IsProcessing = false;
                OnRequestUIUpdate?.Invoke();
            }            
        }
    }
}
