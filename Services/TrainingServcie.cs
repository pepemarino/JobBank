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
    public class TrainingServcie : ITrainingService
    {
        private readonly EmploymentBankContext _context;
        private readonly IMapper _mapper;

        public TrainingServcie(IDbContextFactory<EmploymentBankContext> dbFactory, IMapper mapper)
        {
            _mapper = mapper;
            _context = dbFactory.CreateDbContext();
        }

        public async ValueTask DisposeAsync() => await _context.DisposeAsync();

        public async Task AddTrainingAsync(TrainingDTO trainingDto)
        {
            if (trainingDto.Id != 0) // there will never be the case of an update
                throw new ArgumentException("Interview ID must be zero when adding a new interview.");

            try
            {
                var training = _mapper.Map<Training>(trainingDto);
                _context.Trainings.Add(training);
                await _context.SaveChangesAsync();
                trainingDto.Id = training.Id; // update the DTO with the generated ID
            }
            catch (InvalidOperationException ex)
            {
                throw new DataIntegrityException("AddTrainingAsync: A system error occurred. Please contact support.", ex);
            }
        }

        public async Task<TrainingDTO> GetTrainingByIdAsync(int trainingId)
        {
            try
            {
                return await _context.Trainings
                    .Where(i => i.Id == trainingId)
                    .ProjectTo<TrainingDTO>(_mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync();
            }
            catch (InvalidOperationException ex)
            {
                throw new DataIntegrityException("GetTrainingByIdAsync: A system error occurred. Please contact support.", ex);
            }
        }

        public async Task<TrainingDTO> GetTrainingsByInterviewIdAsync(int interviewId)
        {
            try
            {
                return await _context.Trainings
                    .Where(i => i.InterviewId == interviewId)
                    .ProjectTo<TrainingDTO>(_mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync();
            }
            catch (InvalidOperationException ex)
            {
                throw new DataIntegrityException("GetTrainingsByInterviewIdAsync: A system error occurred. Please contact support.", ex);
            }
        }

        public async Task<IEnumerable<TrainingDTO>> GetTrainingsByUserIdAsync(string userId)
        {
            try
            {
                return await _context.Trainings
                    .Where(i => i.UserId == userId)
                    .ProjectTo<TrainingDTO>(_mapper.ConfigurationProvider)
                    .ToListAsync();
            }
            catch (InvalidOperationException ex)
            {
                throw new DataIntegrityException("GetTrainingsByUserIdAsync: A system error occurred. Please contact support.", ex);
            }
        }
    }
}
