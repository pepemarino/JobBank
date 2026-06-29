using JobBank.Extensions;
using JobBank.Management.Abstraction;
using JobBank.ModelsDTO;
using JobBank.StartUpServices;
using OpenAI.Chat;
using System.Text.Json;

namespace JobBank.Management
{
    public partial class TrainerAssistant : Assistant, ITrainerAssistant
    {  
        private readonly long _timeout;             // ILLMManager nees to manage this

        public TrainerAssistant(PrompService prompService, ILLMManager llmManager)
            : base(llmManager)
        {
            _timeout = prompService.TimeoutSeconds;
        }

        public async Task<InterviewTrainingAnalysisResultDTO> RunLLMAnalysis(TrainerAnalysisMetadataDTO interviewMetadata, string? prompt = null, string? userId = null)
        {
            var targetModel = await GetTargetModelAsync(userId) ?? throw new InvalidOperationException("No suitable LLM model found for the user.");

            var canAnalyse = await _llmManager.IsAvailableAsync(userId);
            if (!canAnalyse)
                return new InterviewTrainingAnalysisResultDTO
                {
                    ErrorMessage = "LLM analysis is not enabled. API key is missing.", 
                    Version = targetModel.Version, 
                    Model = targetModel.LLModel,
                    Prompt = string.IsNullOrEmpty(prompt) ? TrainerPrompt : prompt
                };

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(_timeout));

            try
            {
                return await Analyze(
                    interviewMetadata, 
                    targetModel,
                    string.IsNullOrEmpty(prompt) ? TrainerPrompt : prompt, 
                    userId, cts.Token);
            }
            catch (OperationCanceledException)
            {
                return new InterviewTrainingAnalysisResultDTO
                {
                    ErrorMessage = "Operation was cancelled by the system due to timeout.",
                    Version = targetModel.Version,
                    Model = targetModel.LLModel,
                    Prompt = string.IsNullOrEmpty(prompt) ? TrainerPrompt : prompt
                };
            }
        }

        private async Task<InterviewTrainingAnalysisResultDTO> Analyze(TrainerAnalysisMetadataDTO interviewMetadata, TargetModelDTO targetModel, string prompt, string? userId, CancellationToken token)
        {            
            string jsonInput = JsonSerializer.Serialize(interviewMetadata);

            var options = new ChatCompletionOptions
            {
                ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat(), // Ensure the model is instructed to return a JSON object
                Temperature = DefaultTemperature
            };

            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(prompt),
                new UserChatMessage(jsonInput)
            };

            var chatClient = new ChatClient(apiKey: targetModel.ApiKey, model: targetModel.LLModel);
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

            result.Model = targetModel.LLModel;
            result.Version = targetModel.Version;
            result.Prompt = prompt;

            return result;
        }
    }
}
