using System;
using System.Collections.Generic;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.EventBus.Lite;

namespace Ncqrs.Eventing.ServiceModel.Bus
{
    /// <summary>
    /// A bus that can publish events to handlers.
    /// </summary>
    public interface IEventBus : ILiteEventBus
    {
        void Publish(IPublishableEvent eventMessage);

        void Publish(IEnumerable<IPublishableEvent> eventMessages);
    }
}