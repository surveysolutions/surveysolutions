using System;
using System.Linq;
using System.Collections.Generic;
using RavenQuestionnaire.Core.Events;
using Ncqrs.Eventing.ServiceModel.Bus;
using RavenQuestionnaire.Core.Views.Survey;
using RavenQuestionnaire.Core.Denormalizers;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Events.Questionnaire.Completed;


namespace RavenQuestionnaire.Core.EventHandlers
{
    public class StatisticQuestionnaireBrowseItemDenormalizer : IEventHandler<NewCompleteQuestionnaireCreated>,
                                                                IEventHandler<CompleteQuestionnaireDeleted>,
                                                                IEventHandler<QuestionnaireStatusChanged>,
                                                                IEventHandler<QuestionnaireAssignmentChanged>
    {

        private readonly IDenormalizerStorage<SurveyBrowseItem> documentItemStore;

        public StatisticQuestionnaireBrowseItemDenormalizer(IDenormalizerStorage<SurveyBrowseItem> documentItemStore)
        {
            this.documentItemStore = documentItemStore;
        }

        public void Handle(IPublishedEvent<NewCompleteQuestionnaireCreated> evnt)
        {
            var item = this.documentItemStore.Query().FirstOrDefault(t => t.Id==evnt.Payload.QuestionnaireId.ToString());
            if (item == null)
            {
                var surveyitem = new SurveyItem(evnt.Payload.CreationDate, evnt.Payload.CreationDate,
                    evnt.Payload.QuestionnaireId, evnt.Payload.Questionnaire.Status, evnt.Payload.Questionnaire.Responsible);
                var statistic = new Dictionary<Guid, SurveyItem> { { evnt.Payload.Questionnaire.PublicKey, surveyitem } };
                this.documentItemStore.Store(
                    new SurveyBrowseItem(
                        evnt.Payload.QuestionnaireId.ToString(),
                        evnt.Payload.Questionnaire.Title,
                        evnt.Payload.Questionnaire.Responsible == null ? 1 : 0, 
                        statistic, 1,
                        evnt.Payload.Questionnaire.Status == SurveyStatus.Initial && evnt.Payload.Questionnaire.Responsible == null ? 1 : 0,
                        evnt.Payload.Questionnaire.Status == SurveyStatus.Error ? 1 : 0,
                        evnt.Payload.Questionnaire.Status == SurveyStatus.Complete ? 1 : 0), 
                    evnt.Payload.Questionnaire.PublicKey);
            }
            else
            {
                item.Total++;
                if (evnt.Payload.Questionnaire.Responsible == null)
                    item.Unassigned++;
                else
                    IncrementCount(evnt.Payload.Questionnaire.Status.Name, item);
                item.Statistic.Add(
                    evnt.Payload.Questionnaire.PublicKey, 
                    new SurveyItem(
                        evnt.Payload.CreationDate, 
                        evnt.Payload.CreationDate, 
                        evnt.Payload.Questionnaire.PublicKey,
                        evnt.Payload.Questionnaire.Status, evnt.Payload.Questionnaire.Responsible));
            }
        }

        public void Handle(IPublishedEvent<QuestionnaireStatusChanged> evnt)
        {
            var list = documentItemStore.Query().ToList();
            foreach (var item in list.Where(i => i.Statistic.Any(surveyItem => surveyItem.Key == evnt.Payload.CompletedQuestionnaireId)))
            {
                var val =
                    item.Statistic.Where(t => t.Key == evnt.Payload.CompletedQuestionnaireId).Select(t => t.Value).
                        FirstOrDefault();
                {
                    if (val.Responsible != null && !string.IsNullOrEmpty(val.Responsible.Id))
                    {
                        IncrementCount(val.Status.Name, item);
                        DecrementCount(evnt.Payload.Status.Name, item);
                    }
                    val.Status = evnt.Payload.Status;
                }
            }
        }

        public void Handle(IPublishedEvent<QuestionnaireAssignmentChanged> evnt)
        {
            var list = documentItemStore.Query().ToList();
            foreach (var item in list.Where(i => i.Statistic.Any(surveyItem => surveyItem.Key == evnt.Payload.CompletedQuestionnaireId)))
            {
                var val = item.Statistic.Where(t => t.Key == evnt.Payload.CompletedQuestionnaireId).Select(t => t.Value).FirstOrDefault();
                if (val != null)
                {
                    if (val.Responsible == null && evnt.Payload.Responsible != null && !string.IsNullOrEmpty(evnt.Payload.Responsible.Id))
                    {
                        item.Unassigned--;
                        IncrementCount(val.Status.Name, item);
                        val.Responsible = evnt.Payload.Responsible;
                    }
                }
            }
        }

        public void Handle(IPublishedEvent<CompleteQuestionnaireDeleted> evnt)
        {
            var list = documentItemStore.Query().ToList();
            foreach (var item in list.Where(i => i.Statistic.Any(surveyItem => surveyItem.Key == evnt.Payload.CompletedQuestionnaireId)))
            {
                var val = item.Statistic.Where(t => t.Key == evnt.Payload.CompletedQuestionnaireId).Select(t => t.Value).FirstOrDefault();
                if (val != null)
                {
                    item.Total--;
                    if (val.Responsible == null)
                        item.Unassigned--;
                    else
                       DecrementCount(val.Status.Name, item);
                    item.Statistic.Remove(evnt.Payload.CompletedQuestionnaireId);
                }
            }
        }

        private void DecrementCount(string name, SurveyBrowseItem item)
        {
            switch (name)
            {
                case "Error":
                    item.Error--;
                    break;
                case "Completed":
                    item.Complete--;
                    break;
                default:
                    item.Initial--;
                    break;
            }
        }
        private void IncrementCount(string name, SurveyBrowseItem item)
        {
            switch (name)
            {
                case "Error":
                    item.Error++;
                    break;
                case "Completed":
                    item.Complete++;
                    break;
                default:
                    item.Initial++;
                    break;
            }
        }
    }
}
