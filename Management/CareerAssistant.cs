namespace JobBank.Management
{
    using JobBank.Data;
    using Microsoft.EntityFrameworkCore;
    using OpenAI.Chat;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using System.Text.RegularExpressions;

    public class CareerAssistant
    {
        private readonly ChatClient _client;
        private readonly EmploymentBankContext Context;

        public CareerAssistant(string apiKey, IDbContextFactory<EmploymentBankContext> DbFactory)
        {
            Context = DbFactory.CreateDbContext();

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
                Return a JSON object with exactly two keys: 'InterviewQuestions' and 'StudySubjects', where both are arrays of strings."),
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

        /// <summary>
        /// This is the hash for the canonical form of the job description,
        /// used i the JobAnalysis table to avoid duplicate analyses.
        /// </summary>
        /// <param name="jodDescription"></param>
        /// <returns></returns>
        public string GetCanonicalHash(string jobDescription)
        {
            if (string.IsNullOrWhiteSpace(jobDescription)) return string.Empty;

            // Strip HTML
            string clean = Regex.Replace(jobDescription, "<.*?>", string.Empty);

            // Remove non-alphanumeric (Fixed the 0-9 typo)
            clean = Regex.Replace(clean, @"[^a-zA-Z0-9\s]", "");

            // Lowercase and split into words
            var words = clean.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);

            // Optional: Filter out common "noise" words to focus on keywords
            //           This list can be expanded as needed
            string[] stopWords = { "the", "and", "a", "of", "to", "in", "is" };
            words = words.Where(w => !stopWords.Contains(w)).ToArray();

            // Rejoin and Hash
            string essence = string.Join(" ", words);

            using var sha256 = System.Security.Cryptography.SHA256.Create();
            byte[] bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(essence));
            return Convert.ToHexString(bytes).ToLower();
        }
    }

    // Data model for structured response
    public class AnalysisResult
    {
        [JsonPropertyName("interviewQuestions")] 
        public List<string> InterviewQuestions { get; set; } = new();

        [JsonPropertyName("studySubjects")]
        public List<string> StudySubjects { get; set; } = new();
    }
}
