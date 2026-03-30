using JobBank.Management.Abstraction;
using JobBank.Management.Interview;

namespace JobBank.Management
{
    public class LLMInterviewService : IInterviewService
    {
        public Task<InterviewerDTO> GetInterviewerAnalysisAsync(UserJobApplicantDTO userDTO, string prompt)
        {
            throw new NotImplementedException();
        }
    }
}
