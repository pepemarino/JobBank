using Microsoft.AspNetCore.Components;

namespace JobBank.Components.Pages.Init
{
    public class ViewModelBase<T> : ComponentBase where T : IAsyncInitialization
    {
        [Inject] public T ViewModel { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            ViewModel.OnRequestUIUpdate += NotifyStateChanged;
            await ViewModel.InitializeAsync();
            StateHasChanged();
        }
        private void NotifyStateChanged() => InvokeAsync(StateHasChanged);

        public void Dispose()
        {
            // Clean up to prevent memory leaks
            ViewModel.OnRequestUIUpdate -= NotifyStateChanged;
        }
    }
}
