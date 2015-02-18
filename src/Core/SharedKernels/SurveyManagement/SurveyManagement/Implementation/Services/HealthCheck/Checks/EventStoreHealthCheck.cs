using System;
using Ncqrs.Eventing.Storage;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects.HealthCheck;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services.HealthCheck.Checks
{
    public class EventStoreHealthCheck : IAtomicHealthCheck<EventStoreHealthCheckResult>
    {
        private readonly IStreamableEventStore eventStore;

        public EventStoreHealthCheck(IStreamableEventStore eventStore)
        {
            this.eventStore = eventStore;
        }

        public EventStoreHealthCheckResult Check()
        {
            try
            {
                int count = eventStore.CountOfAllEvents();
                return EventStoreHealthCheckResult.Happy();
            }
            catch (Exception e)
            {
                return EventStoreHealthCheckResult.Down("Can't connect to Event Store. " + e.Message);
            }
        }
    }
}