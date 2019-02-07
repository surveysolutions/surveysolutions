using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Npgsql;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Storage;
using WB.Services.Infrastructure.EventSourcing;

namespace WB.Services.Export.Events
{
    public class EventsProcessor : IEventProcessor
    {
        private readonly ITenantContext tenant;
        private readonly IServiceProvider serviceProvider;
        private readonly IOptions<DbConnectionSettings> connectionSettings;

        public EventsProcessor(
            ITenantContext tenant,
            IServiceProvider serviceProvider, 
            IOptions<DbConnectionSettings> connectionSettings)
        {
            this.tenant = tenant;
            this.serviceProvider = serviceProvider;
            this.connectionSettings = connectionSettings;
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

                var priorityHandlers = scope.ServiceProvider.GetServices<IHighPriorityFunctionalHandler>().ToArray();
                var handlers = scope.ServiceProvider.GetServices<IFunctionalHandler>().ToArray();
                var session = (Session) scope.ServiceProvider.GetRequiredService<ISession>();

                using (var db = new NpgsqlConnection(connectionSettings.Value.DefaultConnection))
                {
                    session.Connection = db;

                    await db.OpenAsync(token);
                    using (var tr = db.BeginTransaction())
                    {
                        foreach (var handler in priorityHandlers.Concat(handlers))
                        {
                            foreach (var ev in feed.Events.Where(ev => ev.Payload != null))
                            {
                                await handler.Handle(ev, token);
                            }

                            await handler.SaveStateAsync(token);
                        }

                        await tr.CommitAsync(token);
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
