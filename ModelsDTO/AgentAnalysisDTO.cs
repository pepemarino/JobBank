namespace JobBank.ModelsDTO
{
    public class AgentAnalysisDTO
    {
        public int JobPostId { get; set; }
        public string AnalysisResult { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string UserSkillSet { get; set; } = string.Empty;
    }

    public class AnalysisOnlyResponse
    {
        public string AnalysisResult { get; set; } = string.Empty;
    }
}
