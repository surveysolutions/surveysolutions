using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Services.Processing;
using WB.Services.Export.Utils;
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

            while(true)
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    scope.PropagateTenantContext(tenant);
                    await scope.ServiceProvider.GetService<TenantDbContext>().Database.MigrateAsync(token);

                    using (var db = scope.ServiceProvider.GetService<TenantDbContext>())
                    {
                        scope.SetDbContext(db);
                        
                        using (var tr = db.Database.BeginTransaction())
                        {
                            var metadata = db.Metadata;
                            startedReadAt = startedReadAt ?? metadata.GlobalSequence;

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

                                foreach (var handler in all)
                                {
                                    foreach (var ev in feed.Events.Where(ev => ev.Payload != null))
                                    {
                                        await handler.Handle(ev);
                                    }

                                    await handler.SaveStateAsync(token);
                                }

                                metadata.GlobalSequence = feed.Events.Last().GlobalSequence;

                                await db.SaveChangesAsync(token);
                            }
                            
                            tr.Commit();

                            var totalEventsToRead = maximumSequenceToQuery - startedReadAt;
                            var eventsProcessed = metadata.GlobalSequence - startedReadAt;
                            var percent = eventsProcessed.PercentOf(totalEventsToRead);

                            dataExportProcessesService.UpdateDataExportProgress(processId, percent);

                            if (metadata.GlobalSequence >= maximumSequenceToQuery) break;
                        }
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
