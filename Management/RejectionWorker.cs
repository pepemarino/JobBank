using JobBank.Data;
using JobBank.Models;
using Microsoft.EntityFrameworkCore;

namespace JobBank.Management
{
    public class RejectionWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly int _targetHour = 2; // Set the target hour for execution (2 AM UTC)
        private readonly string _jobName = nameof(RejectionWorker);
        private readonly ILogger<RejectionWorker> _logger;

        public RejectionWorker(IServiceScopeFactory scopeFactory, ILogger<RejectionWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("RejectionWorker started.");

            using PeriodicTimer timer = new(TimeSpan.FromMinutes(1));

            try
            {
                await ProcessOldRecordsIfTimeAsync(stoppingToken);

                while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    await ProcessOldRecordsIfTimeAsync(stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("RejectionWorker is stopping.");
            }
        }

        private async Task ProcessOldRecordsIfTimeAsync(CancellationToken token)
        {
            DateTime nowUtc = DateTime.UtcNow;

            // Is it the right hour of the day? AND have we not already executed today?
            if (nowUtc.Hour == _targetHour && !await HasRunTodayAsync(token))
            {
                try
                {
                    _logger.LogInformation("Starting daily JobPost Rejection for records older than 1 month...");

                    int rejectedCount = await RejectJobPostsAsync(token);

                    await UpdateLastExecutionDateAsync(token);
                    _logger.LogInformation("Daily JobPost Rejection completed successfully. Rejected {Count} applications.", rejectedCount);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while executing JobPost Rejection.");
                }
            }
        }

        private async Task<bool> HasRunTodayAsync(CancellationToken token)
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<EmploymentBankContext>();

            var execution = await dbContext.BackgroundJobExecutions
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.JobName == _jobName, cancellationToken: token);

            if (execution == null)
                return false;

            return execution.LastExecutionDate.Date == DateTime.UtcNow.Date;
        }

        private async Task UpdateLastExecutionDateAsync(CancellationToken token)
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<EmploymentBankContext>();

            var execution = await dbContext.BackgroundJobExecutions
                .FirstOrDefaultAsync(e => e.JobName == _jobName, cancellationToken: token);

            if (execution == null)
            {
                execution = new BackgroundJobExecution
                {
                    JobName = _jobName,
                    LastExecutionDate = DateTime.UtcNow,
                    CreatedDateTime = DateTime.UtcNow,
                    UpdatedDateTime = DateTime.UtcNow
                };
                dbContext.BackgroundJobExecutions.Add(execution);
            }
            else
            {
                execution.LastExecutionDate = DateTime.UtcNow;
                execution.UpdatedDateTime = DateTime.UtcNow;
            }

            await dbContext.SaveChangesAsync(token);
        }

        private async Task<int> RejectJobPostsAsync(CancellationToken token)
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<EmploymentBankContext>();

            dbContext.Database.SetCommandTimeout(TimeSpan.FromMinutes(10));

            var result = await dbContext.Database
                .SqlQueryRaw<int>("EXEC dbo.spRejectOldJobPosts")
                .ToListAsync(cancellationToken: token);

            return result.FirstOrDefault();
        }
    }
}
