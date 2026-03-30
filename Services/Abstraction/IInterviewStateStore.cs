using JobBank.Management.Interview;

namespace JobBank.Services.Abstraction
{
    public interface IInterviewStateStore
    {
        Task<InterviewState?> LoadAsync(int jobPostId);
        Task SaveAsync(int jobPostId, InterviewState state);
        Task ClearAsync(int jobPostId);
    }

}
