using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WB.Services.Export.Infrastructure;
using WB.Services.Infrastructure.EventSourcing;

namespace WB.Services.Export.Events
{
    public class EventsHandler : IEventsHandler
    {
        private readonly ILogger<EventsHandler> logger;
        private readonly IEventsFilter eventsFilter;
        private readonly IEnumerable<IFunctionalHandler> handlers;
        private readonly ITenantContext tenantContext;
        private readonly TenantDbContext dbContext;

        public EventsHandler(
            ILogger<EventsHandler> logger,
            IEventsFilter eventsFilter,
            IEnumerable<IFunctionalHandler> handlers,
            ITenantContext tenantContext,
            TenantDbContext dbContext)
        {
            this.logger = logger;
            this.eventsFilter = eventsFilter;
            this.handlers = handlers;
            this.tenantContext = tenantContext;
            this.dbContext = dbContext;
        }

        public async Task<long> HandleEventsFeedAsync(EventsFeed feed, CancellationToken token = default)
        {
            try
            {
                using (var tr = this.dbContext.Database.BeginTransaction())
                {
                    var eventsToPublish = eventsFilter != null
                        ? await eventsFilter.FilterAsync(feed.Events)
                        : feed.Events;

                    var metadata = dbContext.Metadata;

                    var eventHandlerStopwatch = new Stopwatch();
                    if (eventsToPublish.Count > 0)
                    {
                        foreach (var handler in handlers)
                        {
                            eventHandlerStopwatch.Restart();
                            foreach (var ev in eventsToPublish)
                            {
                                try
                                {
                                    await handler.Handle(ev, token);
                                }
                                catch (Exception e)
                                {
                                    e.Data.Add("Event", ev.EventTypeName);
                                    e.Data.Add("GlobalSequence", ev.GlobalSequence);
                                    throw;
                                }
                            }

                            await handler.SaveStateAsync(token);
                            eventHandlerStopwatch.Stop();

                            Monitoring.TrackEventHandlerProcessingSped(this.tenantContext.Tenant.Name,
                                handler.GetType().Name, eventsToPublish.Count / eventHandlerStopwatch.Elapsed.TotalSeconds);
                        }
                    }

                    if (feed.Events.Count > 0)
                    {
                        metadata.GlobalSequence = feed.Events.Last().GlobalSequence;
                    }

                    await dbContext.SaveChangesAsync(token);
                    tr.Commit();

                    Monitoring.EventsProcessedCounter.Labels(this.tenantContext.Tenant.Name).Set(metadata.GlobalSequence);

                    return metadata.GlobalSequence;
                }
            }
            catch (Exception e)
            {
                logger.LogCritical(e, "Unhandled exception during event handling");
                throw;
            }
        }
    }
}
