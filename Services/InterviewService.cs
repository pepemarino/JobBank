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
        private readonly EmploymentBankContext _context;
        private readonly IMapper _mapper;

        public InterviewService(IDbContextFactory<EmploymentBankContext> dbFactory, IMapper mapper)
        {
            _mapper = mapper;
            _context = dbFactory.CreateDbContext();
        }

        public async ValueTask DisposeAsync() => await _context.DisposeAsync();

        public async Task<InterviewDTO> GetInterviewByIdAsync(int interviewId)
        {
            try
            {
                return await _context.Interviews
                    .Where(i => i.Id == interviewId)
                    .ProjectTo<InterviewDTO>(_mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync();
            }
            catch (InvalidOperationException ex)
            {
                throw new DataIntegrityException("GetInterviewByIdAsync: A system error occurred. Please contact support.", ex);
            }
        }

        public async Task<IEnumerable<InterviewDTO>> GetInterviewsByJobPostIdAsync(int jobPostId)
        {
            try
            {
                return await _context.Interviews
                    .Where(i => i.JobPostId == jobPostId)
                    .ProjectTo<InterviewDTO>(_mapper.ConfigurationProvider)
                    .ToListAsync();
            }
            catch (InvalidOperationException ex)
            {
                throw new DataIntegrityException("GetInterviewsByJobPostIdAsync: A system error occurred. Please contact support.", ex);
            }
        }

        public async Task<IEnumerable<InterviewDTO>> GetInterviewsByUserIdAsync(string userId)
        {
            try
            {
                return await _context.Interviews
                    .Where(i => i.UserId == userId)
                    .ProjectTo<InterviewDTO>(_mapper.ConfigurationProvider)
                    .ToListAsync();
            }
            catch (InvalidOperationException ex)
            {
                throw new DataIntegrityException("GetInterviewsByUserIdAsync: A system error occurred. Please contact support.", ex);
            }
        }

        public async Task AddInterviewAsync(InterviewDTO interviewDto)
        {
            if (interviewDto.Id != 0) // there will never be the case of an update
                throw new ArgumentException("Interview ID must be zero when adding a new interview.");

            try
            {
                var interview = _mapper.Map<Interview>(interviewDto);
                _context.Interviews.Add(interview);
                await _context.SaveChangesAsync();
            }
            catch (InvalidOperationException ex)
            {
                throw new DataIntegrityException("AddInterviewAsync: A system error occurred. Please contact support.", ex);
            }            
        }
    }
}
