using System;
using System.Collections.Generic;
using Ncqrs.Eventing;
using WB.Core.Infrastructure.FunctionalDenormalization.EventHandlers;

namespace WB.Core.Infrastructure.FunctionalDenormalization.Implementation.EventDispatcher
{
    public class InMemoryTransaction : IDisposable
    {
        private readonly IList<IFunctionalEventHandler> functionalEventHandlers;
        private readonly Guid eventSourceId;

        public InMemoryTransaction(IList<IFunctionalEventHandler> functionalEventHandlers, Guid eventSourceId)
        {
            this.functionalEventHandlers = functionalEventHandlers;
            this.eventSourceId = eventSourceId;

            foreach (var functionalHandler in functionalEventHandlers)
            {
                functionalHandler.ChangeForSingleEventSource(eventSourceId);
            }
        }

        public void Dispose()
        {
            foreach (var functionalHandler in this.functionalEventHandlers)
            {
                functionalHandler.FlushDataToPersistentStorage(this.eventSourceId);
            }
        }
    }
}