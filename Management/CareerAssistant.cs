namespace JobBank.Management
{
    using OpenAI.Chat;
    using System.Text.Json;

    public class CareerAssistant
    {
        private readonly ChatClient _client;

        public CareerAssistant(string apiKey)
        {
            // Use GPT-4o or GPT-4o-mini for best reasoning on job descriptions
            _client = new ChatClient("gpt-4o", apiKey);
        }

        public async Task<AnalysisResult> AnalyzeJobDescription(string jobDescription)
        {
            List<ChatMessage> messages = new()
        {
            new SystemChatMessage(@"You are a technical recruiter. 
                Analyze the provided Job Description. 
                1. Generate 5 behavioral and 5 technical interview questions.
                2. List 5 key study subjects the candidate should master.
                Return the response strictly in JSON format."),
            new UserChatMessage(jobDescription)
        };

            // Enforce JSON output for easy parsing
            ChatCompletionOptions options = new()
            {
                ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat(),
            };

            ChatCompletion completion = await _client.CompleteChatAsync(messages, options);

            string jsonResponse = completion.Content[0].Text;
            return JsonSerializer.Deserialize<AnalysisResult>(jsonResponse);
        }
    }

    // Data model for structured response
    public class AnalysisResult
    {
        public List<string> InterviewQuestions { get; set; }
        public List<string> StudySubjects { get; set; }
    }
}
