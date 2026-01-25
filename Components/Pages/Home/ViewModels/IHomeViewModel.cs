using ChartJs.Blazor;
using ChartJs.Blazor.BarChart;
using JobBank.Data;
using Microsoft.AspNetCore.Mvc;

namespace JobBank.Components.Pages.Home.ViewModels
{
    public interface IHomeViewModel
    {
        string Title { get; }
        string Description { get; }
        BarConfig Config { get; }

        Chart JobChart { get; set; } 
        IEnumerable<DailyStatsViewModel> DailyStatsViewModels { get; }


    }
}
