using JobBank.ModelsDTO;

namespace JobBank.Services.Abstraction
{
    public interface IJobPostService
    {
        Task<JobPostDTO> GetJobPostByIdAsync(int jobPostId);
        Task UpdateOrAddJobPostAsync(JobPostDTO jobPostDto);
    }
}
