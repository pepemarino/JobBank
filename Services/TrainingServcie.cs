using AutoMapper;
using AutoMapper.QueryableExtensions;
using JobBank.Data;
using JobBank.Models;
using JobBank.ModelsDTO;
using JobBank.Services.Abstraction;
using Microsoft.EntityFrameworkCore;

namespace JobBank.Services
{
    public class TrainingServcie : ITrainingService
    {
        private readonly IDbContextFactory<EmploymentBankContext> _factory; // it is better to use the factory pattern for DbContext in services
                                                                            // to ensure proper disposal and avoid issues with scoped lifetimes in dependency injection
                                                                            // note that every method becomes a unit of work, which is a good practice for services
        private readonly IMapper _mapper;

        public TrainingServcie(IDbContextFactory<EmploymentBankContext> dbFactory, IMapper mapper)
        {
            _mapper = mapper;
            _factory = dbFactory;
        }

        public async Task AddTrainingAsync(TrainingDTO trainingDto)
        {
            if (trainingDto.Id != 0) // there will never be the case of an update
                throw new ArgumentException("Interview ID must be zero when adding a new interview.");

            await using var context = await _factory.CreateDbContextAsync();

            var training = _mapper.Map<Training>(trainingDto);
            context.Trainings.Add(training);
            await context.SaveChangesAsync();
            trainingDto.Id = training.Id; // update the DTO with the generated ID

        }

        public async Task<TrainingDTO> GetTrainingByIdAsync(int trainingId)
        {
            await using var context = await _factory.CreateDbContextAsync();

            return await context.Trainings
                    .Where(i => i.Id == trainingId)
                    .ProjectTo<TrainingDTO>(_mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync();
        }

        public async Task<TrainingDTO> GetTrainingsByInterviewIdAsync(int interviewId)
        {
            await using var context = await _factory.CreateDbContextAsync();

            return await context.Trainings
                    .Where(i => i.InterviewId == interviewId)
                    .ProjectTo<TrainingDTO>(_mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<TrainingDTO>> GetTrainingsByUserIdAsync(string userId)
        {
            await using var context = await _factory.CreateDbContextAsync();

            return await context.Trainings
                    .Where(i => i.UserId == userId)
                    .ProjectTo<TrainingDTO>(_mapper.ConfigurationProvider)
                    .ToListAsync();
        }
    }
}
