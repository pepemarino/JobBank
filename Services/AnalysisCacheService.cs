using AutoMapper;
using AutoMapper.QueryableExtensions;
using JobBank.Data;
using JobBank.Exceptions;
using JobBank.ModelsDTO;
using JobBank.Services.Abstraction;
using Microsoft.EntityFrameworkCore;

namespace JobBank.Services
{
    public class AnalysisCacheService : IAnalysisCacheService
    {
        private readonly EmploymentBankContext _context;
        private readonly IMapper _mapper;

        public AnalysisCacheService(IDbContextFactory<EmploymentBankContext> dbFactory, IMapper mapper) 
        {
            _context = dbFactory.CreateDbContext();
            _mapper = mapper;
        }

        public async ValueTask DisposeAsync()
        {
            await _context.DisposeAsync();
        }

        public async Task<JobAnalysisCacheDTO> GetJobAnalysisCacheAsync(string jobDescriptionHash)
        {
            try
            {
                var dto = await _context.JobAnalysisCache                    
                    .Where(c => c.Hash == jobDescriptionHash)
                    .ProjectTo<JobAnalysisCacheDTO>(_mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync();

                return dto;
            }
            catch (InvalidOperationException ex)
            {
                throw new DataIntegrityException("A system error occurred. Please contact support.", ex);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// It adds a new job analysis cache entry to the database. 
        /// The method takes a JobAnalysisCacheDTO object as a parameter, 
        /// which contains the details of the cache entry to be added. 
        /// The method is asynchronous and returns a Task, indicating that it performs an asynchronous operation. 
        /// If the chache exists then it is not added/updated
        /// </summary>
        /// <param name="emp"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<int> AddJobAnalysisCacheAsync(JobAnalysisCacheDTO emp)
        {
            var utcNow = DateTime.UtcNow;
            try
            {
                // Existing cache entry check
                var existingCache = await _context.JobAnalysisCache.FirstOrDefaultAsync(c => c.Hash == emp.Hash);
                if (existingCache != null)
                {
                    // Cache entry already exists, return without adding
                    return 0; // Indicating no new entry was added
                }

                var entity = _mapper.Map<Models.JobAnalysisCache>(emp);
                entity.CreatedDate = utcNow;

                await _context.JobAnalysisCache.AddAsync(entity);
                return await _context.SaveChangesAsync();
            }
            catch
            {
                throw;
            }            
        }
    }
}
