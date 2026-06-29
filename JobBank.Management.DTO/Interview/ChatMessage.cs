namespace JobBank.Management.Interview
{
    public class ChatMessage
    {
        public ChatMessage(string role, string content, DateTime timestamp) 
        {
            Role = role;
            Content = content;
            Timestamp = timestamp;
        }

        public string Role { get; set; } = string.Empty;  // "Interviewer" or "Candidate"
        public string Content { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}
