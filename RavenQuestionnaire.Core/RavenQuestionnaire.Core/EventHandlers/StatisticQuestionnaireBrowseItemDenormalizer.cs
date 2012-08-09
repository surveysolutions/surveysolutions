using System;
using RavenQuestionnaire.Core.Events;
using Ncqrs.Eventing.ServiceModel.Bus;
using RavenQuestionnaire.Core.Denormalizers;
using RavenQuestionnaire.Core.Events.Questionnaire.Completed;
using RavenQuestionnaire.Core.Views.Survey;

namespace RavenQuestionnaire.Core.EventHandlers
{
    public class SurveyBrowseItemDenormalizer : IEventHandler<NewCompleteQuestionnaireCreated>, 
                                                                IEventHandler<CompleteQuestionnaireDeleted>,
                                                                IEventHandler<QuestionnaireStatusChanged>,
                                                                IEventHandler<QuestionnaireAssignmentChanged>
    {
        private IDenormalizerStorage<SurveyBrowseItem> documentItemStore;

        public SurveyBrowseItemDenormalizer(IDenormalizerStorage<SurveyBrowseItem> documentItemStore)
        {
            this.documentItemStore = documentItemStore;
        }

        public void Handle(IPublishedEvent<NewCompleteQuestionnaireCreated> evnt)
        {
            var storage = new SurveyBrowseItem(evnt.Payload.CompletedQuestionnaireId, evnt.Payload.Questionnaire.Title, evnt.Payload.Questionnaire.TemplateId, evnt.Payload.Status, evnt.Payload.Responsible);
            storage.AllQuestionnaire.Add(evnt.Payload.CompletedQuestionnaireId, evnt.Payload.Status);
            if (evnt.Payload.Responsible == null)
                storage.UnAssignment++;
            this.documentItemStore.Store(storage, Guid.NewGuid());
        }

        public void Handle(IPublishedEvent<QuestionnaireStatusChanged> evnt)
        {
            
        }

        public void Handle(IPublishedEvent<QuestionnaireAssignmentChanged> evnt)
        {
            var storage = this.documentItemStore.GetByGuid(evnt.Payload.CompletedQuestionnaireId);
            storage.UnAssignment--;
        }

        public void Handle(IPublishedEvent<CompleteQuestionnaireDeleted> evnt)
        {
            var storage = this.documentItemStore.GetByGuid(evnt.Payload.CompletedQuestionnaireId);
            storage.AllQuestionnaire.Remove(evnt.Payload.CompletedQuestionnaireId);
        }
    }
}
