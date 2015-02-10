using System;
using System.Net;
using System.Threading;
using EventStore.ClientAPI;
using Ncqrs.Eventing.Storage;
using Nito.AsyncEx.Synchronous;
using WB.Core.Infrastructure.HealthCheck;

namespace WB.Core.Infrastructure.Storage.EventStore.Implementation
{
    public class EventStoreHealthCheck : IEventStoreHealthCheck
    {
        private readonly IStreamableEventStore eventStore;

        public EventStoreHealthCheck(IStreamableEventStore eventStore)
        {
            this.eventStore = eventStore;
        }

        public ConnectionHealthCheckResult Check()
        {
            try
            {
                int count = eventStore.CountOfAllEvents();
                return ConnectionHealthCheckResult.Happy();
            }
            catch (Exception e)
            {
                return ConnectionHealthCheckResult.Down("Can't connect to Event Store. " + e.Message);
            }
        }
    }
}