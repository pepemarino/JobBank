using CommunityToolkit.Mvvm.Input;
using JobBank.ModelsDTO;
using JobBank.Services.Abstraction;
using System.ComponentModel;

namespace JobBank.Components.Pages.SkillPages.ViewModels
{
    public class SkillViewModel : ISkillViewModel
    {
        private readonly ISkillsService _skillsService;
        public SkillViewModel(ISkillsService skillsService)
        {
            this._skillsService = skillsService;
            SaveCommand = new AsyncRelayCommand(ExecuteSaveAsync, CanExecute);
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
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private bool CanExecute() => !string.IsNullOrEmpty(RawSkills);

        public string Title { get; set; } = "Skills";

        private string _rawSkills = string.Empty;
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
        public int? UserId { get; set; }        

        public event Action? OnRequestUIUpdate;
        public event PropertyChangedEventHandler? PropertyChanged;

        public async Task InitializeAsync()
        {
            var skillSet = await this._skillsService.GetUserSkillsWithLazyPropsAsync(1); // the user id is not used yet
            if (skillSet != null)
            {
                Version = skillSet.Version;
                UserId = skillSet.UserId;
                _rawSkills = skillSet.RawSkills;  // Double Triggering when setting the property.
                                                  // Set backing field directly to avoid double-triggering
            }
            else
            {
                Version = 1;
            }

            // Force the command to re-evaluate after initialization
            SaveCommand.NotifyCanExecuteChanged();
            OnRequestUIUpdate?.Invoke();

            // TODO: Still problems with the state of the save button after initialization.
            // It should be disabled if there are no changes, but it is enabled.
            // Need more coffe, donuts and debugging to figure this out.
        }
    }
}
