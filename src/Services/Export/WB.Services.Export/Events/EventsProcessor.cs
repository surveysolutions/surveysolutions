using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Services.Processing;
using WB.Services.Infrastructure.EventSourcing;

namespace WB.Services.Export.Events
{
    public class EventsProcessor : IEventProcessor
    {
        private readonly ITenantContext tenant;
        private readonly IServiceProvider serviceProvider;
        private readonly ILogger<EventsProcessor> logger;
        private readonly IDataExportProcessesService dataExportProcessesService;
        
        public EventsProcessor(
            ITenantContext tenant,
            IServiceProvider serviceProvider,
            ILogger<EventsProcessor> logger,
            IDataExportProcessesService dataExportProcessesService,
            IOptions<ExportServiceSettings> settings)
        {
            this.tenant = tenant;
            this.serviceProvider = serviceProvider;
            this.logger = logger;
            this.dataExportProcessesService = dataExportProcessesService;
            pageSize = settings.Value.DefaultEventQueryPageSize;
        }

        private int pageSize;

        long? maximumSequenceToQuery = null;

        private void EnsureMigrated()
        {
            using (var scope = serviceProvider.CreateScope())
            {
                scope.PropagateTenantContext(tenant);

                var db = scope.ServiceProvider.GetService<ITenantContext>().DbContext;
                if(db.Database.IsNpgsql())
                    db.Database.Migrate();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="processId"></param>
        /// <param name="token"></param>
        /// <returns>Last event global sequence</returns>
        public async Task HandleNewEvents(long processId, CancellationToken token = default)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                scope.PropagateTenantContext(tenant);

                EnsureMigrated();

                var tenantDbContext = scope.ServiceProvider.GetService<ITenantContext>().DbContext;
                //await tenantDbContext.Database.MigrateAsync(token);

                var sequenceToStartFrom = tenantDbContext.Metadata.GlobalSequence;

                Stopwatch globalStopwatch = Stopwatch.StartNew();

                var eta = new SimpleRunningAverage(10); // running average window size

                var eventsProducer = new BlockingCollection<EventsFeed>(2);

                var eventsReader = Task.Run(async () =>
                {
                    try
                    {
                        var readingAvg = new SimpleRunningAverage(5);
                        readingAvg.Add(pageSize);

                        var readingSequence = sequenceToStartFrom;
                        var apiTrack = Stopwatch.StartNew();

                        while (true)
                        {
                            if (maximumSequenceToQuery.HasValue
                                && readingSequence >= maximumSequenceToQuery) break;

                            apiTrack.Restart();

                            // just to make sure that we will not query too much data while skipping deleted questionnaires
                            var amount = Math.Min((int) readingAvg.Average, pageSize);

                            var feed = await tenant.Api.GetInterviewEvents(readingSequence, amount);

                            maximumSequenceToQuery = maximumSequenceToQuery ?? feed.Total;

                            readingAvg.Add(feed.Events.Count / apiTrack.Elapsed.TotalSeconds);

                            logger.LogDebug("Read {eventsCount} events from HQ. From {start} to {last} Took {elapsed}",
                                feed.Events.Count,
                                readingSequence,
                                feed.Events.Count > 0 ? feed.Events.Last().GlobalSequence : maximumSequenceToQuery,
                                apiTrack.Elapsed);

                            if (feed.Events.Count > 0)
                            {
                                eventsProducer.Add(feed, token);
                                readingSequence = feed.Events.Last().GlobalSequence;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    finally
                    {
                        eventsProducer.CompleteAdding();
                    }
                }, token);

                dataExportProcessesService.ChangeStatusType(processId, DataExportStatus.Preparing);

                var executionTrack = Stopwatch.StartNew();

                foreach (var feed in eventsProducer.GetConsumingEnumerable())
                {
                    executionTrack.Restart();

                    using (var tr = tenantDbContext.Database.BeginTransaction())
                    {
                        var metadata = tenantDbContext.Metadata;
                        Monitoring.EventsProcessedCounter.Labels(this.tenant.Tenant.Name).Set(metadata.GlobalSequence);

                        var feedProcessingStopwatch = Stopwatch.StartNew();
                        var eventsFilter = scope.ServiceProvider.GetService<IEventsFilter>();
                        var eventsToPublish = eventsFilter != null
                            ? await eventsFilter?.FilterAsync(feed.Events)
                            : feed.Events;

                        try
                        {
                            if (eventsToPublish.Count > 0)
                            {
                                var handlers = scope.ServiceProvider.GetServices<IFunctionalHandler>();

                                foreach (var handler in handlers)
                                {
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
                                }
                            }

                            metadata.GlobalSequence = feed.Events.Last().GlobalSequence;

                            await tenantDbContext.SaveChangesAsync(token);
                        }
                        catch (Exception e)
                        {
                            logger.LogCritical(e, "Unhandled exception during event handling");
                            throw;
                        }

                        tr.Commit();

                        Monitoring.EventsProcessedCounter.Labels(this.tenant.Tenant.Name).Set(metadata.GlobalSequence);

                        // ReSharper disable once PossibleInvalidOperationException - max value will always be set
                        var totalEventsToRead = maximumSequenceToQuery.Value - sequenceToStartFrom;
                        var eventsProcessed = metadata.GlobalSequence - sequenceToStartFrom;
                        var percent = eventsProcessed.PercentOf(totalEventsToRead);

                        pageSize = (int)eta.Add(feed.Events.Count / executionTrack.Elapsed.TotalSeconds);

                        var estimatedTime = eta.Eta(totalEventsToRead - eventsProcessed);
                        dataExportProcessesService.UpdateDataExportProgress(processId, percent, estimatedTime);
                        
                        this.logger.LogInformation(
                        "Processed {pageSize} events. " +
                               "GlobalSequence: {sequence:n0} out of {total:n0}. " +
                               "Took {duration:g}. ETA: {eta}",
                        feed.Events.Count, feed.Events.Count > 0 ? feed.Events.Last().GlobalSequence : 0
                        , feed.Total,
                        feedProcessingStopwatch.Elapsed,estimatedTime);
                    }
                }

                logger.LogInformation("Database refresh done. Took {elapsed:g}", globalStopwatch.Elapsed);
                await eventsReader;
            }
        }
    }
}
