// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompleteQuestionnaireDenormalizer.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The complete questionnaire denormalizer.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Ncqrs.Restoring.EventStapshoot;
using RavenQuestionnaire.Core.ExpressionExecutors;

namespace RavenQuestionnaire.Core.EventHandlers
{
    using Ncqrs.Eventing.ServiceModel.Bus;

    using RavenQuestionnaire.Core.Denormalizers;
    using RavenQuestionnaire.Core.Documents;
    using RavenQuestionnaire.Core.Entities.Composite;
    using RavenQuestionnaire.Core.Entities.Extensions;
    using RavenQuestionnaire.Core.Entities.SubEntities;
    using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
    using RavenQuestionnaire.Core.Events;
    using RavenQuestionnaire.Core.Events.Questionnaire.Completed;

    /// <summary>
    /// The complete questionnaire denormalizer.
    /// </summary>
    public class CompleteQuestionnaireDenormalizer : IEventHandler<NewCompleteQuestionnaireCreated>, 
                                                     IEventHandler<CommentSeted>, 
        IEventHandler<SnapshootLoaded>,
                                                     IEventHandler<CompleteQuestionnaireDeleted>, 
                                                     IEventHandler<AnswerSet>, 
                                                     IEventHandler<PropagatableGroupAdded>, 
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
        /// The evnt.
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
        /// The evnt.
        /// </param>
        public void Handle(IPublishedEvent<CommentSeted> evnt)
        {
            CompleteQuestionnaireStoreDocument item =
                this._documentStorage.GetByGuid(evnt.Payload.CompleteQuestionnaireId);

            GroupHash.CompleteQuestionWrapper questionWrapper =
                item.QuestionHash.GetQuestion(evnt.Payload.QuestionPublickey, evnt.Payload.PropogationPublicKey);
            ICompleteQuestion question = questionWrapper.Question;
            if (question == null)
            {
                return;
            }

            question.SetComments(evnt.Payload.Comments);
            item.LastVisitedGroup = new VisitedGroup(questionWrapper.GroupKey, question.PropogationPublicKey);
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
            CompleteQuestionnaireStoreDocument item = this._documentStorage.GetByGuid(evnt.EventSourceId);

            GroupHash.CompleteQuestionWrapper questionWrapper =
                item.QuestionHash.GetQuestion(evnt.Payload.QuestionPublicKey, evnt.Payload.PropogationPublicKey);
            ICompleteQuestion question = questionWrapper.Question;
            if (question == null)
            {
                return;
            }

            question.SetAnswer(evnt.Payload.AnswerKeys, evnt.Payload.AnswerValue);

            var group = item.FindGroupByKey(questionWrapper.GroupKey, question.PropogationPublicKey);
            var executor = new CompleteQuestionnaireConditionExecutor(item.QuestionHash);
            executor.Execute(group);

            var validator = new CompleteQuestionnaireValidationExecutor(item.QuestionHash);
            validator.Execute(group);

            item.LastVisitedGroup = new VisitedGroup(questionWrapper.GroupKey, question.PropogationPublicKey);
        }

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
        public void Handle(IPublishedEvent<PropagatableGroupAdded> evnt)
        {
            CompleteQuestionnaireStoreDocument item =
                this._documentStorage.GetByGuid(evnt.Payload.CompletedQuestionnaireId);

            var template = item.Find<CompleteGroup>(evnt.Payload.PublicKey);

            var newGroup = new CompleteGroup(template, evnt.Payload.PropagationKey);
            item.Add(newGroup, null);
            item.QuestionHash.AddGroup(newGroup);
        }

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
        public void Handle(IPublishedEvent<PropagatableGroupDeleted> evnt)
        {
            CompleteQuestionnaireStoreDocument item =
                this._documentStorage.GetByGuid(evnt.Payload.CompletedQuestionnaireId);

            var group = new CompleteGroup(item.Find<CompleteGroup>(evnt.Payload.PublicKey), evnt.Payload.PropagationKey);
            try
            {
                item.Remove(group);
                item.QuestionHash.RemoveGroup(group);
            }
            catch (CompositeException)
            {
                // in case if group was deleted earlier
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
            CompleteQuestionnaireStoreDocument item =
                this._documentStorage.GetByGuid(evnt.Payload.CompletedQuestionnaireId);

            item.Responsible = evnt.Payload.Responsible;
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
                this._documentStorage.GetByGuid(evnt.Payload.CompletedQuestionnaireId);
            item.Status = evnt.Payload.Status;
        }

        #endregion

        #region Implementation of IEventHandler<in SnapshootLoaded>

        public void Handle(IPublishedEvent<SnapshootLoaded> evnt)
        {
            var document = evnt.Payload.Template.Payload as CompleteQuestionnaireDocument;
            if (document == null)
                return;

            this._documentStorage.Store(
                (CompleteQuestionnaireStoreDocument) document, document.PublicKey);
        }

        #endregion
    }
}