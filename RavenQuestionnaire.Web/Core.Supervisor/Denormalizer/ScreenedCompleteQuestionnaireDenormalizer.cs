// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompleteQuestionnaireScreenedDenormalizer.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The complete questionnaire denormalizer.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Core.Supervisor.Denormalizer
{
    using Core.Supervisor.Documents;

    using Main.Core.Documents;
    using Main.Core.Events.Questionnaire.Completed;
    using Main.DenormalizerStorage;

    using Ncqrs.Eventing.ServiceModel.Bus;
    using Ncqrs.Restoring.EventStapshoot;

    /// <summary>
    /// The complete questionnaire denormalizer.
    /// </summary>
    public class ScreenedCompleteQuestionnaireDenormalizer : IEventHandler<NewCompleteQuestionnaireCreated>, 
                                                             IEventHandler<CommentSet>, 
                                                             IEventHandler<SnapshootLoaded>, 
                                                             IEventHandler<CompleteQuestionnaireDeleted>, 
                                                             IEventHandler<AnswerSet>, 
                                                             IEventHandler<ConditionalStatusChanged>, 
                                                             IEventHandler<PropagatableGroupAdded>, 
                                                             IEventHandler<PropagateGroupCreated>, 
                                                             IEventHandler<PropagatableGroupDeleted>, 
                                                             IEventHandler<QuestionnaireAssignmentChanged>, 
                                                             IEventHandler<QuestionnaireStatusChanged>
    {
        #region Fields

        /// <summary>
        /// The _document storage.
        /// </summary>
        private readonly IDenormalizerStorage<ScreenedCompleteQuestionnaireDocument> documentStorage;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ScreenedCompleteQuestionnaireDenormalizer"/> class.
        /// </summary>
        /// <param name="documentStorage">
        /// The document storage.
        /// </param>
        public ScreenedCompleteQuestionnaireDenormalizer(
            IDenormalizerStorage<ScreenedCompleteQuestionnaireDocument> documentStorage)
        {
            this.documentStorage = documentStorage;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="evnt">
        /// The event.
        /// </param>
        public void Handle(IPublishedEvent<NewCompleteQuestionnaireCreated> evnt)
        {
            this.documentStorage.Store((ScreenedCompleteQuestionnaireDocument)evnt.Payload.Questionnaire, evnt.Payload.Questionnaire.PublicKey);
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

            this.documentStorage.Store((ScreenedCompleteQuestionnaireDocument)document, document.PublicKey);
        }

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="evnt">
        /// The event.
        /// </param>
        public void Handle(IPublishedEvent<CommentSet> evnt)
        {
            ScreenedCompleteQuestionnaireDocument item = this.documentStorage.GetByGuid(evnt.EventSourceId);

            item.LastEntryDate = evnt.EventTimeStamp;
            this.documentStorage.Store(item, item.PublicKey);
        }

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
        public void Handle(IPublishedEvent<CompleteQuestionnaireDeleted> evnt)
        {
            this.documentStorage.Remove(evnt.Payload.CompletedQuestionnaireId);
        }

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
        public void Handle(IPublishedEvent<AnswerSet> evnt)
        {
            ScreenedCompleteQuestionnaireDocument item = this.documentStorage.GetByGuid(evnt.EventSourceId);

            item.LastEntryDate = evnt.EventTimeStamp;
            this.documentStorage.Store(item, item.PublicKey);
        }

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
        public void Handle(IPublishedEvent<PropagateGroupCreated> evnt)
        {
            ScreenedCompleteQuestionnaireDocument item = this.documentStorage.GetByGuid(evnt.EventSourceId);

            item.LastEntryDate = evnt.EventTimeStamp;
            this.documentStorage.Store(item, item.PublicKey);
        }

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
        public void Handle(IPublishedEvent<PropagatableGroupAdded> evnt)
        {
            ScreenedCompleteQuestionnaireDocument item = this.documentStorage.GetByGuid(evnt.EventSourceId);

            item.LastEntryDate = evnt.EventTimeStamp;
            this.documentStorage.Store(item, item.PublicKey);
        }

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
        public void Handle(IPublishedEvent<PropagatableGroupDeleted> evnt)
        {
            ScreenedCompleteQuestionnaireDocument item = this.documentStorage.GetByGuid(evnt.EventSourceId);

            item.LastEntryDate = evnt.EventTimeStamp;
            this.documentStorage.Store(item, item.PublicKey);
        }

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
        public void Handle(IPublishedEvent<QuestionnaireAssignmentChanged> evnt)
        {
            ScreenedCompleteQuestionnaireDocument item = this.documentStorage.GetByGuid(evnt.Payload.CompletedQuestionnaireId);
            item.Responsible = evnt.Payload.Responsible;
            item.LastEntryDate = evnt.EventTimeStamp;
            this.documentStorage.Store(item, item.PublicKey);
        }

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
        public void Handle(IPublishedEvent<QuestionnaireStatusChanged> evnt)
        {
            ScreenedCompleteQuestionnaireDocument item = this.documentStorage.GetByGuid(evnt.Payload.CompletedQuestionnaireId);
            item.Status = evnt.Payload.Status;
            item.StatusChangeComments.Add(
                new ChangeStatusDocument
                    {
                        Status = evnt.Payload.Status, 
                        Responsible = evnt.Payload.Responsible, 
                        ChangeDate = evnt.EventTimeStamp
                    });
            item.LastEntryDate = evnt.EventTimeStamp;
            this.documentStorage.Store(item, item.PublicKey);
        }

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
        public void Handle(IPublishedEvent<ConditionalStatusChanged> evnt)
        {
            ScreenedCompleteQuestionnaireDocument item = this.documentStorage.GetByGuid(evnt.EventSourceId);

            item.LastEntryDate = evnt.EventTimeStamp;
            this.documentStorage.Store(item, item.PublicKey);
        }

        #endregion
    }
}