using System;
using System.Collections.Generic;

namespace Ncqrs.Eventing.ServiceModel.Bus
{
    /// <summary>
    /// A bus that can publish events to handlers.
    /// </summary>
    public interface IEventBus
    {
        void Publish(IPublishableEvent eventMessage);

        void Publish(IEnumerable<IPublishableEvent> eventMessages);        
    }
}