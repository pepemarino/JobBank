using AutoMapper;
using JobBank.Data;
using JobBank.ModelsDTO;
using JobBank.Services.Abstraction;
using Microsoft.EntityFrameworkCore;

namespace JobBank.Services
{
    public class UserSettingService : IUserSettingService
    {
        private readonly IDbContextFactory<EmploymentBankContext> _factory;
        private readonly ILogger<ISkillsService> _logger;
        private readonly IMapper _mapper;

        public UserSettingService(
            IDbContextFactory<EmploymentBankContext> factory, 
            ILogger<ISkillsService> logger, 
            IMapper mapper)
        {
            _factory = factory;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<UserSettingsDTO?> GetUserSettingAsync(string userId)
        {
            try
            {
                using var context = _factory.CreateDbContext();
                var settings = await context
                    .UserSettings
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.UserId == userId);
               
                if (settings == null)
                {
                    _logger.LogInformation("No user settings found for userId: {UserId}", userId);
                    return null;
                }
                
                return _mapper.Map<UserSettingsDTO>(settings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user settings for userId: {UserId}", userId);
                throw;
            }
        }

        public async Task SaveUserSettingAsync(UserSettingsDTO setting)
        {
            await using var context = await _factory.CreateDbContextAsync();
            var existingSetting = await context.UserSettings
                .Where(s => s.UserId == setting.UserId)
                .SingleOrDefaultAsync();

            if (existingSetting != null)
            {
                existingSetting.UseTutorMode = setting.UseTutorMode;
                existingSetting.ForceMyKey = setting.ForceMyKey;
                existingSetting.UpdatedDateTime = DateTime.UtcNow;
                context.UserSettings.Update(existingSetting);
            }
            else
            {
                var newSetting = new Models.Identity.UserSettings
                {
                    UserId = setting.UserId,
                    UseTutorMode = setting.UseTutorMode,
                    ForceMyKey = setting.ForceMyKey,
                    CreatedDateTime = DateTime.UtcNow
                };
                await context.UserSettings.AddAsync(newSetting);
            }
            await context.SaveChangesAsync();
            _logger.LogInformation("User settings saved for userId: {UserId}", setting.UserId);
        }
    }
}
