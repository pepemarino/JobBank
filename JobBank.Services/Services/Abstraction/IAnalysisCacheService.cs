using JobBank.ModelsDTO;

namespace JobBank.Services.Abstraction
{
    public interface IAnalysisCacheService : IAsyncDisposable
    {
        Task<JobAnalysisCacheDTO> GetJobAnalysisCacheAsync(string jobDescriptionHash, string userId);
        Task<JobAnalysisCacheDTO?> GetAnalysisCache(Models.Identity.JobBankUser currentUser, string userId, string jobDescriptionHash);
        Task<JobAnalysisCacheDTO> GetPublicJobAnalysisCacheAsync(string jobDescriptionHash);
        Task<int> AddJobAnalysisCacheAsync(JobAnalysisCacheDTO emp);
    }
}
