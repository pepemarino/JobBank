using JobBank.ModelsDTO;

namespace JobBank.Services.Abstraction
{
    public interface ITrainingService : IAsyncDisposable
    {
        Task<TrainingDTO> GetTrainingByIdAsync(int id);
        Task<IEnumerable<TrainingDTO>> GetTrainingsByUserIdAsync(string userId);
        Task<TrainingDTO> GetTrainingsByInterviewIdAsync(int interviewId);
        Task AddTrainingAsync(TrainingDTO emp);
    }
}
