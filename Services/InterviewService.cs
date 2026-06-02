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
    public class InterviewService : IInterviewService
    {
        private readonly IDbContextFactory<EmploymentBankContext> _dbFactory;
        private readonly ILogger<IInterviewService> _logger;
        private readonly IMapper _mapper;

        public InterviewService(IDbContextFactory<EmploymentBankContext> dbFactory, IMapper mapper)
        {
            _mapper = mapper;
            _dbFactory = dbFactory;
        }

        public async ValueTask DisposeAsync()
        {
            await ValueTask.CompletedTask;
        }

        public async Task<InterviewDTO> GetInterviewByIdAsync(int interviewId)
        {
            try
            {
                await using var context = _dbFactory.CreateDbContext();
                return await context.Interviews
                    .Where(i => i.Id == interviewId)
                    .Include(i => i.JobPost)
                    .ProjectTo<InterviewDTO>(_mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Error retrieving interview with ID {InterviewId}", interviewId);
                throw new DataIntegrityException("GetInterviewByIdAsync: A system error occurred. Please contact support.", ex);
            }
        }

        public async Task<IEnumerable<InterviewDTO>> GetInterviewsByJobPostIdAsync(int jobPostId)
        {
            try
            {
                await using var context = _dbFactory.CreateDbContext();
                return await context.Interviews
                    .Where(i => i.JobPostId == jobPostId)
                    .Include(i => i.JobPost)
                    .ProjectTo<InterviewDTO>(_mapper.ConfigurationProvider)
                    .ToListAsync();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Error retrieving interviews for JobPostId {JobPostId}", jobPostId);
                throw new DataIntegrityException("GetInterviewsByJobPostIdAsync: A system error occurred. Please contact support.", ex);
            }
        }

        public async Task<IEnumerable<InterviewDTO>> GetInterviewsByUserIdAsync(string userId, bool isDeleted = false)
        {
            try
            {
                await using var context = _dbFactory.CreateDbContext();
                return await context.Interviews
                    .Where(i => i.UserId == userId && i.IsDeleted == isDeleted)
                    .Include(i => i.JobPost)
                    .ProjectTo<InterviewDTO>(_mapper.ConfigurationProvider)
                    .ToListAsync();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Error retrieving interviews for UserId {UserId}", userId);
                throw new DataIntegrityException("GetInterviewsByUserIdAsync: A system error occurred. Please contact support.", ex);
            }
        }

        public async Task<PaginationResult<InterviewDTO>> GetInterviewsByUserIdWithPaginationAsync(
            string userId,
            string companyName,
            int startIndex,
            int take,
            bool isDeleted = false)
        {
            try
            {
                await using var context = _dbFactory.CreateDbContext();
                IQueryable<Interview> query = context.Interviews
                    .Where(i => i.UserId == userId && i.IsDeleted == isDeleted)
                    .Include(i => i.JobPost);

                if (!string.IsNullOrWhiteSpace(companyName))
                {
                    query = query.Where(i => EF.Functions.Like(i.JobPost.Company, $"%{companyName}%"));
                }

                var orderedQuery = query.OrderByDescending(i => i.CompletedAtUtc);

                var totalCount = await orderedQuery.CountAsync();
                var interviews = await orderedQuery
                    .Skip(startIndex)
                    .Take(take)
                    .ProjectTo<InterviewDTO>(_mapper.ConfigurationProvider)
                    .ToListAsync();

                return new PaginationResult<InterviewDTO>
                {
                    Items = interviews,
                    TotalCount = totalCount
                };
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Error retrieving paginated interviews for UserId {UserId} with CompanyName filter {CompanyName}", userId, companyName);
                throw new DataIntegrityException("GetInterviewsByUserIdWithPaginationAsync: A system error occurred. Please contact support.", ex);
            }
        }

        public async Task AddInterviewAsync(InterviewDTO interviewDto)
        {
            if (interviewDto == null)
                throw new ArgumentNullException(nameof(interviewDto), "Interview DTO cannot be null.");

            if (interviewDto.Id != 0)
                throw new ArgumentException("Interview ID must be zero when adding a new interview.");

            try
            {
                await using var context = _dbFactory.CreateDbContext();
                var interview = _mapper.Map<Interview>(interviewDto);
                context.Interviews.Add(interview);
                await context.SaveChangesAsync();
                interviewDto.Id = interview.Id;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Error adding new interview for UserId {UserId} and JobPostId {JobPostId}", interviewDto.UserId, interviewDto.JobPostId);
                throw new DataIntegrityException("AddInterviewAsync: A system error occurred. Please contact support.", ex);
            }
        }
    }
}