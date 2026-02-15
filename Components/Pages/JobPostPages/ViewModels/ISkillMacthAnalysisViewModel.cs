using JobBank.Components.Pages.Init;

namespace JobBank.Components.Pages.JobPostPages.ViewModels
{
    public interface ISkillMacthAnalysisViewModel : IAsyncInitialization
    {
        int JobPostId { get; set; }
        string Title { get; set; }
        string JobTitle { get; set; }
        bool IsLoading { get; set; }
        bool IsErrorOrWarning { get; set; }
        string ErrorOrWarningClass { get; set; }
        string ErrorOrWarningDescription { get; set; }
    }
}
