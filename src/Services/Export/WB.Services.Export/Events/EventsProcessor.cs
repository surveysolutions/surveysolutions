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
        private const double BatchSizeMultiplier = 2;

        private void EnsureMigrated()
        {
            using (var scope = serviceProvider.CreateScope())
            {
                scope.PropagateTenantContext(tenant);

                var db = scope.ServiceProvider.GetService<TenantDbContext>();
                if (db.Database.IsNpgsql())
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
            long sequenceToStartFrom;
            EnsureMigrated();
            using (var scope = serviceProvider.CreateScope())
            {
                scope.PropagateTenantContext(tenant);
                var tenantDbContext = scope.ServiceProvider.GetService<TenantDbContext>();
                sequenceToStartFrom = tenantDbContext.Metadata.GlobalSequence;
            }

            Stopwatch globalStopwatch = Stopwatch.StartNew();

            var runningAverage = new SimpleRunningAverage(5); // running average window size

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
                        var amount = Math.Min((int)(readingAvg.Average * BatchSizeMultiplier), pageSize);

                        var feed = await tenant.Api.GetInterviewEvents(readingSequence, amount);

                        maximumSequenceToQuery = maximumSequenceToQuery ?? feed.Total;

                        readingAvg.Add(feed.Events.Count / apiTrack.Elapsed.TotalSeconds);

                        logger.LogDebug("Read {eventsCount:n0} events from HQ. From {start:n0} to {last:n0} Took {elapsed:g}",
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
                using (var batchScope = this.serviceProvider.CreateScope())
                {
                    batchScope.PropagateTenantContext(tenant);
                    var tenantDbContext = batchScope.ServiceProvider.GetService<TenantDbContext>();
                    using (var tr = tenantDbContext.Database.BeginTransaction())
                    {
                        var metadata = tenantDbContext.Metadata;
                        Monitoring.EventsProcessedCounter.Labels(this.tenant.Tenant.Name).Set(metadata.GlobalSequence);

                        var eventsFilter = batchScope.ServiceProvider.GetService<IEventsFilter>();
                        var eventsToPublish = eventsFilter != null
                            ? await eventsFilter.FilterAsync(feed.Events)
                            : feed.Events;

                        try
                        {
                            if (eventsToPublish.Count > 0)
                            {
                                var handlers = batchScope.ServiceProvider.GetServices<IFunctionalHandler>();

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

                        executionTrack.Stop();
                        Monitoring.EventsProcessedCounter.Labels(this.tenant.Tenant.Name).Set(metadata.GlobalSequence);

                        // ReSharper disable once PossibleInvalidOperationException - max value will always be set
                        var totalEventsToRead = maximumSequenceToQuery.Value - sequenceToStartFrom;
                        var eventsProcessed = metadata.GlobalSequence - sequenceToStartFrom;
                        var percent = eventsProcessed.PercentOf(totalEventsToRead);

                        var size = (int)runningAverage.Add(feed.Events.Count / executionTrack.Elapsed.TotalSeconds);
                        pageSize = (int)(size * BatchSizeMultiplier);

                        var estimatedTime = runningAverage.Eta(totalEventsToRead - eventsProcessed);
                        dataExportProcessesService.UpdateDataExportProgress(processId, percent, estimatedTime);

                        this.logger.LogInformation(
                            "Processed {pageSize} events. " +
                            "GlobalSequence: {sequence:n0} out of {total:n0}. " +
                            "Took {duration:g}. ETA: {eta}",
                            feed.Events.Count, feed.Events.Count > 0 ? feed.Events.Last().GlobalSequence : 0
                            , feed.Total,
                            executionTrack.Elapsed, estimatedTime);
                    }
                }
            }


            logger.LogInformation("Database refresh done. Took {elapsed:g}", globalStopwatch.Elapsed);
            await eventsReader;
        }
    }

}
