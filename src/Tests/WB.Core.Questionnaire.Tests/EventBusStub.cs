using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;

namespace WB.Core.Questionnaire.Tests
{
    public class EventBusStub:IEventBus
    {
        private readonly List<IPublishableEvent> _events = new List<IPublishableEvent>();

        public List<IPublishableEvent> Events
        {
            get { return _events; }
        }

        public void Publish(IPublishableEvent eventMessage)
        {
            Events.Add(eventMessage);
        }

        public void Publish(IEnumerable<IPublishableEvent> eventMessages)
        {
            Events.AddRange(eventMessages);
        }
    }
}
