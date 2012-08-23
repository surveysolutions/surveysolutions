using Ncqrs.Eventing.ServiceModel.Bus;
using RavenQuestionnaire.Core.Denormalizers;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.Composite;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.Events;
using RavenQuestionnaire.Core.Events.Questionnaire.Completed;

namespace RavenQuestionnaire.Core.EventHandlers
{
    public class CompleteQuestionnaireDenormalizer : 
        IEventHandler<NewCompleteQuestionnaireCreated>,
        IEventHandler<CommentSeted>, 
        IEventHandler<CompleteQuestionnaireDeleted>, 
        IEventHandler<AnswerSet>,
        IEventHandler<PropagatableGroupAdded>,
        IEventHandler<PropagatableGroupDeleted>,
        IEventHandler<QuestionnaireAssignmentChanged>,
        IEventHandler<QuestionnaireStatusChanged>
    {
        private IDenormalizerStorage<CompleteQuestionnaireDocument> _documentStorage;

        public CompleteQuestionnaireDenormalizer(IDenormalizerStorage<CompleteQuestionnaireDocument> documentStorage)
        {
            this._documentStorage = documentStorage;
        }

        #region Implementation of IEventHandler<in NewCompleteQuestionnaireCreated>

        public void Handle(IPublishedEvent<NewCompleteQuestionnaireCreated> evnt)
        {
            this._documentStorage.Store(evnt.Payload.Questionnaire, evnt.Payload.CompletedQuestionnaireId);
        }

        #endregion

        #region Implementation of IEventHandler<in CommentSeted>

        public void Handle(IPublishedEvent<CommentSeted> evnt)
        {
            var item = this._documentStorage.GetByGuid(evnt.Payload.CompleteQuestionnaireId);

            var questionWrapper = item.QuestionHash.GetQuestion(evnt.Payload.QuestionPublickey, evnt.Payload.PropogationPublicKey);
            ICompleteQuestion question = questionWrapper.Question;
            if (question == null)
                return;

            question.SetComments(evnt.Payload.Comments);
            item.LastVisitedGroup = new VisitedGroup(questionWrapper.GroupKey, question.PropogationPublicKey);
        }

        #endregion

        #region Implementation of IEventHandler<in CompleteQuestionnaireDeleted>

        public void Handle(IPublishedEvent<CompleteQuestionnaireDeleted> evnt)
        {
            this._documentStorage.Remove(evnt.Payload.CompletedQuestionnaireId);
        }

        #endregion

        #region Implementation of IEventHandler<in AnswerSet>

        public void Handle(IPublishedEvent<AnswerSet> evnt)
        {
            var item = this._documentStorage.GetByGuid(evnt.Payload.CompletedQuestionnaireId);

            var questionWrapper = item.QuestionHash.GetQuestion(evnt.Payload.QuestionPublicKey, evnt.Payload.PropogationPublicKey);
            ICompleteQuestion question = questionWrapper.Question;
            if (question == null)
                return;
            question.SetAnswer(evnt.Payload.Answer);

            item.LastVisitedGroup = new VisitedGroup(questionWrapper.GroupKey, question.PropogationPublicKey);
        }

        #endregion

        #region Implementation of IEventHandler<in PropagatableGroupAdded>

        public void Handle(IPublishedEvent<PropagatableGroupAdded> evnt)
        {
            var item = this._documentStorage.GetByGuid(evnt.Payload.CompletedQuestionnaireId);

            var template = item.Find<CompleteGroup>(evnt.Payload.PublicKey);

            var newGroup = new CompleteGroup(template, evnt.Payload.PropagationKey);
            item.Add(newGroup, null);
            item.QuestionHash.AddGroup(newGroup);
        }

        #endregion

        #region Implementation of IEventHandler<in PropagatableGroupDeleted>

        public void Handle(IPublishedEvent<PropagatableGroupDeleted> evnt)
        {
            var item = this._documentStorage.GetByGuid(evnt.Payload.CompletedQuestionnaireId);

            var group = new CompleteGroup(item.Find<CompleteGroup>(evnt.Payload.PublicKey), evnt.Payload.PropagationKey);
            try
            {
                item.Remove(group);
                item.QuestionHash.RemoveGroup(group);
            }
            catch (CompositeException)
            {
                //in case if group was deleted earlier
            }
        }

        #endregion

        #region Implementation of IEventHandler<in QuestionnaireAssignmentChanged>

        public void Handle(IPublishedEvent<QuestionnaireAssignmentChanged> evnt)
        {
            var item = this._documentStorage.GetByGuid(evnt.Payload.CompletedQuestionnaireId);
            
            item.Responsible = evnt.Payload.Responsible;
        }

        #endregion

        #region Implementation of IEventHandler<in QuestionnaireStatusChanged>

        public void Handle(IPublishedEvent<QuestionnaireStatusChanged> evnt)
        {
            var item = this._documentStorage.GetByGuid(evnt.Payload.CompletedQuestionnaireId);
            item.Status = evnt.Payload.Status;
        }

        #endregion
    }
}
