using AutoMapper;
using AutoMapper.QueryableExtensions;
using JobBank.Data;
using JobBank.Exceptions;
using JobBank.Models;
using JobBank.ModelsDTO;
using JobBank.Services.Abstraction;
using Microsoft.EntityFrameworkCore;

namespace JobBank.Services
{
    public class JobPostService : IJobPostService
    {
        private readonly EmploymentBankContext _context;
        private readonly IMapper _mapper;

        public JobPostService(IDbContextFactory<EmploymentBankContext> dbFactory, IMapper mapper)
        {
            _mapper = mapper;
            _context = dbFactory.CreateDbContext();
        }

        public async Task<JobPostDTO> GetJobPostByIdAsync(int jobPostId)
        {
            try
            {
                // ProjectTo handles JOINs and minimizes SQL traffic automatically
                // Note:  No need for AsNoTRacking because the result is a dto :)
                return await _context.JobPost                    
                    .Where(jp => jp.Id == jobPostId)
                    .ProjectTo<JobPostDTO>(_mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync();
            }
            catch (InvalidOperationException ex)
            {
                throw new DataIntegrityException("A system error occurred. Please contact support.", ex);
            }
        }

        public async Task UpdateOrAddJobPostAsync(JobPostDTO jobPostDto)
        {
            var now = DateTime.UtcNow;

            if (jobPostDto.Id == 0)
            {
                var newJob = _mapper.Map<JobPost>(jobPostDto);
                newJob.Timestamp = now;

                if (newJob.UserSkillMatchReport != null)
                    newJob.UserSkillMatchReport.CreatedDate = now;

                _context.JobPost.Add(newJob);
            }
            else
            {
                // Load the existing entity WITH its nested report
                var existingJob = await _context.JobPost
                    .Include(j => j.UserSkillMatchReport)                   // include UserSkillMatchReport please
                    .FirstOrDefaultAsync(j => j.Id == jobPostDto.Id);

                if (existingJob == null)                                    // Oh, this is something strange. Should never happen
                    throw new DataIntegrityException("Job Application not found.");

                // Guard against removing the report becauase business logic forbids it
                // It can only be updated - there is also protection in the mapping
                if (jobPostDto.UserSkillMatchReport == null && existingJob.UserSkillMatchReport != null)
                    throw new InvalidOperationException("Attempting to remove existing User Skill Match Report.");

                // Maps DTO properties directly onto the existing TRACKED entity
                // UserSkillMatchReport is also mapped courtesy of AutoMapper
                _mapper.Map(jobPostDto, existingJob);

                existingJob.Timestamp = now;
                if (existingJob.UserSkillMatchReport != null)
                    existingJob.UserSkillMatchReport.Timestamp = now;
            }

            await _context.SaveChangesAsync();
        }

    }
}
