using System;
using System.Collections.Generic;
using Ncqrs.Eventing.ServiceModel.Bus;

namespace WB.Core.Infrastructure.EventHandlers
{
    public interface IFunctionalEventHandler
    {
        void Handle(IEnumerable<IUncommittedEvent> publishableEvents, Guid eventSourceId);
        void RegisterHandlersInOldFashionNcqrsBus(InProcessEventBus oldEventBus);
    }
}