using JobBank.ModelsDTO;

namespace JobBank.Management.Abstraction
{
    public interface ICareerAssistant
    {
        Task<LLMAnalysisResult> RunLLMAnalysis(string subjectDescription, string prompt, string? userId = null);
        Task<JobApplicationAnalysisDTO> RunLLMAnalysis(JobApplicationAnalysisDTO analysisDTO, string prompt, string? userId = null);
    }
}
