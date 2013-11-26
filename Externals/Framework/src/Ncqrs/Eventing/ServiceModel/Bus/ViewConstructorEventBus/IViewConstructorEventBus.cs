using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ncqrs.Eventing.ServiceModel.Bus.ViewConstructorEventBus
{
    public interface IViewConstructorEventBus : IEventBus
    {
        void PublishEventsToHandlers(IPublishableEvent eventMessage, IEnumerable<IEventHandler> handlersForPublish);
        void PublishForSingleEventSource(Guid eventSourceId, long sequence = 0);

        IEnumerable<IEventHandler> GetAllRegistredEventHandlers();
        void AddHandler(IEventHandler handler);
        void RemoveHandler(IEventHandler handler);
       
    }
}
