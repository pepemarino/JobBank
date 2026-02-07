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

        public async Task<string> AnalyzeJobDescription(string jobDescription)
        {
            List<ChatMessage> messages = new()
            {
                new SystemChatMessage(@"You are a technical recruiter.
                    Analyze the provided Job Description.
                    1. Generate 5 behavioral and 5 technical interview questions.
                    2. List 5 key study subjects the candidate should master.

                    Return ONLY a valid JSON object with exactly two keys:
                    - InterviewQuestions (array of strings)
                    - StudySubjects (array of strings)

                    Return ONLY valid JSON with no markdown formatting or code blocks."),
                new UserChatMessage(jobDescription)
            };

            // Enforce JSON output for easy parsing
            ChatCompletionOptions options = new();

            ChatCompletion completion = await _client.CompleteChatAsync(messages, options);

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
    }
}
