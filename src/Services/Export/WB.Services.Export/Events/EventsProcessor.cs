using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using WB.Services.Export.Infrastructure;
using WB.Services.Infrastructure.EventSourcing;

namespace WB.Services.Export.Events
{
    public class EventsProcessor : IEventProcessor
    {
        private readonly ITenantContext tenant;
        private readonly IServiceProvider serviceProvider;
        private readonly IOptions<DbConnectionSettings> connectionSettings;
        private readonly ILogger<EventsProcessor> logger;

        public EventsProcessor(
            ITenantContext tenant,
            IServiceProvider serviceProvider, 
            IOptions<DbConnectionSettings> connectionSettings, ILogger<EventsProcessor> logger)
        {
            this.tenant = tenant;
            this.serviceProvider = serviceProvider;
            this.connectionSettings = connectionSettings;
            this.logger = logger;
        }

        public async Task HandleNewEvents(CancellationToken token = default)
        {
            long currentSequence = 0;
            long? max = null;
            var sw = Stopwatch.StartNew();
            do
            {
                sw.Restart();
                var events = await tenant.Api.GetInterviewEvents(currentSequence, 10000);
                await HandleEvents(events, token);

                max = max ?? events.Total;
                if (events.NextSequence == null) break;
                currentSequence = events.NextSequence.Value;

                logger.LogInformation("Handling: Curr: {currentSequence} Max: {max}", currentSequence, max);
                
                var speed = events.Events.Count / sw.Elapsed.TotalSeconds;
                var total = (max.Value - currentSequence) / speed;

                Debug.WriteLine($"Took {sw.Elapsed} to handle {events.Events.Count} events. {TimeSpan.FromSeconds(total)} left");


            } while (currentSequence <= max.Value);
        }

        public async Task HandleEvents(EventsFeed feed, CancellationToken token = default)
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
                        var priorityHandlers = scope.ServiceProvider.GetServices<IHighPriorityFunctionalHandler>()
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

                        await db.SaveChangesAsync(token);
                        tr.Commit();
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
