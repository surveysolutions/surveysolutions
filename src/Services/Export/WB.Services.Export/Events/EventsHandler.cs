using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Npgsql;
using WB.Services.Export.Infrastructure;
using WB.Services.Infrastructure.EventSourcing;
using WB.ServicesIntegration.Export;

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

        public async Task HandleEventsFeedAsync(EventsFeed feed, CancellationToken token = default)
        {
            try
            {
                await using var tr = await this.dbContext.Database.BeginTransactionAsync(token);

                var eventsToPublish = eventsFilter != null
                    ? await eventsFilter.FilterAsync(feed.Events, token)
                    : feed.Events;

                var globalSequence = dbContext.GlobalSequence;
                
                if (eventsToPublish.Count > 0)
                {
                    var eventHandlerStopwatch = new Stopwatch();

                    foreach (var handler in handlers)
                    {
                        token.ThrowIfCancellationRequested();

                        eventHandlerStopwatch.Restart();
                        foreach (var ev in eventsToPublish)
                        {
                            try
                            {
                                await handler.Handle(ev, token);
                            }
                            catch (Exception e)
                            {
                                e.Data.Add("WB:Event", ev.EventTypeName);
                                e.Data.Add("WB:GlobalSequence", ev.GlobalSequence);
                                e.Data.Add("WB:EventSourceId", ev.EventSourceId);
                                throw;
                            }
                        }

                        await handler.SaveStateAsync(token);
                        eventHandlerStopwatch.Stop();

                        Monitoring.TrackEventsProcessingLatency(this.tenantContext?.Tenant?.Name ?? "",
                            eventsToPublish.Count,
                            eventHandlerStopwatch.Elapsed);
                    }
                }

                if (feed.Events.Count > 0)
                {
                    globalSequence.AsLong = feed.Events.Last().GlobalSequence;
                }

                await dbContext.SaveChangesAsync(token);
                tr.Commit();
                    
                Monitoring.TrackEventsProcessedCount(this.tenantContext?.Tenant?.Name, globalSequence.AsLong);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (PostgresException pe) when (pe.SqlState == "57014")
            {
                throw new OperationCanceledException("Job canceled", pe);
            }
            catch (Exception e) when (e.InnerException is TaskCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                e.Data.Add("WB:Events", $"{feed.Events.FirstOrDefault()?.GlobalSequence ?? -1}" +
                                     $":{feed.Events.LastOrDefault()?.GlobalSequence ?? -1}");

                var exc = e;
                while (exc != null)
                {
                    logger.LogCritical(exc, exc.Message);
                    foreach (var key in exc.Data.Keys)
                    {
                        if (key is string keyAsString && keyAsString.StartsWith("WB:"))
                            logger.LogCritical($"Exception Data : {keyAsString} : {exc.Data[key]}");
                    }
                    exc = exc.InnerException;
                }

                throw;
            }
        }
    }
}
