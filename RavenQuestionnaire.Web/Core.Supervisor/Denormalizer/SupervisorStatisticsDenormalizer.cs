// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompleteQuestionnaireBrowseItemDenormalizer.cs" company="">
//   
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
        private readonly IDenormalizerStorage<SupervisorStatisticsItem> documentItemStore;

        /// <summary>
        /// The document item store.
        /// </summary>
        private readonly IDenormalizerStorage<CompleteQuestionnaireBrowseItem> surveys;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SupervisorStatisticsDenormalizer"/> class. 
        /// </summary>
        /// <param name="documentItemStore">
        /// The document item store.
        /// </param>
        /// <param name="surveys">
        /// The surveys.
        /// </param>
        public SupervisorStatisticsDenormalizer(
            IDenormalizerStorage<SupervisorStatisticsItem> documentItemStore, 
            IDenormalizerStorage<CompleteQuestionnaireBrowseItem> surveys)
        {
            this.documentItemStore = documentItemStore;
            this.surveys = surveys;
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
            CompleteQuestionnaireBrowseItem doc = this.surveys.GetByGuid(evnt.Payload.CompletedQuestionnaireId);
            if (doc == null)
            {
                return;
            }
            var userId = doc.Responsible == null ? Guid.Empty : doc.Responsible.Id;

            Guid oldKey = this.GetKey(doc.TemplateId, evnt.Payload.PreviousStatus.PublicId, userId);
            SupervisorStatisticsItem old = this.documentItemStore.GetByGuid(oldKey);
            if (old != null)
            {
                old.Surveys.Remove(evnt.Payload.CompletedQuestionnaireId);
                this.documentItemStore.Store(old, oldKey);
            }

            Guid key = this.GetKey(doc.TemplateId, evnt.Payload.Status.PublicId, userId);
            SupervisorStatisticsItem item = this.documentItemStore.GetByGuid(key)
                                            ??
                                            new SupervisorStatisticsItem
                                                {
                                                    Template = new TemplateLight(doc.TemplateId, doc.QuestionnaireTitle), 
                                                    User = doc.Responsible, 
                                                    Status = evnt.Payload.Status
                                                };
            item.Surveys.Add(evnt.Payload.CompletedQuestionnaireId);
            this.documentItemStore.Store(item, key);
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

            Guid oldKey = this.GetKey(
                doc.TemplateId,
                doc.Status.PublicId,
                evnt.Payload.PreviousResponsible == null ? Guid.Empty : evnt.Payload.PreviousResponsible.Id);
            SupervisorStatisticsItem old = this.documentItemStore.GetByGuid(oldKey);
            if (old != null)
            {
                old.Surveys.Remove(evnt.Payload.CompletedQuestionnaireId);
                this.documentItemStore.Store(old, oldKey);
            }

            Guid key = this.GetKey(doc.TemplateId, doc.Status.PublicId, evnt.Payload.Responsible.Id);
            SupervisorStatisticsItem item = this.documentItemStore.GetByGuid(key)
                                            ??
                                            new SupervisorStatisticsItem
                                                {
                                                    Template = new TemplateLight(doc.TemplateId, doc.QuestionnaireTitle), 
                                                    User = evnt.Payload.Responsible, 
                                                    Status = doc.Status
                                                };
            item.Surveys.Add(evnt.Payload.CompletedQuestionnaireId);
            this.documentItemStore.Store(item, key);
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
            var browseItem = new SupervisorStatisticsItem(document);

            Guid key = this.GetKey(
                document.TemplateId, 
                document.Status.PublicId, 
                document.Responsible == null ? Guid.Empty : document.Responsible.Id);

            this.documentItemStore.Store(browseItem, key);
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
            MD5 md5Hasher = MD5.Create();
            byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(key));
            return new Guid(data);
        }

        #endregion
    }
}