using Azure.Core;
using JobBank.ModelsDTO;

namespace JobBank.Services.Abstraction
{
    public interface IInterviewService : IAsyncDisposable
    {
        Task<InterviewDTO> GetInterviewByIdAsync(int id);
        Task<IEnumerable<InterviewDTO>> GetInterviewsByUserIdAsync(string userId, bool isDeleted = false);            

        Task<PaginationResult<InterviewDTO>> GetInterviewsByUserIdWithPaginationAsync(string userId, string companyName, int startIndex, int take, bool isDeleted = false);

        Task<IEnumerable<InterviewDTO>> GetInterviewsByJobPostIdAsync(int id);
        Task AddInterviewAsync(InterviewDTO emp);
    }

    public class PaginationResult<T> where T : class
    {
        public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
        public int TotalCount { get; set; }
    }
}
