using Microsoft.Extensions.Hosting;

namespace JobBank.EventHandler.ProcessorService
{
    public class EventProcessorWorker : BackgroundService
    {
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            throw new NotImplementedException();
        }
    }
}
