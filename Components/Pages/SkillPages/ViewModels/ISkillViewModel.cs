using CommunityToolkit.Mvvm.Input;
using JobBank.Components.Pages.Init;
using JobBank.ModelsDTO;
using System.ComponentModel;

namespace JobBank.Components.Pages.SkillPages.ViewModels
{
    public interface ISkillViewModel : IAsyncInitialization, INotifyPropertyChanged
    {
        string Title { get; set; }
        string RawSkills { get; set; }
        int Version { get; set; }
        int? UserId { get; set; }
        AsyncRelayCommand SaveCommand { get; }
        event Action? OnRequestUIUpdate;
    }
}