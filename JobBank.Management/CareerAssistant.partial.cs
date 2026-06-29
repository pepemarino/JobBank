namespace JobBank.Management
{
    using JobBank.Management.Abstraction;
    using JobBank.ModelsDTO;
    using Microsoft.CodeAnalysis;
    using OpenAI.Chat;
    using System.Text.Json;
    using System.Text.RegularExpressions;

    public partial class CareerAssistant
    {
        private async Task<LLMAnalysisResult> Analyze(string subjectDescription, TargetModelDTO targetModel, string prompt, CancellationToken token)
        {
            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(prompt),
                new UserChatMessage(subjectDescription)
            };

            var responseText = await ExecuteChatCompletionAsync(targetModel, messages, null, token);
            return new(Analysis: ExtractJson(responseText), Version: targetModel.Version, Model: targetModel.LLModel);
        }

        private async Task<JobApplicationAnalysisDTO> Analyze(
            JobApplicationAnalysisDTO analysisDTO,
            TargetModelDTO targetModel,
            string prompt,
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
                Temperature = DefaultTemperature
            };

            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(prompt),
                new UserChatMessage(jsonInput)
            };

            var rawText = await ExecuteChatCompletionAsync(targetModel, messages, options, token);

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

        private async Task<string> ExecuteChatCompletionAsync(
            TargetModelDTO targetModel,
            List<ChatMessage> messages,
            ChatCompletionOptions? options,
            CancellationToken token)
        {
            var chatClient = new ChatClient(apiKey: targetModel.ApiKey, model: targetModel.LLModel);
            ChatCompletion completion = await chatClient.CompleteChatAsync(messages, options ?? new ChatCompletionOptions(), token);

            if (completion.Content == null || completion.Content.Count == 0)
            {
                throw new InvalidOperationException("Chat completion returned no content.");
            }

            return string.Concat(completion.Content.Select(c => c.Text));
        }

        private string ExtractJson(string rawResponse)
        {
            if (string.IsNullOrWhiteSpace(rawResponse))
                return rawResponse;

            rawResponse = rawResponse.Trim();

            // Match markdown code fences with optional language identifier
            var match = Regex.Match(
                rawResponse,
                @"```(?:json)?\s*\n([\s\S]*?)\n```",
                RegexOptions.Multiline);

            if (match.Success)
            {
                return match.Groups[1].Value.Trim();
            }

            return rawResponse;
        }
    }
}
