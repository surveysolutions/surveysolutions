using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Events.Questionnaire;
using Main.Core.Events.User;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;

namespace WB.Tools.CapiDataGenerator.Models
{
    public class CapiDataGeneratorEventStore : IStreamableEventStore
    {
        private readonly IEventStore capiEventStore;
        private readonly IEventStore supevisorEventStore;
        private readonly IDictionary<Guid, long> capiSequences;
        private readonly IDictionary<Guid, long> supervisorSequences; 

        public CapiDataGeneratorEventStore(IEventStore capiEventStore, IEventStore supervisorEventStore)
        {
            this.capiEventStore = capiEventStore;
            this.supevisorEventStore = supervisorEventStore;
            this.capiSequences = new Dictionary<Guid, long>();
            this.supervisorSequences = new Dictionary<Guid, long>();
        }

        public CommittedEventStream ReadFrom(Guid id, long minVersion, long maxVersion)
        {
            return supevisorEventStore.ReadFrom(id, minVersion, maxVersion);
        }

        public void Store(UncommittedEventStream eventStream)
        {
            Func<object, bool> isSupervisorEvent = (o) => AppSettings.Instance.AreSupervisorEventsNowPublishing;

            Func<object, bool> isCapiEvent = (o) => !AppSettings.Instance.AreSupervisorEventsNowPublishing || o is NewUserCreated ||
                                                    o is TemplateImported;

            var committedEvents = eventStream.Select(x => x.Payload);
            if (committedEvents.Any(isCapiEvent))
            {
                this.StoreInternal(eventStream: eventStream, eventsequences: capiSequences, eventstore: capiEventStore);
            }
            if (committedEvents.Any(isSupervisorEvent))
            {
                this.StoreInternal(eventStream: eventStream, eventsequences: supervisorSequences, eventstore: supevisorEventStore);
            }
        }

        private void StoreInternal(UncommittedEventStream eventStream, IDictionary<Guid, long> eventsequences, IEventStore eventstore)
        {
            var eventstream = new UncommittedEventStream(eventStream.CommitId);

            foreach (var @event in eventStream)
            {
                if (!eventsequences.ContainsKey(@event.EventSourceId))
                {
                    eventsequences.Add(@event.EventSourceId, 1);
                }
                else
                {
                    eventsequences[@event.EventSourceId] += 1;
                }

                eventstream.Append(new UncommittedEvent(eventIdentifier: @event.EventIdentifier,
                    eventSequence: eventsequences[@event.EventSourceId], eventSourceId: @event.EventSourceId,
                    eventTimeStamp: @event.EventTimeStamp, eventVersion: @event.EventVersion,
                    initialVersionOfEventSource: @event.InitialVersionOfEventSource, payload: @event.Payload));
            }

            eventstore.Store(eventstream);
        }

        public IEnumerable<CommittedEvent> GetEventStream()
        {
            throw new NotImplementedException();
        }

        public int CountOfAllEvents()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<CommittedEvent[]> GetAllEvents(int bulkSize = 32)
        {
            throw new NotImplementedException();
        }
    }
}
