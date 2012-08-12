using RavenQuestionnaire.Core.Events;
using Ncqrs.Eventing.ServiceModel.Bus;
using RavenQuestionnaire.Core.Views.Survey;
using RavenQuestionnaire.Core.Denormalizers;
using RavenQuestionnaire.Core.Events.Questionnaire.Completed;


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
            var storage = new SurveyBrowseItem(evnt.Payload.CompletedQuestionnaireId, evnt.Payload.Questionnaire.Title, evnt.Payload.QuestionnaireId, evnt.Payload.Status, evnt.Payload.Responsible);
            storage.AllQuestionnaire.Add(evnt.Payload.CompletedQuestionnaireId, evnt.Payload.Status);
            if (evnt.Payload.Responsible == null)
                storage.UnAssignment++;
            this.documentItemStore.Store(storage, evnt.Payload.CompletedQuestionnaireId);
        }

        public void Handle(IPublishedEvent<QuestionnaireStatusChanged> evnt)
        {
            
        }

        public void Handle(IPublishedEvent<QuestionnaireAssignmentChanged> evnt)
        {
            var storage = this.documentItemStore.GetByGuid(evnt.Payload.CompletedQuestionnaireId);
            if (storage.Responsible == null && storage.UnAssignment != 0)
                storage.UnAssignment--;
            else
                if (storage.Responsible != null && storage.UnAssignment == 0)
                    storage.UnAssignment++;
            this.documentItemStore.Store(storage, evnt.Payload.CompletedQuestionnaireId);
        }

        public void Handle(IPublishedEvent<CompleteQuestionnaireDeleted> evnt)
        {
            var storage = this.documentItemStore.GetByGuid(evnt.Payload.CompletedQuestionnaireId);
            storage.AllQuestionnaire.Remove(evnt.Payload.CompletedQuestionnaireId);
        }
    }
}
