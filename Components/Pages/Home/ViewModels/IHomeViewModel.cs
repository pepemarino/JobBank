using ChartJs.Blazor;
using ChartJs.Blazor.BarChart;

using ChartJs.Blazor.PieChart;
using JobBank.Components.Pages.Init;
using JobBank.Data;
using Microsoft.AspNetCore.Mvc;

namespace JobBank.Components.Pages.Home.ViewModels
{
    public interface IHomeViewModel : IAsyncInitialization, IAsyncDisposable
    {
        string Title { get; set; }
        string Description { get; set; }
        BarConfig Config { get; set; }
        PieConfig PieConfig { get; set; }

        Chart JobChart { get; set; } 
        Chart DonutCart { get; set; }
        IEnumerable<DailyStatsViewModel> DailyStatsViewModels { get; }
    }
}
