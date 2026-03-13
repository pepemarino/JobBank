namespace JobBank.Management
{
    using JobBank.ModelsDTO;
    using Microsoft.CodeAnalysis;
    using OpenAI.Chat;
    using System.Text.Json;

    public partial class CareerAssistant
    {
        private async Task<LLMAnalysisResult> Analyze(string subjectDescription, string prompt, string? userId, CancellationToken token)
        {
            List<ChatMessage> messages = new()
            {
                new SystemChatMessage(prompt),
                new UserChatMessage(subjectDescription)
            };

            await EnsureAPIKeyLoadedAsync(userId);
            var chatClient = new ChatClient(apiKey: _apiKey, model: _llmModel);
            ChatCompletion completion = await chatClient.CompleteChatAsync(messages, new ChatCompletionOptions(), token);

            return new(Analysis: ExtractJson(completion.Content[0].Text), Version: _version, Model: _llmModel);
        }

        private async Task<AgentAnalysisDTO> Analyze(
            AgentAnalysisDTO analysisDTO,
            string prompt,
            string? userId,
            CancellationToken token)
        {
            var modelInput = new
            {
                analysisDTO.Description,
                analysisDTO.UserSkillSet
            };

            string jsonInput = JsonSerializer.Serialize(modelInput);

            var options = new ChatCompletionOptions
            {
                ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat(),
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

            string rawText = string.Concat(completion.Content.Select(c => c.Text));

            AnalysisOnlyResponse? result;

            try
            {
                result = JsonSerializer.Deserialize<AnalysisOnlyResponse>(rawText);
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException(
                    "Model returned invalid JSON for AnalysisResult.",
                    ex);
            }

            if (result == null || string.IsNullOrWhiteSpace(result.AnalysisResult))
            {
                throw new InvalidOperationException(
                    "Model returned empty AnalysisResult.");
            }

            analysisDTO.AnalysisResult = result.AnalysisResult;

            return analysisDTO;
        }

        private string ExtractJson(string rawResponse)
        {
            if (string.IsNullOrWhiteSpace(rawResponse))
                return rawResponse;

            // Remove leading ```json or ``` if present
            rawResponse = rawResponse.Trim();

            if (rawResponse.StartsWith("```"))
            {
                int firstNewLine = rawResponse.IndexOf('\n');
                int lastFence = rawResponse.LastIndexOf("```");

                if (firstNewLine >= 0 && lastFence > firstNewLine)
                {
                    rawResponse = rawResponse.Substring(firstNewLine + 1, lastFence - firstNewLine - 1);
                }
            }

            return rawResponse.Trim();
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
