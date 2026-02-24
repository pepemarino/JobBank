using System.Threading.Channels;

namespace JobBank.Management
{
    // The data you pass to the background, to send meesages to producers and consumers.
    // In this case, we want to pass the JobApplicationId and UserId to the background service,
    // so it can perform the analysis for that specific job application and user.
    public record AnalysisRequest(int JobApplicationId, string UserId);

    public class AnalysisChannel
    {
        private readonly Channel<AnalysisRequest> _channel;
        public AnalysisChannel()
        {
            // Unbounded allows any number of requests, or use CreateBounded for limits
            _channel = Channel.CreateUnbounded<AnalysisRequest>();
        }

        public ChannelWriter<AnalysisRequest> Writer => _channel.Writer;
        public ChannelReader<AnalysisRequest> Reader => _channel.Reader;
    }
}
