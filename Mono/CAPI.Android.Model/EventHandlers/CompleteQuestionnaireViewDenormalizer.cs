using System;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;
using Main.Core.Events.Questionnaire.Completed;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace CAPI.Android.Core.Model.EventHandlers
{
    public class CompleteQuestionnaireViewDenormalizer :
                                                         IEventHandler<PropagatableGroupAdded>,
                                                         IEventHandler<PropagatableGroupDeleted>,
                                                         IEventHandler<QuestionnaireStatusChanged>,
        IEventHandler<AnswerCommented>, 
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
        private readonly IReadSideRepositoryWriter<CompleteQuestionnaireView> documentStorage;

        public CompleteQuestionnaireViewDenormalizer(IReadSideRepositoryWriter<CompleteQuestionnaireView> documentStorage)
        {
            this.documentStorage = documentStorage;
        }
      
        #region old events

        public void Handle(IPublishedEvent<NewAssigmentCreated> evnt)
        {
            var document = evnt.Payload.Source;

            var view = new CompleteQuestionnaireView(document);

            documentStorage.Store(view, document.PublicKey);
        }

        public void Handle(IPublishedEvent<PropagatableGroupAdded> evnt)
        {
            var doc = GetStoredObject(evnt.EventSourceId);
            doc.PropagateGroup(evnt.Payload.PublicKey, evnt.Payload.PropagationKey);
            //   doc.AddScreen(rout, current);
        }

        public void Handle(IPublishedEvent<PropagatableGroupDeleted> evnt)
        {
            var doc = GetStoredObject(evnt.EventSourceId);
            doc.RemovePropagatedGroup(evnt.Payload.PublicKey, evnt.Payload.PropagationKey);
        }

        public void Handle(IPublishedEvent<QuestionnaireStatusChanged> evnt)
        {
            var document = GetStoredObject(evnt.EventSourceId);
            if (document == null)
                return;
            document.Status = evnt.Payload.Status;
        }

        #endregion

        public void Handle(IPublishedEvent<InterviewSynchronized> evnt)
        {
            throw new NotImplementedException();
        }

        public void Handle(IPublishedEvent<AnswerCommented> evnt)
        {
            var doc = GetStoredObject(evnt.EventSourceId);
            doc.SetComment(new ItemPublicKey(evnt.Payload.QuestionId, null),
                           evnt.Payload.Comment);
        }

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

        private CompleteQuestionnaireView GetStoredObject(Guid publicKey)
        {
            var doc = documentStorage.GetById(publicKey);
            return doc;
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

    }
}