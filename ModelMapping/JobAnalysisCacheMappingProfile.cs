using AutoMapper;
using JobBank.Models;
using JobBank.ModelsDTO;

namespace JobBank.ModelMapping
{
    public class JobAnalysisCacheMappingProfile : Profile
    {
        public JobAnalysisCacheMappingProfile() 
        {
            CreateMap<JobAnalysisCache, JobAnalysisCacheDTO>().ReverseMap();
        }
    }
}
