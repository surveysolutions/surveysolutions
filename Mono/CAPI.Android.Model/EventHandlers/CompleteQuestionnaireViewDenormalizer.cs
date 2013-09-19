using System;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;
using Main.Core.Entities.SubEntities;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace CAPI.Android.Core.Model.EventHandlers
{
    public class CompleteQuestionnaireViewDenormalizer :
        IEventHandler<InterviewSynchronized>,
        IEventHandler<GroupPropagated>,
        IEventHandler<InterviewCompleted>,
        IEventHandler<InterviewRestarted>,
        IEventHandler<AnswerCommented>,
        IEventHandler<MultipleOptionsQuestionAnswered>,
        IEventHandler<NumericQuestionAnswered>,
        IEventHandler<TextQuestionAnswered>,
        IEventHandler<SingleOptionQuestionAnswered>,
        IEventHandler<DateTimeQuestionAnswered>,
        IEventHandler<GeoLocationQuestionAnswered>,
        IEventHandler<GroupDisabled>,
        IEventHandler<GroupEnabled>,
        IEventHandler<QuestionDisabled>,
        IEventHandler<QuestionEnabled>,
        IEventHandler<AnswerDeclaredInvalid>,
        IEventHandler<AnswerDeclaredValid>,
        IEventHandler<SynchronizationMetadataApplied>
    {
        private readonly IReadSideRepositoryWriter<CompleteQuestionnaireView> interviewStorage;
        private readonly IVersionedReadSideRepositoryWriter<QuestionnaireDocumentVersioned> questionnarieStorage;

        public CompleteQuestionnaireViewDenormalizer(
            IReadSideRepositoryWriter<CompleteQuestionnaireView> interviewStorage,
            IVersionedReadSideRepositoryWriter<QuestionnaireDocumentVersioned> questionnarieStorage)
        {
            this.interviewStorage = interviewStorage;
            this.questionnarieStorage = questionnarieStorage;
        }

        public void Handle(IPublishedEvent<InterviewSynchronized> evnt)
        {
            var questionnarie = questionnarieStorage.GetById(evnt.Payload.InterviewData.QuestionnaireId,
                                                             evnt.Payload.InterviewData.QuestionnaireVersion);
            if (questionnarie == null)
                return;

            var view = new CompleteQuestionnaireView(evnt.EventSourceId, questionnarie.Questionnaire, evnt.Payload);

            interviewStorage.Store(view, evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<InterviewCompleted> evnt)
        {
            var document = GetStoredObject(evnt.EventSourceId); 
            if (document == null)
                return;
            document.Status = InterviewStatus.Completed;
        }

        public void Handle(IPublishedEvent<InterviewRestarted> evnt)
        {
            var document = GetStoredObject(evnt.EventSourceId);
            if (document == null)
                return;
            document.Status = InterviewStatus.Restarted;
        }

        public void Handle(IPublishedEvent<AnswerCommented> evnt)
        {
            var doc = GetStoredObject(evnt.EventSourceId);
            doc.SetComment(new InterviewItemId(evnt.Payload.QuestionId, evnt.Payload.PropagationVector),
                           evnt.Payload.Comment);
        }

        public void Handle(IPublishedEvent<MultipleOptionsQuestionAnswered> evnt)
        {
            SetSelectableAnswer(evnt.EventSourceId, evnt.Payload.QuestionId, evnt.Payload.PropagationVector,
                                evnt.Payload.SelectedValues);
        }

        public void Handle(IPublishedEvent<GeoLocationQuestionAnswered> evnt)
        {
            SetValueAnswer(evnt.EventSourceId, evnt.Payload.QuestionId, evnt.Payload.PropagationVector,
                new GeoPosition(evnt.Payload.Latitude,evnt.Payload.Longitude, evnt.Payload.Accuracy, evnt.Payload.Timestamp));
        }

        public void Handle(IPublishedEvent<SingleOptionQuestionAnswered> evnt)
        {
            SetSelectableAnswer(evnt.EventSourceId, evnt.Payload.QuestionId, evnt.Payload.PropagationVector,
                                new decimal[] {evnt.Payload.SelectedValue});
        }

        public void Handle(IPublishedEvent<TextQuestionAnswered> evnt)
        {
            SetValueAnswer(evnt.EventSourceId, evnt.Payload.QuestionId, evnt.Payload.PropagationVector,
                           evnt.Payload.Answer);
        }

        public void Handle(IPublishedEvent<NumericQuestionAnswered> evnt)
        {
            SetValueAnswer(evnt.EventSourceId, evnt.Payload.QuestionId, evnt.Payload.PropagationVector,
                           evnt.Payload.Answer);
        }

        public void Handle(IPublishedEvent<DateTimeQuestionAnswered> evnt)
        {
            SetValueAnswer(evnt.EventSourceId, evnt.Payload.QuestionId, evnt.Payload.PropagationVector,
                           evnt.Payload.Answer);
        }

        public void Handle(IPublishedEvent<GroupDisabled> evnt)
        {
            var doc = GetStoredObject(evnt.EventSourceId);
            doc.SetScreenStatus(new InterviewItemId(evnt.Payload.GroupId, evnt.Payload.PropagationVector), false);
        }

        public void Handle(IPublishedEvent<GroupEnabled> evnt)
        {
            var doc = GetStoredObject(evnt.EventSourceId);
            doc.SetScreenStatus(new InterviewItemId(evnt.Payload.GroupId, evnt.Payload.PropagationVector), true);
        }

        public void Handle(IPublishedEvent<QuestionDisabled> evnt)
        {
            var doc = GetStoredObject(evnt.EventSourceId);
            doc.SetQuestionStatus(new InterviewItemId(evnt.Payload.QuestionId, evnt.Payload.PropagationVector), false);
        }

        public void Handle(IPublishedEvent<QuestionEnabled> evnt)
        {
            var doc = GetStoredObject(evnt.EventSourceId);
            doc.SetQuestionStatus(new InterviewItemId(evnt.Payload.QuestionId, evnt.Payload.PropagationVector), true);
        }

        public void Handle(IPublishedEvent<AnswerDeclaredInvalid> evnt)
        {
            var doc = GetStoredObject(evnt.EventSourceId);
            doc.SetQuestionValidity(new InterviewItemId(evnt.Payload.QuestionId, evnt.Payload.PropagationVector), false);
        }

        public void Handle(IPublishedEvent<AnswerDeclaredValid> evnt)
        {
            var doc = GetStoredObject(evnt.EventSourceId);
            doc.SetQuestionValidity(new InterviewItemId(evnt.Payload.QuestionId, evnt.Payload.PropagationVector), true);
        }

        private CompleteQuestionnaireView GetStoredObject(Guid publicKey)
        {
            var doc = interviewStorage.GetById(publicKey);
            return doc;
        }

        private void SetSelectableAnswer(Guid interviewId, Guid questionId, int[] protagationVector, decimal[] answers)
        {
            var doc = GetStoredObject(interviewId);
            doc.SetAnswer(new InterviewItemId(questionId, protagationVector), answers);
        }

        private void SetValueAnswer(Guid interviewId, Guid questionId, int[] protagationVector, object answer)
        {
            var doc = GetStoredObject(interviewId);
            doc.SetAnswer(new InterviewItemId(questionId, protagationVector), answer);
        }

        public void Handle(IPublishedEvent<GroupPropagated> evnt)
        {
            var doc = GetStoredObject(evnt.EventSourceId);
            doc.UpdatePropagateGroupsByTemplate(evnt.Payload.GroupId, evnt.Payload.OuterScopePropagationVector,
                                                evnt.Payload.Count);
        }

        public void Handle(IPublishedEvent<SynchronizationMetadataApplied> evnt)
        {
            this.interviewStorage.Remove(evnt.EventSourceId);
        }
    }
}