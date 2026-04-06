using JobBank.ModelsDTO;

namespace JobBank.Services.Abstraction
{
    public interface IAnalysisCacheService : IAsyncDisposable
    {
        Task<JobAnalysisCacheDTO> GetJobAnalysisCacheAsync(string jobDescriptionHash);
        Task<int> AddJobAnalysisCacheAsync(JobAnalysisCacheDTO emp);
    }
}
