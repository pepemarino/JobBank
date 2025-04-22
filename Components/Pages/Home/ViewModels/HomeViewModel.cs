using JobBank.Data;

namespace JobBank.Components.Pages.Home.ViewModels
{
    public class HomeViewModel : IHomeViewModel
    {
        public HomeViewModel() 
        {
            this.Title = "Job Bank Applications";
        }

        public string Title { get; }
        public string Description { get; }

        public EmploymentBankContext Context { get; }
    }
}
