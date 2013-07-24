using System;
using System.Collections.Generic;
using System.Linq;
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

        public CapiDataGeneratorEventStore(IEventStore capiEventStore, IEventStore supervisorEventStore)
        {
            this.capiEventStore = capiEventStore;
            this.supevisorEventStore = supervisorEventStore;
        }

        public CommittedEventStream ReadFrom(Guid id, long minVersion, long maxVersion)
        {
            return capiEventStore.ReadFrom(id, minVersion, maxVersion);
        }

        public void Store(UncommittedEventStream eventStream)
        {
            var supervisorEvents = new[]
            {
                typeof(NewUserCreated),
                typeof(NewCompleteQuestionnaireCreated),
                typeof(QuestionnaireStatusChanged),
                typeof(QuestionnaireAssignmentChanged),
                typeof(TemplateImported)
            };
            var capiEvents = new[]
            {
                typeof(NewUserCreated),
                typeof(NewAssigmentCreated),
                typeof(QuestionnaireStatusChanged)
            };

            var committedEvents = eventStream.Select(x => x.Payload.GetType());

            if (capiEvents.Intersect(committedEvents).Any())
            {
                capiEventStore.Store(eventStream);
            }

            if (supervisorEvents.Intersect(committedEvents).Any())
            {
                supevisorEventStore.Store(eventStream);
            }
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
