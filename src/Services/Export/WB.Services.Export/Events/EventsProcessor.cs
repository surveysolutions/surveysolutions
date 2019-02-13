using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prometheus;
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
        private static readonly Gauge eventsCounter = Metrics.CreateGauge("wb_events_processed_count", "Count of events processed by Export Service", "site");

        public EventsProcessor(
            ITenantContext tenant,
            IServiceProvider serviceProvider,
            ILogger<EventsProcessor> logger, IDataExportProcessesService dataExportProcessesService)
        {
            this.tenant = tenant;
            this.serviceProvider = serviceProvider;
            this.logger = logger;
            this.dataExportProcessesService = dataExportProcessesService;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="processId"></param>
        /// <param name="token"></param>
        /// <returns>Last event global sequence</returns>
        public async Task HandleNewEvents(long processId, CancellationToken token = default)
        {
            long? maximumSequenceToQuery = null;
            long? startedReadAt = null;

            while (true)
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    scope.PropagateTenantContext(tenant);
                    var tenantDbContext = scope.ServiceProvider.GetService<ITenantContext>().DbContext;
                    await tenantDbContext.Database.MigrateAsync(token);

                    using (var tr = tenantDbContext.Database.BeginTransaction())
                    {
                        var metadata = tenantDbContext.Metadata;
                        startedReadAt = startedReadAt ?? metadata.GlobalSequence;
                        eventsCounter.Labels(this.tenant.Tenant.Name).Set(metadata.GlobalSequence);

                        if (maximumSequenceToQuery.HasValue
                            && metadata.GlobalSequence >= maximumSequenceToQuery) break;

                        dataExportProcessesService.ChangeStatusType(processId, DataExportStatus.Preparing);

                        var feed = await tenant.Api.GetInterviewEvents(metadata.GlobalSequence, 10000);
                        maximumSequenceToQuery = maximumSequenceToQuery ?? feed.Total;

                        if (feed.Events.Any())
                        {
                            var priorityHandlers = scope.ServiceProvider
                                .GetServices<IHighPriorityFunctionalHandler>()
                                .Cast<IStatefulDenormalizer>();
                            var handlers = scope.ServiceProvider.GetServices<IFunctionalHandler>()
                                .Cast<IStatefulDenormalizer>();

                            var all = priorityHandlers.Union(handlers).ToArray();

                            try
                            {
                                foreach (var handler in all)
                                {
                                    foreach (var ev in feed.Events.Where(ev => ev.Payload != null))
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

                                metadata.GlobalSequence = feed.Events.Last().GlobalSequence;

                                await tenantDbContext.SaveChangesAsync(token);
                            }
                            catch (Exception e)
                            {
                                logger.LogCritical(e, "Unhandled exception during event handling");
                                throw;
                            }
                        }

                        tr.Commit();

                        var totalEventsToRead = maximumSequenceToQuery - startedReadAt;
                        var eventsProcessed = metadata.GlobalSequence - startedReadAt;
                        var percent = eventsProcessed.PercentOf(totalEventsToRead);

                        dataExportProcessesService.UpdateDataExportProgress(processId, percent);
                        eventsCounter.Labels(this.tenant.Tenant.Name).Set(metadata.GlobalSequence);

                        if (metadata.GlobalSequence >= maximumSequenceToQuery)
                            break;
                    }
                }
            }
        }
    }

    public interface IEventProcessor
    {
        Task HandleNewEvents(long processId, CancellationToken token = default);
    }
}
