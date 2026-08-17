using AutoMapper;

namespace JobBank.ModelMapping
{
    public class TrainingMappingProfile : Profile
    {
        public TrainingMappingProfile()
        {
            CreateMap<Models.Training, ModelsDTO.TrainingDTO>().ReverseMap();
        }
    }
}
