using Microsoft.AspNetCore.Components;

namespace JobBank.Components.Pages.Init
{
    public class ViewModelBase<T> : ComponentBase, IAsyncDisposable where T : IAsyncInitialization
    {
        [Inject] public T ViewModel { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            ViewModel.OnRequestUIUpdate += NotifyStateChanged;
            await ViewModel.InitializeAsync();
            StateHasChanged();
        }
        private void NotifyStateChanged() => InvokeAsync(StateHasChanged);

        public async ValueTask DisposeAsync()
        {
            if (ViewModel != null)
            {
                ViewModel.OnRequestUIUpdate -= NotifyStateChanged;

                if (ViewModel is IAsyncDisposable asyncDisposable)
                    await asyncDisposable.DisposeAsync();
                else if (ViewModel is IDisposable disposable)
                    disposable.Dispose();
            }
        }
    }
}
