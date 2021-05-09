using System.Threading;
using System.Threading.Tasks;
using WB.ServicesIntegration.Export;

namespace WB.Services.Export.Events
{
    public interface IEventsHandler
    {
        Task HandleEventsFeedAsync(EventsFeed feed, CancellationToken token = default);
    }
}
