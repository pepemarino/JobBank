using AutoMapper;
using AutoMapper.QueryableExtensions;
using ChartJs.Blazor;
using ChartJs.Blazor.BarChart;
using ChartJs.Blazor.BarChart.Axes;
using ChartJs.Blazor.Common;
using ChartJs.Blazor.Common.Axes;
using ChartJs.Blazor.Common.Axes.Ticks;
using ChartJs.Blazor.Common.Enums;
using ChartJs.Blazor.PieChart;
using ChartJs.Blazor.Util;
using JobBank.Data;
using JobBank.Models;
using JobBank.ModelsDTO;
using JobBank.Services;
using JobBank.Services.Abstraction;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;

namespace JobBank.Components.Pages.Home.ViewModels
{
    public class HomeViewModel : IHomeViewModel, IAsyncDisposable
    {
        private readonly bool IgnoreFilterByDayFlag = false;
        private readonly IIdentityService _identityService;
        private readonly IMapper _mapper;

        public FilteredStateService StateService { get; private set; }

        private readonly EmploymentBankContext Context;
        private readonly List<DailyStatsViewModel> _dailyStats = new();
        private readonly List<DailyStatsViewModel> _automaticallyDeclinedStats = new();
        private readonly List<DailyStatsViewModel> _declinedStats = new();
        private readonly List<DailyStatsViewModel> _abortedStats = new();   

        public event Action? OnRequestUIUpdate;

        private DateTime? FromDate { get; set; }
        private DateTime? ToDate { get; set; }

        public HomeViewModel(
            IDbContextFactory<EmploymentBankContext> DbFactory,
            FilteredStateService stateService,
            IIdentityService identityService,
            IMapper mapper)
        {
            StateService = stateService;
            _identityService = identityService;
            _mapper = mapper;

            FromDate = StateService.FromDate;
            ToDate = StateService.ToDate;

            Context = DbFactory.CreateDbContext();
            this.Title = "WORKS Commons Application Dashboard";

            StateService.OnChange += HandleStateChange;
        }

        private void SetDonutChart()
        {
            this.PieConfig = new PieConfig
            {
                Options = new PieOptions
                {
                    Responsive = true,
                    MaintainAspectRatio = false,

                    // This instantly turns Pie chart into a true Donut ring! hungry?
                    CutoutPercentage = 65,

                    Legend = new Legend { Position = Position.Top },
                    Title = new OptionsTitle { Display = true, Text = "Application Status Distribution" }
                }
            };
        }

        private void SetBarChart()
        {
            this.Config = new BarConfig();
            this.Config.Data.Labels.Clear();
            this.Config.Data.Datasets.Clear();

            this.Config.Options = new BarOptions
            {
                Responsive = true,
                MaintainAspectRatio = false,
                Legend = new Legend { Position = Position.Top },
                Title = new OptionsTitle { Display = true, Text = "Applications by Date" },
                Scales = new BarScales
                {
                    YAxes = new List<CartesianAxis>
                    {
                        new BarLinearCartesianAxis
                        {
                            Ticks = new LinearCartesianTicks { BeginAtZero = true, Min = 0 },
                            GridLines = new GridLines { Color = ColorUtil.FromDrawingColor(Color.FromArgb(40, 0, 0, 0)) }
                        }
                    },
                    XAxes = new List<CartesianAxis> { new BarCategoryAxis() }
                }
            };
        }

        public BarConfig AutomaticallyDeclinedHorizontalConfig { get; set; }
        public BarConfig DeclinedHorizontalConfig { get; set; }
        public BarConfig AbortedHorizontalConfig { get; set; }

