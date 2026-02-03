using ChartJs.Blazor;
using ChartJs.Blazor.BarChart;
using ChartJs.Blazor.BarChart.Axes;
using ChartJs.Blazor.Common;
using ChartJs.Blazor.Common.Axes;
using ChartJs.Blazor.Common.Axes.Ticks;
using ChartJs.Blazor.Common.Enums;
using ChartJs.Blazor.Util;
using JobBank.Data;
using JobBank.Services;
using Microsoft.EntityFrameworkCore;
using System.Drawing;

namespace JobBank.Components.Pages.Home.ViewModels
{
    public class HomeViewModel : IHomeViewModel, IAsyncDisposable
    {
        public FilteredStateService StateService { get; private set; }

        private readonly EmploymentBankContext Context;
        private readonly List<DailyStatsViewModel> _dailyStats = new();

        public event Action? OnRequestUIUpdate;

        private DateTime? FromDate { get; set; }
        private DateTime? ToDate { get; set; }

        public HomeViewModel(IDbContextFactory<EmploymentBankContext> DbFactory, FilteredStateService stateService)
        {
            StateService = stateService;

            FromDate = StateService.FromDate;
            ToDate = StateService.ToDate;

            Context = DbFactory.CreateDbContext();
            this.Title = "Job Bank Active Applications";

            // Initialize Config and ensure Data collections exist (ChartJs.Blazor exposes Data as a read-only property).
            this.Config = new BarConfig();
            this.Config.Data.Labels.Clear();
            this.Config.Data.Datasets.Clear();

            // More visible defaults: disable aspect ratio so the chart fills its container,
            // set a clearer title and keep legend on top.
            this.Config.Options = new BarOptions
            {
                Responsive = true,
                MaintainAspectRatio = false,
                Legend = new Legend
                {
                    Position = Position.Top
                },
                Title = new OptionsTitle
                {
                    Display = true,
                    Text = "Applications by Date"
                },
                // Ensure the Y axis begins at zero (prevents chart from starting at 1 when there's a single entry).
                Scales = new BarScales
                {
                    YAxes = new List<CartesianAxis>
                    {
                        new BarLinearCartesianAxis
                        {
                            Ticks = new LinearCartesianTicks
                            {
                                BeginAtZero = true,
                                Min = 0 // explicit min helps with some Chart.js versions
                            },
                            GridLines = new GridLines
                            {
                                Color = ColorUtil.FromDrawingColor(Color.FromArgb(40, 0, 0, 0)) // subtle grid lines
                            }
                        }
                    },
                    XAxes = new List<CartesianAxis>
                    {
                        new BarCategoryAxis
                        {
                            Ticks = new CategoryTicks
                            {
                                // rotate or auto-skip can be configured here if many labels exist
                            }
                        }
                    }
                }
            };

            StateService.OnChange += HandleStateChange;
        }

        public Chart JobChart { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        // back the IEnumerable so callers can inspect the stats if needed
        public IEnumerable<DailyStatsViewModel> DailyStatsViewModels => _dailyStats;

        private BarConfig Config { get; set; }

        BarConfig IHomeViewModel.Config
        {
            get => Config;
            set => Config = value;
        }

        public async ValueTask DisposeAsync()
        {
            await Context.DisposeAsync();
            StateService.OnChange -= HandleStateChange;
        }

        public async Task InitializeAsync()
        {            
            await LoadData();
        }

        private async void HandleStateChange()
        {
            FromDate = StateService.FromDate;
            ToDate = StateService.ToDate;

            await LoadData();
            OnRequestUIUpdate?.Invoke();

        }

        private async Task LoadData()
        {
            // build labels and a single dataset with counts for each date
            var labels = new List<string>();
            var values = new List<int>();

            List<Models.JobPost> jobPosts = await DateFilteredJobPosts();

            // group by application date and count
            // project to DailyStatsViewModel for easier consumption
            var stats = jobPosts
                .GroupBy(pg => pg.ApplicationDate)
                .Select(g => new DailyStatsViewModel
                {
                    Count = g.Count(),
                    Name = g.Key!.Value.ToString("yyyy-MM-dd")
                })
                .ToList();

            _dailyStats.Clear();
            _dailyStats.AddRange(stats);

            foreach (var stat in stats)
            {
                labels.Add(stat.Name);
                values.Add(stat.Count);
            }

            // replace labels & datasets (clear previous)
            this.Config.Data.Labels.Clear();
            foreach (var l in labels)
            {
                this.Config.Data.Labels.Add(l);
            }

            this.Config.Data.Datasets.Clear();

            if (values.Count == 0)
            {
                // show a subtle "no data" placeholder so the UI doesn't look broken
                this.Config.Data.Labels.Add("No Data");
                this.Config.Data.Datasets.Add(
                    new BarDataset<int>(new[] { 0 })
                    {
                        Label = "No data",
                        BackgroundColor = ColorUtil.FromDrawingColor(Color.LightGray),
                        BorderColor = ColorUtil.FromDrawingColor(Color.DarkGray),
                        BorderWidth = 1
                    }
                );
            }
            else
            {
                // single dataset with visible styling
                this.Config.Data.Datasets.Add(
                    new BarDataset<int>(values)
                    {
                        Label = "Applications",
                        // semi-transparent blue for good contrast
                        BackgroundColor = ColorUtil.FromDrawingColor(Color.FromArgb(200, 0, 123, 255)),
                        BorderColor = ColorUtil.FromDrawingColor(Color.FromArgb(255, 0, 82, 204)),
                        BorderWidth = 1
                    }
                );
            }
        }

        /// <summary>
        /// Filtering and ordering job posts by application date is done at the database level for efficiency.
        /// </summary>
        /// <returns></returns>
        private async Task<List<Models.JobPost>> DateFilteredJobPosts()
        {
            var query = Context.JobPost.AsNoTracking();

            if (FromDate.HasValue)
                query = query.Where(jp => jp.ApplicationDate >= FromDate.Value); // push date filter to the database

            if (ToDate.HasValue)
                query = query.Where(jp => jp.ApplicationDate <= ToDate.Value);   // push date filter to the database

            return await query
                .Where(jp => jp.ApplicationDate.HasValue && !jp.ApplicationDeclined)
                .OrderBy(jp => jp.ApplicationDate)
                .ToListAsync();
        }
    }
}
