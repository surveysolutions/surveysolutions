using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ncqrs;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using RavenQuestionnaire.Web.App_Start;

namespace RavenQuestionnaire.Core.Events
{
    public interface IEventSync
    {
        IEnumerable<AggregateRootEventStream> ReadEvents();
        void WriteEvents(IEnumerable<AggregateRootEventStream> stream);
    }

    public class RavenEventSync : IEventSync
    {
        #region Implementation of IEventSync

        public IEnumerable<AggregateRootEventStream> ReadEvents()
        {
            var myEventStore = NcqrsEnvironment.Get<IEventStore>();

            if (myEventStore == null)
                throw new Exception("IEventStore is not correct.");
            return myEventStore.ReadByAggregateRoot().Select(c => new AggregateRootEventStream(c));
        }

        public void WriteEvents(IEnumerable<AggregateRootEventStream> stream)
        {
            var eventStore = NcqrsEnvironment.Get<IEventStore>();
            if (eventStore == null)
                throw new Exception("IEventStore is not properly initialized.");
            //((InProcessEventBus)myEventBus).RegisterHandler();
            foreach (AggregateRootEventStream commitedEventStream in stream)
            {
                Guid commitId = Guid.NewGuid();
                var currentEventStore = eventStore.ReadFrom(commitedEventStream.SourceId,
                                                            commitedEventStream.FromVersion,
                                                            commitedEventStream.ToVersion);
                var uncommitedStream = new UncommittedEventStream(commitId);
                foreach (CommittedEvent committedEvent in commitedEventStream.Events)
                {
                    if (currentEventStore.Count(ce => ce.EventIdentifier == committedEvent.EventIdentifier) > 0)
                        continue;

                    uncommitedStream.Append(new UncommittedEvent(committedEvent.EventIdentifier,
                                                                 committedEvent.EventSourceId,
                                                                 committedEvent.EventSequence, 0,
                                                                 committedEvent.EventTimeStamp,
                                                                 committedEvent.Payload,
                                                                 committedEvent.EventVersion));
                }
                eventStore.Store(uncommitedStream);
            }
            NCQRSInit.RebuildReadLayer();
        }

        #endregion
    }
}
