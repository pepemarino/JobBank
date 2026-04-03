using AutoMapper;

namespace JobBank.ModelMapping
{
    public class InterviewMappingProfile : Profile
    {
        public InterviewMappingProfile() 
        {
            CreateMap<Models.Interview, ModelsDTO.InterviewDTO>().ReverseMap();
        }
    }
}
