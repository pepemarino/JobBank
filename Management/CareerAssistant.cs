namespace JobBank.Management
{
    using OpenAI.Chat;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    public class CareerAssistant
    {
        private readonly ChatClient _client;

        public CareerAssistant(string apiKey, string llmModel)
        {
            _client = new ChatClient(llmModel, apiKey);
        }

        public async Task<string> RunLLMAnalysis(string subjectDescription, string prompt, long timeoutSeconds)
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));

            try
            {
                // Pass the token directly into the async flow because the chat client
                // accepts a cancellation token!
                return await Analyze(subjectDescription, prompt, cts.Token);
            }
            catch (OperationCanceledException)
            {
                return "Operation was cancelled by the system due to timeout.";
            }
        }

        private async Task<string> Analyze(string subjectDescription, string prompt, CancellationToken token)
        {
            List<ChatMessage> messages = new()
            {
                new SystemChatMessage(prompt),
                new UserChatMessage(subjectDescription)
            };

            ChatCompletion completion = await _client.CompleteChatAsync(messages, new ChatCompletionOptions(), token);

            return ExtractJson(completion.Content[0].Text);

            string ExtractJson(string rawResponse)
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
