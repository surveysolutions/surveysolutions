using System;
using Ncqrs.Eventing.Storage;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.HealthCheck;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services.HealthCheck.Checks
{
    public class EventStoreHealthChecker : IAtomicHealthCheck<EventStoreHealthCheckResult>
    {
        private readonly IHeadquartersEventStore eventStore;

        public EventStoreHealthChecker(IHeadquartersEventStore eventStore)
        {
            this.eventStore = eventStore;
        }

        public EventStoreHealthCheckResult Check()
        {
            try
            {
                int count = this.eventStore.CountOfAllEvents();
                return EventStoreHealthCheckResult.Happy();
            }
            catch (Exception e)
            {
                return EventStoreHealthCheckResult.Down("Can't connect to Event Store. " + e.Message);
            }
        }
    }
}
