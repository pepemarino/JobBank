using JobBank.Management.Interview;

namespace JobBank.Management.Abstraction
{
    public interface IInterviewLLMService
    {
        public enum InterviewRole
        {
            Interviewer,
            User
        }

        Task<InterviewerLLMDTO> GetInterviewerAnalysisAsync(UserJobApplicantDTO userDTO, string prompt);
    }
}
