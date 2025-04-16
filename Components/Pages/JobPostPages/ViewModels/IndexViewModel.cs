using JobBank.Data;
using JobBank.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace JobBank.Components.Pages.JobPostPages.ViewModels
{
    public class IndexViewModel : IIndexViewModel , IAsyncDisposable
    {
        public IQueryable<JobPostViewModel> JobPostsQueriable 
        { 
            get
            {
                return Context.JobPost                    
                    .Where(Predicate!)
                    .Select(jp => new JobPostViewModel(jp)).AsEnumerable()
                    .OrderByDescending(jp => jp.ApplicationDate)
                    .AsQueryable();
            }
        }

        public string JobTypeSearch { get; set; } = string.Empty;

        public bool ApplicationDeclined { get; set; }

        public bool DeclinedVisible { get; set; }

        public bool PendingOnly { get; set; }

        public bool PendingVisible { get; set; }

        public EmploymentBankContext Context { get; }

        public Expression<Func<JobPost, bool>>? Predicate 
        {
            get
            {
                return jp => ApplicationDeclined
                ? !string.IsNullOrEmpty(JobTypeSearch)
                    ? jp.ApplicationDeclined == true && jp.JobType!.Contains(JobTypeSearch)
                    : jp.ApplicationDeclined == true
                : PendingOnly
                    ? !string.IsNullOrEmpty(JobTypeSearch)
                        ? jp.ApplicationDeclined == false && jp.JobType!.Contains(JobTypeSearch)
                        : jp.ApplicationDeclined == false
                    : !string.IsNullOrEmpty(JobTypeSearch)
                        ? jp.JobType!.Contains(JobTypeSearch)
                        : true;
            }
        }

        public IndexViewModel(IDbContextFactory<EmploymentBankContext> DbFactory)
        {
            Context = DbFactory.CreateDbContext();
            DeclinedVisible = true;
            PendingVisible = true;
        }

        public async ValueTask DisposeAsync() => await Context.DisposeAsync();

        public void LoadRejectedApplication(ChangeEventArgs ev)
        {
            ApplicationDeclined = (bool)ev.Value!;
            PendingVisible = !ApplicationDeclined;
        }

        public void LoadPendingApplication(ChangeEventArgs ev)
        {
            PendingOnly = (bool)ev.Value!;
            DeclinedVisible = !PendingOnly;
        }
    }
}
