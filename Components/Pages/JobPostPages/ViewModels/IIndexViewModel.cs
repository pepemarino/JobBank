using JobBank.Data;
using JobBank.Models;
using Microsoft.AspNetCore.Components;
using System.Linq.Expressions;

namespace JobBank.Components.Pages.JobPostPages.ViewModels
{
    public interface IIndexViewModel
    {
        IQueryable<JobPostViewModel> FilteredJobPosts { get; }

        string JobTypeSearch { get; set; }

        string CompanySearch { get; set; }

        bool ApplicationDeclined { get; set; }

        bool DeclinedVisible  { get; set; }
    
        bool PendingOnly { get; set; }

        bool PendingVisible { get; set; }

        DateTime? FromDateTime { get; set; }

        DateTime? ToDateTime { get; set; }

        // Today's date in yyyy-MM-dd (for input[type=date] max)
        string? MaximumDateString { get; }

        // For binding max on the input
        string? FromMax { get; }

        // For binding min on the input
        string? ToMin { get; }

        EmploymentBankContext Context { get; } 

        void LoadRejectedApplication(ChangeEventArgs ev);

        void LoadPendingApplication(ChangeEventArgs ev);

        string GetRowCssClass(JobPostViewModel jobPost);
    }
}
