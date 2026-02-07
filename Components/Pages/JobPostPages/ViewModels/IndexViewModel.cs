using JobBank.Data;
using JobBank.Models;
using JobBank.Services;
using LinqKit;
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

        /// <summary>
        /// Builds the filter expression based on the current search criteria.
        /// </summary>
        /// <returns>
        /// Returns an expression that can be used to filter JobPost entities.
        /// </returns>
        private Expression<Func<JobPost, bool>> BuildFilter()
        {
            // Initialize with 'true' so we can append AND conditions
            var filter = PredicateBuilder.New<JobPost>(true);

            if (!string.IsNullOrEmpty(JobTypeSearch))
                filter = filter.And(jp => jp.JobType!.Contains(JobTypeSearch));

            if (!string.IsNullOrEmpty(CompanySearch))
                filter = filter.And(jp => jp.Company!.Contains(CompanySearch));

            // UI handles exclusivity, so we just check which one is active
            if (ApplicationDeclined)
                filter = filter.And(jp => jp.ApplicationDeclined);
            else if (PendingOnly)
                filter = filter.And(jp => !jp.ApplicationDeclined);

            return filter;
        }

        /// <summary>
        /// Note fitlered job posts based on the current Predicate and date range
        /// are applied in the server query for efficiency.
        /// Thus only the relevant records are retrieved from the database.
        /// and sorted by ApplicationDate descending.
        /// The grid component sorts the projection.
        /// No paging is applied here; the grid component handles that IF we enable it.
        /// </summary>
        public IQueryable<JobPostViewModel> FilteredJobPosts
        {
            get
            {
                return Context.JobPost.AsNoTracking().AsExpandable()
                    .Where(BuildFilter())
                    .Where(jp => !FromDateTime.HasValue || jp.ApplicationDate >= FromDateTime.Value)
                    .Where(jp => !ToDateTime.HasValue || jp.ApplicationDate < ToDateTime.Value.Date.AddDays(1))
                    .OrderByDescending(jp => jp.ApplicationDate)
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

        public string CompanySearch { get; set; }

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

        /// <summary>
        /// This method returns the CSS class for a job post row based on its status.
        /// Take a look in the grid where this is used to see the color coding.
        /// </summary>
        /// <param name="jobPost"></param>
        /// <returns></returns>
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
