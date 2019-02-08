using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WB.Services.Export.Infrastructure;
using WB.Services.Infrastructure.EventSourcing;

namespace WB.Services.Export.Events
{
    public class EventsProcessor : IEventProcessor
    {
        private readonly ITenantContext tenant;
        private readonly IServiceProvider serviceProvider;
        private readonly ILogger<EventsProcessor> logger;

        public EventsProcessor(
            ITenantContext tenant,
            IServiceProvider serviceProvider,
            ILogger<EventsProcessor> logger)
        {
            this.tenant = tenant;
            this.serviceProvider = serviceProvider;
            this.logger = logger;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <returns>Last event global sequence</returns>
        public async Task HandleNewEvents(CancellationToken token = default)
        {
            long? maximumSequenceToQuery = null;
            long? last = null;

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
                            if (maximumSequenceToQuery.HasValue 
                                && db.Metadata.GlobalSequence >= maximumSequenceToQuery) break;
                            
                            var feed = await tenant.Api.GetInterviewEvents(db.Metadata.GlobalSequence, 10000);
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
                                        await handler.Handle(ev, token);
                                    }

                                    await handler.SaveStateAsync(token);
                                }

                                db.Metadata.GlobalSequence = feed.Events.Last().GlobalSequence;

                                await db.SaveChangesAsync(token);
                            }

                            tr.Commit();
                        }
                    }
                }
            }
        }
    }

    public interface IEventProcessor
    {
        Task HandleNewEvents(CancellationToken token = default);
    }
}
