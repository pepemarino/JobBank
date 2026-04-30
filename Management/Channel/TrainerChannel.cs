using System.Threading.Channels;

namespace JobBank.Management
{
    public record TrainerRequest(int InterviewId, string UserId);

    public class TrainerChannel
    {
        private readonly Channel<TrainerRequest> _channel;
        public TrainerChannel()
        {
            _channel = Channel.CreateUnbounded<TrainerRequest>();
        }

        public ChannelWriter<TrainerRequest> Writer => _channel.Writer;
        public ChannelReader<TrainerRequest> Reader => _channel.Reader;
    }
}
