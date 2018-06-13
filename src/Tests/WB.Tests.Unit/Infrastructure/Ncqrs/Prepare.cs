using System;
using System.Collections.Generic;
using Ncqrs.Eventing;
using IEvent = WB.Core.Infrastructure.EventBus.IEvent;

namespace Ncqrs.Spec
{
    internal static class Prepare
    {
        public class PrepareTheseEvents
        {
            private readonly IEnumerable<IEvent> _events;

            public PrepareTheseEvents(IEnumerable<IEvent> events)
            {
                _events = events;
            }

            public CommittedEventStream ForSource(Guid id, int sequenceOffset = 0)
            {
                int sequence = sequenceOffset + 1;

                var commitId = Guid.NewGuid();
                var comittedEvents = new List<CommittedEvent>();
                foreach (var evnt in _events)
                {
                    var committedEvent = new CommittedEvent(commitId, null, Guid.NewGuid(), id, sequence, DateTime.UtcNow, 0, evnt);
                    sequence++;
                    comittedEvents.Add(committedEvent);
                }
                return new CommittedEventStream(id, comittedEvents);
            }

            public UncommittedEventStream ForSourceUncomitted(Guid id, Guid commitId, int sequenceOffset = 0)
            {                
                int initialVersion = sequenceOffset == 0 ? 1 : sequenceOffset;
                int sequence = initialVersion;

                var comittedEvents = new List<CommittedEvent>();
                var result = new UncommittedEventStream(commitId, null);
                foreach (var evnt in _events)
                {
                    var uncommittedEvent = new UncommittedEvent(Guid.NewGuid(), id, sequence, initialVersion, DateTime.UtcNow,
                                                            evnt);
                    result.Append(uncommittedEvent);
                    sequence++;
                }
                return result;
            }
        }

        public static PrepareTheseEvents Events(IEnumerable<IEvent> events)
        {
            return new PrepareTheseEvents(events);
        }

        public static PrepareTheseEvents Events(params IEvent[] events)
        {
            return new PrepareTheseEvents(events);
        }
    }
}
