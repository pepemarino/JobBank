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

            // Mapping for AgentAnalysisDTO - this is the DTO that the agent will use to perform its analysis.
            CreateMap<JobPost, AgentAnalysisDTO>()
                .ForMember(dest => dest.JobPostId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.AnalysisResult, opt => opt.Ignore()) // This will be set by the agent, so we ignore it in the mapping
                .ForMember(dest => dest.UserSkillSet, opt => opt.Ignore());  // This will be set by WORKS Commons, so we ignore it in the mapping
        }
    }
}
