using JobBank.ModelsDTO;

namespace JobBank.Management.Abstraction
{
    public interface ITrainerAssistant
    {
        Task<InterviewTrainingAnalysisResultDTO> RunLLMAnalysis(TrainerAnalysisMetadataDTO interviewMetadata, string? prompt = null, string? userId = null);
    }
}