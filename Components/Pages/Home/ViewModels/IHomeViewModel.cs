using JobBank.Data;

namespace JobBank.Components.Pages.Home.ViewModels
{
    public interface IHomeViewModel
    {
        string Title { get; }
        string Description { get; }
        EmploymentBankContext Context { get; }
    }
}
