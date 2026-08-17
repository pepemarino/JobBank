using JobBank.Data;
using JobBank.Management;
using JobBank.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace JobBank.EventHandler.ProcessorService
{
    public sealed class EventProcessorWorker : BackgroundService
    {
        private readonly AnalysisChannel _analysisChannel;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<EventProcessorWorker> _logger;

        private const int TargetHourUtc = 21;
        private const string JobName = nameof(EventProcessorWorker);

        public EventProcessorWorker(
            AnalysisChannel analysisChannel,
            IServiceScopeFactory scopeFactory,
            ILogger<EventProcessorWorker> logger)
        {
            _analysisChannel = analysisChannel;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("EventProcessorWorker started.");

            using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));

            try
            {
                // Run once immediately (if needed)
                await TryRunDailyJobAsync(stoppingToken);

                while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    await TryRunDailyJobAsync(stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("EventProcessorWorker stopping due to cancellation.");
            }
        }

        private async Task TryRunDailyJobAsync(CancellationToken token)
        {
            var nowUtc = DateTime.UtcNow;

            // Only run during the target hour AND only once per day
            if (nowUtc.Hour != TargetHourUtc)
                return;

            if (await HasRunTodayAsync(token))
                return;

            _logger.LogInformation("Starting daily JobPost Rejection Event processing...");

            try
            {
                // Mark execution BEFORE running to avoid double execution
                await UpdateLastExecutionDateAsync(token);

                var processedCount = await ProcessEventsAsync(token);

                _logger.LogInformation(
                    "Daily JobPost Rejection Event processing completed. Processed {Count} events.",
                    processedCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during daily JobPost Rejection Event processing.");
            }
        }

        private async Task<int> ProcessEventsAsync(CancellationToken token)
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<EmploymentBankContext>();

            // Fetch batch eagerly
            var events = await dbContext.RejectionEvents
                .Where(e => !e.IsProcessed)
                .OrderBy(e => e.EventDate)
                .Take(2000)
                .ToListAsync(token);

            if (events.Count == 0)
                return 0;

            using var tx = await dbContext.Database.BeginTransactionAsync(token);

            foreach (var evt in events)
            {
                try
                {
                    // Write to channel (may block if bounded)
                    await _analysisChannel.Writer.WriteAsync(
                        new AnalysisRequest(evt.JobId, evt.UserId),
                        token);

                    // Mark processed only AFTER successful enqueue
                    evt.IsProcessed = true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Error processing event for JobId {JobId}. Event will not be marked processed.",
                        evt.JobId);
                }
            }

            var saved = await dbContext.SaveChangesAsync(token);
            await tx.CommitAsync(token);

            return saved;
        }

        private async Task<bool> HasRunTodayAsync(CancellationToken token)
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<EmploymentBankContext>();

            var execution = await dbContext.BackgroundJobExecutions
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.JobName == JobName, token);

            return execution?.LastExecutionDate.Date == DateTime.UtcNow.Date;
        }

        private async Task UpdateLastExecutionDateAsync(CancellationToken token)
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<EmploymentBankContext>();

            var execution = await dbContext.BackgroundJobExecutions
                .FirstOrDefaultAsync(e => e.JobName == JobName, token);

            var now = DateTime.UtcNow;

            if (execution == null)
            {
                execution = new BackgroundJobExecution
                {
                    JobName = JobName,
                    LastExecutionDate = now,
                    CreatedDateTime = now,
                    UpdatedDateTime = now
                };

                dbContext.BackgroundJobExecutions.Add(execution);
            }
            else
            {
                execution.LastExecutionDate = now;
                execution.UpdatedDateTime = now;
            }

            await dbContext.SaveChangesAsync(token);
        }
    }

}
