using AutoMapper;
using JobBank.Models;
using JobBank.ModelsDTO;

namespace JobBank.ModelMapping
{
    public class JobPostMappingProfile : Profile
    {
        public JobPostMappingProfile()
        {
            CreateMap<JobPost, JobPostDTO>()
                .ReverseMap() // Creates the JobPostDTO -> JobPost direction
                .ForMember(dest => dest.UserSkillMatchReport, opt => {                    
                    opt.Condition(src => src.UserSkillMatchReport != null);  // RULE: If the DTO report is null, don't touch the Entity's report
                                                                             // This is a violation and when detected throw exception.
                                                                             // this is extra protection
                });
        }
    }
}
