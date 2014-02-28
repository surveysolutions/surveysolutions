using System;
using System.Collections.Generic;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.FunctionalDenormalization.Implementation.EventDispatcher;
using WB.Core.Infrastructure.ReadSide.Repository;

namespace WB.Core.Infrastructure.FunctionalDenormalization
{
    public interface IEventDispatcher : IEventBus
    {
        void PublishEventToHandlers(IPublishableEvent eventMessage, IEnumerable<IEventHandler> handlers);

        IEnumerable<IEventHandler> GetAllRegistredEventHandlers();

        void Register(IEventHandler handler);
        void Unregister(IEventHandler handler);
    }
}
