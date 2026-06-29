namespace JobBank.Management.Interview
{
    public class InterviewMetadata
    {
        public required List<string> CoveredTopics { get; set; } = new();  
        public required List<string> WeakAreas { get; set; } = new();     
        public required List<EvaluationResult> Evaluations { get; set; } = new();
    }
}
