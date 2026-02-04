namespace JobBank.Components.Pages.JobPostPages.ViewModels
{
    public class LLMAdvisorViewModel : ILLMAdvisorViewModel
    {        
        public int JobPostId { get; set; }

        public string Title { get; set; } = "LLM Advisor";

        public event Action? OnRequestUIUpdate;

        public Task InitializeAsync()
        {
            //
            return Task.CompletedTask; // to make the compiler happy
        }
    }
}
