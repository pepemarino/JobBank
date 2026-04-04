namespace JobBank.Management.Interview
{
    public class InterviewMetadata
    {
        public List<string> CoveredTopics { get; set; } = new();  
        public List<string> WeakAreas { get; set; } = new();     
        public List<EvaluationResult> Evaluations { get; set; } = new();
    }
}
