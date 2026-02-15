using AutoMapper;
using JobBank.Models;
using JobBank.ModelsDTO;

namespace JobBank.ModelMapping
{
    public class UserSkillMatchReportMappingProfile : Profile
    {
        public UserSkillMatchReportMappingProfile()
        {
            // Maps from Entity to DTO and vice versa
            CreateMap<UserSkillMatchReport, UserSkillMatchReportDTO>().ReverseMap();
        }
    }
}
