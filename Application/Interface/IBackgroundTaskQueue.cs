using System.Threading;
using System.Threading.Tasks;
using ASP.NET_Hands_on.Domain.Model;

namespace ASP.NET_Hands_on.Application.Interface
{
    public interface IBackgroundTaskQueue
    {
        ValueTask QueueEmailAsync(EmailJob job);
        ValueTask<EmailJob> DequeueAsync(CancellationToken cancellationToken);
    }
}
