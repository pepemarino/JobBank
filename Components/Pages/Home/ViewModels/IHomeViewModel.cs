using ChartJs.Blazor;
using ChartJs.Blazor.BarChart;
using ChartJs.Blazor.PieChart;
using JobBank.Components.Pages.Init;

namespace JobBank.Components.Pages.Home.ViewModels
{
    public interface IHomeViewModel : IAsyncInitialization, IAsyncDisposable
    {
        string Title { get; set; }
        string Description { get; set; }
        BarConfig Config { get; set; }
        BarConfig AutomaticallyDeclinedHorizontalConfig { get; set; }
        BarConfig DeclinedHorizontalConfig { get; set; }
        BarConfig AbortedHorizontalConfig { get; set; }
        PieConfig PieConfig { get; set; }

        Chart JobChart { get; set; }
        Chart DonutCart { get; set; }
        Chart AutomaticallyDeclinedHorizontalChart { get; set; }
        Chart DeclinedHorizontalChart { get; set; }
        Chart AbortedHorizontalChart { get; set; }
        IEnumerable<DailyStatsViewModel> DailyStatsViewModels { get; }
    }
}