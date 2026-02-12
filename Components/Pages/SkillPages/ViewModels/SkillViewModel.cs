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
            SaveCommand = new RelayCommand(ExecuteSave, CanExecute);
        }

        private void ExecuteSave()
        {
            try
            {
                var skillsDto = new UserSkillsDTO
                {
                    UserId = UserId,
                    RawSkills = RawSkills,
                    Version = Version
                };
                Task.Run(() => _skillsService.UpdateOrAddUserSkillsAsync(skillsDto));  // smell?
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

        public RelayCommand SaveCommand { get; }
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
                RawSkills = skillSet.RawSkills;
            }
            else
            {
                Version = 1;
            }
        }
    }
}
