using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Events.Questionnaire;
using Main.Core.Events.User;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tools.CapiDataGenerator.Ninject
{
    public class CapiDataGeneratorEventStore : IStreamableEventStore
    {
        private readonly IEventStore capiEventStore;
        private readonly IEventStore supevisorEventStore;
        private readonly IEventStore headquartersEventStore;
        
        private readonly IDictionary<Guid, long> capiSequences;
        private readonly IDictionary<Guid, long> supervisorSequences;
        private readonly IDictionary<Guid, long> headquartersSequences;


        public CapiDataGeneratorEventStore(IEventStore capiEventStore, IEventStore supervisorEventStore, IEventStore headquartersEventStore)
        {
            this.capiEventStore = capiEventStore;
            this.supevisorEventStore = supervisorEventStore;
            this.headquartersEventStore = headquartersEventStore;

            this.capiSequences = new Dictionary<Guid, long>();
            this.supervisorSequences = new Dictionary<Guid, long>();
            this.headquartersSequences = new Dictionary<Guid, long>();
        }

        public CommittedEventStream ReadFrom(Guid id, long minVersion, long maxVersion)
        {
            if (AppSettings.Instance.CurrentMode == GenerationMode.DataOnHeadquarterApproved ||
                AppSettings.Instance.CurrentMode == GenerationMode.DataOnHeadquarterRejected)
            {
                return this.headquartersEventStore.ReadFrom(id, minVersion, maxVersion);
            }

            return this.supevisorEventStore.ReadFrom(id, minVersion, maxVersion);
        }

        public void Store(UncommittedEventStream eventStream)
        {
            if (AppSettings.Instance.CurrentMode == GenerationMode.DataSplitSupervisorHeadquarter)
            {
                this.supevisorEventStore.Store(eventStream);

                Func<object, bool> isHQEvent = (o) => o is NewUserCreated ||
                                                      o is TemplateImported ||
                                                      o is InterviewCreated ||
                                                      o is InterviewerAssigned;

                var committedEvents = eventStream.Select(x => x.Payload);

                if (committedEvents.Any(isHQEvent))
                {
                    StoreInternal(eventStream: eventStream, eventsequences: this.headquartersSequences, eventstore: this.headquartersEventStore);
                }
            }
            else if (AppSettings.Instance.CurrentMode == GenerationMode.DataOnHeadquarterApproved || 
                AppSettings.Instance.CurrentMode == GenerationMode.DataOnHeadquarterRejected)
            {
                this.headquartersEventStore.Store(eventStream);
            }

            else if (AppSettings.Instance.CurrentMode == GenerationMode.DataSplitCapiAndSupervisor)
            {
                Func<object, bool> isSupervisorEvent = (o) => o is NewUserCreated ||
                                                              o is TemplateImported ||
                                                              o is InterviewCreated ||
                                                              o is InterviewApproved ||
                                                              o is InterviewerAssigned;


                Func<object, bool> isCapiNotAllowedEvent = (o) => o is InterviewCreated ||
                                                        o is InterviewApproved ||
                                                        o is InterviewerAssigned;

                var committedEvents = eventStream.Select(x => x.Payload);

                //analyze icomming stream to determine target store to save
                //could be saved to one or both 

                if (!committedEvents.Any(isCapiNotAllowedEvent))
                {
                    StoreInternal(eventStream: eventStream, eventsequences: this.capiSequences, eventstore: this.capiEventStore);
                }
                if (committedEvents.Any(isSupervisorEvent))
                {
                    StoreInternal(eventStream: eventStream, eventsequences: this.supervisorSequences, eventstore: this.supevisorEventStore);
                }
            }
            else if (AppSettings.Instance.CurrentMode == GenerationMode.DataSplitOnCapiCreatedAndSupervisor)
            {
                Func<object, bool> isSupervisorEvent = (o) => o is NewUserCreated ||
                    o is TemplateImported;

                Func<object, bool> isCapiNotAllowedEvent = (o) => o is InterviewCreated ||
                                                        o is InterviewApproved;

                var committedEvents = eventStream.Select(x => x.Payload);

                //analyze icomming stream to determine target store to save
                //could be saved to one or both 

                if (!committedEvents.Any(isCapiNotAllowedEvent))
                {
                    StoreInternal(eventStream: eventStream, eventsequences: this.capiSequences, eventstore: this.capiEventStore);
                }
                if (committedEvents.Any(isSupervisorEvent))
                {
                    StoreInternal(eventStream: eventStream, eventsequences: this.supervisorSequences, eventstore: this.supevisorEventStore);
                }
            }

        }

        private static void StoreInternal(UncommittedEventStream eventStream, IDictionary<Guid, long> eventsequences, IEventStore eventstore)
        {
            var eventstream = new UncommittedEventStream(eventStream.CommitId, null);

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

        public IEnumerable<CommittedEvent[]> GetAllEvents(int bulkSize, int skipEvents)
        {
            throw new NotImplementedException();
        }

        public long GetLastEventSequence(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}
