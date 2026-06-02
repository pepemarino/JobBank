using AutoMapper;
using AutoMapper.QueryableExtensions;
using JobBank.Data;
using JobBank.Exceptions;
using JobBank.Models;
using JobBank.ModelsDTO;
using JobBank.Services.Abstraction;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace JobBank.Services
{
    public class JobPostService : IJobPostService
    {
        private readonly IDbContextFactory<EmploymentBankContext> _factory;
        private readonly IMapper _mapper;
        private readonly ILogger<IJobPostService> _logger;

        public JobPostService(IDbContextFactory<EmploymentBankContext> dbFactory, IMapper mapper, ILogger<IJobPostService> logger)
        {
            _mapper = mapper;
            _factory = dbFactory;
            _logger = logger;
        }

        public async Task<JobPostDTO> GetJobPostByIdAsync(int jobPostId)
        {
            try
            {
                await using var context = await _factory.CreateDbContextAsync();
                // ProjectTo handles JOINs and minimizes SQL traffic automatically
                // Note:  No need for AsNoTRacking because the result is a dto :)
                return await context.JobPost
                    .Where(jp => jp.Id == jobPostId)
                    .ProjectTo<JobPostDTO>(_mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Error retrieving JobPost with ID {JobPostId}", jobPostId);
                throw new DataIntegrityException("A system error occurred. Please contact support.", ex);
            }
        }

        public async Task<IEnumerable<T>> GetJobPostsByQueryAsync<T>(Expression<Func<JobPost, bool>>? predicate)
            where T : class
        {
            try
            {
                await using var context = await _factory.CreateDbContextAsync();
                var query = context.JobPost.AsQueryable();

                if (predicate != null)
                {
                    query = query.Where(predicate);
                }

                // Project directly to the requested type T and execute the query
                return await query
                    .ProjectTo<T>(_mapper.ConfigurationProvider)
                    .ToListAsync();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Error retrieving JobPosts with provided query");
                // Custom exception handling for data integrity or mapping issues
                throw new DataIntegrityException("A system error occurred. Please contact support.", ex);
            }
        }

        public async Task<bool> JobPostExistsAsync(int jobPostId)
        {
            await using var context = await _factory.CreateDbContextAsync();
            return await context.JobPost.AnyAsync(jp => jp.Id == jobPostId);
        }

        public async Task UpdateOrAddJobPostAsync(JobPostDTO jobPostDto)
        {
            var now = DateTime.UtcNow;
            await using var context = await _factory.CreateDbContextAsync();
            if (jobPostDto.Id == 0)
            {
                _logger.LogInformation("Adding new JobPost for user {UserId} with title '{Title}'.", jobPostDto.UserId, jobPostDto.Title);
                var newJob = _mapper.Map<JobPost>(jobPostDto);
                newJob.Timestamp = now;

                if (newJob.UserSkillMatchReport != null)
                    newJob.UserSkillMatchReport.CreatedDate = now;

                context.JobPost.Add(newJob);
            }
            else
            {
                _logger.LogInformation("Updating existing JobPost with ID {JobPostId} for user {UserId}.", jobPostDto.Id, jobPostDto.UserId);
                // Load the existing entity WITH its nested report
                var existingJob = await context.JobPost
                    .Include(j => j.UserSkillMatchReport)                   // include UserSkillMatchReport please
                    .Include(j => j.JobRejectionAnalysis)
                    .FirstOrDefaultAsync(j => j.Id == jobPostDto.Id);

                if (existingJob == null)                                    // Oh, this is something strange. Should never happen
                {
                    _logger.LogError("Attempted to update JobPost with ID {JobPostId}, but it was not found.", jobPostDto.Id);
                    throw new DataIntegrityException("Job Application not found.");
                }

                // Guard against removing the report becauase business logic forbids it
                // It can only be updated - there is also protection in the mapping
                if (jobPostDto.UserSkillMatchReport == null && existingJob.UserSkillMatchReport != null)
                {
                    _logger.LogError("Attempted to remove existing User Skill Match Report for JobPost ID {JobPostId}.", jobPostDto.Id);
                    throw new InvalidOperationException("Attempting to remove existing User Skill Match Report.");
                }

                // Maps DTO properties directly onto the existing TRACKED entity
                // UserSkillMatchReport is also mapped courtesy of AutoMapper
                _mapper.Map(jobPostDto, existingJob);

                existingJob.Timestamp = now;
                if (existingJob.UserSkillMatchReport != null)
                    existingJob.UserSkillMatchReport.Timestamp = now;
            }

            await context.SaveChangesAsync();
            _logger.LogInformation("JobPost with ID {JobPostId} has been successfully added/updated.", jobPostDto.Id);
        }
    }
}
