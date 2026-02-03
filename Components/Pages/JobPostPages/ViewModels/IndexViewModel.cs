using JobBank.Data;
using JobBank.Models;
using JobBank.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace JobBank.Components.Pages.JobPostPages.ViewModels
{
    public class IndexViewModel : IIndexViewModel, IAsyncDisposable
    {
        public string JobTypeSearch { get; set; } = string.Empty;

        public bool ApplicationDeclined { get; set; }

        public bool DeclinedVisible { get; set; }

        public bool PendingOnly { get; set; }

        public bool PendingVisible { get; set; }
        public FilteredStateService StateService { get; private set; }
        public EmploymentBankContext Context { get; }

        private DateTime? _fromDateTime;
        private DateTime? _toDateTime;

        // From must be <= To. Setting either side will adjust the other if needed.
        public DateTime? FromDateTime
        {
            get => _fromDateTime;
            set
            {
                if (_fromDateTime == value) return;

                _fromDateTime = value;
                if (_fromDateTime.HasValue && _toDateTime.HasValue && _fromDateTime.Value.Date > _toDateTime.Value.Date)
                {
                    // keep To at least as large as From
                    _toDateTime = _fromDateTime;                                     
                }

                // BROADCAST only on change   
                StateService.UpdateFilters(_fromDateTime, _toDateTime);
            }
        }

        public DateTime? ToDateTime
        {
            get => _toDateTime;
            set
            {
                if (_toDateTime == value) return;
                _toDateTime = value;

                if (_fromDateTime.HasValue && _toDateTime.HasValue && _toDateTime.Value.Date < _fromDateTime.Value.Date)
                {
                    // keep From no greater than To
                    _fromDateTime = _toDateTime;
                }

                // BROADCAST only on change
                StateService.UpdateFilters(_fromDateTime, _toDateTime);
            }
        }

        // Today's date for the input[max]
        public string? MaximumDateString => DateTime.Today.ToString("yyyy-MM-dd");

        // Max for the From control: either To (if set and earlier than today) or today
        public string? FromMax =>
            ToDateTime.HasValue
                ? (ToDateTime.Value.Date < DateTime.Today ? ToDateTime.Value.ToString("yyyy-MM-dd") : DateTime.Today.ToString("yyyy-MM-dd"))
                : DateTime.Today.ToString("yyyy-MM-dd");

        // Min for the To control: the From value (if set)
        public string? ToMin => FromDateTime?.ToString("yyyy-MM-dd");

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

        /// <summary>
        /// Note fitlered job posts based on the current Predicate and date range
        /// are applied in the server query for efficiency.
        /// Thus only the relevant records are retrieved from the database.
        /// and sorted by ApplicationDate descending.
        /// The grid component will then page through these results (TODO) and sort the projection.
        /// </summary>
        public IQueryable<JobPostViewModel> FilteredJobPosts
        {
            get
            {
                var query = Context.JobPost.AsNoTracking();

                if (Predicate != null) query = query.Where(Predicate);

                if (FromDateTime.HasValue)
                    query = query.Where(jp => jp.ApplicationDate >= FromDateTime.Value);

                if (ToDateTime.HasValue)
                {
                    var endOfDay = ToDateTime.Value.Date.AddDays(1);
                    query = query.Where(jp => jp.ApplicationDate < endOfDay);
                }

                return query.OrderByDescending(jp => jp.ApplicationDate)
                            .Select(jp => new JobPostViewModel
                            {
                                Id = jp.Id,
                                Title = jp.Title,
                                Company = jp.Company,
                                InterviewDate = jp.InterviewDate,
                                InterviewOutcome = jp.InterviewOutcome,
                                IsApplied = jp.IsApplied,
                                JobType = jp.JobType,
                                ActionToTake = jp.ActionToTake,
                                ApplicationDate = jp.ApplicationDate,
                                ApplicationDeclined = jp.ApplicationDeclined
                            });
            }
        }

        public IndexViewModel(IDbContextFactory<EmploymentBankContext> DbFactory, FilteredStateService stateService)
        {
            StateService = stateService;
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

        public string GetRowCssClass(JobPostViewModel jobPost)
        {
            if (jobPost.ApplicationDeclined)
            {
                return "declined-row"; 
            }
            else if (jobPost.InterviewDate.HasValue && !string.IsNullOrEmpty(jobPost.InterviewOutcome))
            {
                return "table-success";
            }
            else if (jobPost.InterviewDate.HasValue && string.IsNullOrEmpty(jobPost.InterviewOutcome))
            {
                return "table-warning"; 
            }

            return "applied-row";
        }
    }
}
