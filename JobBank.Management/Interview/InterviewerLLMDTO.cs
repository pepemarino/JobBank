namespace JobBank.Management.Interview
{
    public class InterviewerLLMDTO
    {
        public string AgentQuestion { get; set; } = string.Empty;
        public string QuestionTopic { get; set; } = string.Empty;

        public EvaluationResult? Evaluation { get; set; }

        public List<string> CoveredTopics { get; set; } = new();
        public List<string> WeakAreas { get; set; } = new();
    }

    public class UserJobApplicantDTO
    {
        public string UserAnswer { get; set; } = string.Empty;
        public string QuestionTopic { get; set; } = string.Empty;
        public string JobDescription { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;

        public List<ChatMessage> History { get; set; } = new();
        public List<string> CoveredTopics { get; set; } = new();
        public List<string> WeakAreas { get; set; } = new();
        public List<EvaluationResult> Evaluations { get; set; } = new();
        public bool IsInterviewCompleted { get; set; }
    }
}
