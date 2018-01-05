using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventHandlers;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.DataCollection.Views.Interview;
using WB.Core.SharedKernels.Questionnaire.Documents;

namespace WB.Core.BoundedContexts.Headquarters.EventHandler
{
    internal class InterviewDenormalizer : AbstractDenormalizer<InterviewState>,
        IEventHandler,
        IUpdateOrRemoveHandler<InterviewState, GroupPropagated>,
        IUpdateOrRemoveHandler<InterviewState, RosterInstancesAdded>,
        IUpdateOrRemoveHandler<InterviewState, RosterInstancesRemoved>,
        IUpdateOrRemoveHandler<InterviewState, MultipleOptionsQuestionAnswered>,
        IUpdateOrRemoveHandler<InterviewState, NumericRealQuestionAnswered>,
        IUpdateOrRemoveHandler<InterviewState, NumericIntegerQuestionAnswered>,
        IUpdateOrRemoveHandler<InterviewState, TextQuestionAnswered>,
        IUpdateOrRemoveHandler<InterviewState, TextListQuestionAnswered>,
        IUpdateOrRemoveHandler<InterviewState, SingleOptionQuestionAnswered>,
        IUpdateOrRemoveHandler<InterviewState, SingleOptionLinkedQuestionAnswered>,
        IUpdateOrRemoveHandler<InterviewState, MultipleOptionsLinkedQuestionAnswered>,
        IUpdateOrRemoveHandler<InterviewState, DateTimeQuestionAnswered>,
        IUpdateOrRemoveHandler<InterviewState, GeoLocationQuestionAnswered>,
        IUpdateOrRemoveHandler<InterviewState, QRBarcodeQuestionAnswered>,
        IUpdateOrRemoveHandler<InterviewState, PictureQuestionAnswered>,
        IUpdateOrRemoveHandler<InterviewState, YesNoQuestionAnswered>,
        IUpdateOrRemoveHandler<InterviewState, AnswersRemoved>,
        IUpdateOrRemoveHandler<InterviewState, GroupsDisabled>,
        IUpdateOrRemoveHandler<InterviewState, GroupsEnabled>,
        IUpdateOrRemoveHandler<InterviewState, StaticTextsEnabled>,
        IUpdateOrRemoveHandler<InterviewState, StaticTextsDisabled>,
        IUpdateOrRemoveHandler<InterviewState, StaticTextsDeclaredInvalid>,
        IUpdateOrRemoveHandler<InterviewState, StaticTextsDeclaredValid>,
        IUpdateOrRemoveHandler<InterviewState, QuestionsDisabled>,
        IUpdateOrRemoveHandler<InterviewState, QuestionsEnabled>,
        IUpdateOrRemoveHandler<InterviewState, AnswersDeclaredInvalid>,
        IUpdateOrRemoveHandler<InterviewState, AnswersDeclaredValid>,
        IUpdateOrRemoveHandler<InterviewState, InterviewHardDeleted>,
        IUpdateOrRemoveHandler<InterviewState, AnswerRemoved>,
        IUpdateOrRemoveHandler<InterviewState, QuestionsMarkedAsReadonly>,
        IUpdateOrRemoveHandler<InterviewState, VariablesChanged>,
        IUpdateOrRemoveHandler<InterviewState, VariablesDisabled>,
        IUpdateOrRemoveHandler<InterviewState, VariablesEnabled>,
        IUpdateOrRemoveHandler<InterviewState, AreaQuestionAnswered>,
        IUpdateOrRemoveHandler<InterviewState, AudioQuestionAnswered>,
        IUpdateOrRemoveHandler<InterviewState, InterviewCreated>,
        IUpdateOrRemoveHandler<InterviewState, InterviewFromPreloadedDataCreated>,
        IUpdateOrRemoveHandler<InterviewState, InterviewOnClientCreated>
    {
        public string Name => "Interview detalis";
        public object[] Readers => new object[0];
        public object[] Writers => new object[0];

        private readonly IInterviewFactory repository;
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> summaries;
        private readonly IQuestionnaireStorage questionnaireStorage;

        public InterviewDenormalizer(IInterviewFactory interviewFactory,
            IQueryableReadSideRepositoryReader<InterviewSummary> summaries, IQuestionnaireStorage questionnaireStorage)
        {
            this.repository = interviewFactory;
            this.summaries = summaries;
            this.questionnaireStorage = questionnaireStorage;
        }

        private readonly Dictionary<Guid, QuestionnaireIdentity> interviewToQuestionnaire =
            new Dictionary<Guid, QuestionnaireIdentity>();

        public void Update(InterviewState state, IPublishedEvent<GroupPropagated> evnt) =>
            this.SetEnablementInState(state, Identity.Create(evnt.Payload.GroupId, evnt.Payload.OuterScopeRosterVector), true);

        public void Update(InterviewState state, IPublishedEvent<RosterInstancesAdded> evnt)
        {
            foreach (var rosterInstance in evnt.Payload.Instances)
                this.SetEnablementInState(state, rosterInstance.GetIdentity(), true);
        }

        public void Update(InterviewState state, IPublishedEvent<RosterInstancesRemoved> evnt)
        {
            if(!this.interviewToQuestionnaire.TryGetValue(evnt.EventSourceId, out var questionnaireIdentity))
            {
                questionnaireIdentity = this.summaries.GetQuestionnaireIdentity(evnt.EventSourceId);
                this.AddQuestionnaireToDictionary(evnt.EventSourceId, questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version);                
            }

            var questionnaire = this.questionnaireStorage.GetQuestionnaireDocument(questionnaireIdentity);
            if (questionnaire == null) return;

            var removeEntities = new List<Identity>();
            
            foreach (var instance in evnt.Payload.Instances.Select(x => x.GetIdentity()).ToArray())
            {
                removeEntities.Add(instance);

                var roster = questionnaire.Find<IGroup>(instance.Id);
                
                removeEntities.AddRange(roster.Children.TreeToEnumerable(x => x.Children)
                    .Select(entity => Identity.Create(entity.PublicKey, instance.RosterVector)));
            }

            foreach (var removedEntity in removeEntities)
            {
                state.Answers.Remove(removedEntity);
                state.Enablement.Remove(removedEntity);
                state.Validity.Remove(removedEntity);
                state.ReadOnly.Remove(removedEntity);

                if (!state.Removed.Contains(removedEntity))
                    state.Removed.Add(removedEntity);
            }
        }

        public void Update(InterviewState state, IPublishedEvent<MultipleOptionsQuestionAnswered> evnt) =>
            this.SetAnswerInState(state, Identity.Create(evnt.Payload.QuestionId, evnt.Payload.RosterVector),
                evnt.Payload.SelectedValues.Select(Convert.ToInt32).ToArray());

        public void Update(InterviewState state, IPublishedEvent<NumericRealQuestionAnswered> evnt) =>
            this.SetAnswerInState(state, Identity.Create(evnt.Payload.QuestionId, evnt.Payload.RosterVector), Convert.ToDouble(evnt.Payload.Answer));

        public void Update(InterviewState state, IPublishedEvent<NumericIntegerQuestionAnswered> evnt) =>
            this.SetAnswerInState(state, Identity.Create(evnt.Payload.QuestionId, evnt.Payload.RosterVector), evnt.Payload.Answer);

        public void Update(InterviewState state, IPublishedEvent<TextQuestionAnswered> evnt) =>
            this.SetAnswerInState(state, Identity.Create(evnt.Payload.QuestionId, evnt.Payload.RosterVector), evnt.Payload.Answer);

        public void Update(InterviewState state, IPublishedEvent<TextListQuestionAnswered> evnt) =>
            this.SetAnswerInState(state, Identity.Create(evnt.Payload.QuestionId, evnt.Payload.RosterVector),
                evnt.Payload.Answers.Select(_ => new InterviewTextListAnswer(_.Item1, _.Item2)).ToArray());

        public void Update(InterviewState state, IPublishedEvent<SingleOptionQuestionAnswered> evnt) =>
            this.SetAnswerInState(state, Identity.Create(evnt.Payload.QuestionId, evnt.Payload.RosterVector), Convert.ToInt32(evnt.Payload.SelectedValue));

        public void Update(InterviewState state, IPublishedEvent<SingleOptionLinkedQuestionAnswered> evnt) =>
            this.SetAnswerInState(state, Identity.Create(evnt.Payload.QuestionId, evnt.Payload.RosterVector),
                evnt.Payload.SelectedRosterVector.Select(Convert.ToInt32).ToArray());

        public void Update(InterviewState state, IPublishedEvent<MultipleOptionsLinkedQuestionAnswered> evnt) =>
            this.SetAnswerInState(state, Identity.Create(evnt.Payload.QuestionId, evnt.Payload.RosterVector),
                evnt.Payload.SelectedRosterVectors.Select(_ => _.Select(Convert.ToInt32).ToArray()).ToArray());

        public void Update(InterviewState state, IPublishedEvent<DateTimeQuestionAnswered> evnt) =>
            this.SetAnswerInState(state, Identity.Create(evnt.Payload.QuestionId, evnt.Payload.RosterVector), evnt.Payload.Answer);

        public void Update(InterviewState state, IPublishedEvent<GeoLocationQuestionAnswered> evnt) =>
            this.SetAnswerInState(state, Identity.Create(evnt.Payload.QuestionId, evnt.Payload.RosterVector),
                new GeoPosition(evnt.Payload.Latitude, evnt.Payload.Longitude, evnt.Payload.Accuracy,
                    evnt.Payload.Altitude, evnt.Payload.Timestamp));

        public void Update(InterviewState state, IPublishedEvent<QRBarcodeQuestionAnswered> evnt) =>
            this.SetAnswerInState(state, Identity.Create(evnt.Payload.QuestionId, evnt.Payload.RosterVector), evnt.Payload.Answer);

        public void Update(InterviewState state, IPublishedEvent<PictureQuestionAnswered> evnt) =>
            this.SetAnswerInState(state, Identity.Create(evnt.Payload.QuestionId, evnt.Payload.RosterVector), evnt.Payload.PictureFileName);

        public void Update(InterviewState state, IPublishedEvent<YesNoQuestionAnswered> evnt) =>
            this.SetAnswerInState(state, Identity.Create(evnt.Payload.QuestionId, evnt.Payload.RosterVector), evnt.Payload.AnsweredOptions);

        public void Update(InterviewState state, IPublishedEvent<AreaQuestionAnswered> evnt) =>
            this.SetAnswerInState(state, Identity.Create(evnt.Payload.QuestionId, evnt.Payload.RosterVector), new Area(
                evnt.Payload.Geometry, evnt.Payload.MapName, evnt.Payload.AreaSize,
                evnt.Payload.Length, evnt.Payload.Coordinates, evnt.Payload.DistanceToEditor));

        public void Update(InterviewState state, IPublishedEvent<AudioQuestionAnswered> evnt) =>
            this.SetAnswerInState(state, Identity.Create(evnt.Payload.QuestionId, evnt.Payload.RosterVector),
                AudioAnswer.FromString(evnt.Payload.FileName, evnt.Payload.Length));

        public void Update(InterviewState state, IPublishedEvent<AnswersRemoved> evnt)
        {
            foreach (var question in evnt.Payload.Questions)
                this.SetAnswerInState(state, question, null);
        }

        public void Update(InterviewState state, IPublishedEvent<GroupsDisabled> evnt)
        {
            foreach (var section in evnt.Payload.Groups)
                this.SetEnablementInState(state, section, false);
        }

        public void Update(InterviewState state, IPublishedEvent<GroupsEnabled> evnt)
        {
            foreach (var section in evnt.Payload.Groups)
                this.SetEnablementInState(state, section, true);
        }

        public void Update(InterviewState state, IPublishedEvent<StaticTextsEnabled> evnt)
        {
            foreach (var staticText in evnt.Payload.StaticTexts)
                this.SetEnablementInState(state, staticText, true);
        }

        public void Update(InterviewState state, IPublishedEvent<StaticTextsDisabled> evnt)
        {
            foreach (var staticText in evnt.Payload.StaticTexts)
                this.SetEnablementInState(state, staticText, false);
        }

        public void Update(InterviewState state, IPublishedEvent<StaticTextsDeclaredInvalid> evnt)
        {
            foreach (var staticText in evnt.Payload.FailedValidationConditions)
                this.SetValidityInState(state, staticText.Key, staticText.Value?.Select(y => y.FailedConditionIndex)?.ToArray());
        }

        public void Update(InterviewState state, IPublishedEvent<StaticTextsDeclaredValid> evnt)
        {
            foreach (var staticText in evnt.Payload.StaticTexts)
                this.SetValidityInState(state, staticText, null);
        }

        public void Update(InterviewState state, IPublishedEvent<QuestionsDisabled> evnt)
        {
            foreach (var question in evnt.Payload.Questions)
                this.SetEnablementInState(state, question, false);
        }

        public void Update(InterviewState state, IPublishedEvent<QuestionsEnabled> evnt)
        {
            foreach (var question in evnt.Payload.Questions)
                this.SetEnablementInState(state, question, true);
        }

        public void Update(InterviewState state, IPublishedEvent<AnswersDeclaredInvalid> evnt)
        {
            foreach (var question in evnt.Payload.FailedValidationConditions)
                this.SetValidityInState(state, question.Key, question.Value?.Select(y => y.FailedConditionIndex)?.ToArray());
        }

        public void Update(InterviewState state, IPublishedEvent<AnswersDeclaredValid> evnt)
        {
            foreach (var question in evnt.Payload.Questions)
                this.SetValidityInState(state, question, null);
        }

        public void Update(InterviewState state, IPublishedEvent<InterviewHardDeleted> evnt) =>
            this.repository.RemoveInterview(evnt.EventSourceId);

        public void Update(InterviewState state, IPublishedEvent<AnswerRemoved> evnt) =>
            this.SetAnswerInState(state, Identity.Create(evnt.Payload.QuestionId, evnt.Payload.RosterVector), null);

        public void Update(InterviewState state, IPublishedEvent<QuestionsMarkedAsReadonly> evnt)
        {
            foreach (var readOnlyQuestion in evnt.Payload.Questions)
                this.SetReadOnlyInState(state, readOnlyQuestion);
        }

        public void Update(InterviewState state, IPublishedEvent<VariablesChanged> evnt)
        {
            foreach (var variable in evnt.Payload.ChangedVariables)
                this.SetAnswerInState(state, variable.Identity, variable.NewValue);
        }

        public void Update(InterviewState state, IPublishedEvent<VariablesDisabled> evnt)
        {
            foreach (var variable in evnt.Payload.Variables)
                this.SetEnablementInState(state, variable, false);
        }

        public void Update(InterviewState state, IPublishedEvent<VariablesEnabled> evnt)
        {
            foreach (var variable in evnt.Payload.Variables)
                this.SetEnablementInState(state, variable, true);
        }

        public void Update(InterviewState state, IPublishedEvent<InterviewCreated> evnt) => 
            this.AddQuestionnaireToDictionary(evnt.EventSourceId, evnt.Payload.QuestionnaireId, evnt.Payload.QuestionnaireVersion);

        public void Update(InterviewState state, IPublishedEvent<InterviewFromPreloadedDataCreated> evnt) => 
            this.AddQuestionnaireToDictionary(evnt.EventSourceId, evnt.Payload.QuestionnaireId, evnt.Payload.QuestionnaireVersion);

        public void Update(InterviewState state, IPublishedEvent<InterviewOnClientCreated> evnt) =>
            this.AddQuestionnaireToDictionary(evnt.EventSourceId, evnt.Payload.QuestionnaireId, evnt.Payload.QuestionnaireVersion);

        private void SetEnablementInState(InterviewState state, Identity entityId, bool isEnabled)
        {
            if (state.Removed.Contains(entityId))
                state.Removed.Remove(entityId);

            state.Enablement[entityId] = isEnabled;
        }

        private void SetValidityInState(InterviewState state, Identity entityId, int[] invalidValidations)
        {
            if (state.Removed.Contains(entityId))
                state.Removed.Remove(entityId);

            state.Validity[entityId] = invalidValidations;
        }

        private void SetAnswerInState(InterviewState state, Identity entityId, object answer)
        {
            if (state.Removed.Contains(entityId))
                state.Removed.Remove(entityId);

            state.Answers[entityId] = answer;
        }

        private void SetReadOnlyInState(InterviewState state, Identity entityId)
        {
            if (state.Removed.Contains(entityId))
                state.Removed.Remove(entityId);

            if (!state.ReadOnly.Contains(entityId))
                state.ReadOnly.Add(entityId);
        }

        private void AddQuestionnaireToDictionary(Guid interviewId, Guid questionnaireId, long questionnaireVersion)
            => this.interviewToQuestionnaire[interviewId] = new QuestionnaireIdentity(questionnaireId, questionnaireVersion);

        protected override void SaveState(InterviewState state) =>
            this.repository.Save(this.interviewToQuestionnaire[state.Id], state);
    }
}
