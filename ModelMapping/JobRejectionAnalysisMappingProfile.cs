using AutoMapper;
using JobBank.Models;
using JobBank.ModelsDTO;

namespace JobBank.ModelMapping
{
    public class JobRejectionAnalysisMappingProfile : Profile
    {
        public JobRejectionAnalysisMappingProfile() 
        {
            CreateMap<JobRejectionAnalysis, JobRejectionAnalysisDTO>().ReverseMap();
        }
    }
}
