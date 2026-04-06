using JobBank.Management.Abstraction;
using JobBank.Management.Interview;
using JobBank.StartUpServices;

namespace JobBank.Management
{
    public partial class LLMInterviewService : IInterviewLLMService
    {
        private readonly string _version = "v1";
        private readonly string _llmModel;
        private readonly long _timeout;
        private string _apiKey;
        private readonly ILogger<LLMInterviewService> _logger;

        private readonly ILLMManager _llmManager;

        public LLMInterviewService(
            PrompService prompService, 
            ILLMManager llmManager,
            ILogger<LLMInterviewService> logger)
        {
            _timeout = prompService.TimeoutSeconds;
            _llmModel = prompService.LLMModel;
            _llmManager = llmManager;
            _logger = logger;
        }

        public async Task<InterviewerLLMDTO> GetInterviewerAnalysisAsync(UserJobApplicantDTO userDTO, string prompt)
        {
            var canAnalyse = await _llmManager.IsAvailableAsync(userDTO.UserId);
            if (!canAnalyse)
            {
                var message = $"LLM analysis is not available for user {userDTO.UserId}.";
                _logger.LogError("{message}: Model {_llmModel}, Version {_version}", message, _llmModel, _version);
                throw new InvalidOperationException(message);
            }

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(_timeout));

            try
            {
                return await Analize(userDTO, prompt, cts.Token);
            }
            catch (OperationCanceledException)
            {
                var message = $"Interview Agent Error for {userDTO.UserId}.";
                _logger.LogError("{message}: Model {_llmModel}, Version {_version}", message, _llmModel, _version);
                throw new OperationCanceledException(message);
            }
        }     
    }
}
