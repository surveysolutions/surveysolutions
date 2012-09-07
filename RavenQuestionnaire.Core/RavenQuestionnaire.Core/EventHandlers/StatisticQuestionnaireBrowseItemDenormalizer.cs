// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StatisticQuestionnaireBrowseItemDenormalizer.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The statistic questionnaire browse item denormalizer.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.EventHandlers
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using RavenQuestionnaire.Core.Events;
    using Ncqrs.Eventing.ServiceModel.Bus;
    using RavenQuestionnaire.Core.Views.Survey;
    using RavenQuestionnaire.Core.Denormalizers;
    using RavenQuestionnaire.Core.Entities.SubEntities;
    using RavenQuestionnaire.Core.Events.Questionnaire.Completed;
    
    /// <summary>
    /// The statistic questionnaire browse item denormalizer.
    /// </summary>
    public class StatisticQuestionnaireBrowseItemDenormalizer : IEventHandler<NewCompleteQuestionnaireCreated>, 
                                                                IEventHandler<CompleteQuestionnaireDeleted>, 
                                                                IEventHandler<QuestionnaireStatusChanged>, 
                                                                IEventHandler<QuestionnaireAssignmentChanged>
    {
        #region Fields

        /// <summary>
        /// The document item store.
        /// </summary>
        private readonly IDenormalizerStorage<SurveyBrowseItem> documentItemStore;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="StatisticQuestionnaireBrowseItemDenormalizer"/> class.
        /// </summary>
        /// <param name="documentItemStore">
        /// The document item store.
        /// </param>
        public StatisticQuestionnaireBrowseItemDenormalizer(IDenormalizerStorage<SurveyBrowseItem> documentItemStore)
        {
            this.documentItemStore = documentItemStore;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
        public void Handle(IPublishedEvent<NewCompleteQuestionnaireCreated> evnt)
        {
            var item = this.documentItemStore.Query().FirstOrDefault(t => t.Id == evnt.Payload.Questionnaire.TemplateId);
            if (item == null)
            {
                var surveyitem = new SurveyItem(
                    evnt.Payload.CreationDate, 
                    evnt.Payload.CreationDate, 
                    evnt.Payload.Questionnaire.TemplateId, 
                    evnt.Payload.Questionnaire.Status, 
                    evnt.Payload.Questionnaire.Responsible);
                var statistic = new Dictionary<Guid, SurveyItem>
                    {
                       { evnt.Payload.Questionnaire.PublicKey, surveyitem } 
                    };
                this.documentItemStore.Store(
                    new SurveyBrowseItem(
                        evnt.Payload.Questionnaire.TemplateId, 
                        evnt.Payload.Questionnaire.Title, 
                        evnt.Payload.Questionnaire.Responsible == null ? 1 : 0, 
                        statistic, 
                        1, 
                        evnt.Payload.Questionnaire.Status == SurveyStatus.Initial && evnt.Payload.Questionnaire.Responsible == null ? 1 : 0, 
                        evnt.Payload.Questionnaire.Status == SurveyStatus.Error ? 1 : 0, 
                        evnt.Payload.Questionnaire.Status == SurveyStatus.Complete ? 1 : 0,
                        evnt.Payload.Questionnaire.Status == SurveyStatus.Approve ? 1 : 0, new Dictionary<Guid, string>()), 
                    evnt.Payload.Questionnaire.PublicKey);
            }
            else
            {
                item.Total++;
                if (evnt.Payload.Questionnaire.Responsible == null) 
                    item.Unassigned++;
                else 
                    this.IncrementCount(evnt.Payload.Questionnaire.Status, item);
                item.Statistic.Add(
                    evnt.Payload.Questionnaire.PublicKey, 
                    new SurveyItem(
                        evnt.Payload.CreationDate, 
                        evnt.Payload.CreationDate, 
                        evnt.Payload.Questionnaire.PublicKey, 
                        evnt.Payload.Questionnaire.Status, 
                        evnt.Payload.Questionnaire.Responsible));
            }
        }

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
        public void Handle(IPublishedEvent<QuestionnaireStatusChanged> evnt)
        {
            var list = this.documentItemStore.Query().ToList();
            foreach (
                var item in
                    list.Where(
                        i => i.Statistic.Any(surveyItem => surveyItem.Key == evnt.Payload.CompletedQuestionnaireId)))
            {
                var val =
                    item.Statistic.Where(t => t.Key == evnt.Payload.CompletedQuestionnaireId).Select(t => t.Value).
                        FirstOrDefault();
                {
                    if (val.Responsible != null && (val.Responsible.Id != Guid.Empty))
                    {
                        this.IncrementCount(evnt.Payload.Status, item);
                        this.DecrementCount(val.Status, item);
                    }
                    val.Status = evnt.Payload.Status;
                }
            }
        }

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
        public void Handle(IPublishedEvent<QuestionnaireAssignmentChanged> evnt)
        {
            var list = this.documentItemStore.Query().ToList();
            foreach (
                var item in
                    list.Where(
                        i => i.Statistic.Any(surveyItem => surveyItem.Key == evnt.Payload.CompletedQuestionnaireId)))
            {
                var val =
                    item.Statistic.Where(t => t.Key == evnt.Payload.CompletedQuestionnaireId).Select(t => t.Value).
                        FirstOrDefault();
                if (val != null)
                {
                    if (val.Responsible == null && evnt.Payload.Responsible != null
                        && (evnt.Payload.Responsible.Id != Guid.Empty) )
                    {
                        item.Unassigned--;
                        this.IncrementCount(val.Status, item);
                        val.Responsible = evnt.Payload.Responsible;
                    }
                }
            }
        }

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
        public void Handle(IPublishedEvent<CompleteQuestionnaireDeleted> evnt)
        {
            List<SurveyBrowseItem> list = this.documentItemStore.Query().ToList();
            foreach (
                var item in
                    list.Where(
                        i => i.Statistic.Any(surveyItem => surveyItem.Key == evnt.Payload.CompletedQuestionnaireId)))
            {
                SurveyItem val =
                    item.Statistic.Where(t => t.Key == evnt.Payload.CompletedQuestionnaireId).Select(t => t.Value).
                        FirstOrDefault();
                if (val != null)
                {
                    item.Total--;
                    if (val.Responsible == null) 
                        item.Unassigned--;
                    else 
                        this.DecrementCount(val.Status, item);
                    item.Statistic.Remove(evnt.Payload.CompletedQuestionnaireId);
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// The decrement count.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="item">
        /// The item.
        /// </param>
        private void DecrementCount(SurveyStatus status, SurveyBrowseItem item)
        {
            if (status.PublicId == SurveyStatus.Error.PublicId) item.Error--;
            if (status.PublicId == SurveyStatus.Complete.PublicId) item.Completed--;
            if (status.PublicId == SurveyStatus.Approve.PublicId) item.Approve--;
            if (status.PublicId == SurveyStatus.Initial.PublicId) item.Initial--;
        }

        /// <summary>
        /// The increment count.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="item">
        /// The item.
        /// </param>
        private void IncrementCount(SurveyStatus status, SurveyBrowseItem item)
        {
            if (status.PublicId == SurveyStatus.Error.PublicId) item.Error++;
            if (status.PublicId == SurveyStatus.Complete.PublicId) item.Completed++;
            if (status.PublicId == SurveyStatus.Approve.PublicId) item.Approve++;
            if (status.PublicId == SurveyStatus.Initial.PublicId) item.Initial++;
        }

        #endregion
    }
}