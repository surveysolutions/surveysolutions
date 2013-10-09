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
    public class InterviewViewModelDenormalizer :
        IEventHandler<InterviewSynchronized>,
        IEventHandler<GroupPropagated>,
        IEventHandler<InterviewCompleted>,
        IEventHandler<InterviewRestarted>,
        IEventHandler<AnswerCommented>,
        IEventHandler<MultipleOptionsQuestionAnswered>,
        IEventHandler<NumericIntegerQuestionAnswered>,
        IEventHandler<NumericRealQuestionAnswered>,
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
        IEventHandler<SynchronizationMetadataApplied>,
        IEventHandler<AnswerRemoved>,
        IEventHandler<SingleOptionLinkedQuestionAnswered>, IEventHandler<MultipleOptionsLinkedQuestionAnswered>
    {
        private readonly IReadSideRepositoryWriter<InterviewViewModel> interviewStorage;
        private readonly IVersionedReadSideRepositoryWriter<QuestionnaireDocumentVersioned> questionnarieStorage;

        public InterviewViewModelDenormalizer(
            IReadSideRepositoryWriter<InterviewViewModel> interviewStorage,
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

            var view = new InterviewViewModel(evnt.EventSourceId, questionnarie.Questionnaire, evnt.Payload.InterviewData);

            interviewStorage.Store(view, evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<InterviewCompleted> evnt)
        {
            var document = this.GetStoredViewModel(evnt.EventSourceId); 
            if (document == null)
                return;
            document.Status = InterviewStatus.Completed;
        }

        public void Handle(IPublishedEvent<InterviewRestarted> evnt)
        {
            var document = this.GetStoredViewModel(evnt.EventSourceId);
            if (document == null)
                return;
            document.Status = InterviewStatus.Restarted;
        }

        public void Handle(IPublishedEvent<AnswerCommented> evnt)
        {
            var doc = this.GetStoredViewModel(evnt.EventSourceId);
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

        public void Handle(IPublishedEvent<NumericIntegerQuestionAnswered> evnt)
        {
            SetValueAnswer(evnt.EventSourceId, evnt.Payload.QuestionId, evnt.Payload.PropagationVector,
                           evnt.Payload.Answer);
        }

        public void Handle(IPublishedEvent<NumericRealQuestionAnswered> evnt)
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

        public void Handle(IPublishedEvent<SingleOptionLinkedQuestionAnswered> evnt)
        {
            SetValueAnswer(evnt.EventSourceId, evnt.Payload.QuestionId, evnt.Payload.PropagationVector,
                          evnt.Payload.SelectedPropagationVector);
        }

        public void Handle(IPublishedEvent<MultipleOptionsLinkedQuestionAnswered> evnt)
        {
            SetValueAnswer(evnt.EventSourceId, evnt.Payload.QuestionId, evnt.Payload.PropagationVector,
                          evnt.Payload.SelectedPropagationVectors);
        }

        public void Handle(IPublishedEvent<AnswerRemoved> evnt)
        {
            this.RemoveAnswer(evnt.EventSourceId, evnt.Payload.QuestionId, evnt.Payload.PropagationVector);
        }

        public void Handle(IPublishedEvent<GroupDisabled> evnt)
        {
            var doc = this.GetStoredViewModel(evnt.EventSourceId);
            doc.SetScreenStatus(new InterviewItemId(evnt.Payload.GroupId, evnt.Payload.PropagationVector), false);
        }

        public void Handle(IPublishedEvent<GroupEnabled> evnt)
        {
            var doc = this.GetStoredViewModel(evnt.EventSourceId);
            doc.SetScreenStatus(new InterviewItemId(evnt.Payload.GroupId, evnt.Payload.PropagationVector), true);
        }

        public void Handle(IPublishedEvent<QuestionDisabled> evnt)
        {
            var doc = this.GetStoredViewModel(evnt.EventSourceId);
            doc.SetQuestionStatus(new InterviewItemId(evnt.Payload.QuestionId, evnt.Payload.PropagationVector), false);
        }

        public void Handle(IPublishedEvent<QuestionEnabled> evnt)
        {
            var doc = this.GetStoredViewModel(evnt.EventSourceId);
            doc.SetQuestionStatus(new InterviewItemId(evnt.Payload.QuestionId, evnt.Payload.PropagationVector), true);
        }

        public void Handle(IPublishedEvent<AnswerDeclaredInvalid> evnt)
        {
            var doc = this.GetStoredViewModel(evnt.EventSourceId);
            doc.SetQuestionValidity(new InterviewItemId(evnt.Payload.QuestionId, evnt.Payload.PropagationVector), false);
        }

        public void Handle(IPublishedEvent<AnswerDeclaredValid> evnt)
        {
            var doc = this.GetStoredViewModel(evnt.EventSourceId);
            doc.SetQuestionValidity(new InterviewItemId(evnt.Payload.QuestionId, evnt.Payload.PropagationVector), true);
        }

        private InterviewViewModel GetStoredViewModel(Guid publicKey)
        {
            var doc = interviewStorage.GetById(publicKey);
            return doc;
        }

        private void SetSelectableAnswer(Guid interviewId, Guid questionId, int[] protagationVector, decimal[] answers)
        {
            var doc = this.GetStoredViewModel(interviewId);
            doc.SetAnswer(new InterviewItemId(questionId, protagationVector), answers);
        }

        private void SetValueAnswer(Guid interviewId, Guid questionId, int[] protagationVector, object answer)
        {
            var doc = this.GetStoredViewModel(interviewId);
            doc.SetAnswer(new InterviewItemId(questionId, protagationVector), answer);
        }

        private void RemoveAnswer(Guid interviewId, Guid questionId, int[] propagationVector)
        {
            InterviewViewModel viewModel = this.GetStoredViewModel(interviewId);

            viewModel.RemoveAnswer(new InterviewItemId(questionId, propagationVector));
        }

        public void Handle(IPublishedEvent<GroupPropagated> evnt)
        {
            var doc = this.GetStoredViewModel(evnt.EventSourceId);
            doc.UpdatePropagateGroupsByTemplate(evnt.Payload.GroupId, evnt.Payload.OuterScopePropagationVector,
                                                evnt.Payload.Count);
        }

        public void Handle(IPublishedEvent<SynchronizationMetadataApplied> evnt)
        {
            this.interviewStorage.Remove(evnt.EventSourceId);
        }
    }
}