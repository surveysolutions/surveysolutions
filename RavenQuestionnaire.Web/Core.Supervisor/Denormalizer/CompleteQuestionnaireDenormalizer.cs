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
    public class CompleteQuestionnaireDenormalizer : UserBaseDenormalizer,
                                                     IEventHandler<NewCompleteQuestionnaireCreated>,
                                                     IEventHandler<CommentSet>,
                                                     IEventHandler<FlagSet>,
                                                     IEventHandler<AnswerSet>, 
                                                     IEventHandler<ConditionalStatusChanged>, 
                                                     IEventHandler<PropagatableGroupAdded>, 
                                                     IEventHandler<PropagateGroupCreated>, 
                                                     IEventHandler<PropagatableGroupDeleted>, 
                                                     IEventHandler<QuestionnaireAssignmentChanged>, 
                                                     IEventHandler<QuestionnaireStatusChanged>
    {
        private readonly ISynchronizationDataStorage syncStorage;
        private readonly IReadSideRepositoryWriter<CompleteQuestionnaireStoreDocument> documentStorage;

        public CompleteQuestionnaireDenormalizer(ISynchronizationDataStorage syncStorage, 
            IReadSideRepositoryWriter<CompleteQuestionnaireStoreDocument> documentStorage,
            IReadSideRepositoryWriter<UserDocument> users)
            :base(users)
        {
            this.syncStorage = syncStorage;
            this.documentStorage = documentStorage;
        }

        public CompleteQuestionnaireDenormalizer(
            IReadSideRepositoryWriter<CompleteQuestionnaireStoreDocument> documentStorage, 
            IReadSideRepositoryWriter<UserDocument> users)
            : base(users)
        {
            this.documentStorage = documentStorage;
        }

        public void Handle(IPublishedEvent<NewCompleteQuestionnaireCreated> evnt)
        {
            this.documentStorage.Store(
                (CompleteQuestionnaireStoreDocument)evnt.Payload.Questionnaire, evnt.Payload.Questionnaire.PublicKey);
        }

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

        public void Handle(IPublishedEvent<PropagateGroupCreated> evnt)
        {
            CompleteQuestionnaireStoreDocument item = this.documentStorage.GetById(evnt.EventSourceId);
            item.Add(evnt.Payload.Group, evnt.Payload.ParentKey, evnt.Payload.ParentPropagationKey);
            this.documentStorage.Store(item, item.PublicKey);
        }

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

        public void Handle(IPublishedEvent<PropagatableGroupDeleted> evnt)
        {
            CompleteQuestionnaireStoreDocument item = this.documentStorage.GetById(evnt.EventSourceId);

            item.Remove(evnt.Payload.PublicKey, evnt.Payload.PropagationKey, evnt.Payload.ParentKey, evnt.Payload.ParentPropagationKey);

            this.documentStorage.Store(item, item.PublicKey);
        }

        public void Handle(IPublishedEvent<QuestionnaireAssignmentChanged> evnt)
        {
            var responsible = this.FillResponsiblesName(evnt.Payload.Responsible);

            CompleteQuestionnaireStoreDocument item =
                this.documentStorage.GetById(evnt.EventSourceId);

            item.Responsible = responsible;
            item.LastEntryDate = evnt.EventTimeStamp;
            this.documentStorage.Store(item, item.PublicKey);

            syncStorage.SaveInterview(item, evnt.Payload.Responsible.Id);
        }

        public void Handle(IPublishedEvent<QuestionnaireStatusChanged> evnt)
        {
            CompleteQuestionnaireStoreDocument item =
                this.documentStorage.GetById(evnt.EventSourceId);
            item.Status = evnt.Payload.Status;
            item.StatusChangeComments.Add(
                new ChangeStatusDocument
                    {
                        Status = evnt.Payload.Status,
                        Responsible = item.Responsible, 
                        ChangeDate = evnt.EventTimeStamp
                    });
            item.LastEntryDate = evnt.EventTimeStamp;
            this.documentStorage.Store(item, item.PublicKey);

            if (SurveyStatus.IsStatusAllowDownSupervisorSync(evnt.Payload.Status))
                syncStorage.SaveInterview(item, item.Responsible.Id);
            else
                syncStorage.MarkInterviewForClientDeleting(evnt.EventSourceId, item.Responsible.Id);

            //when deleting logic is implemented call syncStorage.DeleteInterview(id) in apropriate place    

        }

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
    }
}