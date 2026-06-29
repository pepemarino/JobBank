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
                Temperature = DefaultTemperature
            };

            var messages = new List<OpenAI.Chat.ChatMessage>
            {
                new SystemChatMessage(prompt),
                new UserChatMessage(jsonUserDTO)
            };

            var targetModel = await GetTargetModelAsync(userDTO.UserId) ?? throw new InvalidOperationException("No suitable LLM model found for the user.");

            var chatClient = new ChatClient(apiKey: targetModel.ApiKey, model: targetModel.LLModel);
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
    }
}
