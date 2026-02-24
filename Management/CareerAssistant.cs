namespace JobBank.Management
{
    using JobBank.ModelsDTO;
    using JobBank.StartUpServices;
    using OpenAI.Chat;
    using System.Text.Json.Serialization;

    public record LLMAnalysisResult(        
        string Version,               // Required
        string Model,                 // Required
        string Analysis = "Analysis Error",
        int? JobApplicationId = null, 
        string? UserId = null,                    
        string? Prompt = null,
        string? ErrorMessage = null
    );

    public partial class CareerAssistant
    {
        private readonly ChatClient _client;

        private readonly string _version = "v1";
        private readonly string _llmModel;
        private readonly string _prompt;
        private readonly string _apiKey;
        private readonly long _timeout;

        public CareerAssistant(PrompService prompService)
        {
            _timeout = prompService.TimeoutSeconds;
            _llmModel = prompService.LLMModel;
            _prompt = prompService.InterviewQuestions;

            _apiKey = Environment.GetEnvironmentVariable(prompService.ApiKeyName);
            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                throw new InvalidOperationException($"API key not configured. Set environment variable '{prompService.ApiKeyName}'.");
            }

            _client = new ChatClient(_llmModel, _apiKey);
        }

        private ChatClient ChatClient  => _client;

        public async Task<LLMAnalysisResult> RunLLMAnalysis(string subjectDescription)
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(_timeout));

            try
            {
                // Pass the token directly into the async flow because the chat client
                // accepts a cancellation token!
                return await Analyze(subjectDescription, _prompt, cts.Token);
            }
            catch (OperationCanceledException)
            {
                return new (ErrorMessage: "Operation was cancelled by the system due to timeout.", Version: _version, Model: _llmModel);
            }
        }

        public async Task<AgentAnalysisDTO> RunLLMAnalysis(AgentAnalysisDTO analysisDTO, string prompt)
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(_timeout));
            try
            {
                return await Analyze(analysisDTO, prompt, cts.Token);
            }
            catch (OperationCanceledException ex)
            {
                throw;
            }
        }
    }

    // Data model for structured response
    public class AnalysisResult
    {
        [JsonPropertyName("InterviewQuestions")]
        public List<string> InterviewQuestions { get; set; } = new();

        [JsonPropertyName("StudySubjects")]
        public List<string> StudySubjects { get; set; } = new();

        //[JsonPropertyName("JobDescriptionSkills")]  // not ready
        //public List<string> JobDescriptionSkills { get; set; } = new();
    }
}
