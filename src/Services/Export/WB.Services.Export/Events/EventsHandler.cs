using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
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
            TenantDbContext dbContext,
            IOptions<ExportServiceSettings> settings)
        {
            this.logger = logger;
            this.eventsFilter = eventsFilter;
            this.handlers = handlers;
            this.tenantContext = tenantContext;
            this.dbContext = dbContext;
            saveEventsPageSize = settings.Value.MaxSaveEventsPageSize;
        }

        readonly int? saveEventsPageSize;


        public async Task HandleEventsFeedAsync(EventsFeed feed, CancellationToken token = default)
        {
            if (saveEventsPageSize.HasValue)
                foreach (var eventsBatch in feed.Events.Batch(saveEventsPageSize.Value))
                    await HandleEventsFeedAsync(eventsBatch.ToList(), token, 0);
            else        
                await HandleEventsFeedAsync(feed.Events, token, 0);
        }
        
        private async Task HandleEventsFeedAsync(List<Event> events, CancellationToken token, int attempt)
        {
            try
            {
                await using var tr = await this.dbContext.Database.BeginTransactionAsync(token);

                var eventsToPublish = await eventsFilter.FilterAsync(events, token);

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

                if (events.Count > 0)
                {
                    globalSequence.AsLong = events.Last().GlobalSequence;
                }

                logger.LogInformation($"Saving changes ending at {events.LastOrDefault()?.GlobalSequence ?? -1} for tenant {dbContext.TenantContext.Tenant.Id} and sequence {dbContext.GlobalSequence.AsLong}");
                await dbContext.SaveChangesAsync(token);
                await tr.CommitAsync(token);
                    
                Monitoring.TrackEventsProcessedCount(this.tenantContext?.Tenant?.Name, globalSequence.AsLong);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (TimeoutException te)
            {
                logger.LogCritical(te, $"Attempt:#{attempt}. exception: {te.Message}");

                if (attempt > 2)
                    throw;

                logger.LogCritical($"Attempt:#{attempt}. Will try next attempt.");
                await Task.Delay(TimeSpan.FromSeconds(10), token);
                await HandleEventsFeedAsync(events, token, attempt + 1);
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
                e.Data.Add("WB:Events", $"{events.FirstOrDefault()?.GlobalSequence ?? -1}" +
                                     $":{events.LastOrDefault()?.GlobalSequence ?? -1}");

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
