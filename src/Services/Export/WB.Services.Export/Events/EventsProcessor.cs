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

        long? maximumSequenceToQuery;
        private const double BatchSizeMultiplier = 1;
        private const string ApiEventsQueryMonitoringKey = "api_events_query";

        private async Task EnsureMigrated()
        {
            using (var scope = serviceProvider.CreateScope())
            {
                scope.PropagateTenantContext(tenant);

                var tenantDbContext = scope.ServiceProvider.GetService<TenantDbContext>();
                if (tenantDbContext.Database.IsNpgsql())
                {
                   await tenantDbContext.CheckSchemaVersionAndMigrate();
                }
            }
        }

        public async Task HandleNewEvents(long exportProcessId, CancellationToken token = default)
        {
            long sequenceToStartFrom;
            await EnsureMigrated();

            using (var scope = serviceProvider.CreateScope())
            {
                scope.PropagateTenantContext(tenant);
                var tenantDbContext = scope.ServiceProvider.GetService<TenantDbContext>();
                sequenceToStartFrom = tenantDbContext.GlobalSequence.AsLong;
            }

            await HandleNewEventsImplementation(exportProcessId, sequenceToStartFrom, token);
        }

        private async Task HandleNewEventsImplementation(long exportProcessId, long sequenceToStartFrom,
            CancellationToken token)
        {
            Stopwatch globalStopwatch = Stopwatch.StartNew();

            var runningAverage = new SimpleRunningAverage(50); // running average window size

            var eventsProducer = new BlockingCollection<EventsFeed>(2);

            var eventsReader = Task.Run(() => EventsReaderWorker(token, sequenceToStartFrom, eventsProducer), token);

            dataExportProcessesService.ChangeStatusType(exportProcessId, DataExportStatus.Preparing);

            var executionTrack = Stopwatch.StartNew();

            foreach (var feed in eventsProducer.GetConsumingEnumerable())
            {
                executionTrack.Restart();

                using var batchScope = this.serviceProvider.CreateTenantScope(tenant);

                var eventsHandler = batchScope.ServiceProvider.GetRequiredService<IEventsHandler>();

                var lastProcessedGlobalSequence = await eventsHandler.HandleEventsFeedAsync(feed, token);

                executionTrack.Stop();
                    
                // ReSharper disable once PossibleInvalidOperationException - max value will always be set
                var totalEventsToRead = maximumSequenceToQuery.Value - sequenceToStartFrom;
                var eventsProcessed = lastProcessedGlobalSequence - sequenceToStartFrom;
                var percent = eventsProcessed.PercentDOf(totalEventsToRead);

                // in events/second
                var thisBatchProcessingSpeed = feed.Events.Count / executionTrack.Elapsed.TotalSeconds;

                // setting next batch size to be equal average processing speed * multiplier
                var size = (int) runningAverage.Add(thisBatchProcessingSpeed);
                pageSize = (int) (size * BatchSizeMultiplier);

                // estimation by average processing speed, seconds
                var estimatedAverage = runningAverage.Eta(totalEventsToRead - eventsProcessed);

                // estimation by overall progress timer, seconds
                var estimationByTotal = globalStopwatch.Elapsed.TotalSeconds / (percent / 100.0) 
                                        - globalStopwatch.Elapsed.TotalSeconds;

                // taking average between two estimations
                var estimatedTime = TimeSpan.FromSeconds((estimatedAverage.TotalSeconds + estimationByTotal) / 2.0);

                dataExportProcessesService.UpdateDataExportProgress(exportProcessId, (int) percent, estimatedTime);
                    
                this.logger.LogInformation(
                    "Published {pageSize} events. " +
                    "GlobalSequence: {sequence:n0}/{total:n0}. " +
                    "Batch time {duration:g}. Total time {globalDuration:g}.  ETA: {eta}",
                    feed.Events.Count, feed.Events.Count > 0 ? feed.Events.Last().GlobalSequence : 0
                    , feed.Total,
                    executionTrack.Elapsed, 
                    globalStopwatch.Elapsed,
                    estimatedTime);
            }

            logger.LogInformation("Database refresh done. Took {elapsed:g}", globalStopwatch.Elapsed);
            await eventsReader;
        }

        private async Task EventsReaderWorker(CancellationToken token, long sequenceToStartFrom,
            BlockingCollection<EventsFeed> eventsProducer)
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
                    apiTrack.Stop();

                    var lastSequence = feed.Events.Count > 0
                        ? feed.Events.Last().GlobalSequence
                        : maximumSequenceToQuery;

                    maximumSequenceToQuery ??= feed.Total;

                    var readingSpeed = feed.Events.Count / apiTrack.Elapsed.TotalSeconds;
                    readingAvg.Add(readingSpeed);

                    logger.LogDebug("Read {eventsCount:n0} events from HQ. From {start:n0} to {last:n0} Took {elapsed:g}",
                        feed.Events.Count, readingSequence, lastSequence, apiTrack.Elapsed);

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
        }
    }

}
