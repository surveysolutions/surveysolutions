using System;
using Main.Core.Entities.SubEntities;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Capi.EventHandler
{
    public class InterviewViewModelDenormalizer :
        IEventHandler<InterviewSynchronized>,
        IEventHandler<GroupPropagated>,
        IEventHandler<RosterRowAdded>,
        IEventHandler<RosterRowDeleted>,
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
        IEventHandler<SingleOptionLinkedQuestionAnswered>, 
        IEventHandler<MultipleOptionsLinkedQuestionAnswered>,
        IEventHandler<InterviewForTestingCreated>,
        IEventHandler<RosterTitleChanged>
    {
        private readonly IReadSideRepositoryWriter<InterviewViewModel> interviewStorage;
        private readonly IVersionedReadSideRepositoryWriter<QuestionnaireDocumentVersioned> questionnarieStorage;
        private readonly IVersionedReadSideRepositoryWriter<QuestionnaireRosterStructure> questionnaireRosterStructureStorage;

        public InterviewViewModelDenormalizer(
            IReadSideRepositoryWriter<InterviewViewModel> interviewStorage,
            IVersionedReadSideRepositoryWriter<QuestionnaireDocumentVersioned> questionnarieStorage,
            IVersionedReadSideRepositoryWriter<QuestionnaireRosterStructure> questionnaireRosterStructureStorage)
        {
            this.interviewStorage = interviewStorage;
            this.questionnarieStorage = questionnarieStorage;
            this.questionnaireRosterStructureStorage = questionnaireRosterStructureStorage;
        }

        public void Handle(IPublishedEvent<InterviewSynchronized> evnt)
        {
            var questionnaire = this.questionnarieStorage.GetById(evnt.Payload.InterviewData.QuestionnaireId,
                                                             evnt.Payload.InterviewData.QuestionnaireVersion);
            if (questionnaire == null)
                return;

            var propagationStructure = this.GetPropagationStructureOfQuestionnaireAndBuildItIfAbsent(questionnaire);
            var view = new InterviewViewModel(evnt.EventSourceId, questionnaire.Questionnaire, propagationStructure, evnt.Payload.InterviewData);

            this.interviewStorage.Store(view, evnt.EventSourceId);
        }

        private QuestionnaireRosterStructure GetPropagationStructureOfQuestionnaireAndBuildItIfAbsent(QuestionnaireDocumentVersioned questionnaire)
        {
            var propagationStructure = this.questionnaireRosterStructureStorage.GetById(questionnaire.Questionnaire.PublicKey,
                questionnaire.Version);

            if (propagationStructure != null)
                return propagationStructure;

#warning it's bad to write data to other storage, but I've wrote this code for backward compatibility with old versions of CAPI where QuestionnaireRosterStructureDenormalizer haven't been running

            propagationStructure = new QuestionnaireRosterStructure(questionnaire.Questionnaire, questionnaire.Version);
            this.questionnaireRosterStructureStorage.Store(propagationStructure, propagationStructure.QuestionnaireId);
            return propagationStructure;
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
            this.SetSelectableAnswer(evnt.EventSourceId, evnt.Payload.QuestionId, evnt.Payload.PropagationVector,
                                evnt.Payload.SelectedValues);
        }

        public void Handle(IPublishedEvent<GeoLocationQuestionAnswered> evnt)
        {
            this.SetValueAnswer(evnt.EventSourceId, evnt.Payload.QuestionId, evnt.Payload.PropagationVector,
                new GeoPosition(evnt.Payload.Latitude, evnt.Payload.Longitude, evnt.Payload.Accuracy, evnt.Payload.Timestamp));
        }

        public void Handle(IPublishedEvent<SingleOptionQuestionAnswered> evnt)
        {
            this.SetSelectableAnswer(evnt.EventSourceId, evnt.Payload.QuestionId, evnt.Payload.PropagationVector,
                                new decimal[] { evnt.Payload.SelectedValue });
        }

        public void Handle(IPublishedEvent<TextQuestionAnswered> evnt)
        {
            this.SetValueAnswer(evnt.EventSourceId, evnt.Payload.QuestionId, evnt.Payload.PropagationVector,
                           evnt.Payload.Answer);
        }

        public void Handle(IPublishedEvent<NumericIntegerQuestionAnswered> evnt)
        {
            this.SetValueAnswer(evnt.EventSourceId, evnt.Payload.QuestionId, evnt.Payload.PropagationVector,
                           evnt.Payload.Answer);
        }

        public void Handle(IPublishedEvent<NumericRealQuestionAnswered> evnt)
        {
            this.SetValueAnswer(evnt.EventSourceId, evnt.Payload.QuestionId, evnt.Payload.PropagationVector,
                         evnt.Payload.Answer);
        }

        public void Handle(IPublishedEvent<NumericQuestionAnswered> evnt)
        {
            this.SetValueAnswer(evnt.EventSourceId, evnt.Payload.QuestionId, evnt.Payload.PropagationVector,
                          evnt.Payload.Answer);
        }

        public void Handle(IPublishedEvent<DateTimeQuestionAnswered> evnt)
        {
            this.SetValueAnswer(evnt.EventSourceId, evnt.Payload.QuestionId, evnt.Payload.PropagationVector,
                           evnt.Payload.Answer);
        }

        public void Handle(IPublishedEvent<SingleOptionLinkedQuestionAnswered> evnt)
        {
            this.SetValueAnswer(evnt.EventSourceId, evnt.Payload.QuestionId, evnt.Payload.PropagationVector,
                          evnt.Payload.SelectedPropagationVector);
        }

        public void Handle(IPublishedEvent<MultipleOptionsLinkedQuestionAnswered> evnt)
        {
            this.SetValueAnswer(evnt.EventSourceId, evnt.Payload.QuestionId, evnt.Payload.PropagationVector,
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
            var doc = this.interviewStorage.GetById(publicKey);
            return doc;
        }

        private void SetSelectableAnswer(Guid interviewId, Guid questionId, decimal[] protagationVector, decimal[] answers)
        {
            var doc = this.GetStoredViewModel(interviewId);
            doc.SetAnswer(new InterviewItemId(questionId, protagationVector), answers);
        }

        private void SetValueAnswer(Guid interviewId, Guid questionId, decimal[] protagationVector, object answer)
        {
            var doc = this.GetStoredViewModel(interviewId);
            doc.SetAnswer(new InterviewItemId(questionId, protagationVector), answer);
        }

        private void RemoveAnswer(Guid interviewId, Guid questionId, decimal[] propagationVector)
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

        public void Handle(IPublishedEvent<RosterRowAdded> evnt)
        {
            var doc = this.GetStoredViewModel(evnt.EventSourceId);
            doc.AddPropagateScreen(evnt.Payload.GroupId, evnt.Payload.OuterRosterVector, evnt.Payload.RosterInstanceId, evnt.Payload.TargetIndex);
        }

        public void Handle(IPublishedEvent<RosterRowDeleted> evnt)
        {
            var doc = this.GetStoredViewModel(evnt.EventSourceId);
            doc.RemovePropagatedScreen(evnt.Payload.GroupId, evnt.Payload.OuterRosterVector, evnt.Payload.RosterInstanceId);
        }

        public void Handle(IPublishedEvent<RosterTitleChanged> evnt)
        {
            var doc = this.GetStoredViewModel(evnt.EventSourceId);
            doc.UpdateRosterTitle(evnt.Payload.QuestionId, evnt.Payload.PropagationVector, evnt.Payload.RosterTitle);
        }

        public void Handle(IPublishedEvent<SynchronizationMetadataApplied> evnt)
        {
            this.interviewStorage.Remove(evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<InterviewForTestingCreated> evnt)
        {
            var questionnaire = this.questionnarieStorage.GetById(evnt.Payload.QuestionnaireId,
                                                             evnt.Payload.QuestionnaireVersion);
            if (questionnaire == null)
                return;

            var propagationStructure = this.GetPropagationStructureOfQuestionnaireAndBuildItIfAbsent(questionnaire);

            var view = new InterviewViewModel(evnt.EventSourceId, questionnaire.Questionnaire, propagationStructure);

            this.interviewStorage.Store(view, evnt.EventSourceId);
            
        }
    }
}