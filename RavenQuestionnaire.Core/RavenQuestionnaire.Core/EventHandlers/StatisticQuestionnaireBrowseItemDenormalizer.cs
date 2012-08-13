using RavenQuestionnaire.Core.Events;
using Ncqrs.Eventing.ServiceModel.Bus;
using RavenQuestionnaire.Core.Views.Survey;
using RavenQuestionnaire.Core.Denormalizers;
using RavenQuestionnaire.Core.Events.Questionnaire.Completed;


namespace RavenQuestionnaire.Core.EventHandlers
{
    public class StatisticQuestionnaireBrowseItemDenormalizer : IEventHandler<NewCompleteQuestionnaireCreated>, 
                                                                IEventHandler<CompleteQuestionnaireDeleted>,
                                                                IEventHandler<QuestionnaireStatusChanged>,
                                                                IEventHandler<QuestionnaireAssignmentChanged>
    {

        private IDenormalizerStorage<SurveyBrowseItem> documentItemStore;
        public int UnAssignment { get; set; }


        public StatisticQuestionnaireBrowseItemDenormalizer(IDenormalizerStorage<SurveyBrowseItem> documentItemStore)
        {
            this.documentItemStore = documentItemStore;

        }

        public void Handle(IPublishedEvent<NewCompleteQuestionnaireCreated> evnt)
        {
            this.documentItemStore.Store(new SurveyBrowseItem(evnt.Payload.CompletedQuestionnaireId, evnt.Payload.QuestionnaireId, evnt.Payload.QuestionnaireId, evnt.Payload.Status, evnt.Payload.Responsible), evnt.Payload.CompletedQuestionnaireId);
            if (evnt.Payload.Responsible == null)
                UnAssignment++;
        }

        public void Handle(IPublishedEvent<QuestionnaireStatusChanged> evnt)
        {
            

        }

        public void Handle(IPublishedEvent<QuestionnaireAssignmentChanged> evnt)
        {
            UnAssignment--;
        }

        public void Handle(IPublishedEvent<CompleteQuestionnaireDeleted> evnt)
        {
            documentItemStore.Remove(evnt.Payload.CompletedQuestionnaireId);
        }
    }
}
