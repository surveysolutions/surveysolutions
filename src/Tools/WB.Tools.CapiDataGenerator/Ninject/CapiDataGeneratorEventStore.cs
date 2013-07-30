using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Main.Core.Events.Questionnaire.Completed;
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
            return capiEventStore.ReadFrom(id, minVersion, maxVersion);
        }

        public void Store(UncommittedEventStream eventStream)
        {
            var eventstream = new UncommittedEventStream(eventStream.CommitId);

            Func<object, bool> supervisorExpression = (o) => o is NewUserCreated || o is NewCompleteQuestionnaireCreated ||
                o is TemplateImported || o is QuestionnaireAssignmentChanged ||
                (o is QuestionnaireStatusChanged && (((QuestionnaireStatusChanged)o).Status.PublicId == SurveyStatus.Unassign.PublicId || 
                ((QuestionnaireStatusChanged)o).Status.PublicId == SurveyStatus.Initial.PublicId));

            Func<object, bool> capiExpression = (o) => !(o is NewCompleteQuestionnaireCreated || o is TemplateImported || o is QuestionnaireAssignmentChanged ||
                 (o is QuestionnaireStatusChanged && (((QuestionnaireStatusChanged)o).Status.PublicId == SurveyStatus.Unassign.PublicId ||
                 ((QuestionnaireStatusChanged)o).Status.PublicId == SurveyStatus.Initial.PublicId)));

            
            IDictionary<Guid, long> eventsequences = null;
            IEventStore eventstore = null;

            var committedEvents = eventStream.Select(x => x.Payload);
            if (committedEvents.Any(capiExpression))
            {
                eventsequences = capiSequences;
                eventstore = capiEventStore;
            }
            if (committedEvents.Any(supervisorExpression))
            {
                eventsequences = supervisorSequences;
                eventstore = supevisorEventStore;
            }

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

        public int CountOfAllEventsWithoutSnapshots()
        {
            throw new NotImplementedException();
        }

        public int CountOfAllEventsIncludingSnapshots()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<CommittedEvent[]> GetAllEventsWithoutSnapshots(int bulkSize = 256)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<CommittedEvent[]> GetAllEventsIncludingSnapshots(int bulkSize = 32)
        {
            throw new NotImplementedException();
        }
    }
}
