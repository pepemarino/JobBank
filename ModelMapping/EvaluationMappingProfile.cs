using AutoMapper;
using JobBank.Management.Interview;
using JobBank.Models;
using JobBank.ModelsDTO;
using System.Linq;

namespace JobBank.ModelMapping
{
    public class EvaluationMappingProfile : Profile
    {
        public EvaluationMappingProfile()
        {
            CreateMap<Evaluation, EvaluationDTO>()
                .ReverseMap()
                .ForMember(dest => dest.Interview, opt => opt.Ignore()); // Ignore the Interview property to avoid circular reference

            CreateMap<EvaluationResult, Evaluation>()
                .ForMember(dest => dest.Interview, opt => opt.Ignore())
                .ForMember(dest => dest.Gaps, opt => opt.MapFrom(src => string.Join(",", src.Gaps)))
                .ForMember(dest => dest.Strengths, opt => opt.MapFrom(src => string.Join(",", src.Strengths)));
        }
    }
}
