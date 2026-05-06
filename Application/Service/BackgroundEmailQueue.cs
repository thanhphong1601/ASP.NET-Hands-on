using ASP.NET_Hands_on.Application.Interface;
using ASP.NET_Hands_on.Domain.Model;
using System.Threading.Channels;

namespace ASP.NET_Hands_on.Application.Service
{
    public class BackgroundEmailQueue : IBackgroundTaskQueue
    {
        private readonly Channel<EmailJob> _queue;

        public BackgroundEmailQueue()
        {
            _queue = Channel.CreateUnbounded<EmailJob>(new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false
            });
        }

        public async ValueTask QueueEmailAsync(EmailJob job)
        {
            await _queue.Writer.WriteAsync(job);
        }

        public async ValueTask<EmailJob> DequeueAsync(CancellationToken cancellationToken)
        {
            var job = await _queue.Reader.ReadAsync(cancellationToken);
            return job;
        }
    }
}
