using System.Collections.Generic;
using System.Diagnostics;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.EventBus;

namespace WB.Core.Infrastructure
{
    public interface IEventDispatcher : IEventBus
    {
        void PublishEventToHandlers(IPublishableEvent eventMessage, IReadOnlyDictionary<IEventHandler, Stopwatch> handlersWithStopwatch);



        void Register(IEventHandler handler);
        void Unregister(IEventHandler handler);
    }
}
