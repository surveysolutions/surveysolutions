using System;
using Ncqrs.Eventing.Storage;
using WB.Core.SharedKernels.SurveyManagement.Services.HealthCheck.Checks;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects.HealthCheck;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services.HealthCheck
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