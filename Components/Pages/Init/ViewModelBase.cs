using Microsoft.AspNetCore.Components;

namespace JobBank.Components.Pages.Init
{
    public class ViewModelBase<T> : ComponentBase where T : IAsyncInitialization
    {
        [Inject] public T ViewModel { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            await ViewModel.InitializeAsync();
            StateHasChanged();
        }       
    }
}
