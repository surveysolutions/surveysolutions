// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompleteQuestionnaireDenormalizer.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The complete questionnaire denormalizer.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Extensions;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Complete;
using Main.Core.Events.Questionnaire.Completed;
using Main.Core.ExpressionExecutors;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Synchronization;

namespace Core.Supervisor.Denormalizer
{
    /// <summary>
    /// The complete questionnaire denormalizer.
    /// </summary>
    public class CompleteQuestionnaireDenormalizer : IEventHandler<NewCompleteQuestionnaireCreated>,
                                                     IEventHandler<CommentSet>,
                                                     IEventHandler<FlagSet>, 
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

        private readonly ISynchronizationDataStorage syncStorage;
        private readonly IReadSideRepositoryWriter<CompleteQuestionnaireStoreDocument> documentStorage;

        #endregion

        #region Constructors and Destructors

        public CompleteQuestionnaireDenormalizer(ISynchronizationDataStorage syncStorage, IReadSideRepositoryWriter<CompleteQuestionnaireStoreDocument> documentStorage)
        {
            this.syncStorage = syncStorage;
            this.documentStorage = documentStorage;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteQuestionnaireDenormalizer"/> class.
        /// </summary>
        /// <param name="documentStorage">
        /// The document storage.
        /// </param>
        public CompleteQuestionnaireDenormalizer(
            IReadSideRepositoryWriter<CompleteQuestionnaireStoreDocument> documentStorage)
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
            this.documentStorage.Store(
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
            CompleteQuestionnaireStoreDocument item = this.documentStorage.GetById(evnt.EventSourceId);

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
            this.documentStorage.Store(item, item.PublicKey);
        }

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="evnt">
        /// The event.
        /// </param>
        public void Handle(IPublishedEvent<FlagSet> evnt)
        {
            CompleteQuestionnaireStoreDocument item = this.documentStorage.GetById(evnt.EventSourceId);

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
            CompleteQuestionnaireStoreDocument item = this.documentStorage.GetById(evnt.EventSourceId);

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
            CompleteQuestionnaireStoreDocument item = this.documentStorage.GetById(evnt.EventSourceId);
            item.Add(evnt.Payload.Group, evnt.Payload.ParentKey, evnt.Payload.ParentPropagationKey);
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
            CompleteQuestionnaireStoreDocument item = this.documentStorage.GetById(evnt.EventSourceId);

            CompleteGroup template =
                item.Find<CompleteGroup>(g => g.PublicKey == evnt.Payload.PublicKey && g.PropagationPublicKey == null).FirstOrDefault();

            var newGroup = new CompleteGroup(template, evnt.Payload.PropagationKey);
            item.Add(newGroup, evnt.Payload.ParentKey, evnt.Payload.ParentPropagationKey);

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
            CompleteQuestionnaireStoreDocument item = this.documentStorage.GetById(evnt.EventSourceId);

            item.Remove(evnt.Payload.PublicKey, evnt.Payload.PropagationKey, evnt.Payload.ParentKey, evnt.Payload.ParentPropagationKey);

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
            CompleteQuestionnaireStoreDocument item =
                this.documentStorage.GetById(evnt.Payload.CompletedQuestionnaireId);

            item.Responsible = evnt.Payload.Responsible;
            item.LastEntryDate = evnt.EventTimeStamp;
            this.documentStorage.Store(item, item.PublicKey);

            syncStorage.SaveQuestionnarie(item, evnt.Payload.Responsible.Id);
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
                this.documentStorage.GetById(evnt.Payload.CompletedQuestionnaireId);
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

            if (SurveyStatus.IsStatusAllowDownSupervisorSync(evnt.Payload.Status))
                syncStorage.SaveQuestionnarie(item, evnt.Payload.Responsible.Id);
            else
                syncStorage.DeleteQuestionnarie(evnt.EventSourceId, evnt.Payload.Responsible.Id);
        }

        /// <summary>
        /// The handle.
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
        public void Handle(IPublishedEvent<ConditionalStatusChanged> evnt)
        {
            CompleteQuestionnaireStoreDocument doc = this.documentStorage.GetById(evnt.EventSourceId);

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

            this.documentStorage.Store(doc, doc.PublicKey);
        }

        #endregion
    }
}