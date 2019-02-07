using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WB.Services.Export.Events.Interview;
using WB.Services.Export.Infrastructure;
using WB.Services.Infrastructure.EventSourcing;

namespace WB.Services.Export.Events.Handlers
{
    class DebugHandler : IFunctionalHandler, 
        IEventHandler<InterviewCreated>
    {
        private readonly ILogger<DebugHandler> logger;
        private readonly ITenantContext tenant;
        private long EventsHandled = 0;

        public DebugHandler(ILogger<DebugHandler>logger, ITenantContext tenant)
        {
            this.logger = logger;
            this.tenant = tenant;
        }
        public Task SaveStateAsync(CancellationToken cancellationToken = default)
        {
            logger.LogInformation($"Saving state. EventsHandled: {EventsHandled}");
            return Task.CompletedTask;
        }

        public Task HandleAsync(PublishedEvent<InterviewCreated> @event, CancellationToken cancellationToken = default)
        {
            EventsHandled++;
            logger.LogInformation($"Handle event: {@event.EventSourceId} {@event.Event.GetType().Name}");
            return Task.CompletedTask;
        }
    }
}
