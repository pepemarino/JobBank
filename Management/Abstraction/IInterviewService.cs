using JobBank.Management.Interview;

namespace JobBank.Management.Abstraction
{
    public interface IInterviewService
    {
        public enum InterviewRole
        {
            Interviewer,
            User
        }

        Task<InterviewerDTO> GetInterviewerAnalysisAsync(UserJobApplicantDTO userDTO, string prompt);
    }
}
