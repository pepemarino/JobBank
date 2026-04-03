using JobBank.Management.Abstraction;
using JobBank.Management.Interview;

namespace JobBank.Management
{
    public class LLMInterviewService : IInterviewLLMService
    {
        public Task<InterviewerLLMDTO> GetInterviewerAnalysisAsync(UserJobApplicantDTO userDTO, string prompt)
        {
            throw new NotImplementedException();
        }
    }
}
