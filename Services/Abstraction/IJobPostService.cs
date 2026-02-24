using JobBank.Models;
using JobBank.ModelsDTO;
using System.Linq.Expressions;

namespace JobBank.Services.Abstraction
{
    public interface IJobPostService
    {
        /// <summary>
        /// This is leaky but flexibe and I do not drink and drive anymore, so let me at least be dangerous here!
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="predicate"></param>
        /// <returns></returns>
        Task<IEnumerable<T>> GetJobPostsByQueryAsync<T>(Expression<Func<JobPost, bool>>? predicate)  where T : class;
        Task<JobPostDTO> GetJobPostByIdAsync(int jobPostId);
        Task UpdateOrAddJobPostAsync(JobPostDTO jobPostDto);
    }
}
