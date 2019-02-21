using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Humanizer;
using Microsoft.Extensions.Logging;
using WB.Services.Export.Infrastructure;
using WB.Services.Infrastructure.EventSourcing;

namespace WB.Services.Export.Events
{
    public class EventsHandler : IEventsHandler, IDisposable
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

                            Monitoring.TrackEventHandlerProcessingSped(
                                this.tenantContext?.Tenant?.Name,
                                GetHandlerMonitoringKey(handler.GetType()), 
                                eventsToPublish.Count / eventHandlerStopwatch.Elapsed.TotalSeconds);
                        }
                    }

                    if (feed.Events.Count > 0)
                    {
                        metadata.GlobalSequence = feed.Events.Last().GlobalSequence;
                    }

                    await dbContext.SaveChangesAsync(token);
                    tr.Commit();

                    Monitoring.TrackEventsProcessedCount(this.tenantContext?.Tenant?.Name, metadata.GlobalSequence);

                    return metadata.GlobalSequence;
                }
            }
            catch (Exception e)
            {
                logger.LogCritical(e, "Unhandled exception during event handling");
                throw;
            }
        }

        private string GetHandlerMonitoringKey(Type type) =>type.Name.Humanize(LetterCasing.LowerCase).Replace(" ", "_");

        public void Dispose()
        {
            foreach (var handler in handlers)
            {
                Monitoring.TrackEventHandlerProcessingSped(this.tenantContext?.Tenant?.Name,
                    GetHandlerMonitoringKey(handler.GetType()), 0);
            }
        }
    }
}
