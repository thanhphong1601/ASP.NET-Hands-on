using System.Threading;
using System.Threading.Tasks;
using ASP.NET_Hands_on.Model;

namespace ASP.NET_Hands_on.Interface
{
    public interface IBackgroundTaskQueue
    {
        ValueTask QueueEmailAsync(EmailJob job);
        ValueTask<EmailJob> DequeueAsync(CancellationToken cancellationToken);
    }
}
