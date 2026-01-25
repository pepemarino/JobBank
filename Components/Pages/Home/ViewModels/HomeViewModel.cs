using ChartJs.Blazor;
using ChartJs.Blazor.BarChart;
using ChartJs.Blazor.Common;
using ChartJs.Blazor.Common.Enums;
using ChartJs.Blazor.Util;
using JobBank.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mono.TextTemplating;
using System.Drawing;

namespace JobBank.Components.Pages.Home.ViewModels
{
    public class HomeViewModel : IHomeViewModel, IAsyncDisposable
    {
        private Chart _chart;

        public HomeViewModel(IDbContextFactory<EmploymentBankContext> DbFactory)
        {
            Context = DbFactory.CreateDbContext();
            this.Title = "Job Bank Applications";
            this.Config = new BarConfig
            {
                Options = new BarOptions
                {
                    Responsive = true,
                    Legend = new Legend
                    {
                        Position = Position.Top
                    },
                    Title = new OptionsTitle
                    {
                        Display = true,
                        Text = "ChartJs.Blazor Bar Chart"
                    },
                    MaintainAspectRatio = true,
                }
            };

            foreach (var stat in Context.JobPost
                .OrderBy(jp => jp.ApplicationDate)
                .GroupBy(pg => pg.ApplicationDate)
                .Select(g => new DailyStatsViewModel
                {
                    Count = g.Count(),
                    Name = g.Key.Value.ToString("yyyy-MM-dd")
                }))
            {
                this.Config.Data.Datasets.Add
                (
                   new BarDataset<int>([stat.Count], false)
                       {
                           Label = stat.Name,
                           BackgroundColor = ColorUtil.FromDrawingColor(Color.FromArgb(128)),
                           BorderColor = ColorUtil.FromDrawingColor(Color.Red),
                           BorderWidth = 1
                       }
                   );
                }
            JobChart = new Chart { Config = this.Config };
        }

        public Chart JobChart { get; set; }
        public string Title { get; }
        public string Description { get; }

        public EmploymentBankContext Context { get; }

        public IEnumerable<DailyStatsViewModel> DailyStatsViewModels { get; }

        private BarConfig Config { get; }

        BarConfig IHomeViewModel.Config => Config;

        public async ValueTask DisposeAsync() => await Context.DisposeAsync();
    }
}
