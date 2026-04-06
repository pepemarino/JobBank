using JobBank.ModelsDTO;

namespace JobBank.Services.Abstraction
{
    public interface IInterviewService : IAsyncDisposable
    {
        Task<InterviewDTO> GetInterviewByIdAsync(int id);
        Task<IEnumerable<InterviewDTO>> GetInterviewsByUserIdAsync(string userId);
        Task<IEnumerable<InterviewDTO>> GetInterviewsByJobPostIdAsync(int id);
        Task AddInterviewAsync(InterviewDTO emp);
    }
}
