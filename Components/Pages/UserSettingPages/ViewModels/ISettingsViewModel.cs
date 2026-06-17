using CommunityToolkit.Mvvm.Input;
using JobBank.Components.Pages.Init;
using JobBank.ModelsDTO;
using JobBank.Util;
using System.ComponentModel;

namespace JobBank.Components.Pages.UserSettingPages.ViewModels
{
    public interface ISettingsViewModel : IAsyncInitialization, INotifyPropertyChanged
    {
        string? UserId { get; set; }
        string Title { get; set; }

        string? ErrorMessage { get; set; }
        void ClearError();

        ChangeTracker<UserSettingsDTO> UserSettingsTracker { get; set; }

        AsyncRelayCommand SaveCommand { get; }
    }
}
