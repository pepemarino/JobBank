using JobBank.ModelsDTO;

namespace JobBank.Services.Abstraction
{
    public interface ISkillsService
    {
        Task<UserSkillsDTO> GetUserSkillsAsync(string userId);
        Task UpdateOrAddUserSkillsAsync(UserSkillsDTO emp);
    }
}