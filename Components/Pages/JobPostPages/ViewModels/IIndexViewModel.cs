using JobBank.Data;
using JobBank.Models;
using Microsoft.AspNetCore.Components;
using System.Linq.Expressions;

namespace JobBank.Components.Pages.JobPostPages.ViewModels
{
    public interface IIndexViewModel
    {
        IQueryable<JobPostViewModel> JobPostsQueriable {get; }

        string JobTypeSearch { get; set; }

        bool ApplicationDeclined { get; set; }

        bool DeclinedVisible  { get; set; }
    
        bool PendingOnly { get; set; }

        bool PendingVisible { get; set; }

        EmploymentBankContext Context { get; } 

        Expression<Func<JobPost, bool>>? Predicate { get; }

        void LoadRejectedApplication(ChangeEventArgs ev);

        void LoadPendingApplication(ChangeEventArgs ev);
    }
}
