using System.ComponentModel;

namespace JobBank.Management.Abstraction
{
    public interface IInterviewHistoryPlugin
    {
        Task<string> GetPastFailuresAsync([Description("The unique identifier of the job applicant.")] string userId);
    }
}
