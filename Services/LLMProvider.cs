using JobBank.Services.Abstraction;
using JobBank.StartUpServices;

namespace JobBank.Services
{
    public class LLMProvider : ILLMProvider
    {
        private readonly PrompService _prompService;

        public LLMProvider(PrompService prompService)
        {
            _prompService = prompService;
        }

        // Central check for availability
        public bool IsAvailable =>
            _prompService.LLMEnabled &&
            !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(_prompService.ApiKeyName));

        public string? GetApiKey()
        {
            if (!_prompService.LLMEnabled) return null;

            return Environment.GetEnvironmentVariable(_prompService.ApiKeyName);
        }
    }
}
