// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SupervisorStatisticsDenormalizer.cs" company="The World Bank">
//   Complete Questionnaire Browse Item Denormalizer
// </copyright>
// <summary>
//   The complete questionnaire browse item denormalizer.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

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
    using Ncqrs.Restoring.EventStapshoot;

    /// <summary>
    /// The complete questionnaire browse item denormalizer.
    /// </summary>
    public class SupervisorStatisticsDenormalizer : IEventHandler<NewCompleteQuestionnaireCreated>, 
                                                    IEventHandler<QuestionnaireStatusChanged>, 
                                                    IEventHandler<QuestionnaireAssignmentChanged>, 
                                                    IEventHandler<SnapshootLoaded>
    {
        #region Constants and Fields

        /// <summary>
        /// The document item store.
        /// </summary>
        private readonly IDenormalizerStorage<SupervisorStatisticsItem> statistics;

        /// <summary>
        /// Information, grouped by date
        /// </summary>
        private readonly IDenormalizerStorage<HistoryStatusStatistics> history;

        /// <summary>
        /// Hash of statistics key to easier find previous CQ state
        /// </summary>
        private readonly IDenormalizerStorage<StatisticsItemKeysHash> keysHash;

        /// <summary>
        /// The document item store.
        /// </summary>
        private readonly IDenormalizerStorage<CompleteQuestionnaireBrowseItem> surveys;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SupervisorStatisticsDenormalizer"/> class. 
        /// </summary>
        /// <param name="statistics">
        /// The document item store.
        /// </param>
        /// <param name="surveys">
        /// The surveys.
        /// </param>
        /// <param name="keysHash">
        /// Statistics storage hash
        /// </param>
        /// <param name="history">
        /// The history.
        /// </param>
        public SupervisorStatisticsDenormalizer(
            IDenormalizerStorage<SupervisorStatisticsItem> statistics, 
            IDenormalizerStorage<CompleteQuestionnaireBrowseItem> surveys, 
            IDenormalizerStorage<StatisticsItemKeysHash> keysHash, 
            IDenormalizerStorage<HistoryStatusStatistics> history)
        {
            this.statistics = statistics;
            this.surveys = surveys;
            this.keysHash = keysHash;
            this.history = history;
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
            this.HandleNewQuestionnaire(evnt.Payload.Questionnaire);
        }

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
        public void Handle(IPublishedEvent<SnapshootLoaded> evnt)
        {
            var document = evnt.Payload.Template.Payload as CompleteQuestionnaireDocument;
            if (document == null)
            {
                return;
            }

            this.HandleStatusChanges(SurveyStatus.Unknown, document.Status, evnt.EventTimeStamp, document.PublicKey);
            
            this.HandleNewQuestionnaire(document);
        }

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
        public void Handle(IPublishedEvent<QuestionnaireStatusChanged> evnt)
        {
            this.HandleStatusChanges(evnt.Payload.PreviousStatus, evnt.Payload.Status, evnt.EventTimeStamp, evnt.Payload.CompletedQuestionnaireId);
            
            CompleteQuestionnaireBrowseItem doc = this.surveys.GetByGuid(evnt.Payload.CompletedQuestionnaireId);
            if (doc == null)
            {
                return;
            }

            var userId = doc.Responsible == null ? Guid.Empty : doc.Responsible.Id;

            this.RemoveOldStatistics(doc.CompleteQuestionnaireId);

            Guid key = this.GetKey(doc.TemplateId, evnt.Payload.Status.PublicId, userId);
            SupervisorStatisticsItem item = this.statistics.GetByGuid(key)
                                            ??
                                            new SupervisorStatisticsItem
                                                {
                                                    Template = new TemplateLight(doc.TemplateId, doc.QuestionnaireTitle), 
                                                    User = doc.Responsible ?? new UserLight(Guid.Empty, string.Empty), 
                                                    Status = evnt.Payload.Status
                                                };
            item.Surveys.Add(evnt.Payload.CompletedQuestionnaireId);
           
            this.statistics.Store(item, key);
            this.keysHash.Store(new StatisticsItemKeysHash { StorageKey = key }, doc.CompleteQuestionnaireId);
        }

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
        public void Handle(IPublishedEvent<QuestionnaireAssignmentChanged> evnt)
        {
            CompleteQuestionnaireBrowseItem doc = this.surveys.GetByGuid(evnt.Payload.CompletedQuestionnaireId);
            if (doc == null)
            {
                return;
            }

            this.RemoveOldStatistics(doc.CompleteQuestionnaireId);

            Guid key = this.GetKey(doc.TemplateId, doc.Status.PublicId, evnt.Payload.Responsible.Id);
            SupervisorStatisticsItem item = this.statistics.GetByGuid(key)
                                            ??
                                            new SupervisorStatisticsItem
                                                {
                                                    Template = new TemplateLight(doc.TemplateId, doc.QuestionnaireTitle),
                                                    User = evnt.Payload.Responsible, 
                                                    Status = doc.Status
                                                };

            item.Surveys.Add(evnt.Payload.CompletedQuestionnaireId);
            this.statistics.Store(item, key);
            this.keysHash.Store(new StatisticsItemKeysHash { StorageKey = key }, doc.CompleteQuestionnaireId);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Handle status changes of CQ in time
        /// </summary>
        /// <param name="prev">
        /// The prev.
        /// </param>
        /// <param name="next">
        /// The next.
        /// </param>
        /// <param name="date">
        /// The date.
        /// </param>
        /// <param name="cqId">
        /// The cq id.
        /// </param>
        protected void HandleStatusChanges(SurveyStatus prev, SurveyStatus next, DateTime date, Guid cqId)
        {
            var key = this.GetDateKey(date);
            var historyItem = this.history.GetByGuid(key) ?? new HistoryStatusStatistics(date);

            if (prev != SurveyStatus.Unknown && prev.PublicId != Guid.Empty)
            {
                historyItem.Remove(prev, cqId);
            }

            historyItem.Add(next,cqId);

            this.history.Store(historyItem, key);
        }

        /// <summary>
        /// Handle new questionnaire
        /// </summary>
        /// <param name="document">
        /// The document.
        /// </param>
        protected void HandleNewQuestionnaire(CompleteQuestionnaireDocument document)
        {
            this.HandleStatusChanges(SurveyStatus.Unknown, document.Status, document.CreationDate, document.PublicKey);
            
            this.RemoveOldStatistics(document.PublicKey);

            Guid key = this.GetKey(
                document.TemplateId, 
                document.Status.PublicId, 
                document.Responsible == null ? Guid.Empty : document.Responsible.Id);

            SupervisorStatisticsItem item = this.statistics.GetByGuid(key) ?? new SupervisorStatisticsItem(document);
            item.Surveys.Add(document.PublicKey);
            this.statistics.Store(item, key);
            this.keysHash.Store(new StatisticsItemKeysHash { StorageKey = key }, document.PublicKey);
        }

        /// <summary>
        /// Converts string into the md5 hash and coverts bytes to Guid
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <returns>
        /// The guid
        /// </returns>
        private static Guid StringToGuid(string key)
        {
            MD5 md5Hasher = MD5.Create();
            byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(key));
            return new Guid(data);
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
            return StringToGuid(key);
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
            return StringToGuid(key);
        }

        /// <summary>
        /// Removes CQ guid from previous statistics item
        /// </summary>
        /// <param name="completedQuestionnaireId">
        /// The completed questionnaire id.
        /// </param>
        private void RemoveOldStatistics(Guid completedQuestionnaireId)
        {
            var oldKey = this.keysHash.GetByGuid(completedQuestionnaireId);

            if (oldKey == null)
            {
                return;
            }

            var old = this.statistics.GetByGuid(oldKey.StorageKey);
            old.Surveys.Remove(completedQuestionnaireId);
            this.statistics.Store(old, oldKey.StorageKey);
        }
        #endregion
    }
}