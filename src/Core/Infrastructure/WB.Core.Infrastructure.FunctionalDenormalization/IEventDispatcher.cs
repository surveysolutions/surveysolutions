using System;
using System.Collections.Generic;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.FunctionalDenormalization.Implementation.EventDispatcher;
using WB.Core.Infrastructure.ReadSide.Repository;

namespace WB.Core.Infrastructure.FunctionalDenormalization
{
    public interface IEventDispatcher : IEventBus
    {
        void PublishEventToHandlers(IPublishableEvent eventMessage, IEnumerable<IEventHandler> handlers);
        void PublishByEventSource<T>(CommittedEventStream eventStream, IStorageStrategy<T> storage) where T : class, IReadSideRepositoryEntity;

        IEnumerable<IEventHandler> GetAllRegistredEventHandlers();

        void Register(IEventHandler handler);
        void Unregister(IEventHandler handler);
    }
}
