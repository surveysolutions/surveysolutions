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
        public void Handle(IPublishedEvent<CommentSet> evnt)
        {
            CompleteQuestionnaireStoreDocument item = this._documentStorage.GetByGuid(evnt.EventSourceId);

            CompleteQuestionWrapper questionWrapper = item.GetQuestionWrapper(evnt.Payload.QuestionPublickey, evnt.Payload.PropagationPublicKey);
            ICompleteQuestion question = questionWrapper.Question;
            if (question == null)
            {
                return;
            }

            question.SetComments(evnt.Payload.Comments);
            item.LastVisitedGroup = new VisitedGroup(questionWrapper.GroupKey, question.PropagationPublicKey);
            item.LastEntryDate = evnt.EventTimeStamp;
            this._documentStorage.Store(item,item.PublicKey);
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

            CompleteQuestionWrapper questionWrapper = item.GetQuestionWrapper(evnt.Payload.QuestionPublicKey, evnt.Payload.PropogationPublicKey);
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

        #region Implementation of IEventHandler<in PropagateGroupCreated>

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
        public void Handle(IPublishedEvent<PropagateGroupCreated> evnt)
        {
            CompleteQuestionnaireStoreDocument item = this._documentStorage.GetByGuid(evnt.EventSourceId);


            var parentToAdd =
                item.FirstOrDefault<CompleteGroup>(
                    g => g.PublicKey == evnt.Payload.ParentPublicKey && g.PropagationPublicKey == evnt.Payload.ParentPropagationKey);

            if (parentToAdd == null)
            {
                return; ////is it good or exception is better decision?
            }

            parentToAdd.Add(evnt.Payload.Group, null);
            item.QuestionHash.AddGroup(evnt.Payload.Group);
        }

        #endregion

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
        public void Handle(IPublishedEvent<PropagatableGroupAdded> evnt)
        {
            CompleteQuestionnaireStoreDocument item = this._documentStorage.GetByGuid(evnt.EventSourceId);

            var template = item.Find<CompleteGroup>(evnt.Payload.PublicKey);

            var newGroup = new CompleteGroup(template, evnt.Payload.PropagationKey);
            item.Add(newGroup, null);
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
            CompleteQuestionnaireStoreDocument item = this._documentStorage.GetByGuid(evnt.Payload.CompletedQuestionnaireId);

            var group = new CompleteGroup(item.Find<CompleteGroup>(evnt.Payload.PublicKey), evnt.Payload.PropagationKey);
            try
            {
                item.Remove(group);
                item.LastEntryDate = evnt.EventTimeStamp;
                this._documentStorage.Store(item, item.PublicKey);
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
            CompleteQuestionnaireStoreDocument item = this._documentStorage.GetByGuid(evnt.Payload.CompletedQuestionnaireId);

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
            CompleteQuestionnaireStoreDocument item = this._documentStorage.GetByGuid(evnt.Payload.CompletedQuestionnaireId);
            item.Status = evnt.Payload.Status;
            item.StatusChangeComments.Add(new ChangeStatusDocument
                {
                    Status = evnt.Payload.Status, 
                    Responsible = evnt.Payload.Responsible,
                    ChangeDate = evnt.EventTimeStamp
                });
            item.LastEntryDate = evnt.EventTimeStamp;
            this._documentStorage.Store(item, item.PublicKey);
        }

        #endregion

        #region Implementation of IEventHandler<in SnapshootLoaded>


        public void Handle(IPublishedEvent<SnapshootLoaded> evnt)
        {
            var document = evnt.Payload.Template.Payload as CompleteQuestionnaireDocument;
            if (document == null)
            {
                return;
            }

            this._documentStorage.Store((CompleteQuestionnaireStoreDocument)document, document.PublicKey);
        }

        #endregion

        #region Implementation of IEventHandler<in ConditionalStatusChanged>

        public void Handle(IPublishedEvent<ConditionalStatusChanged> evnt)
        {
            CompleteQuestionnaireStoreDocument doc = this._documentStorage.GetByGuid(evnt.EventSourceId);

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
                var question = doc.QuestionHash.GetQuestionByKey(item.Key);
                if (question != null)
                {
                    question.Question.Enabled = item.Value != false;
                }
            }
        }

        #endregion
    }
}