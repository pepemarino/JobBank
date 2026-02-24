namespace JobBank.ModelsDTO
{
    public class AgentAnalysisResult
    {
        public bool IsError { get; set; } = false;
        public string ErrorMessage { get; set; } = string.Empty;
        public IEnumerable<AgentAnalysisDTO> Analysis { get; set; } = new List<AgentAnalysisDTO>();
    }
}
