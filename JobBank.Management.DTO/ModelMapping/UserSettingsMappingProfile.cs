using AutoMapper;
using JobBank.Models.Identity;
using JobBank.ModelsDTO;

namespace JobBank.ModelMapping
{
    public class UserSettingsMappingProfile : Profile
    {
        public UserSettingsMappingProfile()
        {
            CreateMap<UserSettings, UserSettingsDTO>()
                .ReverseMap(); 
        }
    }
}
