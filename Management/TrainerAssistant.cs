using JobBank.Extensions;
using JobBank.Management.Abstraction;
using JobBank.ModelsDTO;
using JobBank.StartUpServices;
using OpenAI.Chat;
using System.Text.Json;

namespace JobBank.Management
{
    public partial class TrainerAssistant
    {
        private readonly string _version = "v1";
        private readonly string _llmModel;          // ILLMManager nees to manage this
        private readonly long _timeout;             // ILLMManager nees to manage this
        private string _apiKey; // Store

        private readonly ILLMManager _llmManager;

        public TrainerAssistant(PrompService prompService, ILLMManager llmManager)
        {
            _timeout = prompService.TimeoutSeconds;
            _llmModel = prompService.LLMModel;
            _llmManager = llmManager;
        }

        public async Task<InterviewTrainingAnalysisResultDTO> RunLLMAnalysis(TrainerAnalysisMetadataDTO interviewMetadata, string? prompt = null, string? userId = null)
        {
            var canAnalyse = await _llmManager.IsAvailableAsync(userId);
            if (!canAnalyse)
                return new InterviewTrainingAnalysisResultDTO
                {
                    ErrorMessage = "LLM analysis is not enabled. API key is missing.", 
                    Version = _version, 
                    Model = _llmModel,
                    Prompt = string.IsNullOrEmpty(prompt) ? TrainerPrompt : prompt
                };

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(_timeout));

            try
            {
                return await Analyze(
                    interviewMetadata, 
                    string.IsNullOrEmpty(prompt) ? TrainerPrompt : prompt, 
                    userId, cts.Token);
            }
            catch (OperationCanceledException)
            {
                return new InterviewTrainingAnalysisResultDTO
                {
                    ErrorMessage = "Operation was cancelled by the system due to timeout.", 
                    Version = _version, 
                    Model = _llmModel,
                    Prompt = string.IsNullOrEmpty(prompt) ? TrainerPrompt : prompt
                };
            }
        }

        private async Task<InterviewTrainingAnalysisResultDTO> Analyze(TrainerAnalysisMetadataDTO interviewMetadata, string prompt, string? userId, CancellationToken token)
        {            
            string jsonInput = JsonSerializer.Serialize(interviewMetadata);

            var options = new ChatCompletionOptions
            {
                ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat(), // Ensure the model is instructed to return a JSON object
                Temperature = 0.2f
            };

            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(prompt),
                new UserChatMessage(jsonInput)
            };

            await EnsureAPIKeyLoadedAsync(userId);

            var chatClient = new ChatClient(apiKey: _apiKey, model: _llmModel);
            ChatCompletion completion = await chatClient.CompleteChatAsync(messages, options, token);

            string rawText = string.Concat(completion.Content.Select(c => c.Text))
                .ExtractJson()                                            // this is defencesive becaus the option explicitly ask for json format. The prompt also asks for json
                .Trim();

            if (string.IsNullOrWhiteSpace(rawText))
            {
                throw new InvalidOperationException(
                    "Model returned empty response content.");
            }

            InterviewTrainingAnalysisResultDTO? result;

            try
            {
                result = JsonSerializer.Deserialize<InterviewTrainingAnalysisResultDTO>(rawText);
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException(
                    $"Model returned invalid JSON for AnalysisResult. Response: {rawText}",
                    ex);
            }

            if (result == null)
            {
                throw new InvalidOperationException(
                    "Model returned null after deserialization.");
            }

            result.Model = _llmModel;
            result.Version = _version;
            result.Prompt = prompt;

            return result;
        }

        private async Task EnsureAPIKeyLoadedAsync(string? userId = null)
        {
            if (string.IsNullOrEmpty(_apiKey))
                _apiKey = await _llmManager.GetApiKeyAsync(userId);

            if (string.IsNullOrEmpty(_apiKey))
                throw new InvalidOperationException("API key is not available for LLM analysis.");
        }
    }
}
