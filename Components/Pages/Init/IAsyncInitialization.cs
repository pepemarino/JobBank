namespace JobBank.Components.Pages.Init
{
    public interface IAsyncInitialization
    {
        Task InitializeAsync();
        event Action? OnRequestUIUpdate;
    }
}

