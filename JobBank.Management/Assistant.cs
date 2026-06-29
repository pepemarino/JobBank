using JobBank.Management.Abstraction;
using JobBank.ModelsDTO;

namespace JobBank.Management
{
    public class Assistant
    {
        protected readonly ILLMManager _llmManager;
        protected const float DefaultTemperature = 0.2f;

        public Assistant(ILLMManager llmManager)
        {
            _llmManager = llmManager;
        }

        protected async Task<TargetModelDTO> GetTargetModelAsync(string? userId = null)
        {
            return await _llmManager.GetTargetModelAsync(userId);
        }
    }
}