        public Chart JobChart { get; set; }
        public Chart DonutChart { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        public IEnumerable<DailyStatsViewModel> DailyStatsViewModels => _dailyStats;

        private BarConfig Config { get; set; }

        BarConfig IHomeViewModel.Config
        {
            get => Config;
            set => Config = value;
        }

        PieConfig IHomeViewModel.PieConfig
        {
            get => PieConfig;
            set => PieConfig = value;
        }

        public PieConfig PieConfig { get; set; }
        public Chart DonutCart { get; set; }
        public Chart AutomaticallyDeclinedHorizontalChart { get; set; }
        public Chart DeclinedHorizontalChart { get; set; }

        public Chart AbortedHorizontalChart { get; set; }

        public async ValueTask DisposeAsync()
        {
            await Context.DisposeAsync();
            StateService.OnChange -= HandleStateChange;
        }

        public async Task InitializeAsync()
        {
            SetBarChart();
            SetAutomaticallyDeclinedHorizontalChart();
            SetDeclinedHorizontalChart();
            SetAbortedHorizontalConfig();
            SetDonutChart();
            await LoadBarChartData();
            await LoadAutomaticallyDeclinedHorizontalData();
            await LoadDeclinedHorizontalData();
            await LoadAbortedHorizontalData();
            await LoadDonutChartData();
        }

        private async Task LoadAbortedHorizontalData()
        {
            var stats = await (await UserJobApplicationsQuery(IgnoreFilterByDayFlag))
                .Where(jobPost => jobPost.IsSelfWitdrawn)
                .Where(jobPost => jobPost.Company != null && jobPost.Company != string.Empty)
                .GroupBy(jobPost => jobPost.Company)
                .Select(group => new DailyStatsViewModel
                {
                    Name = group.Key!,
                    Count = group.Count()
                })
                .OrderByDescending(stat => stat.Count)
                .ThenBy(stat => stat.Name)
                .ToListAsync();

            _abortedStats.Clear();
            _abortedStats.AddRange(stats);

            this.AbortedHorizontalConfig.Data.Labels.Clear();
            this.AbortedHorizontalConfig.Data.Datasets.Clear();
            if (stats.Count == 0)
            {
                this.AbortedHorizontalConfig.Data.Labels.Add("No Data");
                this.AbortedHorizontalConfig.Data.Datasets.Add(
                    new BarDataset<int>(data: new[] { 0 }, horizontal: true)
                    {
                        Label = "No data",
                        BackgroundColor = ColorUtil.FromDrawingColor(Color.LightGray),
                        BorderColor = ColorUtil.FromDrawingColor(Color.DarkGray),
                        BorderWidth = 1
                    }
                );

                return;
            }

            foreach (var stat in stats)
            {
                this.AbortedHorizontalConfig.Data.Labels.Add(stat.Name);
            }

            this.AbortedHorizontalConfig.Data.Datasets.Add(
                new BarDataset<int>(
                    data: stats.Select(stat => stat.Count).ToList(),
                    horizontal: true)
                {
                    Label = "Applications",
                    BackgroundColor = "#FFCE56",
                    BorderColor = "#997b32",
                    BorderWidth = 1
                }
            );
        }

        private void SetAbortedHorizontalConfig()
        {
            this.AbortedHorizontalConfig = new BarConfig(horizontal: true);

            this.AbortedHorizontalConfig.Data.Labels.Clear();
            this.AbortedHorizontalConfig.Data.Datasets.Clear();

            this.AbortedHorizontalConfig.Options = new BarOptions
            {
                Responsive = true,
                MaintainAspectRatio = false,
                Legend = new Legend { Position = Position.Top },
                Title = new OptionsTitle { Display = true, Text = "Top Aborted" },
                Scales = new BarScales
                {
                    XAxes = new List<CartesianAxis>
                    {
                        new BarLinearCartesianAxis
                        {
                            Ticks = new LinearCartesianTicks { BeginAtZero = true, Min = 0 },
                            GridLines = new GridLines { Color = ColorUtil.FromDrawingColor(Color.FromArgb(40, 0, 0, 0)) }
                        }
                    },
                    YAxes = new List<CartesianAxis> { new BarCategoryAxis() }
                }
            };
        }

        private async Task LoadDeclinedHorizontalData()
        {
            var stats = await (await UserJobApplicationsQuery(IgnoreFilterByDayFlag))
                .Where(jobPost => !jobPost.AutomaticallyRejected && jobPost.ApplicationDeclined && !jobPost.IsSelfWitdrawn)
                .Where(jobPost => jobPost.Company != null && jobPost.Company != string.Empty)
                .GroupBy(jobPost => jobPost.Company)
                .Select(group => new DailyStatsViewModel
                {
                    Name = group.Key!,
                    Count = group.Count()
                })
                .Where(stat => stat.Count >= 2)
                .OrderByDescending(stat => stat.Count)
                .ThenBy(stat => stat.Name)
                .ToListAsync();

                _declinedStats.Clear();
                _declinedStats.AddRange(stats);

            this.DeclinedHorizontalConfig.Data.Labels.Clear();
            this.DeclinedHorizontalConfig.Data.Datasets.Clear();
            if (stats.Count == 0)
            {
                this.DeclinedHorizontalConfig.Data.Labels.Add("No Data");
                this.DeclinedHorizontalConfig.Data.Datasets.Add(
                    new BarDataset<int>(data: new[] { 0 }, horizontal: true)
                    {
                        Label = "No data",
                        BackgroundColor = ColorUtil.FromDrawingColor(Color.LightGray),
                        BorderColor = ColorUtil.FromDrawingColor(Color.DarkGray),
                        BorderWidth = 1
                    }
                );

                return;
            }

            foreach (var stat in stats)
            {
                this.DeclinedHorizontalConfig.Data.Labels.Add(stat.Name);
            }

            this.DeclinedHorizontalConfig.Data.Datasets.Add(
                new BarDataset<int>(
                    data: stats.Select(stat => stat.Count).ToList(),
                    horizontal: true)
                {
                    Label = "Applications",
                    BackgroundColor = "#FF6384",
                    BorderColor = "#96384c",
                    BorderWidth = 1
                }
            );
        }

        private void SetDeclinedHorizontalChart()
        {
            this.DeclinedHorizontalConfig = new BarConfig(horizontal: true);

            this.DeclinedHorizontalConfig.Data.Labels.Clear();
            this.DeclinedHorizontalConfig.Data.Datasets.Clear();

            this.DeclinedHorizontalConfig.Options = new BarOptions
            {
                Responsive = true,
                MaintainAspectRatio = false,
                Legend = new Legend { Position = Position.Top },
                Title = new OptionsTitle { Display = true, Text = "Top Declined" },
                Scales = new BarScales
                {
                    XAxes = new List<CartesianAxis>
                    {
                        new BarLinearCartesianAxis
                        {
                            Ticks = new LinearCartesianTicks { BeginAtZero = true, Min = 0 },
                            GridLines = new GridLines { Color = ColorUtil.FromDrawingColor(Color.FromArgb(40, 0, 0, 0)) }
                        }
                    },
                    YAxes = new List<CartesianAxis> { new BarCategoryAxis() }
                }
            };
        }

        private void SetAutomaticallyDeclinedHorizontalChart()
        {
            this.AutomaticallyDeclinedHorizontalConfig = new BarConfig(horizontal: true);

            this.AutomaticallyDeclinedHorizontalConfig.Data.Labels.Clear();
            this.AutomaticallyDeclinedHorizontalConfig.Data.Datasets.Clear();

            this.AutomaticallyDeclinedHorizontalConfig.Options = new BarOptions
            {
                Responsive = true,
                MaintainAspectRatio = false,
                Legend = new Legend { Position = Position.Top },
                Title = new OptionsTitle { Display = true, Text = "Top Automatically Declined" },
                Scales = new BarScales
                {
                    XAxes = new List<CartesianAxis>
                    {
                        new BarLinearCartesianAxis
                        {
                            Ticks = new LinearCartesianTicks { BeginAtZero = true, Min = 0 },
                            GridLines = new GridLines { Color = ColorUtil.FromDrawingColor(Color.FromArgb(40, 0, 0, 0)) }
                        }
                    },
                    YAxes = new List<CartesianAxis> { new BarCategoryAxis() }
                }
            };
        }

        private async void HandleStateChange()
        {
            FromDate = StateService.FromDate;
            ToDate = StateService.ToDate;

            // Instead of calling .Clear() instantly here, let the load methods update the 
            // arrays and call .Update() on the JS wrapper instance. This prevents rendering flashes.

            await LoadBarChartData();
            await LoadAutomaticallyDeclinedHorizontalData();
            await LoadDonutChartData();

            // Ask the components to explicitly redraw their JS instances
            if (JobChart != null) await JobChart.Update();
            if (DonutChart != null) await DonutChart.Update();

            OnRequestUIUpdate?.Invoke();
        }

        private async Task LoadDonutChartData()
        {
            var valueCounts = await ApplicationStatusCounts();

            var labels = valueCounts.Keys.Select(k => k.ToString()).ToList();
            var values = valueCounts.Values.ToList();

            // Instantiated a PieDataset instead of DoughnutDataset
            PieDataset<int> dataset = new PieDataset<int>(values)
            {
                // Used a refined pastel/modern color palette instead of harsh raw colors
                BackgroundColor = new[] { "#36A2EB", "#FF6384", "#FFCE56", "#4BC0C0" },
                BorderColor = new[] { "#FFFFFF", "#FFFFFF", "#FFFFFF", "#FFFFFF" }, // White borders separate arcs elegantly
                BorderWidth = 2,
                HoverBorderWidth = 3
            };

            this.PieConfig.Data.Labels.Clear();
            foreach (var label in labels)
            {
                this.PieConfig.Data.Labels.Add(label);
            }
            this.PieConfig.Data.Datasets.Clear();
            this.PieConfig.Data.Datasets.Add(dataset);
        }

        private async Task LoadAutomaticallyDeclinedHorizontalData()
        {
            var stats = await (await UserJobApplicationsQuery(IgnoreFilterByDayFlag))
                .Where(jobPost => jobPost.AutomaticallyRejected)
                .Where(jobPost => jobPost.Company != null && jobPost.Company != string.Empty)
                .GroupBy(jobPost => jobPost.Company)
                .Select(group => new DailyStatsViewModel
                {
                    Name = group.Key!,
                    Count = group.Count()
                })
                .Where(stat => stat.Count >= 2)
                .OrderByDescending(stat => stat.Count)
                .ThenBy(stat => stat.Name)
                .ToListAsync();

            _automaticallyDeclinedStats.Clear();
            _automaticallyDeclinedStats.AddRange(stats);

            this.AutomaticallyDeclinedHorizontalConfig.Data.Labels.Clear();
            this.AutomaticallyDeclinedHorizontalConfig.Data.Datasets.Clear();

            if (stats.Count == 0)
            {
                this.AutomaticallyDeclinedHorizontalConfig.Data.Labels.Add("No Data");
                this.AutomaticallyDeclinedHorizontalConfig.Data.Datasets.Add(
                    new BarDataset<int>(data: new[] { 0 }, horizontal: true)
                    {
                        Label = "No data",
                        BackgroundColor = ColorUtil.FromDrawingColor(Color.LightGray),
                        BorderColor = ColorUtil.FromDrawingColor(Color.DarkGray),
                        BorderWidth = 1
                    }
                );

                return;
            }

            foreach (var stat in stats)
            {
                this.AutomaticallyDeclinedHorizontalConfig.Data.Labels.Add(stat.Name);
            }

            this.AutomaticallyDeclinedHorizontalConfig.Data.Datasets.Add(
                new BarDataset<int>(
                    data: stats.Select(stat => stat.Count).ToList(),
                    horizontal: true)
                {
                    Label = "Applications",
                    BackgroundColor = "#36A2EB",
                    BorderColor = "#0e4163",
                    BorderWidth = 1
                }
            );
        }

        private async Task LoadBarChartData()
        {
            var labels = new List<string>();
            var values = new List<int>();
            List<JobPostDTO> jobPosts = await DateFilteredJobPosts();

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

            this.Config.Data.Labels.Clear();
            foreach (var l in labels)
            {
                this.Config.Data.Labels.Add(l);
            }

            this.Config.Data.Datasets.Clear();

            if (values.Count == 0)
            {
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
                this.Config.Data.Datasets.Add(
                    new BarDataset<int>(values)
                    {
                        Label = "Applications",
                        BackgroundColor = "#4BC0C0", // Unified palette color matching the donut chart
                        BorderColor = "#2482C3",
                        BorderWidth = 1
                    }
                );
            }
        }

        #region Private Data Access Methods
        private async Task<List<JobPostDTO>> DateFilteredJobPosts()
        {
            IQueryable<Models.JobPost> query = await UserJobApplicationsQuery();
            return await query
                .Where(jp => jp.ApplicationDate.HasValue && !jp.ApplicationDeclined && !jp.IsSelfWitdrawn)
                .OrderBy(jp => jp.ApplicationDate)
                .ProjectTo<JobPostDTO>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        private async Task<Dictionary<ApplicationStatus, int>> ApplicationStatusCounts()
        {
            var query = await UserJobApplicationsQuery();

            var stats = await query
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    AutomaticallyRejected = g.Count(jp => jp.AutomaticallyRejected),
                    Declined = g.Count(jp => jp.ApplicationDeclined && !jp.AutomaticallyRejected),
                    Withdrawn = g.Count(jp => jp.IsSelfWitdrawn),
                    Active = g.Count(jp => !jp.AutomaticallyRejected && !jp.ApplicationDeclined && !jp.IsSelfWitdrawn)
                })
                .FirstOrDefaultAsync();

            if (stats == null)
            {
                return new Dictionary<ApplicationStatus, int>();
            }

            return new Dictionary<ApplicationStatus, int>
            {
                [ApplicationStatus.AutomaticallyRejected] = stats.AutomaticallyRejected,
                [ApplicationStatus.Declined] = stats.Declined,
                [ApplicationStatus.Withdrawn] = stats.Withdrawn,
                [ApplicationStatus.Active] = stats.Active
            };
        }

        private enum ApplicationStatus
        {
            Active,
            Declined,
            Withdrawn,
            AutomaticallyRejected
        }

        private async Task<IQueryable<Models.JobPost>> UserJobApplicationsQuery(bool includeDateRange = true)
        {
            var userId = await _identityService.GetUserIdAsync();
            var query = Context.JobPost.AsNoTracking()
                               .Where(jp => jp.UserId == userId && jp.Description != null && jp.Description != "");
            if (includeDateRange)
            {
                if (FromDate.HasValue)
                    query = query.Where(jp => jp.ApplicationDate >= FromDate.Value);
                if (ToDate.HasValue)
                    query = query.Where(jp => jp.ApplicationDate <= ToDate.Value);
            }
            return query;
        }

        #endregion Private Data Access Methods
    }
}