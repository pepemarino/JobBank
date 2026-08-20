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
using JobBank.ModelsDTO;
using JobBank.Services;
using JobBank.Services.Abstraction;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Drawing;

namespace JobBank.Components.Pages.Home.ViewModels
{
    public class HomeViewModel : IHomeViewModel, IAsyncDisposable
    {
        private readonly IIdentityService _identityService;
        private readonly IMapper _mapper;

        public FilteredStateService StateService { get; private set; }

        private readonly EmploymentBankContext Context;
        private readonly List<DailyStatsViewModel> _dailyStats = new();

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
            this.Title = "WORKS Commons Active Applications";

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

        public async ValueTask DisposeAsync()
        {
            await Context.DisposeAsync();
            StateService.OnChange -= HandleStateChange;
        }

        public async Task InitializeAsync()
        {
            SetBarChart();
            SetDonutChart();
            await LoadBarChartData();
            await LoadDonutChartData();
        }

        private async void HandleStateChange()
        {
            FromDate = StateService.FromDate;
            ToDate = StateService.ToDate;

            // Instead of calling .Clear() instantly here, let the load methods update the 
            // arrays and call .Update() on the JS wrapper instance. This prevents rendering flashes.

            await LoadBarChartData();
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
                BackgroundColor = new[] { "#36A2EB", "#FF6384", "#FFCE56" },
                BorderColor = new[] { "#FFFFFF", "#FFFFFF", "#FFFFFF" }, // White borders separate arcs elegantly
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
                        BackgroundColor = "#36A2EB", // Unified palette color matching the donut chart
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
                .Where(jp => jp.ApplicationDate.HasValue && !jp.ApplicationDeclined)
                .OrderBy(jp => jp.ApplicationDate)
                .ProjectTo<JobPostDTO>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        private async Task<Dictionary<ApplicationStatus, int>> ApplicationStatusCounts()
        {
            IQueryable<Models.JobPost> query = await UserJobApplicationsQuery();
            var counts = await query
                .GroupBy(jp => new { jp.AutomaticallyRejected, jp.ApplicationDeclined })
                .Select(g => new
                {
                    Status = g.Key.AutomaticallyRejected ? ApplicationStatus.AutomaticallyRejected
                            : g.Key.ApplicationDeclined ? ApplicationStatus.Declined
                            : ApplicationStatus.Active,
                    Count = g.Count()
                })
                .ToListAsync();
            return counts.ToDictionary(c => c.Status, c => c.Count);
        }

        private enum ApplicationStatus
        {
            Active,
            Declined,
            AutomaticallyRejected
        }

        private async Task<IQueryable<Models.JobPost>> UserJobApplicationsQuery()
        {
            var userId = await _identityService.GetUserIdAsync();
            var query = Context.JobPost.AsNoTracking()
                               .Where(jp => jp.UserId == userId && jp.Description != null && jp.Description != "");
            if (FromDate.HasValue)
                query = query.Where(jp => jp.ApplicationDate >= FromDate.Value);
            if (ToDate.HasValue)
                query = query.Where(jp => jp.ApplicationDate <= ToDate.Value);
            return query;
        }

        #endregion Private Data Access Methods
    }
}