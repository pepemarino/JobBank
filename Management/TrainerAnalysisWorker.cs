using JobBank.Management.Abstraction;

namespace JobBank.Management
{
    public class TrainerAnalysisWorker : BackgroundService
    {
        private readonly TrainerChannel _trainerChannel;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<TrainerAnalysisWorker> _logger;

        public TrainerAnalysisWorker(
            TrainerChannel trainerChannel,
            IServiceScopeFactory scopeFactory,
            ILogger<TrainerAnalysisWorker> logger
            )
        {
            _scopeFactory = scopeFactory;
            _trainerChannel = trainerChannel;
            _logger = logger;
        }

        /// <summary>
        /// It is beautiful
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (await _trainerChannel.Reader.WaitToReadAsync(stoppingToken))
            {
                while (_trainerChannel.Reader.TryRead(out var request))
                {
                    try
                    {                       
                        await using var scope = _scopeFactory.CreateAsyncScope();
                        var trainerAssistantManager = scope.ServiceProvider.GetRequiredService<ITrainerAssistantManager>();

                        if (string.IsNullOrEmpty(request.UserId))
                        {
                            _logger.LogWarning("TrainerAnalysisWorker: Received request with empty UserId. Skipping analysis.");
                            continue;
                        }
                        
                        if (request.InterviewId <= 0)
                        {
                            _logger.LogWarning("TrainerAnalysisWorker: Invalid Interview ID {InterviewId}. Skipping analysis.", request.InterviewId);
                            continue;
                        }                       

                        await trainerAssistantManager.AnalyzeInterviewAsync(userId: request.UserId, interviewId: request.InterviewId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "TrainerAnalysisWorker: Unexpected error processing Interview ID {InterviewId}", 
                            request.InterviewId);
                    }
                }
            }
        }
    }
}
