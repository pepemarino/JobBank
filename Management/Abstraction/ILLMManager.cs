namespace JobBank.Management.Abstraction
{
    public interface ILLMManager
    {
        Task<bool> IsAvailableAsync(string? userId = null);
        Task<string?> GetApiKeyAsync(string? userId = null);
    }
}
