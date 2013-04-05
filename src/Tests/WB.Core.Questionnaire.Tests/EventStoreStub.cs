using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;

namespace WB.Core.Questionnaire.Tests
{
    public class EventStoreStub:IEventStore
    {
        private readonly List<UncommittedEvent> _events = new List<UncommittedEvent>();

        public List<UncommittedEvent> Events
        {
            get { return _events; }
        }
        public CommittedEventStream ReadFrom(Guid id, long minVersion, long maxVersion)
        {
            return new CommittedEventStream(id);
        }

        public void Store(UncommittedEventStream eventStream)
        {
            this.Events.AddRange(eventStream);
        }
    }
}
