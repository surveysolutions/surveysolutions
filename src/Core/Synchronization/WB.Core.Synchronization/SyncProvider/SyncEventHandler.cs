using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Events;
using Ncqrs;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Storage;

namespace WB.Core.Synchronization.SyncProvider
{
    internal class SyncEventHandler : ISyncEventHandler
    {
        /// <summary>
        /// The event store.
        /// </summary>
        private readonly IEventStore eventStore;

        public SyncEventHandler()
        {
            this.eventStore = NcqrsEnvironment.Get<IEventStore>();
            this.IncomeEvents = new List<UncommittedEventStream>();
        }

        public bool Process(IEnumerable<AggregateRootEvent> stream)
        {
            Merge(stream);
            Commit();
            return true;
        }

        /// <summary>
        /// The merge events.
        /// </summary>
        /// <param name="stream">
        /// The stream.
        /// </param>
        private void Merge(IEnumerable<AggregateRootEvent> stream)
        {
            if (stream != null)
            {
               // this.IncomeEvents.AddRange(this.BuildEventStreams(stream));
                var newStream = new UncommittedEventStream(Guid.NewGuid(),"backup");
                foreach (var aggregateRootEvent in stream)
                {
                    newStream.Append(new UncommittedEvent(aggregateRootEvent.EventIdentifier, aggregateRootEvent.EventSourceId,
                        aggregateRootEvent.EventSequence, 0, aggregateRootEvent.EventTimeStamp, aggregateRootEvent.Payload));
                }
                this.IncomeEvents.AddRange(new[] { newStream });
            }

           /* // check for events with null Payload
            if (this.IncomeEvents.SelectMany(eventStream => eventStream).Any(c => c.Payload == null))
            {
                throw new Exception("Event is wrong");
            }*/
        }

        /// <summary>
        /// The commit.
        /// </summary>
        private void Commit()
        {
          /*  var myEventBus = NcqrsEnvironment.Get<IEventBus>();
            if (myEventBus == null)
            {
                throw new Exception("IEventBus is not properly initialized.");
            }*/

            foreach (UncommittedEventStream uncommittedEventStream in this.IncomeEvents)
            {
                this.eventStore.Store(uncommittedEventStream);
              //  myEventBus.Publish(uncommittedEventStream.Select(e => e as IPublishableEvent));
            }
        }

        protected IEnumerable<UncommittedEventStream> BuildEventStreams(IEnumerable<AggregateRootEvent> stream)
        {
            return
                stream.GroupBy(x => x.EventSourceId).Select(
                    g => g.CreateUncommittedEventStream(this.eventStore.ReadFrom(g.Key, int.MinValue, int.MaxValue)));
        }

        /// <summary>
        /// Gets or sets IncomeEvents.
        /// </summary>
        private List<UncommittedEventStream> IncomeEvents { get; set; }
    }
}
