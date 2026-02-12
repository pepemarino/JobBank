using JobBank.Components.Pages.Init;
using JobBank.Data;

namespace JobBank.Components.Pages.JobPostPages.ViewModels
{
    public interface ILLMAdvisorViewModel : IAsyncInitialization, IAsyncDisposable
    {
        int JobPostId { get; set; }
        string Title { get; set; }
        string JobTitle { get; set; }
        bool IsLoading { get; set; }
        bool IsError { get; set; }
        string ErrorDescription { get; set; }
        string[] InterviewQuestions { get; set; }
        string[] StudySubjects { get; set; }

        EmploymentBankContext Context { get; }
    }
}
