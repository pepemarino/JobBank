using JobBank.Management.Abstraction;
using JobBank.Services.Abstraction;
using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace JobBank.Management.Plugins
{
    public class InterviewHistoryPlugin : IInterviewHistoryPlugin
    {
        private readonly IInterviewService _interviewService;

        public InterviewHistoryPlugin(IInterviewService interviewService)
        {
            _interviewService = interviewService;
        }

        [KernelFunction("GetPastFailures"), Description("Retrieves specific knowledge gaps and failed topics from the user's past interviews for this job role.")]
        public async Task<string> GetPastFailuresAsync([Description("The unique identifier of the job applicant.")] string userId)
        {
            var gaps = await _interviewService.GetGapsForApplicantAsync(userId)
                ?? Enumerable.Empty<string>();

            return string.Join(", ", gaps);
        }
    }
}
