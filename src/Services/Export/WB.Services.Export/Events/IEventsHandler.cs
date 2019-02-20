using System.Threading;
using System.Threading.Tasks;

namespace WB.Services.Export.Events
{
    public interface IEventsHandler
    {
        Task<long> HandleEventsFeedAsync(EventsFeed feed, CancellationToken token = default);
    }
}