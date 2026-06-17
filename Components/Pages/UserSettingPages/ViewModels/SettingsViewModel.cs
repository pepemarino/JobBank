using CommunityToolkit.Mvvm.Input;
using JobBank.ModelsDTO;
using JobBank.Services.Abstraction;
using JobBank.Util;
using System.ComponentModel;

namespace JobBank.Components.Pages.UserSettingPages.ViewModels
{
    public class SettingsViewModel : ISettingsViewModel
    {
        private readonly IUserSettingService _userSettingService;
        private readonly IIdentityService _identityService;
        private string? _errorMessage;
        private readonly ILogger<ISkillsService> _logger;

        public event Action? OnRequestUIUpdate;
        public event PropertyChangedEventHandler? PropertyChanged;
        public AsyncRelayCommand SaveCommand { get; }
        public string Title { get; set; } = "User Settings";

        public string? UserId { get; set; }
        public ChangeTracker<UserSettingsDTO> UserSettingsTracker { get; set; } = new(new UserSettingsDTO());

        public string? ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
                OnRequestUIUpdate?.Invoke();  // Trigger UI re-render
            }
        }

        public SettingsViewModel(
            IUserSettingService settingService, 
            IIdentityService identityService,
            ILogger<ISkillsService> logger )
        {
            _userSettingService = settingService;
            _identityService = identityService;
            SaveCommand = new AsyncRelayCommand(ExecuteSaveAsync, CanExecute);
            _logger = logger;
        }

        private async Task ExecuteSaveAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(UserId))
                {
                    ErrorMessage = "User ID is not available. Please log in again.";
                    return;
                }

                await _userSettingService.SaveUserSettingAsync(UserSettingsTracker.Current);

                // Reset change tracking after save
                UserSettingsTracker = new ChangeTracker<UserSettingsDTO>(UserSettingsTracker.Current);
                SaveCommand.NotifyCanExecuteChanged();
                ErrorMessage = null;
                OnRequestUIUpdate?.Invoke();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving user settings for user {UserId}", UserId);
                ErrorMessage = $"Error saving settings: {ex.Message}";
            }
        }

        public async Task InitializeAsync()
        {
            try
            {
                UserId = await _identityService.GetUserIdAsync();

                if (string.IsNullOrEmpty(UserId))
                {
                    ErrorMessage = "Unable to determine current user.";
                    return;
                }

                var settingsFromDb = await _userSettingService.GetUserSettingAsync(UserId);

                if (settingsFromDb != null)
                {
                    UserSettingsTracker = new ChangeTracker<UserSettingsDTO>(settingsFromDb);
                }
                else
                {
                    UserSettingsTracker = new ChangeTracker<UserSettingsDTO>(
                        new UserSettingsDTO { UserId = UserId, UseTutorMode = false });
                }

                SaveCommand.NotifyCanExecuteChanged();
                OnRequestUIUpdate?.Invoke();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user settings for user {UserId}", UserId);
                ErrorMessage = $"Error loading settings: {ex.Message}";
            }
        }

        public void ClearError() => ErrorMessage = null;

        private bool CanExecute() => UserSettingsTracker.HasChanged();

        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
