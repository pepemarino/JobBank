namespace JobBank.Services.Abstraction
{
    public interface ILLMProvider
    {
        bool IsAvailable { get; }
        string? GetApiKey();
    }
}
