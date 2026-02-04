using JobBank.Components.Pages.Init;

namespace JobBank.Components.Pages.JobPostPages.ViewModels
{
    public interface ILLMAdvisorViewModel : IAsyncInitialization
    {
        int JobPostId { get; set; }
        string Title { get; set; }
    }
}
