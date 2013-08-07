using WB.Core.Infrastructure;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace Core.Supervisor.Denormalizer
{
    using System;
    using System.Security.Cryptography;
    using System.Text;

    using Core.Supervisor.DenormalizerStorageItem;

    using Main.Core.Documents;
    using Main.Core.Entities;
    using Main.Core.Entities.SubEntities;
    using Main.Core.Events.Questionnaire.Completed;
    using Main.Core.View.CompleteQuestionnaire;
    using Main.DenormalizerStorage;

    using Ncqrs.Eventing.ServiceModel.Bus;

    /// <summary>
    /// The complete questionnaire browse item denormalizer.
    /// </summary>
    
    public class SupervisorStatisticsDenormalizer : UserBaseDenormalizer,
                                                    IEventHandler<NewCompleteQuestionnaireCreated>, 
                                                    IEventHandler<QuestionnaireStatusChanged>, 
                                                    IEventHandler<QuestionnaireAssignmentChanged>,
                                                    IEventHandler<InterviewDeleted>,
        IEventHandler<InterviewMetaInfoUpdated>
    {
        private readonly IReadSideRepositoryWriter<SupervisorStatisticsItem> statistics;

        /// <summary>
        /// Information, grouped by date
        /// </summary>
        private readonly IReadSideRepositoryWriter<HistoryStatusStatistics> history;

        /// <summary>
        /// Hash of statistics key to easier find previous CQ state
        /// </summary>
        private readonly IReadSideRepositoryWriter<StatisticsItemKeysHash> keysHash;

        private readonly IReadSideRepositoryWriter<CompleteQuestionnaireBrowseItem> surveys;

        public SupervisorStatisticsDenormalizer(
            IReadSideRepositoryWriter<SupervisorStatisticsItem> statistics,
            IReadSideRepositoryWriter<CompleteQuestionnaireBrowseItem> surveys,
            IReadSideRepositoryWriter<StatisticsItemKeysHash> keysHash,
            IReadSideRepositoryWriter<HistoryStatusStatistics> history, 
            IReadSideRepositoryWriter<UserDocument> users)
             :base(users)
        {
            this.statistics = statistics;
            this.surveys = surveys;
            this.keysHash = keysHash;
            this.history = history;
        }

        #region Public Methods and Operators

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
        public void Handle(IPublishedEvent<NewCompleteQuestionnaireCreated> evnt)
        {
            this.HandleNewQuestionnaire(evnt.Payload.Questionnaire);
        }

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
        public void Handle(IPublishedEvent<QuestionnaireStatusChanged> evnt)
        {
            HandleStatusChange(evnt.EventSourceId,evnt.Payload.Status.PublicId);
        }

        private void HandleStatusChange(Guid interviewId, Guid statusId)
        {
            CompleteQuestionnaireBrowseItem doc = this.surveys.GetById(interviewId);
            if (doc == null)
            {
                return;
            }

            var userId = doc.Responsible == null ? Guid.Empty : doc.Responsible.Id;

            this.RemoveOldStatistics(doc.CompleteQuestionnaireId);

            Guid key = this.GetKey(doc.TemplateId, statusId, userId);
            SupervisorStatisticsItem item = this.statistics.GetById(key)
                                            ??
                                            new SupervisorStatisticsItem
                                                {
                                                    Template = new TemplateLight(doc.TemplateId, doc.QuestionnaireTitle),
                                                    User = doc.Responsible ?? new UserLight(Guid.Empty, string.Empty),
                                                    Status = SurveyStatus.GetStatusByIdOrDefault(statusId)
                                                };
            item.Surveys.Add(interviewId);

            this.statistics.Store(item, key);
            this.keysHash.Store(new StatisticsItemKeysHash {StorageKey = key}, doc.CompleteQuestionnaireId);
        }

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
        public void Handle(IPublishedEvent<QuestionnaireAssignmentChanged> evnt)
        {
            CompleteQuestionnaireBrowseItem doc = this.surveys.GetById(evnt.EventSourceId);
            if (doc == null)
            {
                return;
            }

            var responsible = this.FillResponsiblesName(evnt.Payload.Responsible);


            this.RemoveOldStatistics(doc.CompleteQuestionnaireId);

            Guid key = this.GetKey(doc.TemplateId, doc.Status.PublicId, responsible.Id);
            SupervisorStatisticsItem item = this.statistics.GetById(key)
                                            ??
                                            new SupervisorStatisticsItem
                                                {
                                                    Template = new TemplateLight(doc.TemplateId, doc.QuestionnaireTitle),
                                                    User = responsible, 
                                                    Status = doc.Status
                                                };

            item.Surveys.Add(evnt.EventSourceId);
            this.statistics.Store(item, key);
            this.keysHash.Store(new StatisticsItemKeysHash { StorageKey = key }, doc.CompleteQuestionnaireId);
        }

        public void Handle(IPublishedEvent<InterviewDeleted> evnt)
        {
            this.RemoveOldStatistics(evnt.EventSourceId);
            //this.keysHash.Remove(evnt.EventSourceId);
        }

        #endregion



        #region Methods


        /// <summary>
        /// Handle new questionnaire
        /// </summary>
        /// <param name="document">
        /// The document.
        /// </param>
        protected void HandleNewQuestionnaire(CompleteQuestionnaireDocument document)
        {
            
            this.RemoveOldStatistics(document.PublicKey);

            Guid key = this.GetKey(
                document.TemplateId, 
                document.Status.PublicId, 
                document.Responsible == null ? Guid.Empty : document.Responsible.Id);

            SupervisorStatisticsItem item = this.statistics.GetById(key) ?? new SupervisorStatisticsItem(document);
            item.Surveys.Add(document.PublicKey);
            this.statistics.Store(item, key);
            this.keysHash.Store(new StatisticsItemKeysHash { StorageKey = key }, document.PublicKey);
        }

        /// <summary>
        /// Generates guids to store statistics objects in denormalizer storage
        /// </summary>
        /// <param name="templateId">
        /// The template id.
        /// </param>
        /// <param name="statusId">
        /// The status id.
        /// </param>
        /// <param name="userId">
        /// The user id.
        /// </param>
        /// <returns>
        /// Guid key
        /// </returns>
        private Guid GetKey(Guid templateId, Guid statusId, Guid userId)
        {
            string key = string.Format("{0}-{1}-{2}", templateId, statusId, userId);
            return Helper.StringToGuid(key);
        }

        /// <summary>
        /// Converts date to guid
        /// </summary>
        /// <param name="date">
        /// The date.
        /// </param>
        /// <returns>
        /// The guid
        /// </returns>
        private Guid GetDateKey(DateTime date)
        {
            string key = string.Format("{0}-{1}-{2}", date.Year, date.Month, date.Day);
            return Helper.StringToGuid(key);
        }

        /// <summary>
        /// Removes CQ guid from previous statistics item
        /// </summary>
        /// <param name="completedQuestionnaireId">
        /// The completed questionnaire id.
        /// </param>
        private void RemoveOldStatistics(Guid completedQuestionnaireId)
        {
            var oldKey = this.keysHash.GetById(completedQuestionnaireId);

            if (oldKey == null)
            {
                return;
            }

            var old = this.statistics.GetById(oldKey.StorageKey);
            // temporary fix
            // in some cases old is null
            if (old == null)
            {
                return;
            }

            old.Surveys.Remove(completedQuestionnaireId);
            this.statistics.Store(old, oldKey.StorageKey);
        }
        #endregion

        public void Handle(IPublishedEvent<InterviewMetaInfoUpdated> evnt)
        {
            HandleStatusChange(evnt.EventSourceId, evnt.Payload.StatusId);
        }
    }
}