using static JobBank.Components.Pages.Interviewer.ViewModels.IInterviewerViewModel;

namespace JobBank.Management.Interview
{
    public class InterviewState
    {
        public int JobPostId { get; set; }
        public string JobDescription { get; set; } = string.Empty;

        public List<ChatMessage> History { get; set; } = new();  // captures the full conversation history,
                                                                 // which can be used for context in future questions and
                                                                 // for evaluation at the end of the interview

        public List<string> CoveredTopics { get; set; } = new();  // prevents repetition
        public List<string> WeakAreas { get; set; } = new ();     // drives adaptive questioning

        public List<EvaluationResult> Evaluations { get; set; } = new();

        public int QuestionCount { get; set; }
        public bool IsFinished { get; set; }
    }
}
