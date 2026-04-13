using AutoMapper;
using AutoMapper.QueryableExtensions;
using JobBank.Data;
using JobBank.Exceptions;
using JobBank.Management.Abstraction;
using JobBank.ModelsDTO;
using JobBank.Services.Abstraction;
using Microsoft.EntityFrameworkCore;

namespace JobBank.Services
{
    public class AnalysisCacheService : IAnalysisCacheService
    {
        private readonly IDbContextFactory<EmploymentBankContext> _dbFactory;
        private readonly IMapper _mapper;
        private readonly ILLMManager _llmManager;
        private readonly ILogger<AnalysisCacheService> _logger;

        public AnalysisCacheService(
            IDbContextFactory<EmploymentBankContext> dbFactory, 
            IMapper mapper,
            ILLMManager llmManager,
            ILogger<AnalysisCacheService> logger) 
        {
            _dbFactory = dbFactory;
            _mapper = mapper;
            _logger = logger;
            _llmManager = llmManager;
        }

        public async ValueTask DisposeAsync()
        {
            await ValueTask.CompletedTask;
        }

        public async Task<JobAnalysisCacheDTO> GetJobAnalysisCacheAsync(string jobDescriptionHash, string userId)
        {
            if (string.IsNullOrWhiteSpace(jobDescriptionHash) || string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogWarning("GetJobAnalysisCacheAsync called with empty parameters");
                return null; // the Model will be called, no cache.
            }

            using var context = _dbFactory.CreateDbContext();
            try
            {
                var dto = await context.JobAnalysisCache                    
                    .Where(c => c.Hash == jobDescriptionHash && c.UserId == userId)
                    .ProjectTo<JobAnalysisCacheDTO>(_mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync();

                return dto;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Error retrieving cached analysis for jobDescriptionHash={Hash}, userId={UserId}",
                    jobDescriptionHash, userId);
                throw new DataIntegrityException("A system error occurred. Please contact support.", ex);
            }
            catch (Exception genExc)
            {
                _logger.LogError(genExc, "Error retreiving cached analysis: {genExc.Message}", genExc.Message);
                throw;
            }
        }

        public async Task<JobAnalysisCacheDTO?> GetAnalysisCache(Models.Identity.JobBankUser currentUser, string userId, string jobDescriptionHash)
        {
            var HasPrivateKey = await _llmManager.UserHasValidPrivateKeyAsync(currentUser.Id);

            // get the JobAnalysisCache for the JobPost
            var analysisCache = await GetJobAnalysisCacheAsync(jobDescriptionHash, userId);

            if (analysisCache == null && (!HasPrivateKey || (HasPrivateKey && !currentUser.ForceMyKeyy)))
            {
                analysisCache = await GetPublicJobAnalysisCacheAsync(jobDescriptionHash);
            }

            return analysisCache;
        }

        public async Task<JobAnalysisCacheDTO> GetPublicJobAnalysisCacheAsync(string jobDescriptionHash)
        {
            if (string.IsNullOrWhiteSpace(jobDescriptionHash))
            {
                _logger.LogWarning("GetPublicJobAnalysisCacheAsync called with empty jobDescriptionHash");
                return null; // the Model will be called, no cache.
            }

            using var context = _dbFactory.CreateDbContext();
            try
            {
                var dto = await context.JobAnalysisCache
                    .Where(c => c.Hash == jobDescriptionHash && c.IsPublic)
                    .ProjectTo<JobAnalysisCacheDTO>(_mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync();
                return dto;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Error retrieving public cached analysis for jobDescriptionHash={Hash}",
                    jobDescriptionHash);
                throw new DataIntegrityException("A system error occurred. Please contact support.", ex);
            }
            catch (Exception genExc)
            {
                _logger.LogError(genExc, "Error retreiving public cached analysis: {genExc.Message}", genExc.Message);
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
            // Validate input parameters before proceeding
            ArgumentNullException.ThrowIfNull(emp);
            ArgumentException.ThrowIfNullOrWhiteSpace(emp.Hash);
            ArgumentException.ThrowIfNullOrWhiteSpace(emp.UserId); // especially for this paramerter.

            var utcNow = DateTime.UtcNow;
            using var context = _dbFactory.CreateDbContext();
            try
            {
                // Existing cache entry check
                var existingCache = await context.JobAnalysisCache
                        .FirstOrDefaultAsync(c => c.Hash == emp.Hash && c.UserId == emp.UserId);
                if (existingCache != null)
                {
                    // Cache entry already exists, return without adding
                    _logger.LogWarning("Cache entry already exists for {JobPostDescription} and UserId {UserId}",
                         emp.JobPostDescription, emp.UserId);

                    return 0; // Indicating no new entry was added
                }

                var entity = _mapper.Map<Models.JobAnalysisCache>(emp);
                entity.CreatedDate = utcNow;

                await context.JobAnalysisCache.AddAsync(entity);
                return await context.SaveChangesAsync();
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error adding job analysis cache for {Hash}/{UserId}",
                       emp.Hash, emp.UserId);
                throw;
            }            
        }
    }
}
