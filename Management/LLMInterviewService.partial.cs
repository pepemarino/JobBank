using JobBank.Management.Interview;
using OpenAI.Chat;
using System.Text.Json;

namespace JobBank.Management
{
    public partial class LLMInterviewService
    {
        private async Task<InterviewerLLMDTO> Analize(UserJobApplicantDTO userDTO, string prompt, CancellationToken token)
        {
            var jsonUserDTO = JsonSerializer.Serialize(userDTO);

            var options = new ChatCompletionOptions
            {
                ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat(),
                Temperature = 0.2f                
            };

            var messages = new List<OpenAI.Chat.ChatMessage>
            {
                new SystemChatMessage(prompt),
                new UserChatMessage(jsonUserDTO)
            };

            await EnsureAPIKeyLoadedAsync(userDTO.UserId);
            var chatClient = new ChatClient(apiKey: _apiKey, model: _llmModel);
            ChatCompletion completion = await chatClient.CompleteChatAsync(messages, options, token);

            string rawText = string.Concat(completion.Content.Select(c => c.Text));
            
            InterviewerLLMDTO? interviewerResponse;

            try
            {
                interviewerResponse = JsonSerializer.Deserialize<InterviewerLLMDTO>(rawText);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Invalid JSON response from LLM for user {UserId}", userDTO.UserId);
                throw new InvalidOperationException("LLM returned invalid JSON for InterviewerLLMDTO.", ex);
            }

            if (interviewerResponse == null)
            {
                _logger.LogError("Failed to deserialize LLM response for user {UserId}", userDTO.UserId);
                throw new InvalidOperationException("LLM returned null or empty response structure.");
            }

            _logger.LogInformation("Interview analysis completed for user {UserId}, Topic: {Topic}", 
                userDTO.UserId, interviewerResponse.QuestionTopic);

            return interviewerResponse;
        }

        /// <summary>
        /// #55 This needs to change because there is a lot of repetition between the services 
        /// CarrierAssistant and LLMInterviewService.  Note that ILLMManager is only taking care of the API Key.
        /// This could fail because the key is matched to the model and the user, and the model used now is a system model
        /// This is a bug. Architecture - Fix before trainer is implemented.  
        /// We need to move the API key management to a different service that can handle the model and user matching, 
        /// and then inject that service into both CareerAssistant and LLMInterviewService.
        /// <param name="userId"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        private async Task EnsureAPIKeyLoadedAsync(string? userId = null)
        {
            if (string.IsNullOrEmpty(_apiKey))
                _apiKey = await _llmManager.GetApiKeyAsync(userId);

            if (string.IsNullOrEmpty(_apiKey))
                throw new InvalidOperationException("API key is not available for LLM analysis.");
        }
    }
}
