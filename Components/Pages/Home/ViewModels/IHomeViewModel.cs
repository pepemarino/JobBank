using ChartJs.Blazor;
using ChartJs.Blazor.BarChart;
using JobBank.Components.Pages.Init;
using JobBank.Data;
using Microsoft.AspNetCore.Mvc;

namespace JobBank.Components.Pages.Home.ViewModels
{
    public interface IHomeViewModel : IAsyncInitialization
    {
        string Title { get; set; }
        string Description { get; set; }
        BarConfig Config { get; set; }

        Chart JobChart { get; set; } 
        IEnumerable<DailyStatsViewModel> DailyStatsViewModels { get; }
    }
}
