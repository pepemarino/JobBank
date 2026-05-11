using JobBank.Management.Interview;

namespace JobBank.Management.Abstraction
{
    public partial interface IInterviewLLMService
    {
        Task<InterviewerLLMDTO> GetInterviewerAnalysisAsync(UserJobApplicantDTO userDTO, string prompt);
    }
}