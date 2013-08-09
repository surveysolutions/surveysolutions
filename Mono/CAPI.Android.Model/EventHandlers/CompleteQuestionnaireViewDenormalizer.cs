using System;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;
using Main.Core.Events.Questionnaire.Completed;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace CAPI.Android.Core.Model.EventHandlers
{
    public class CompleteQuestionnaireViewDenormalizer : IEventHandler<NewAssigmentCreated>, 
                                                         IEventHandler<CommentSet>,
                                                         IEventHandler<PropagatableGroupAdded>,
                                                         IEventHandler<PropagatableGroupDeleted>,
                                                         IEventHandler<QuestionnaireStatusChanged>,
        IEventHandler<CommentAnswer>, 
        IEventHandler<InterviewSynchronized>, 
        IEventHandler<MultipleOptionsQuestionAnswered>
        , IEventHandler<NumericQuestionAnswered>
        , IEventHandler<TextQuestionAnswered>
        , IEventHandler<SingleOptionQuestionAnswered>
        , IEventHandler<DateTimeQuestionAnswered>
         , IEventHandler<GroupDisabled>
         , IEventHandler<GroupEnabled>
         , IEventHandler<QuestionDisabled>
         , IEventHandler<QuestionEnabled>
         , IEventHandler<AnswerDeclaredInvalid>
         , IEventHandler<AnswerDeclaredValid>
    {
        private readonly IReadSideRepositoryWriter<CompleteQuestionnaireView> _documentStorage;

        public CompleteQuestionnaireViewDenormalizer(IReadSideRepositoryWriter<CompleteQuestionnaireView> documentStorage)
        {
            _documentStorage = documentStorage;
        }

        private CompleteQuestionnaireView GetStoredObject(Guid publicKey)
        {
            var doc = _documentStorage.GetById(publicKey);
            return doc;
        }

        #region Implementation of IEventHandler<in NewCompleteQuestionnaireCreated>

        public void Handle(IPublishedEvent<NewAssigmentCreated> evnt)
        {
            var document = evnt.Payload.Source;
            
            var view = new CompleteQuestionnaireView(document);

            _documentStorage.Store(view, document.PublicKey);
        }


        #endregion

        #region Implementation of IEventHandler<in CommentSeted>

        public void Handle(IPublishedEvent<CommentSet> evnt)
        {
            var doc = GetStoredObject(evnt.EventSourceId);
            doc.SetComment(new ItemPublicKey(evnt.Payload.QuestionPublickey, evnt.Payload.PropagationPublicKey),
                           evnt.Payload.Comments);
        }

        #endregion

        #region Implementation of IEventHandler<in PropagatableGroupAdded>

        public void Handle(IPublishedEvent<PropagatableGroupAdded> evnt)
        {
            var doc = GetStoredObject(evnt.EventSourceId);
                doc.PropagateGroup(evnt.Payload.PublicKey, evnt.Payload.PropagationKey);
            //   doc.AddScreen(rout, current);
        }

        #endregion

        #region Implementation of IEventHandler<in PropagatableGroupDeleted>

        public void Handle(IPublishedEvent<PropagatableGroupDeleted> evnt)
        {
            var doc = GetStoredObject(evnt.EventSourceId);
            doc.RemovePropagatedGroup(evnt.Payload.PublicKey, evnt.Payload.PropagationKey);
        }

        #endregion

        #region Implementation of IEventHandler<in QuestionnaireStatusChanged>

        public void Handle(IPublishedEvent<QuestionnaireStatusChanged> evnt)
        {
            var document = GetStoredObject(evnt.EventSourceId);
            if(document==null)
                return;
            document.Status = evnt.Payload.Status;
        }

        #endregion

        #region Implementation of set answer

        public void Handle(IPublishedEvent<MultipleOptionsQuestionAnswered> evnt)
        {
            SetSelectableAnswer(evnt.EventSourceId, evnt.Payload.QuestionId, evnt.Payload.SelectedValues);
        }
        public void Handle(IPublishedEvent<SingleOptionQuestionAnswered> evnt)
        {
            SetSelectableAnswer(evnt.EventSourceId, evnt.Payload.QuestionId, new decimal[] { evnt.Payload.SelectedValue });
        }
        public void Handle(IPublishedEvent<TextQuestionAnswered> evnt)
        {
            SetValueAnswer(evnt.EventSourceId, evnt.Payload.QuestionId, evnt.Payload.Answer);
        }
        public void Handle(IPublishedEvent<NumericQuestionAnswered> evnt)
        {
            SetValueAnswer(evnt.EventSourceId, evnt.Payload.QuestionId, evnt.Payload.Answer);
        }
        public void Handle(IPublishedEvent<DateTimeQuestionAnswered> evnt)
        {
            SetValueAnswer(evnt.EventSourceId, evnt.Payload.QuestionId, evnt.Payload.Answer);
        }
        private void SetSelectableAnswer(Guid interviewId, Guid questionId, decimal[] answers)
        {
            var doc = GetStoredObject(interviewId);
            doc.SetAnswer(new ItemPublicKey(questionId, null), answers);
        }
        private void SetValueAnswer(Guid interviewId, Guid questionId, object answer)
        {
            var doc = GetStoredObject(interviewId);
            doc.SetAnswer(new ItemPublicKey(questionId, null), answer);
        }

        #endregion

        #region Implementation of conditions

        public void Handle(IPublishedEvent<GroupDisabled> evnt)
        {
            var doc = GetStoredObject(evnt.EventSourceId);
            doc.SetScreenStatus(new ItemPublicKey(evnt.Payload.GroupId,null), false);
        }

        public void Handle(IPublishedEvent<GroupEnabled> evnt)
        {
            var doc = GetStoredObject(evnt.EventSourceId);
            doc.SetScreenStatus(new ItemPublicKey(evnt.Payload.GroupId, null), true);
        }
        public void Handle(IPublishedEvent<QuestionDisabled> evnt)
        {
            var doc = GetStoredObject(evnt.EventSourceId);
            doc.SetQuestionStatus(new ItemPublicKey(evnt.Payload.QuestionId, null), false);
        }
        public void Handle(IPublishedEvent<QuestionEnabled> evnt)
        {
            var doc = GetStoredObject(evnt.EventSourceId);
            doc.SetQuestionStatus(new ItemPublicKey(evnt.Payload.QuestionId, null), true);
        }

        public void Handle(IPublishedEvent<AnswerDeclaredInvalid> evnt)
        {
            var doc = GetStoredObject(evnt.EventSourceId);
            doc.SetQuestionValidity(new ItemPublicKey(evnt.Payload.QuestionId, null), false);
        }
        public void Handle(IPublishedEvent<AnswerDeclaredValid> evnt)
        {
            var doc = GetStoredObject(evnt.EventSourceId);
            doc.SetQuestionValidity(new ItemPublicKey(evnt.Payload.QuestionId, null), true);
        }
        #endregion
 
        
    }
}