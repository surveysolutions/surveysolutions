// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InterviewerStatisticsDenormalizer.cs" company="The World Bank">
//   2012
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.EventHandlers
{
    using Ncqrs.Eventing.ServiceModel.Bus;

    using RavenQuestionnaire.Core.Denormalizers;
    using RavenQuestionnaire.Core.Documents;
    using RavenQuestionnaire.Core.Events.Questionnaire.Completed;
    using RavenQuestionnaire.Core.Views.Interviewer;

    /// <summary>
    /// The CQ statistic grouped by user.
    /// </summary>
    public class InterviewerStatisticsDenormalizer : IEventHandler<CompleteQuestionnaireDeleted>, 
                                                     IEventHandler<QuestionnaireStatusChanged>, 
                                                     IEventHandler<QuestionnaireAssignmentChanged>
    {
        #region Constants and Fields

        /// <summary>
        /// Complete questionnaire store
        /// </summary>
        private readonly IDenormalizerStorage<CompleteQuestionnaireStoreDocument> completeDocumentItemStore;

        /// <summary>
        /// The document item store.
        /// </summary>
        private readonly IDenormalizerStorage<InterviewerStatisticsItem> documentItemStore;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="InterviewerStatisticsDenormalizer"/> class. 
        /// </summary>
        /// <param name="documentItemStore">
        /// The document item store.
        /// </param>
        /// <param name="completeDocumentItemStore">
        /// The complete Document Item Store.
        /// </param>
        public InterviewerStatisticsDenormalizer(
            IDenormalizerStorage<InterviewerStatisticsItem> documentItemStore, 
            IDenormalizerStorage<CompleteQuestionnaireStoreDocument> completeDocumentItemStore)
        {
            this.documentItemStore = documentItemStore;
            this.completeDocumentItemStore = completeDocumentItemStore;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
        public void Handle(IPublishedEvent<QuestionnaireStatusChanged> evnt)
        {
            CompleteQuestionnaireStoreDocument cq =
                this.completeDocumentItemStore.GetByGuid(evnt.Payload.CompletedQuestionnaireId);
            InterviewerStatisticsItem item = this.documentItemStore.GetByGuid(cq.Responsible.Id);
            item.AddCQ(evnt.Payload.CompletedQuestionnaireId, cq.TemplateId, evnt.Payload.Status);
        }

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
        public void Handle(IPublishedEvent<QuestionnaireAssignmentChanged> evnt)
        {
            CompleteQuestionnaireStoreDocument cq =
                this.completeDocumentItemStore.GetByGuid(evnt.Payload.CompletedQuestionnaireId);
            InterviewerStatisticsItem item = this.documentItemStore.GetByGuid(evnt.Payload.Responsible.Id);
            if (evnt.Payload.PreviousResponsible == null && item == null)
            {
                item = new InterviewerStatisticsItem
                    {
                       Id = evnt.Payload.Responsible.Id, Name = evnt.Payload.Responsible.Name 
                    };
                this.documentItemStore.Store(item, item.Id);
            }

            if (evnt.Payload.PreviousResponsible != null
                && evnt.Payload.Responsible.Id != evnt.Payload.PreviousResponsible.Id)
            {
                InterviewerStatisticsItem pitem = this.documentItemStore.GetByGuid(evnt.Payload.PreviousResponsible.Id);
                pitem.RemoveCQ(cq.PublicKey);
            }

            item.AddCQ(cq.PublicKey, cq.TemplateId, cq.Status);
        }

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
        public void Handle(IPublishedEvent<CompleteQuestionnaireDeleted> evnt)
        {
            CompleteQuestionnaireStoreDocument cq =
                this.completeDocumentItemStore.GetByGuid(evnt.Payload.CompletedQuestionnaireId);
            if (cq == null)
            {
                foreach (InterviewerStatisticsItem item in this.documentItemStore.Query())
                {
                    item.RemoveCQ(evnt.Payload.CompletedQuestionnaireId);
                }
            }
            else
            {
                InterviewerStatisticsItem item = this.documentItemStore.GetByGuid(cq.Responsible.Id);
                item.RemoveCQ(evnt.Payload.CompletedQuestionnaireId);
            }
        }

        #endregion
    }
}