using System;
using Main.Core.Entities.SubEntities;
using Microsoft.Practices.ServiceLocation;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.BoundedContexts.Supervisor.Factories;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Capi.EventHandler
{
    public class InterviewViewModelDenormalizer :
        IEventHandler<InterviewSynchronized>,
        IEventHandler<GroupPropagated>,
        IEventHandler<RosterInstancesTitleChanged>,
        IEventHandler<RosterInstancesAdded>,
        IEventHandler<RosterInstancesRemoved>,
        IEventHandler<InterviewCompleted>,
        IEventHandler<InterviewRestarted>,
        IEventHandler<AnswerCommented>,
        IEventHandler<MultipleOptionsQuestionAnswered>,
        IEventHandler<NumericIntegerQuestionAnswered>,
        IEventHandler<NumericRealQuestionAnswered>,
        IEventHandler<TextQuestionAnswered>,
        IEventHandler<TextListQuestionAnswered>,
        IEventHandler<SingleOptionQuestionAnswered>,
        IEventHandler<DateTimeQuestionAnswered>,
        IEventHandler<GeoLocationQuestionAnswered>,
        IEventHandler<GroupsDisabled>,
        IEventHandler<GroupsEnabled>,
        IEventHandler<QuestionsDisabled>,
        IEventHandler<QuestionsEnabled>,
        IEventHandler<AnswersDeclaredInvalid>,
        IEventHandler<AnswersDeclaredValid>,
        IEventHandler<SynchronizationMetadataApplied>,
        IEventHandler<AnswersRemoved>,
        IEventHandler<SingleOptionLinkedQuestionAnswered>, 
        IEventHandler<MultipleOptionsLinkedQuestionAnswered>,
        IEventHandler<InterviewForTestingCreated>,
        IEventHandler<InterviewOnClientCreated>,
        IEventHandler<QRBarcodeQuestionAnswered>,
        IEventHandler<PictureQuestionAnswered>
    {
        private readonly IReadSideRepositoryWriter<InterviewViewModel> interviewStorage;
        private readonly IReadSideKeyValueStorage<QuestionnaireDocumentVersioned> questionnarieStorage;
        private readonly IReadSideKeyValueStorage<QuestionnaireRosterStructure> questionnaireRosterStructureStorage;
        private readonly IQuestionnaireRosterStructureFactory questionnaireRosterStructureFactory;

        private static ILogger Logger
        {
            get { return ServiceLocator.Current.GetInstance<ILogger>(); }
        }

        public InterviewViewModelDenormalizer(
            IReadSideRepositoryWriter<InterviewViewModel> interviewStorage,
            IReadSideKeyValueStorage<QuestionnaireDocumentVersioned> questionnarieStorage,
            IReadSideKeyValueStorage<QuestionnaireRosterStructure> questionnaireRosterStructureStorage, IQuestionnaireRosterStructureFactory questionnaireRosterStructureFactory)
        {
            this.interviewStorage = interviewStorage;
            this.questionnarieStorage = questionnarieStorage;
            this.questionnaireRosterStructureStorage = questionnaireRosterStructureStorage;
            this.questionnaireRosterStructureFactory = questionnaireRosterStructureFactory;
        }

        public void Handle(IPublishedEvent<InterviewSynchronized> evnt)
        {
            var questionnaire = this.questionnarieStorage.AsVersioned().Get(
                evnt.Payload.InterviewData.QuestionnaireId.FormatGuid(), evnt.Payload.InterviewData.QuestionnaireVersion);

            if (questionnaire == null)
                return;

            var propagationStructure = this.GetPropagationStructureOfQuestionnaireAndBuildItIfAbsent(questionnaire);
            var view = new InterviewViewModel(evnt.EventSourceId, questionnaire.Questionnaire, propagationStructure, evnt.Payload.InterviewData);

            this.interviewStorage.Store(view, evnt.EventSourceId);
        }

        private QuestionnaireRosterStructure GetPropagationStructureOfQuestionnaireAndBuildItIfAbsent(QuestionnaireDocumentVersioned questionnaire)
        {
              QuestionnaireRosterStructure propagationStructure = null;
            try
            {
                propagationStructure = this.questionnaireRosterStructureStorage.AsVersioned().Get(
                    questionnaire.Questionnaire.PublicKey.FormatGuid(), questionnaire.Version);
            }
            catch (Exception e)
            {
                Logger.Error("error during restore QuestionnaireRosterStructure", e);
            }

            if (propagationStructure != null)
                return propagationStructure;

#warning it's bad to write data to other storage, but I've wrote this code for backward compatibility with old versions of CAPI where QuestionnaireRosterStructureDenormalizer haven't been running

            propagationStructure = questionnaireRosterStructureFactory.CreateQuestionnaireRosterStructure(questionnaire.Questionnaire, questionnaire.Version);
            this.questionnaireRosterStructureStorage.AsVersioned().Store(propagationStructure, propagationStructure.QuestionnaireId.FormatGuid(), propagationStructure.Version);
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
            doc.SetComment(ConversionHelper.ConvertIdAndRosterVectorToString(evnt.Payload.QuestionId, evnt.Payload.PropagationVector),
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
                new GeoPosition(evnt.Payload.Latitude, evnt.Payload.Longitude, evnt.Payload.Accuracy, 
                    evnt.Payload.Altitude, evnt.Payload.Timestamp));
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

        public void Handle(IPublishedEvent<QRBarcodeQuestionAnswered> evnt)
        {
            this.SetValueAnswer(evnt.EventSourceId, evnt.Payload.QuestionId, evnt.Payload.PropagationVector,
                           evnt.Payload.Answer);
        }

        public void Handle(IPublishedEvent<PictureQuestionAnswered> evnt)
        {
            this.SetValueAnswer(evnt.EventSourceId, evnt.Payload.QuestionId, evnt.Payload.PropagationVector,
                          evnt.Payload.PictureFileName);
        }
        
        public void Handle(IPublishedEvent<TextListQuestionAnswered> evnt)
        {
           this.SetValueAnswer(evnt.EventSourceId, evnt.Payload.QuestionId, evnt.Payload.PropagationVector,
                          evnt.Payload.Answers);
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

        public void Handle(IPublishedEvent<AnswersRemoved> evnt)
        {
            foreach (var question in evnt.Payload.Questions)
            {
                this.RemoveAnswer(evnt.EventSourceId, question.Id, question.RosterVector);
            }
        }

        public void Handle(IPublishedEvent<GroupsDisabled> evnt)
        {
            var doc = this.GetStoredViewModel(evnt.EventSourceId);

            foreach (var group in evnt.Payload.Groups)
            {
                doc.SetScreenStatus(ConversionHelper.ConvertIdAndRosterVectorToString(group.Id, group.RosterVector), false);
            }
        }

        public void Handle(IPublishedEvent<GroupsEnabled> evnt)
        {
            var doc = this.GetStoredViewModel(evnt.EventSourceId);

            foreach (var group in evnt.Payload.Groups)
            {
                doc.SetScreenStatus(ConversionHelper.ConvertIdAndRosterVectorToString(group.Id, group.RosterVector), true);
            }
        }

        public void Handle(IPublishedEvent<QuestionsDisabled> evnt)
        {
            var doc = this.GetStoredViewModel(evnt.EventSourceId);

            foreach (var question in evnt.Payload.Questions)
            {
                doc.SetQuestionStatus(ConversionHelper.ConvertIdAndRosterVectorToString(question.Id, question.RosterVector), false);
            }
        }

        public void Handle(IPublishedEvent<QuestionsEnabled> evnt)
        {
            var doc = this.GetStoredViewModel(evnt.EventSourceId);

            foreach (var question in evnt.Payload.Questions)
            {
                doc.SetQuestionStatus(ConversionHelper.ConvertIdAndRosterVectorToString(question.Id, question.RosterVector), true);
            }
        }

        public void Handle(IPublishedEvent<AnswersDeclaredInvalid> evnt)
        {
            var doc = this.GetStoredViewModel(evnt.EventSourceId);

            foreach (var question in evnt.Payload.Questions)
            {
                doc.SetQuestionValidity(ConversionHelper.ConvertIdAndRosterVectorToString(question.Id, question.RosterVector), false);
            }
        }

        public void Handle(IPublishedEvent<AnswersDeclaredValid> evnt)
        {
            var doc = this.GetStoredViewModel(evnt.EventSourceId);

            foreach (var question in evnt.Payload.Questions)
            {
                doc.SetQuestionValidity(ConversionHelper.ConvertIdAndRosterVectorToString(question.Id, question.RosterVector), true);
            }
        }

        private InterviewViewModel GetStoredViewModel(Guid publicKey)
        {
            var doc = this.interviewStorage.GetById(publicKey);
            return doc;
        }

        private void SetSelectableAnswer(Guid interviewId, Guid questionId, decimal[] protagationVector, decimal[] answers)
        {
            var doc = this.GetStoredViewModel(interviewId);
            doc.SetAnswer(ConversionHelper.ConvertIdAndRosterVectorToString(questionId, protagationVector), answers);
        }

        private void SetValueAnswer(Guid interviewId, Guid questionId, decimal[] protagationVector, object answer)
        {
            var doc = this.GetStoredViewModel(interviewId);
            doc.SetAnswer(ConversionHelper.ConvertIdAndRosterVectorToString(questionId, protagationVector), answer);
        }

        private void RemoveAnswer(Guid interviewId, Guid questionId, decimal[] propagationVector)
        {
            InterviewViewModel viewModel = this.GetStoredViewModel(interviewId);

            viewModel.RemoveAnswer(ConversionHelper.ConvertIdAndRosterVectorToString(questionId, propagationVector));
        }

        public void Handle(IPublishedEvent<GroupPropagated> evnt)
        {
            var doc = this.GetStoredViewModel(evnt.EventSourceId);
            doc.UpdatePropagateGroupsByTemplate(evnt.Payload.GroupId, evnt.Payload.OuterScopePropagationVector,
                                                evnt.Payload.Count);
        }

        public void Handle(IPublishedEvent<RosterInstancesTitleChanged> evnt)
        {
            var doc = this.GetStoredViewModel(evnt.EventSourceId);
            foreach (var changedRosterRowTitleDto in evnt.Payload.ChangedInstances)
            {
                doc.UpdateRosterRowTitle(changedRosterRowTitleDto.RosterInstance.GroupId, changedRosterRowTitleDto.RosterInstance.OuterRosterVector, changedRosterRowTitleDto.RosterInstance.RosterInstanceId, changedRosterRowTitleDto.Title);
            }
        }

        public void Handle(IPublishedEvent<RosterInstancesAdded> evnt)
        {
            var doc = this.GetStoredViewModel(evnt.EventSourceId);

            foreach (var instance in evnt.Payload.Instances)
            {
                doc.AddRosterScreen(instance.GroupId, instance.OuterRosterVector, instance.RosterInstanceId, instance.SortIndex);
            }
        }

        public void Handle(IPublishedEvent<RosterInstancesRemoved> evnt)
        {
            var doc = this.GetStoredViewModel(evnt.EventSourceId);

            foreach (var instance in evnt.Payload.Instances)
            {
                doc.RemovePropagatedScreen(instance.GroupId, instance.OuterRosterVector, instance.RosterInstanceId);
            }
        }

        public void Handle(IPublishedEvent<SynchronizationMetadataApplied> evnt)
        {
            this.interviewStorage.Remove(evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<InterviewForTestingCreated> evnt)
        {
            var questionnaire = this.questionnarieStorage.AsVersioned().Get(evnt.Payload.QuestionnaireId.FormatGuid(), evnt.Payload.QuestionnaireVersion);
            if (questionnaire == null)
                return;

            var propagationStructure = this.GetPropagationStructureOfQuestionnaireAndBuildItIfAbsent(questionnaire);

            var view = new InterviewViewModel(evnt.EventSourceId, questionnaire.Questionnaire, propagationStructure);

            this.interviewStorage.Store(view, evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<InterviewOnClientCreated> evnt)
        {
            var questionnaire = this.questionnarieStorage.AsVersioned().Get(evnt.Payload.QuestionnaireId.FormatGuid(), evnt.Payload.QuestionnaireVersion);
            if (questionnaire == null)
                return;

            var propagationStructure = this.GetPropagationStructureOfQuestionnaireAndBuildItIfAbsent(questionnaire);

            var view = new InterviewViewModel(evnt.EventSourceId, questionnaire.Questionnaire, propagationStructure);

            this.interviewStorage.Store(view, evnt.EventSourceId);
        }
    }
}