using AutoMapper;

using JobBank.Models;
using JobBank.ModelsDTO;

namespace JobBank.ModelMapping
{
    public class InterviewMappingProfile : Profile
    {
        public InterviewMappingProfile() 
        {
            CreateMap<Interview, InterviewDTO>()
                .ForMember(dest => dest.JobTitle, opt => opt.MapFrom(src => src.JobPost != null ? src.JobPost.Title : string.Empty))
                .ForMember(dest => dest.Company, opt => opt.MapFrom(src => src.JobPost != null ? src.JobPost.Company : string.Empty))
                .ForMember(dest => dest.TrainingId, opt => opt.MapFrom(src => src.Training != null ? src.Training.Id : 0));

            CreateMap<InterviewDTO, Interview>();
        }
    }
}
