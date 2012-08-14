using System;
using System.Linq;
using System.Collections.Generic;
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

        public StatisticQuestionnaireBrowseItemDenormalizer(IDenormalizerStorage<SurveyBrowseItem> documentItemStore)
        {
            this.documentItemStore = documentItemStore;
        }

        public void Handle(IPublishedEvent<NewCompleteQuestionnaireCreated> evnt)
        {
            var item =
                this.documentItemStore.Query().Where(t => t.Id==evnt.Payload.QuestionnaireId.ToString()).FirstOrDefault();
            if (item == null)
            {
                var surveyitem = new SurveyItem(evnt.Payload.CreationDate, evnt.Payload.CreationDate, 
                    evnt.Payload.QuestionnaireId, evnt.Payload.Status, evnt.Payload.Responsible);
                var statistic = new Dictionary<Guid, SurveyItem> { { evnt.Payload.CompletedQuestionnaireId, surveyitem } };
                this.documentItemStore.Store(
                    new SurveyBrowseItem(
                        evnt.Payload.QuestionnaireId.ToString(),
                        evnt.Payload.Questionnaire.Title,
                        evnt.Payload.Responsible == null ? 1 : 0, 
                        statistic), 
                    evnt.Payload.CompletedQuestionnaireId);
            }
            else
            {
                if (evnt.Payload.Responsible == null)
                    item.UnAssigment++;
                item.Statistic.Add(
                    evnt.Payload.CompletedQuestionnaireId, 
                    new SurveyItem(
                        evnt.Payload.CreationDate, 
                        evnt.Payload.CreationDate, 
                        evnt.Payload.CompletedQuestionnaireId, 
                        evnt.Payload.Status, evnt.Payload.Responsible));
            }
        }

        public void Handle(IPublishedEvent<QuestionnaireStatusChanged> evnt)
        {

        }

        public void Handle(IPublishedEvent<QuestionnaireAssignmentChanged> evnt)
        {
            var list = documentItemStore.Query().ToList();
            foreach (var item in list.Where(i => i.Statistic.Any(surveyItem => surveyItem.Key == evnt.Payload.CompletedQuestionnaireId)))
            {
                SurveyItem val = item.Statistic.Where(t => t.Key == evnt.Payload.CompletedQuestionnaireId).Select(t=>t.Value).FirstOrDefault();
                if (val.Responsible == null)
                    item.UnAssigment--;
                val.Responsible = evnt.Payload.Responsible;
            }
        }

        public void Handle(IPublishedEvent<CompleteQuestionnaireDeleted> evnt)
        {
            documentItemStore.Remove(evnt.Payload.CompletedQuestionnaireId);
        }
    }
}
