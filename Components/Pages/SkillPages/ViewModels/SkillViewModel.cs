using CommunityToolkit.Mvvm.Input;
using JobBank.ModelsDTO;
using JobBank.Services.Abstraction;
using System.ComponentModel;

namespace JobBank.Components.Pages.SkillPages.ViewModels
{
    public class SkillViewModel : ISkillViewModel
    {
        private readonly ISkillsService _skillsService;
        private readonly IIdentityService _identityService;

        public SkillViewModel(
            ISkillsService skillsService, IIdentityService identityService)
        {
            this._skillsService = skillsService;
            SaveCommand = new AsyncRelayCommand(ExecuteSaveAsync, CanExecute);
            _identityService = identityService;
        }

        private async Task ExecuteSaveAsync()
        {
            try
            {
                var skillsDto = new UserSkillsDTO
                {
                    UserId = UserId,
                    RawSkills = RawSkills,
                    Version = Version
                };
                await _skillsService.UpdateOrAddUserSkillsAsync(skillsDto);
                
                // After successful save, update the original value
                _originalRawSkills = RawSkills;
                SaveCommand.NotifyCanExecuteChanged();
                OnRequestUIUpdate?.Invoke();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private bool CanExecute() => !string.IsNullOrEmpty(RawSkills) && RawSkills != _originalRawSkills;

        public string Title { get; set; } = "Skills";

        private string _rawSkills = string.Empty;
        private string _originalRawSkills = string.Empty;  // Track original value from database

        public string RawSkills
        {
            get => _rawSkills;
            set
            {
                if (_rawSkills != value)
                {
                    _rawSkills = value;                    
                    SaveCommand.NotifyCanExecuteChanged();  // Refresh the Command state                   
                    OnRequestUIUpdate?.Invoke();            // Tell the Base Component to call StateHasChanged
                }
            }
        }

        public AsyncRelayCommand SaveCommand { get; }
        public int Version { get; set; }
        public string? UserId { get; set; }        

        public event Action? OnRequestUIUpdate;
        public event PropertyChangedEventHandler? PropertyChanged;

        public async Task InitializeAsync()
        {
            UserId = await _identityService.GetUserIdAsync();
            var skillSet = await this._skillsService.GetUserSkillsAsync(UserId);
            if (skillSet != null)
            {
                Version = skillSet.Version;                
                _rawSkills = skillSet.RawSkills;
                _originalRawSkills = skillSet.RawSkills;  // Store the original value
            }
            else
            {
                Version = 1;
                _originalRawSkills = string.Empty;
            }

            // force the command to re-evaluate after initialization
            SaveCommand.NotifyCanExecuteChanged();
            OnRequestUIUpdate?.Invoke();
        }
    }
}
