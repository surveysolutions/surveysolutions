// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompleteQuestionnaireDenormalizer.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The complete questionnaire denormalizer.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Main.Core.EventHandlers
{
    using System.Linq;

    using Main.Core.Documents;
    using Main.Core.Entities.Composite;
    using Main.Core.Entities.Extensions;
    using Main.Core.Entities.SubEntities;
    using Main.Core.Entities.SubEntities.Complete;
    using Main.Core.Events.Questionnaire.Completed;
    using Main.Core.ExpressionExecutors;
    using Main.DenormalizerStorage;

    using Ncqrs.Eventing.ServiceModel.Bus;
    using Ncqrs.Restoring.EventStapshoot;

    /// <summary>
    /// The complete questionnaire denormalizer.
    /// </summary>
    public class CompleteQuestionnaireDenormalizer : IEventHandler<NewCompleteQuestionnaireCreated>,
                                                     IEventHandler<CommentSet>,
                                                     IEventHandler<FlagSet>, 
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
        private readonly IDenormalizerStorage<CompleteQuestionnaireStoreDocument> _documentStorage;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteQuestionnaireDenormalizer"/> class.
        /// </summary>
        /// <param name="documentStorage">
        /// The document storage.
        /// </param>
        public CompleteQuestionnaireDenormalizer(
            IDenormalizerStorage<CompleteQuestionnaireStoreDocument> documentStorage)
        {
            this._documentStorage = documentStorage;
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
            this._documentStorage.Store(
                (CompleteQuestionnaireStoreDocument)evnt.Payload.Questionnaire, evnt.Payload.Questionnaire.PublicKey);
        }

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="evnt">
        /// The event.
        /// </param>
        public void Handle(IPublishedEvent<CommentSet> evnt)
        {
            CompleteQuestionnaireStoreDocument item = this._documentStorage.GetById(evnt.EventSourceId);

            CompleteQuestionWrapper questionWrapper = item.GetQuestionWrapper(
                evnt.Payload.QuestionPublickey, evnt.Payload.PropagationPublicKey);
            ICompleteQuestion question = questionWrapper.Question;
            if (question == null)
            {
                return;
            }

            question.SetComments(evnt.Payload.Comments, evnt.EventTimeStamp, evnt.Payload.User);
            item.LastVisitedGroup = new VisitedGroup(questionWrapper.GroupKey, question.PropagationPublicKey);
            item.LastEntryDate = evnt.EventTimeStamp;
            this._documentStorage.Store(item, item.PublicKey);
        }

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="evnt">
        /// The event.
        /// </param>
        public void Handle(IPublishedEvent<FlagSet> evnt)
        {
            CompleteQuestionnaireStoreDocument item = this._documentStorage.GetById(evnt.EventSourceId);

            CompleteQuestionWrapper questionWrapper = item.GetQuestionWrapper(
                evnt.Payload.QuestionPublickey, evnt.Payload.PropagationPublicKey);
            ICompleteQuestion question = questionWrapper.Question;
            if (question == null)
            {
                return;
            }

            question.IsFlaged = evnt.Payload.IsFlaged;
            item.LastVisitedGroup = new VisitedGroup(questionWrapper.GroupKey, question.PropagationPublicKey);
            item.LastEntryDate = evnt.EventTimeStamp;
            this._documentStorage.Store(item, item.PublicKey);
        }

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
        public void Handle(IPublishedEvent<CompleteQuestionnaireDeleted> evnt)
        {
            this._documentStorage.Remove(evnt.Payload.CompletedQuestionnaireId);
        }

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
        public void Handle(IPublishedEvent<AnswerSet> evnt)
        {
            CompleteQuestionnaireStoreDocument item = this._documentStorage.GetById(evnt.EventSourceId);

            CompleteQuestionWrapper questionWrapper = item.GetQuestionWrapper(
                evnt.Payload.QuestionPublicKey, evnt.Payload.PropogationPublicKey);
            ICompleteQuestion question = questionWrapper.Question;
            if (question == null)
            {
                return;
            }

            question.SetAnswer(evnt.Payload.AnswerKeys, evnt.Payload.AnswerValue);
            question.AnswerDate = evnt.EventTimeStamp;

            item.LastVisitedGroup = new VisitedGroup(questionWrapper.GroupKey, question.PropagationPublicKey);
            item.LastEntryDate = evnt.EventTimeStamp;
            this._documentStorage.Store(item, item.PublicKey);
        }

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
        public void Handle(IPublishedEvent<PropagateGroupCreated> evnt)
        {
            CompleteQuestionnaireStoreDocument item = this._documentStorage.GetById(evnt.EventSourceId);
            item.Add(evnt.Payload.Group, evnt.Payload.ParentKey, evnt.Payload.ParentPropagationKey);
            this._documentStorage.Store(item, item.PublicKey);
        }

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
        public void Handle(IPublishedEvent<PropagatableGroupAdded> evnt)
        {
            CompleteQuestionnaireStoreDocument item = this._documentStorage.GetById(evnt.EventSourceId);

            CompleteGroup template =
                item.Find<CompleteGroup>(g => g.PublicKey == evnt.Payload.PublicKey && g.PropagationPublicKey == null).FirstOrDefault();

            var newGroup = new CompleteGroup(template, evnt.Payload.PropagationKey);
            item.Add(newGroup, evnt.Payload.ParentKey, evnt.Payload.ParentPropagationKey);

            item.LastEntryDate = evnt.EventTimeStamp;
            this._documentStorage.Store(item, item.PublicKey);
        }

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
        public void Handle(IPublishedEvent<PropagatableGroupDeleted> evnt)
        {
            CompleteQuestionnaireStoreDocument item = this._documentStorage.GetById(evnt.EventSourceId);

            item.Remove(evnt.Payload.PublicKey, evnt.Payload.PropagationKey, evnt.Payload.ParentKey, evnt.Payload.ParentPropagationKey);
        }

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
        public void Handle(IPublishedEvent<QuestionnaireAssignmentChanged> evnt)
        {
            CompleteQuestionnaireStoreDocument item =
                this._documentStorage.GetById(evnt.Payload.CompletedQuestionnaireId);

            item.Responsible = evnt.Payload.Responsible;
            item.LastEntryDate = evnt.EventTimeStamp;
            this._documentStorage.Store(item, item.PublicKey);
        }

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
        public void Handle(IPublishedEvent<QuestionnaireStatusChanged> evnt)
        {
            CompleteQuestionnaireStoreDocument item =
                this._documentStorage.GetById(evnt.Payload.CompletedQuestionnaireId);
            item.Status = evnt.Payload.Status;
            item.StatusChangeComments.Add(
                new ChangeStatusDocument
                    {
                        Status = evnt.Payload.Status, 
                        Responsible = evnt.Payload.Responsible, 
                        ChangeDate = evnt.EventTimeStamp
                    });
            item.LastEntryDate = evnt.EventTimeStamp;
            this._documentStorage.Store(item, item.PublicKey);
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

            this._documentStorage.Store((CompleteQuestionnaireStoreDocument)document, document.PublicKey);
        }

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
        public void Handle(IPublishedEvent<ConditionalStatusChanged> evnt)
        {
            CompleteQuestionnaireStoreDocument doc = this._documentStorage.GetById(evnt.EventSourceId);

            // to do the serching and set status. 
            foreach (var item in evnt.Payload.ResultGroupsStatus)
            {
                var group =
                    doc.Find<ICompleteGroup>(
                        q => CompleteQuestionnaireConditionExecuteCollector.GetGroupHashKey(q) == item.Key).FirstOrDefault();
                if (group != null)
                {
                    group.Enabled = item.Value != false;
                }
            }

            foreach (var item in evnt.Payload.ResultQuestionsStatus)
            {
                var question = doc.GetQuestionByKey(item.Key);
                if (question != null)
                {
                    question.Question.Enabled = item.Value != false;
                }
            }

            this._documentStorage.Store(doc, doc.PublicKey);
        }

        #endregion
    }
}