using JobBank.ModelsDTO;

namespace JobBank.Services.Abstraction
{
    public interface IUserSettingService
    {
        Task<UserSettingsDTO?> GetUserSettingAsync(string userId);
        Task SaveUserSettingAsync(UserSettingsDTO setting);
    }
}
