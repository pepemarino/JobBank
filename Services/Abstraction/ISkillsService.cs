using JobBank.ModelsDTO;

namespace JobBank.Services.Abstraction
{
    public interface ISkillsService : IAsyncDisposable
    {
        Task<UserSkillsDTO> GetUserSkillsAsync(string userId);
        Task UpdateOrAddUserSkillsAsync(UserSkillsDTO emp);
    }
}