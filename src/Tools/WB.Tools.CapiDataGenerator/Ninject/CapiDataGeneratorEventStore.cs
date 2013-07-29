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
            Func<object, bool> supervisorExpression = (o) => o is NewUserCreated || o is NewCompleteQuestionnaireCreated ||
                o is TemplateImported || o is QuestionnaireAssignmentChanged ||
                (o is QuestionnaireStatusChanged && (((QuestionnaireStatusChanged)o).Status.PublicId == SurveyStatus.Unassign.PublicId || 
                ((QuestionnaireStatusChanged)o).Status.PublicId == SurveyStatus.Initial.PublicId));

            Func<object, bool> capiExpression = (o) => !(o is NewCompleteQuestionnaireCreated || o is TemplateImported || o is QuestionnaireAssignmentChanged ||
                 (o is QuestionnaireStatusChanged && (((QuestionnaireStatusChanged)o).Status.PublicId == SurveyStatus.Unassign.PublicId ||
                 ((QuestionnaireStatusChanged)o).Status.PublicId == SurveyStatus.Initial.PublicId)));

            var committedEvents = eventStream.Select(x => x.Payload);

            if (committedEvents.Any(capiExpression))
            {
                capiEventStore.Store(eventStream);
            }

            if (committedEvents.Any(supervisorExpression))
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
