using JobBank.ModelsDTO;

namespace JobBank.Services.Abstraction
{
    public interface ISkillsService : IAsyncDisposable
    {
        Task<UserSkillsDTO> GetUserSkillsWithLazyPropsAsync(int userId);
        Task UpdateOrAddUserSkillsAsync(UserSkillsDTO emp);
    }
}