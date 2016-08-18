using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.EventBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.DataTransferObjects;
using WB.Core.SharedKernels.Enumerator.Entities.Interview;
using WB.Core.SharedKernels.Enumerator.Events;

using Identity = WB.Core.SharedKernels.DataCollection.Identity;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Aggregates
{
    internal class StatefulInterview : Interview, IStatefulInterview
    {
        private class CalculatedState
        {
            public ConcurrentDictionary<Identity, ReadOnlyCollection<Identity>> EnabledInterviewerChildQuestions = new ConcurrentDictionary<Identity, ReadOnlyCollection<Identity>>();
            public ConcurrentDictionary<Identity, ReadOnlyCollection<Identity>> GroupsAndRostersInGroup = new ConcurrentDictionary<Identity, ReadOnlyCollection<Identity>>();
            public ConcurrentDictionary<Identity, bool> IsEnabled = new ConcurrentDictionary<Identity, bool>();
            public ConcurrentDictionary<Identity, ReadOnlyCollection<Identity>> EnabledStaticTextChildQuestions = new ConcurrentDictionary<Identity, ReadOnlyCollection<Identity>>();
        }

        private class LocalDelta
        {
            public HashSet<Identity> EnablementChanged = new HashSet<Identity>();
            public HashSet<Identity> ValidityChanged = new HashSet<Identity>();
            public HashSet<Identity> SubstitutionsInQuestionsChanged = new HashSet<Identity>();
            public HashSet<Identity> SubstitutionsInGroupsChanged = new HashSet<Identity>();
            public HashSet<Identity> SubstitutionsInStaticTextsChanged = new HashSet<Identity>();
        }

        private CalculatedState calculated = null;
        private LocalDelta delta = null;
        private IQuestionnaire cachedQuestionnaire = null;

        private readonly ConcurrentDictionary<string, BaseInterviewAnswer> answers;
        private readonly ConcurrentDictionary<string, InterviewEnablementState> groups;
        private readonly ConcurrentDictionary<Identity, int?> sortIndexesOfRosterInstanses;
        private readonly ConcurrentDictionary<string, bool> notAnsweredQuestionsValidityStatus;
        private readonly ConcurrentDictionary<string, IList<FailedValidationCondition>> notAnsweredFailedConditions;
        private readonly ConcurrentDictionary<string, bool> notAnsweredQuestionsEnablementStatus;
        private readonly ConcurrentDictionary<Identity, List<QuestionComment>> questionsComments;
        private bool createdOnClient;
        private bool hasLinkedOptionsChangedEvents = false;

        public StatefulInterview(ILogger logger,
                                 IQuestionnaireStorage questionnaireRepository,
                                 IInterviewExpressionStatePrototypeProvider expressionProcessorStatePrototypeProvider)
            : base(logger, questionnaireRepository, expressionProcessorStatePrototypeProvider)
        {
            this.answers = new ConcurrentDictionary<string, BaseInterviewAnswer>();
            this.groups = new ConcurrentDictionary<string, InterviewEnablementState>();
            this.sortIndexesOfRosterInstanses = new ConcurrentDictionary<Identity, int?>();
            this.notAnsweredQuestionsValidityStatus = new ConcurrentDictionary<string, bool>();
            this.notAnsweredQuestionsEnablementStatus = new ConcurrentDictionary<string, bool>();
            this.notAnsweredFailedConditions = new ConcurrentDictionary<string, IList<FailedValidationCondition>>();
            this.questionsComments = new ConcurrentDictionary<Identity, List<QuestionComment>>();

            this.ResetCalculatedState();
            this.ResetLocalDelta();
        }

        private void ResetCalculatedState()
        {
            this.calculated = new CalculatedState();
        }

        private void ResetLocalDelta()
        {
            this.delta = new LocalDelta();
        }

        protected new void Apply(SynchronizationMetadataApplied @event)
        {
            base.Apply(@event);
            this.InitializeCreatedInterview(this.EventSourceId, @event.QuestionnaireId, @event.QuestionnaireVersion);
            this.ResetLocalDelta();
        }

        protected new void Apply(InterviewOnClientCreated @event)
        {
            base.Apply(@event);
            this.InitializeCreatedInterview(this.EventSourceId, @event.QuestionnaireId, @event.QuestionnaireVersion);
            this.createdOnClient = true;
        }

        private void InitializeCreatedInterview(Guid eventSourceId, Guid questionnaireGuid, long version)
        {
            this.ResetCalculatedState();

            this.Id = eventSourceId;
            this.QuestionnaireIdentity = new QuestionnaireIdentity(questionnaireGuid, version);
        }



        #region Applying answers

        public new void Apply(InterviewSynchronized @event)
        {
            base.Apply(@event);
            this.ResetCalculatedState();

            this.createdOnClient = @event.InterviewData.CreatedOnClient;

            var orderedRosterInstances =
                @event.InterviewData.RosterGroupInstances.SelectMany(x => x.Value)
                    .OrderBy(x => x.OuterScopeRosterVector.Length)
                    .ToList();
            foreach (RosterSynchronizationDto roster in orderedRosterInstances)
            {
                AddRosterInstance(new AddedRosterInstance(roster.RosterId, roster.OuterScopeRosterVector,
                    roster.RosterInstanceId, roster.SortIndex));
                ChangeRosterTitle(
                    new RosterInstance(roster.RosterId, roster.OuterScopeRosterVector, roster.RosterInstanceId),
                    roster.RosterTitle);
            }

            @event.InterviewData.ValidAnsweredQuestions.ForEach(
                x => DeclareAnswerAsValid(x.Id, x.InterviewItemRosterVector));
            @event.InterviewData.FailedValidationConditions.ForEach(
                x => this.DeclareAnswerAsInvalid(x.Key.Id, x.Key.RosterVector, x.Value));

            @event.InterviewData.DisabledQuestions.ForEach(x => DisableQuestion(x.Id, x.InterviewItemRosterVector));
            @event.InterviewData.DisabledGroups.ForEach(x => DisableGroup(x.Id, x.InterviewItemRosterVector));
            @event.InterviewData.Answers.ForEach(x => BuildCommentsHistory(x.Id, x.QuestionRosterVector, x.AllComments, @event.InterviewData.SupervisorId, @event.InterviewData.UserId));

            SupervisorRejectComment = @event.InterviewData.Comments;

            this.ResetLocalDelta();
        }

        public void Apply(InterviewAnswersFromSyncPackageRestored @event)
        {
            foreach (var answerDto in @event.Answers)
            {
                this.SaveAnswerFromAnswerDto(answerDto);
            }

            this.ResetLocalDelta();
        }

        internal new void Apply(LinkedOptionsChanged @event)
        {
            base.Apply(@event);
            this.hasLinkedOptionsChangedEvents = true;
        }

        internal new void Apply(TextQuestionAnswered @event)
        {
            base.Apply(@event);
            this.ResetCalculatedState();

            var answer = this.GetOrCreateAnswer<TextAnswer>(@event);
            answer.SetAnswer(@event.Answer);
        }

        internal new void Apply(QRBarcodeQuestionAnswered @event)
        {
            base.Apply(@event);
            this.ResetCalculatedState();

            var answer = this.GetOrCreateAnswer<TextAnswer>(@event);
            answer.SetAnswer(@event.Answer);
        }

        internal new void Apply(PictureQuestionAnswered @event)
        {
            base.Apply(@event);
            this.ResetCalculatedState();

            var answer = this.GetOrCreateAnswer<MultimediaAnswer>(@event);
            answer.SetAnswer(@event.PictureFileName);
        }

        internal new void Apply(NumericRealQuestionAnswered @event)
        {
            base.Apply(@event);
            this.ResetCalculatedState();

            var answer = this.GetOrCreateAnswer<RealNumericAnswer>(@event);
            answer.SetAnswer(@event.Answer);
        }

        internal new void Apply(NumericIntegerQuestionAnswered @event)
        {
            base.Apply(@event);
            this.ResetCalculatedState();

            var answer = this.GetOrCreateAnswer<IntegerNumericAnswer>(@event);
            answer.SetAnswer(@event.Answer);
        }

        internal new void Apply(DateTimeQuestionAnswered @event)
        {
            base.Apply(@event);
            this.ResetCalculatedState();

            var answer = this.GetOrCreateAnswer<DateTimeAnswer>(@event);
            answer.SetAnswer(@event.Answer);
        }

        internal new void Apply(SingleOptionQuestionAnswered @event)
        {
            base.Apply(@event);
            this.ResetCalculatedState();

            var answer = this.GetOrCreateAnswer<SingleOptionAnswer>(@event);
            answer.SetAnswer(@event.SelectedValue);
        }

        internal new void Apply(MultipleOptionsQuestionAnswered @event)
        {
            base.Apply(@event);
            this.ResetCalculatedState();

            var answer = this.GetOrCreateAnswer<MultiOptionAnswer>(@event);
            answer.SetAnswers(@event.SelectedValues);
        }

        internal new void Apply(YesNoQuestionAnswered @event)
        {
            base.Apply(@event);
            this.ResetCalculatedState();

            var answer = this.GetOrCreateAnswer<YesNoAnswer>(@event);
            answer.SetAnswers(@event.AnsweredOptions);
        }

        internal new void Apply(GeoLocationQuestionAnswered @event)
        {
            base.Apply(@event);
            this.ResetCalculatedState();

            var answer = this.GetOrCreateAnswer<GpsCoordinatesAnswer>(@event);
            answer.SetAnswer(@event.Latitude, @event.Longitude, @event.Accuracy, @event.Altitude);
        }

        internal new void Apply(TextListQuestionAnswered @event)
        {
            base.Apply(@event);
            this.ResetCalculatedState();

            var answer = this.GetOrCreateAnswer<TextListAnswer>(@event);
            answer.SetAnswers(@event.Answers);
        }

        internal new void Apply(SingleOptionLinkedQuestionAnswered @event)
        {
            base.Apply(@event);
            this.ResetCalculatedState();

            var answer = this.GetOrCreateAnswer<LinkedSingleOptionAnswer>(@event);
            answer.SetAnswer(@event.SelectedRosterVector);
        }

        internal new void Apply(MultipleOptionsLinkedQuestionAnswered @event)
        {
            base.Apply(@event);
            this.ResetCalculatedState();

            var answer = this.GetOrCreateAnswer<LinkedMultiOptionAnswer>(@event);
            answer.SetAnswers(@event.SelectedRosterVectors);
        }

        #endregion

        internal new void Apply(AnswerCommented @event)
        {
            base.Apply(@event);
            this.ResetCalculatedState();
            this.CommentQuestion(@event.QuestionId, @event.RosterVector, @event.Comment, @event.UserId, @event.CommentTime);
        }


        #region Command handlers

        public void Complete(Guid userId, string comment, DateTime completeTime)
        {
            ThrowIfInterviewHardDeleted();
            this.ThrowIfInterviewStatusIsNotOneOfExpected(
                InterviewStatus.InterviewerAssigned, InterviewStatus.Restarted, InterviewStatus.RejectedBySupervisor);

            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();

            ReadOnlyCollection<Guid> allQuestions = questionnaire.GetAllQuestions();
            ReadOnlyCollection<Identity> allQuestionInstances = this.GetInstancesOfEntitiesWithSameAndDeeperRosterLevelOrThrow(
                this.interviewState, allQuestions, RosterVector.Empty, questionnaire).ToReadOnlyCollection();

            var validQuestions = allQuestionInstances.Where(this.delta.ValidityChanged.Contains).Where(question => this.IsValid(question)).ToArray();
            var invalidQuestions = allQuestionInstances.Where(this.delta.ValidityChanged.Contains).Where(question => !this.IsValid(question)).ToDictionary(
                question => question,
                question => this.GetFailedValidationConditions(question));

            var enabledQuestions = allQuestionInstances.Where(this.delta.EnablementChanged.Contains).Where(question => this.IsEnabled(question)).ToArray();
            var disabledQuestions = allQuestionInstances.Where(this.delta.EnablementChanged.Contains).Where(question => !this.IsEnabled(question)).ToArray();
            var linkedQuestionsOption = this.interviewState.LinkedQuestionOptions.Select(question => new ChangedLinkedOptions(question.Key, question.Value)).ToArray();

            ReadOnlyCollection<Guid> allGroups = questionnaire.GetAllGroups();
            ReadOnlyCollection<Identity> allGroupInstances = this.GetInstancesOfGroupsWithSameAndDeeperRosterLevelOrThrow(
                this.interviewState, allGroups, RosterVector.Empty, questionnaire).ToReadOnlyCollection();

            var enabledGroups = allGroupInstances.Where(this.delta.EnablementChanged.Contains).Where(group => this.IsEnabled(group)).ToArray();
            var disabledGroups = allGroupInstances.Where(this.delta.EnablementChanged.Contains).Where(group => !this.IsEnabled(group)).ToArray();
            var variableValues = this.interviewState.VariableValues.Select(v => new ChangedVariable(v.Key, v.Value)).ToArray();

            ReadOnlyCollection<Guid> allStaticTextIds = questionnaire.GetAllStaticTexts();
            ReadOnlyCollection<Identity> allStaticTextIdentities = this.GetInstancesOfEntitiesWithSameAndDeeperRosterLevelOrThrow(
                this.interviewState, allStaticTextIds, RosterVector.Empty, questionnaire).ToReadOnlyCollection();
            var validStaticTexts = allStaticTextIdentities.Where(this.delta.ValidityChanged.Contains).Where(this.IsValid).ToArray();
            var invalidStaticTexts = allStaticTextIdentities.Where(this.delta.ValidityChanged.Contains)
                .Where(staticText => !this.IsValid(staticText))
                .Select(staticText => new KeyValuePair<Identity, IReadOnlyList<FailedValidationCondition>>(
                    staticText,
                    this.GetFailedValidationConditions(staticText))).ToList();
            var enabledStaticTexts = allStaticTextIdentities.Where(this.delta.EnablementChanged.Contains).Where(this.IsEnabled).ToArray();
            var disabledStaticTexts = allStaticTextIdentities.Where(this.delta.EnablementChanged.Contains).Where(staticText => !this.IsEnabled(staticText)).ToArray();

            ReadOnlyCollection<Guid> allVariables = questionnaire.GetAllVariables();
            ReadOnlyCollection<Identity> alVariableInstances = this.GetInstancesOfEntitiesWithSameAndDeeperRosterLevelOrThrow(
                this.interviewState, allVariables, RosterVector.Empty, questionnaire).ToReadOnlyCollection();

            var enabledVariables = alVariableInstances.Where(this.delta.EnablementChanged.Contains).ToArray();
            var disableVariables = alVariableInstances.Where(v => !this.delta.EnablementChanged.Contains(v)).ToArray();


            if (validQuestions.Length > 0) this.ApplyEvent(new AnswersDeclaredValid(validQuestions));
            if (invalidQuestions.Count > 0) this.ApplyEvent(new AnswersDeclaredInvalid(invalidQuestions));

            if (enabledQuestions.Length > 0) this.ApplyEvent(new QuestionsEnabled(enabledQuestions));
            if (disabledQuestions.Length > 0) this.ApplyEvent(new QuestionsDisabled(disabledQuestions));

            if (enabledGroups.Length > 0) this.ApplyEvent(new GroupsEnabled(enabledGroups));
            if (disabledGroups.Length > 0) this.ApplyEvent(new GroupsDisabled(disabledGroups));

            if (enabledStaticTexts.Length > 0) this.ApplyEvent(new StaticTextsEnabled(enabledStaticTexts));
            if (disabledStaticTexts.Length > 0) this.ApplyEvent(new StaticTextsDisabled(disabledStaticTexts));

            if (validStaticTexts.Length > 0) this.ApplyEvent(new StaticTextsDeclaredValid(validStaticTexts));
            if (invalidStaticTexts.Count > 0) this.ApplyEvent(new StaticTextsDeclaredInvalid(invalidStaticTexts));

            if (linkedQuestionsOption.Length > 0) this.ApplyEvent(new LinkedOptionsChanged(linkedQuestionsOption));

            if (enabledVariables.Length > 0) this.ApplyEvent(new VariablesEnabled(enabledVariables));
            if (disableVariables.Length > 0) this.ApplyEvent(new VariablesDisabled(disableVariables));
            if (variableValues.Length > 0) this.ApplyEvent(new VariablesChanged(variableValues));

            this.ApplyEvent(new InterviewCompleted(userId, completeTime, comment));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.Completed, comment));

            if (this.delta.SubstitutionsInGroupsChanged.Any() ||
                this.delta.SubstitutionsInQuestionsChanged.Any() ||
                this.delta.SubstitutionsInStaticTextsChanged.Any())
            {
                this.ApplyEvent(new SubstitutionTitlesChanged(this.delta.SubstitutionsInQuestionsChanged.ToArray(),
                    this.delta.SubstitutionsInStaticTextsChanged.ToArray(),
                    this.delta.SubstitutionsInGroupsChanged.ToArray()));
            }

            this.ApplyEvent(this.HasInvalidAnswers() || this.HasInvalidStaticTexts
                ? new InterviewDeclaredInvalid() as IEvent
                : new InterviewDeclaredValid());
        }

        #endregion


        #region Group and question status and validity

        public new void Apply(AnswersRemoved @event)
        {
            base.Apply(@event);
            this.ResetCalculatedState();

            @event.Questions.ForEach(x =>
            {
                var questionId = ConversionHelper.ConvertIdAndRosterVectorToString(x.Id, x.RosterVector);
                if (this.Answers.ContainsKey(questionId))
                {
                    this.Answers[questionId].RemoveAnswer();
                }
            });
        }

        internal new void Apply(AnswerRemoved @event)
        {
            base.Apply(@event);
            this.ResetCalculatedState();

            var questionId = ConversionHelper.ConvertIdAndRosterVectorToString(@event.QuestionId, @event.RosterVector);
            if (this.Answers.ContainsKey(questionId))
                this.Answers[questionId].RemoveAnswer();
        }

        public new void Apply(SubstitutionTitlesChanged @event)
        {
            @event.Groups.ForEach(x => this.delta.SubstitutionsInGroupsChanged.Add(new Identity(x.Id, x.RosterVector)));
            @event.Questions.ForEach(x => this.delta.SubstitutionsInQuestionsChanged.Add(new Identity(x.Id, x.RosterVector)));
            @event.StaticTexts.ForEach(x => this.delta.SubstitutionsInStaticTextsChanged.Add(new Identity(x.Id, x.RosterVector)));
        }

        public new void Apply(AnswersDeclaredValid @event)
        {
            base.Apply(@event);
            this.ResetCalculatedState();

            @event.Questions.ForEach(x => this.DeclareAnswerAsValid(x.Id, x.RosterVector));
            @event.Questions.ForEach(x => this.delta.ValidityChanged.Add(new Identity(x.Id, x.RosterVector)));
        }

        public new void Apply(AnswersDeclaredInvalid @event)
        {
            base.Apply(@event);
            this.ResetCalculatedState();

            @event.FailedValidationConditions.ForEach(x => this.DeclareAnswerAsInvalid(x.Key.Id, x.Key.RosterVector, x.Value.ToList()));
            @event.Questions.ForEach(x => this.delta.ValidityChanged.Add(new Identity(x.Id, x.RosterVector)));
        }

        public new void Apply(GroupsDisabled @event)
        {
            base.Apply(@event);
            this.ResetCalculatedState();

            @event.Groups.ForEach(x => this.DisableGroup(x.Id, x.RosterVector));
            @event.Groups.ForEach(x => this.delta.EnablementChanged.Add(new Identity(x.Id, x.RosterVector)));
        }


        public new void Apply(GroupsEnabled @event)
        {
            base.Apply(@event);
            this.ResetCalculatedState();

            @event.Groups.ForEach(x =>
            {
                var groupOrRoster = this.GetOrCreateGroupOrRoster(x.Id, x.RosterVector);
                groupOrRoster.IsDisabled = false;
            });
            @event.Groups.ForEach(x => this.delta.EnablementChanged.Add(new Identity(x.Id, x.RosterVector)));
        }

        public new void Apply(VariablesEnabled @event)
        {
            base.Apply(@event);

            @event.Variables.ForEach(x => this.delta.EnablementChanged.Add(new Identity(x.Id, x.RosterVector)));
        }


        public new void Apply(VariablesDisabled @event)
        {
            base.Apply(@event);

            @event.Variables.ForEach(x => this.delta.EnablementChanged.Remove(new Identity(x.Id, x.RosterVector)));
        }

        public new void Apply(QuestionsDisabled @event)
        {
            base.Apply(@event);
            this.ResetCalculatedState();

            @event.Questions.ForEach(x => this.DisableQuestion(x.Id, x.RosterVector));
            @event.Questions.ForEach(x => this.delta.EnablementChanged.Add(new Identity(x.Id, x.RosterVector)));
        }

        public new void Apply(QuestionsEnabled @event)
        {
            base.Apply(@event);
            this.ResetCalculatedState();

            @event.Questions.ForEach(x =>
            {
                var questionKey = ConversionHelper.ConvertIdAndRosterVectorToString(x.Id, x.RosterVector);
                var answer = this.GetExistingAnswerOrNull(questionKey);
                if (answer != null)
                {
                    answer.IsEnabled = true;
                }
                else
                {
                    this.notAnsweredQuestionsEnablementStatus[questionKey] = true;
                }
            });
            @event.Questions.ForEach(x => this.delta.EnablementChanged.Add(new Identity(x.Id, x.RosterVector)));
        }

        public new void Apply(StaticTextsEnabled @event)
        {
            base.Apply(@event);
            this.ResetCalculatedState();

            @event.StaticTexts.ForEach(x => this.delta.EnablementChanged.Add(new Identity(x.Id, x.RosterVector)));
        }

        public new void Apply(StaticTextsDisabled @event)
        {
            base.Apply(@event);
            this.ResetCalculatedState();

            @event.StaticTexts.ForEach(x => this.delta.EnablementChanged.Add(new Identity(x.Id, x.RosterVector)));
        }

        public new void Apply(StaticTextsDeclaredValid @event)
        {
            base.Apply(@event);
            this.ResetCalculatedState();

            @event.StaticTexts.ForEach(x => this.delta.ValidityChanged.Add(new Identity(x.Id, x.RosterVector)));
        }

        public new void Apply(StaticTextsDeclaredInvalid @event)
        {
            base.Apply(@event);
            this.ResetCalculatedState();

            @event.GetFailedValidationConditionsDictionary().Keys.ForEach(x => this.delta.ValidityChanged.Add(new Identity(x.Id, x.RosterVector)));
        }

        #endregion

        #region Roster instances and titles

        public new void Apply(RosterInstancesTitleChanged @event)
        {
            base.Apply(@event);
            this.ResetCalculatedState();

            foreach (var changedRosterInstanceTitle in @event.ChangedInstances)
            {
                this.ChangeRosterTitle(changedRosterInstanceTitle.RosterInstance, changedRosterInstanceTitle.Title);
            }
        }

        void ChangeRosterTitle(RosterInstance rosterInstance, string title)
        {
            var rosterKey = ConversionHelper.ConvertIdAndRosterVectorToString(
                rosterInstance.GroupId,
                GetFullRosterVector(rosterInstance));
            var roster = (InterviewRoster)this.groups[rosterKey];
            roster.Title = title;
        }

        public new void Apply(RosterInstancesAdded @event)
        {
            base.Apply(@event);
            this.ResetCalculatedState();

            foreach (var rosterInstance in @event.Instances)
            {
                this.AddRosterInstance(rosterInstance);
            }
        }

        public new void Apply(RosterInstancesRemoved @event)
        {
            base.Apply(@event);
            this.ResetCalculatedState();

            foreach (var rosterInstance in @event.Instances)
            {
                var fullRosterVector = GetFullRosterVector(rosterInstance);

                var rosterKey = ConversionHelper.ConvertIdAndRosterVectorToString(rosterInstance.GroupId, fullRosterVector);
                InterviewEnablementState removedGroup;
                this.groups.TryRemove(rosterKey, out removedGroup);

                int? removeSortIndex;
                this.sortIndexesOfRosterInstanses.TryRemove(new Identity(rosterInstance.GroupId, fullRosterVector), out removeSortIndex);
            }
        }

        #endregion

        #region Interview status and validity

        public new void Apply(InterviewCompleted @event)
        {
            base.Apply(@event);
            this.ResetCalculatedState();

            this.InterviewerCompleteComment = @event.Comment;
            this.IsCompleted = true;
        }

        public new void Apply(InterviewRejected @event)
        {
            base.Apply(@event);

            this.SupervisorRejectComment = @event.Comment;
        }

        public new void Apply(InterviewRestarted @event)
        {
            base.Apply(@event);
            this.ResetCalculatedState();

            this.IsCompleted = false;
        }

        public new void Apply(InterviewDeclaredValid @event)
        {
            base.Apply(@event);
            this.ResetCalculatedState();

            this.HasErrors = false;
        }

        public new void Apply(InterviewDeclaredInvalid @event)
        {
            base.Apply(@event);
            this.ResetCalculatedState();

            this.HasErrors = true;
        }

        #endregion

        public QuestionnaireIdentity QuestionnaireIdentity { get; set; }
        public string QuestionnaireId => this.QuestionnaireIdentity.ToString();
        public Guid InterviewerId => this.interviewerId;
        public InterviewStatus Status => this.status;
        public Guid Id { get; set; }
        public string InterviewerCompleteComment { get; private set; }
        public string SupervisorRejectComment { get; private set; }

        public IReadOnlyDictionary<string, BaseInterviewAnswer> Answers
        {
            get
            {
                return new ReadOnlyDictionary<string, BaseInterviewAnswer>(this.answers);
            }
        }

        public string Language => this.language;

        public bool HasErrors { get; private set; }

        public bool IsCompleted { get; private set; }

        public InterviewRoster GetRoster(Identity identity)
        {
            return (InterviewRoster)this.groups[ConversionHelper.ConvertIdentityToString(identity)];
        }

        public GpsCoordinatesAnswer GetGpsCoordinatesAnswer(Identity identity)
        {
            return this.GetQuestionAnswer<GpsCoordinatesAnswer>(identity);
        }

        public DateTimeAnswer GetDateTimeAnswer(Identity identity)
        {
            return this.GetQuestionAnswer<DateTimeAnswer>(identity);
        }

        public MultimediaAnswer GetMultimediaAnswer(Identity identity)
        {
            return this.GetQuestionAnswer<MultimediaAnswer>(identity);
        }

        public TextAnswer GetQRBarcodeAnswer(Identity identity)
        {
            return this.GetQuestionAnswer<TextAnswer>(identity);
        }

        public virtual TextListAnswer GetTextListAnswer(Identity identity)
        {
            return this.GetQuestionAnswer<TextListAnswer>(identity);
        }

        public LinkedSingleOptionAnswer GetLinkedSingleOptionAnswer(Identity identity)
        {
            return this.GetQuestionAnswer<LinkedSingleOptionAnswer>(identity);
        }

        public MultiOptionAnswer GetMultiOptionAnswer(Identity identity)
        {
            return this.GetQuestionAnswer<MultiOptionAnswer>(identity);
        }

        public LinkedMultiOptionAnswer GetLinkedMultiOptionAnswer(Identity identity)
        {
            return this.GetQuestionAnswer<LinkedMultiOptionAnswer>(identity);
        }

        public IntegerNumericAnswer GetIntegerNumericAnswer(Identity identity)
        {
            return this.GetQuestionAnswer<IntegerNumericAnswer>(identity);
        }

        public RealNumericAnswer GetRealNumericAnswer(Identity identity)
        {
            return this.GetQuestionAnswer<RealNumericAnswer>(identity);
        }

        public TextAnswer GetTextAnswer(Identity identity)
        {
            return this.GetQuestionAnswer<TextAnswer>(identity);
        }

        public SingleOptionAnswer GetSingleOptionAnswer(Identity identity)
        {
            return this.GetQuestionAnswer<SingleOptionAnswer>(identity);
        }

        public YesNoAnswer GetYesNoAnswer(Identity identity)
        {
            return this.GetQuestionAnswer<YesNoAnswer>(identity);
        }

        public void RestoreInterviewStateFromSyncPackage(Guid userId, InterviewSynchronizationDto synchronizedInterview)
        {
            ThrowIfInterviewHardDeleted();
            IQuestionnaire questionnaire = GetQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion, this.language);
            var answerDtos = synchronizedInterview
                .Answers
                .Select(answerDto => new InterviewAnswerDto(answerDto.Id, answerDto.QuestionRosterVector, questionnaire.GetAnswerType(answerDto.Id), answerDto.Answer))
                .ToArray();

            this.ApplyEvent(new InterviewSynchronized(synchronizedInterview));
            this.ApplyEvent(new InterviewAnswersFromSyncPackageRestored(answerDtos, synchronizedInterview.UserId));
        }

        public bool HasGroup(Identity group)
        {
            if (group == null)
                return false;

            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();

            if (!questionnaire.HasGroup(group.Id))
                return false;

            Guid[] rosterIdsStartingFromTop = questionnaire.GetRostersFromTopToSpecifiedGroup(@group.Id).ToArray();

            if (!DoesRosterVectorLengthCorrespondToParentRosterGroupsCount(group.RosterVector, rosterIdsStartingFromTop))
                return false;

            if (!this.DoesRosterInstanceExist(this.interviewState, group.RosterVector, rosterIdsStartingFromTop))
                return false;

            return true;
        }

        public string GetRosterTitle(Identity rosterIdentity)
        {
            var convertIdentityToString = ConversionHelper.ConvertIdentityToString(rosterIdentity);

            if (!this.groups.ContainsKey(convertIdentityToString)) return string.Empty;

            var roster = this.groups[convertIdentityToString] as InterviewRoster;

            return roster == null ? string.Empty : roster.Title;
        }

        public BaseInterviewAnswer FindBaseAnswerByOrDeeperRosterLevel(Guid questionId, RosterVector targetRosterVector)
        {
            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();

            int questionRosterLevel = questionnaire.GetRosterLevelForQuestion(questionId);
            var rosterVector = targetRosterVector.Shrink(questionRosterLevel);
            var questionKey = ConversionHelper.ConvertIdAndRosterVectorToString(questionId, rosterVector);

            return this.Answers.ContainsKey(questionKey) ? this.Answers[questionKey] : null;
        }

        public object GetVariableValueByOrDeeperRosterLevel(Guid variableId, RosterVector variableRosterVector)
        {
            do
            {
                var variableIdentity = new Identity(variableId, variableRosterVector);

                if (this.interviewState.VariableValues.ContainsKey(variableIdentity))
                    return this.interviewState.VariableValues[variableIdentity];

                if (variableRosterVector.Length == 0) break;

                variableRosterVector = variableRosterVector.Shrink(variableRosterVector.Length - 1);
            } while (variableRosterVector.Length >= 0);

            return null;
        }

        public IEnumerable<BaseInterviewAnswer> FindAnswersOfReferencedQuestionForLinkedQuestion(Guid referencedQuestionId, Identity linkedQuestion)
        {
            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();

            if (!this.interviewState.LinkedQuestionOptions.ContainsKey(linkedQuestion))
                return Enumerable.Empty<BaseInterviewAnswer>();

            var linkedQuestionOptions = this.interviewState.LinkedQuestionOptions[linkedQuestion];

            IEnumerable<Identity> targetQuestions =
                linkedQuestionOptions.Select(x => new Identity(referencedQuestionId, x));

            var rostersFromTopToSpecifiedQuestion = questionnaire.GetRostersFromTopToSpecifiedQuestion(referencedQuestionId).ToArray();

            var answers = targetQuestions
                .Select(this.GetExistingAnswerOrNull)
                .Where(answer => answer != null).ToArray()
                .OrderBy(x => x.RosterVector.Length);

            for (int rosterDepth = 1; rosterDepth <= rostersFromTopToSpecifiedQuestion.Length; rosterDepth++)
            {
                var currentDepth = rosterDepth;
                answers =
                    answers.ThenBy(
                        x =>
                            GetRosterSortIndex(new Identity(rostersFromTopToSpecifiedQuestion[currentDepth - 1],
                                new RosterVector(x.RosterVector.Take(currentDepth)))));
            }
            return answers.ToArray();
        }

        public IEnumerable<InterviewRoster> FindReferencedRostersForLinkedQuestion(Guid rosterId, Identity linkedQuestion)
        {
            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();

            if (!this.interviewState.LinkedQuestionOptions.ContainsKey(linkedQuestion))
                return Enumerable.Empty<InterviewRoster>();

            RosterVector[] targetRosters = this.interviewState.LinkedQuestionOptions[linkedQuestion];

            var rosters =
                targetRosters
                    .Select(x => this.GetRoster(new Identity(rosterId, x))).ToArray()
                    .OrderBy(x => x.RosterVector.Length);

            var rostersFromTopToSpecifiedQuestion = questionnaire.GetRostersFromTopToSpecifiedGroup(rosterId).ToArray();
            for (int rosterDepth = 1; rosterDepth <= rostersFromTopToSpecifiedQuestion.Length; rosterDepth++)
            {
                var currentDepth = rosterDepth;
                rosters =
                    rosters.ThenBy(
                        x =>
                            GetRosterSortIndex(new Identity(rostersFromTopToSpecifiedQuestion[currentDepth - 1],
                                new RosterVector(x.RosterVector.Take(currentDepth)))));
            }
            return rosters.ToArray();
        }

        private void SaveAnswerFromAnswerDto(InterviewAnswerDto answerDto)
        {
            switch (answerDto.Type)
            {
                case AnswerType.Integer:
                    this.GetOrCreateAnswer<IntegerNumericAnswer>(answerDto.Id, answerDto.RosterVector)
                        .SetAnswer(answerDto.Answer == null ? null : (int?)Convert.ToInt32(answerDto.Answer));
                    break;
                case AnswerType.Decimal:
                    this.GetOrCreateAnswer<RealNumericAnswer>(answerDto.Id, answerDto.RosterVector)
                        .SetAnswer((decimal?)answerDto.Answer);
                    break;
                case AnswerType.DateTime:
                    this.GetOrCreateAnswer<DateTimeAnswer>(answerDto.Id, answerDto.RosterVector)
                        .SetAnswer((DateTime?)answerDto.Answer);
                    break;
                case AnswerType.OptionCodeArray:
                    this.GetOrCreateAnswer<MultiOptionAnswer>(answerDto.Id, answerDto.RosterVector)
                        .SetAnswers(answerDto.Answer as decimal[]);
                    break;
                case AnswerType.YesNoArray:
                    this.GetOrCreateAnswer<YesNoAnswer>(answerDto.Id, answerDto.RosterVector)
                        .SetAnswers((AnsweredYesNoOption[])answerDto.Answer);
                    break;
                case AnswerType.RosterVectorArray:
                    this.GetOrCreateAnswer<LinkedMultiOptionAnswer>(answerDto.Id, answerDto.RosterVector)
                        .SetAnswers(answerDto.Answer as decimal[][]);
                    break;
                case AnswerType.OptionCode:
                    this.GetOrCreateAnswer<SingleOptionAnswer>(answerDto.Id, answerDto.RosterVector)
                        .SetAnswer(answerDto.Answer == null ? null : (decimal?)Convert.ToDecimal(answerDto.Answer));
                    break;
                case AnswerType.RosterVector:
                    this.GetOrCreateAnswer<LinkedSingleOptionAnswer>(answerDto.Id, answerDto.RosterVector)
                        .SetAnswer(answerDto.Answer as decimal[]);
                    break;
                case AnswerType.DecimalAndStringArray:
                    this.GetOrCreateAnswer<TextListAnswer>(answerDto.Id, answerDto.RosterVector)
                        .SetAnswers(answerDto.Answer as Tuple<decimal, string>[]);
                    break;
                case AnswerType.String:
                    this.GetOrCreateAnswer<TextAnswer>(answerDto.Id, answerDto.RosterVector).SetAnswer((string)answerDto.Answer);
                    break;
                case AnswerType.GpsData:
                    var geoAnswer = answerDto.Answer as GeoPosition;
                    var gpsQuestion = this.GetOrCreateAnswer<GpsCoordinatesAnswer>(answerDto.Id, answerDto.RosterVector);
                    if (geoAnswer != null)
                        this.GetOrCreateAnswer<GpsCoordinatesAnswer>(answerDto.Id, answerDto.RosterVector)
                        .SetAnswer(geoAnswer.Latitude, geoAnswer.Longitude, geoAnswer.Accuracy, geoAnswer.Altitude);
                    else
                        gpsQuestion.RemoveAnswer();
                    break;
                case AnswerType.FileName:
                    this.GetOrCreateAnswer<MultimediaAnswer>(answerDto.Id, answerDto.RosterVector)
                        .SetAnswer((string)answerDto.Answer);
                    break;
            }
        }

        private void DeclareAnswerAsValid(Guid id, RosterVector rosterVector)
        {
            var questionKey = ConversionHelper.ConvertIdAndRosterVectorToString(id, rosterVector);
            var answer = this.GetExistingAnswerOrNull(questionKey);
            if (answer != null)
            {
                answer.IsValid = true;
                answer.FailedValidations.Clear();
            }
            else
            {
                this.notAnsweredQuestionsValidityStatus[questionKey] = true;
            }
        }

        private void DeclareAnswerAsInvalid(Guid id, RosterVector rosterVector, IList<FailedValidationCondition> value)
        {
            var questionKey = ConversionHelper.ConvertIdAndRosterVectorToString(id, rosterVector);
            var answer = this.GetExistingAnswerOrNull(questionKey);
            if (answer != null)
            {
                answer.IsValid = false;
                answer.FailedValidations.Clear();
                foreach (var failedValidationCondition in value)
                {
                    answer.FailedValidations.Add(failedValidationCondition);
                }
            }
            else
            {
                this.notAnsweredQuestionsValidityStatus[questionKey] = false;
                this.notAnsweredFailedConditions[questionKey] = value;
            }
        }

        private void DisableGroup(Guid id, RosterVector rosterVector)
        {
            var groupOrRoster = this.GetOrCreateGroupOrRoster(id, rosterVector);
            groupOrRoster.IsDisabled = true;
        }

        private void BuildCommentsHistory(Guid questionId, RosterVector rosterVector, CommentSynchronizationDto[] commentDtos, Guid? supervisorId, Guid interviewerId)
        {
            if (commentDtos == null || !commentDtos.Any())
                return;

            var commentsHistory = new List<QuestionComment>();
            QuestionComment lastComment = null;
            foreach (var commentDto in commentDtos)
            {
                if (lastComment != null && lastComment.UserId == commentDto.UserId)
                {
                    lastComment.Comment += Environment.NewLine + commentDto.Text;
                    continue;
                }

                var role = GetUserRole(supervisorId, interviewerId, commentDto);

                var comment = new QuestionComment(commentDto.Text, commentDto.UserId, role);
                commentsHistory.Add(comment);
                lastComment = comment;
            }

            var questionIdentity = new Identity(questionId, rosterVector);

            this.questionsComments[questionIdentity] = commentsHistory;
        }

        [Obsolete("Removed after clients with version 5.11 and below will complete theirs surveys. Release date: 1st of Sep 2016")]
        private static UserRoles GetUserRole(Guid? supervisorId, Guid interviewerId, CommentSynchronizationDto commentDto)
        {
            UserRoles role;
            if (commentDto.UserId == interviewerId)
            {
                role = UserRoles.Operator;
            }
            else if (supervisorId.HasValue && commentDto.UserId == supervisorId)
            {
                role = UserRoles.Supervisor;
            }
            else
            {
                // Decided to mark those comments as Supervisor's by default.
                role = commentDto.UserRole ?? UserRoles.Supervisor;
            }
            return role;
        }

        private void CommentQuestion(Guid questionId, RosterVector rosterVector, string comment, Guid userId, DateTime commentTime)
        {
            var questionComment = new QuestionComment(comment, userId, UserRoles.Operator);

            var questionIdentity = new Identity(questionId, rosterVector);

            if (!this.questionsComments.ContainsKey(questionIdentity))
                this.questionsComments[questionIdentity] = new List<QuestionComment>();

            this.questionsComments[questionIdentity].Add(questionComment);
        }

        private void DisableQuestion(Guid id, RosterVector rosterVector)
        {
            var questionKey = ConversionHelper.ConvertIdAndRosterVectorToString(id, rosterVector);
            var answer = this.GetExistingAnswerOrNull(questionKey);
            if (answer != null)
            {
                answer.IsEnabled = false;
            }
            else
            {
                this.notAnsweredQuestionsEnablementStatus[questionKey] = false;
            }
        }

        private void AddRosterInstance(AddedRosterInstance rosterInstance)
        {
            var rosterIdentity = new Identity(rosterInstance.GroupId, GetFullRosterVector(rosterInstance));

            var rosterKey = ConversionHelper.ConvertIdentityToString(rosterIdentity);

            this.groups[rosterKey] = new InterviewRoster
            {
                Id = rosterIdentity.Id,
                RosterVector = rosterIdentity.RosterVector,
                ParentRosterVector = rosterInstance.OuterRosterVector,
                RowCode = rosterInstance.RosterInstanceId
            };
            this.sortIndexesOfRosterInstanses[rosterIdentity] = rosterInstance.SortIndex;
        }

        public InterviewRoster FindRosterByOrDeeperRosterLevel(Guid rosterId, RosterVector targetRosterVector)
        {
            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();

            int rosterLevel = questionnaire.GetRosterLevelForGroup(rosterId);
            var rosterVector = targetRosterVector.Shrink(rosterLevel);
            var rosterKey = ConversionHelper.ConvertIdAndRosterVectorToString(rosterId, rosterVector);

            return this.groups.ContainsKey(rosterKey) ? this.groups[rosterKey] as InterviewRoster : null;
        }

        public IEnumerable<string> GetParentRosterTitlesWithoutLast(Guid questionId, RosterVector rosterVector)
        {
            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();
            return GetParentRosterTitlesWithoutLast(questionnaire.GetRostersFromTopToSpecifiedQuestion(questionId), rosterVector);
        }

        public IEnumerable<string> GetParentRosterTitlesWithoutLastForRoster(Guid rosterId, RosterVector rosterVector)
        {
            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();
            return GetParentRosterTitlesWithoutLast(questionnaire.GetRostersFromTopToSpecifiedGroup(rosterId), rosterVector);
        }

        private IEnumerable<string> GetParentRosterTitlesWithoutLast(IEnumerable<Guid> parentRosters, RosterVector rosterVector)
        {
            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();
            var parentRostersWithout = parentRosters.WithoutLast().ToArray();
            foreach (var parentRosterId in parentRostersWithout)
            {
                int parentRosterLevel = questionnaire.GetRosterLevelForGroup(parentRosterId);
                var parentRosterVector = rosterVector.Shrink(parentRosterLevel);
                yield return this.GetRosterTitle(new Identity(parentRosterId, parentRosterVector));
            }
        }

        public int CountInterviewerQuestionsInGroupRecursively(Identity groupIdentity)
        {
            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();
            IEnumerable<Guid> allQuestionsInGroup = questionnaire.GetAllUnderlyingInterviewerQuestions(groupIdentity.Id);

            return this
                .GetInstancesOfEntitiesWithSameAndDeeperRosterLevelOrThrow(this.interviewState, allQuestionsInGroup, groupIdentity.RosterVector, questionnaire)
                .Count();
        }

        public int CountActiveInterviewerQuestionsInGroupRecursively(Identity groupIdentity)
        {
            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();
            IEnumerable<Guid> allQuestionsInGroup = questionnaire.GetAllUnderlyingInterviewerQuestions(groupIdentity.Id);

            var questionInstances = this
                .GetInstancesOfEntitiesWithSameAndDeeperRosterLevelOrThrow(this.interviewState, allQuestionsInGroup, groupIdentity.RosterVector, questionnaire)
                .ToList();

            return questionInstances.Count(this.IsEnabled);
        }

        public int CountActiveInterviewerQuestionsInGroupOnly(Identity group)
        {
            if (group == null) throw new ArgumentNullException("group");
            return this
                .GetEnabledInterviewerChildQuestions(group)
                .Count;
        }

        public int GetGroupsInGroupCount(Identity group)
        {
            return this
                .GetGroupsAndRostersInGroup(group)
                .Count();
        }

        public int CountAnsweredQuestionsInInterview()
        {
            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();
            var sectionInstances = questionnaire.GetAllSections().Select(x => new Identity(x, new decimal[0]));

            return sectionInstances.Sum(section => this.CountAnsweredInterviewerQuestionsInGroupRecursively(section));
        }

        public int CountActiveQuestionsInInterview()
        {
            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();
            var sectionInstances = questionnaire.GetAllSections().Select(x => new Identity(x, new decimal[0]));

            return sectionInstances.Sum(section => this.CountActiveInterviewerQuestionsInGroupRecursively(section));
        }

        public int CountInvalidEntitiesInInterview()
        {
            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();
            var sectionInstances = questionnaire.GetAllSections().Select(x => new Identity(x, new decimal[0]));

            return sectionInstances.Sum(section => this.CountInvalidInterviewerAnswersInGroupRecursively(section) + this.CountInvalidStaticTextsInGroupRecursively(section));
        }

        public bool HasLinkedOptionsChangedEvents => this.hasLinkedOptionsChangedEvents;

        [Obsolete("it should be removed when all clients has version 5.7 or higher")]
        public void MigrateLinkedOptionsToFiltered()
        {
            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();

            var expressionProcessorState = this.ExpressionProcessorStatePrototype.Clone();

            var linkedQuestionOptionsChanges = this.CreateChangedLinkedOptions(
                expressionProcessorState,
                this.interviewState,
                questionnaire, null,
                null, null, null).ToArray();

            this.ApplyEvent(new LinkedOptionsChanged(linkedQuestionOptionsChanges));
        }

        public int CountAnsweredInterviewerQuestionsInGroupRecursively(Identity groupIdentity)
        {
            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();
            IEnumerable<Guid> allQuestionsInGroup = questionnaire.GetAllUnderlyingInterviewerQuestions(groupIdentity.Id);

            var questionInstances = this
                .GetInstancesOfEntitiesWithSameAndDeeperRosterLevelOrThrow(this.interviewState, allQuestionsInGroup, groupIdentity.RosterVector, questionnaire)
                .ToList();

            return questionInstances.Count(this.WasAnswered);
        }

        public int CountAnsweredInterviewerQuestionsInGroupOnly(Identity group)
        {
            return this
                .GetEnabledInterviewerChildQuestions(group)
                .Count(this.WasAnswered);
        }

        public int CountInvalidInterviewerAnswersInGroupRecursively(Identity groupIdentity)
        {
            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();
            IEnumerable<Guid> allQuestionsInGroup = questionnaire.GetAllUnderlyingInterviewerQuestions(groupIdentity.Id);

            var questionInstances = this
                .GetInstancesOfEntitiesWithSameAndDeeperRosterLevelOrThrow(this.interviewState, allQuestionsInGroup, groupIdentity.RosterVector, questionnaire)
                .Select(ConversionHelper.ConvertIdentityToString)
                .ToList();

            return this.Answers.Where(x => questionInstances.Contains(x.Key)).Count(
                x => x.Value != null
                && x.Value.IsEnabled
                && x.Value.IsAnswered
                && !x.Value.IsValid);
        }


        public int CountInvalidStaticTextsInGroupRecursively(Identity groupIdentity)
        {
            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();
            IEnumerable<Guid> allQuestionsInGroup = questionnaire.GetAllUnderlyingStaticTexts(groupIdentity.Id);

            List<Identity> questionInstances = this
                .GetInstancesOfEntitiesWithSameAndDeeperRosterLevelOrThrow(this.interviewState, allQuestionsInGroup, groupIdentity.RosterVector, questionnaire)
                .ToList();

            IEnumerable<Identity> allInvalidChildStaticTexts = this.interviewState.InvalidStaticTexts.Where(x => questionInstances.Contains(x.Key))
                                                                                  .Select(x => x.Key);

            return allInvalidChildStaticTexts.Count(x => !this.interviewState.DisabledStaticTexts.Contains(x));
        }

        public int CountInvalidInterviewerEntitiesInGroupOnly(Identity group)
        {
            return this
                .GetEnabledInterviewerChildQuestions(group)
                .Union(this.GetEnabledInvalidChildStaticTexts(group))
                .Count(question => !this.IsValid(question));
        }

        public IEnumerable<Identity> GetInvalidEntitiesInInterview()
        {
            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();
            var sectionInstances = questionnaire.GetAllSections().Select(x => new Identity(x, new decimal[0]));

            foreach (var sectionInstance in sectionInstances)
            {
                IEnumerable<Guid> allQuestionsInGroup = questionnaire.GetAllUnderlyingInterviewerQuestions(sectionInstance.Id);
                IEnumerable<Guid> allStaticTextInGroup = questionnaire.GetAllUnderlyingStaticTexts(sectionInstance.Id);

                var invalidEntitiesInSection = this
                    .GetInstancesOfEntitiesWithSameAndDeeperRosterLevelOrThrow(this.interviewState, allQuestionsInGroup, sectionInstance.RosterVector, questionnaire)
                    .Concat(this
                        .GetInstancesOfEntitiesWithSameAndDeeperRosterLevelOrThrow(this.interviewState, allStaticTextInGroup, sectionInstance.RosterVector, questionnaire))
                    .Where(entity => !this.IsValid(entity));

                foreach (var identity in invalidEntitiesInSection)
                {
                    yield return identity;
                }
            }
        }

        public IEnumerable<Identity> GetCommentedBySupervisorQuestionsInInterview()
        {
            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();

            var commentedEnabledInterviewerQuestionIds = this
                  .questionsComments
                  .Where(x => x.Value.Any(c => c.UserRole != UserRoles.Operator))
                  .Select(x => x.Key)
                  .Where(this.IsEnabled)
                  .Where(x => questionnaire.IsInterviewierQuestion(x.Id));

            var orderedCommentedQuestions = commentedEnabledInterviewerQuestionIds
                .Select(x => new
                {
                    Id = x,
                    HasInterviewerReply = this.HasSupervisorCommentInterviewerReply(x)
                })
                .OrderBy(x => x.HasInterviewerReply)
                .Select(x => x.Id);

            return orderedCommentedQuestions;
        }

        public string GetLastSupervisorComment()
        {
            return SupervisorRejectComment;
        }

        private bool HasSupervisorCommentInterviewerReply(Identity questionId)
        {
            var interviewerAnswerComments = this.GetQuestionComments(questionId);
            var indexOfLastNotInterviewerComment = interviewerAnswerComments.FindLastIndex(0, x => x.UserRole != UserRoles.Operator);
            return interviewerAnswerComments.Skip(indexOfLastNotInterviewerComment + 1).Any();
        }

        public bool HasInvalidInterviewerQuestionsInGroupOnly(Identity group)
        {
            return this
                .GetEnabledInterviewerChildQuestions(group)
                .Union(this.GetEnabledInvalidChildStaticTexts(group))
                .Any(question => !this.IsValid(question));
        }

        public bool HasUnansweredInterviewerQuestionsInGroupOnly(Identity group)
        {
            return this
                .GetEnabledInterviewerChildQuestions(group)
                .Any(question => !this.WasAnswered(question));
        }

        public Identity GetParentGroup(Identity groupOrQuestion)
        {
            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();

            Guid? parentId = questionnaire.GetParentGroup(groupOrQuestion.Id);

            if (!parentId.HasValue)
                return null;

            return this.GetInstanceOfGroupWithSameAndUpperRosterLevelOrThrow(parentId.Value, groupOrQuestion.RosterVector, questionnaire);
        }

        public IEnumerable<Identity> GetChildQuestions(Identity groupIdentity)
        {
            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();
            IEnumerable<Guid> allQuestionsInGroup = questionnaire.GetChildQuestions(groupIdentity.Id);

            IEnumerable<Identity> questionInstances = this
                .GetInstancesOfEntitiesWithSameAndDeeperRosterLevelOrThrow(this.interviewState, allQuestionsInGroup, groupIdentity.RosterVector, questionnaire);

            return questionInstances;
        }

        public IEnumerable<Identity> GetInterviewerEntities(Identity groupIdentity)
        {
            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();
            IEnumerable<Guid> allEntitiesInGroup = questionnaire.GetChildEntityIds(groupIdentity.Id);

            foreach (var entity in allEntitiesInGroup)
            {
                if (questionnaire.IsRosterGroup(entity))
                {
                    foreach (var rosterInstance in
                        this.GetInstancesOfGroupsByGroupIdWithSameAndDeeperRosterLevelOrThrow(this.interviewState, entity, groupIdentity.RosterVector, questionnaire)
                            .OrderBy(GetRosterSortIndex))
                    {
                        yield return rosterInstance;
                    }
                }
                else
                {
                    if (questionnaire.IsQuestion(entity) && !questionnaire.IsInterviewierQuestion(entity)) continue;

                    foreach (var entityInstance in this.GetInstancesOfEntitiesWithSameAndDeeperRosterLevelOrThrow(this.interviewState, entity, groupIdentity.RosterVector, questionnaire))
                    {
                        yield return entityInstance;
                    }
                }
            }
        }

        public IEnumerable<Identity> GetEnabledGroupInstances(Guid groupId, RosterVector parentRosterVector)
        {
            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();
            var resultInstances = this.GetInstancesOfGroupsByGroupIdWithSameAndDeeperRosterLevelOrThrow(this.interviewState, groupId, parentRosterVector, questionnaire);

            return resultInstances.Where(this.IsEnabled);
        }

        public IEnumerable<Identity> GetEnabledSubgroups(Identity group)
        {
            return this
                .GetGroupsAndRostersInGroup(group)
                .Where(this.IsEnabled);
        }

        public bool IsValid(Identity identity)
        {
            var questionKey = ConversionHelper.ConvertIdentityToString(identity);
            if (this.interviewState.InvalidStaticTexts.ContainsKey(identity)) return false;
            if (this.interviewState.ValidStaticTexts.Contains(identity)) return true;

            if (!this.Answers.ContainsKey(questionKey))
            {
                return !this.notAnsweredQuestionsValidityStatus.ContainsKey(questionKey) || this.notAnsweredQuestionsValidityStatus[questionKey];
            }

            var interviewAnswerModel = this.Answers[questionKey];
            return interviewAnswerModel.IsValid;
        }

        public IReadOnlyList<FailedValidationCondition> GetFailedValidationConditions(Identity questionId)
        {
            var convertIdentityToString = ConversionHelper.ConvertIdentityToString(questionId);
            if (this.interviewState.InvalidStaticTexts.ContainsKey(questionId))
            {
                return this.interviewState.InvalidStaticTexts[questionId];
            }

            if (this.Answers.ContainsKey(convertIdentityToString))
            {
                return this.Answers[convertIdentityToString].FailedValidations.ToReadOnlyCollection();
            }
            if (this.notAnsweredFailedConditions.ContainsKey(convertIdentityToString))
            {
                return this.notAnsweredFailedConditions[convertIdentityToString].ToReadOnlyCollection();
            }

            return new List<FailedValidationCondition>();
        }

        public bool IsEnabled(Identity entityIdentity)
        {
            return GetOrCalculate(
                entityIdentity,
                this.IsEnabledImpl,
                this.calculated.IsEnabled);
        }

        public bool CreatedOnClient
        {
            get { return this.createdOnClient; }
        }

        private bool IsEnabledImpl(Identity entityIdentity)
        {
            var entityKey = ConversionHelper.ConvertIdentityToString(entityIdentity);

            if (this.groups.ContainsKey(entityKey))
            {
                var group = this.groups[entityKey];
                return !group.IsDisabled;
            }

            if (this.Answers.ContainsKey(entityKey))
            {
                var answer = this.Answers[entityKey];
                return answer.IsEnabled;
            }

            if (base.interviewState.DisabledStaticTexts.Contains(entityIdentity))
            {
                return false;
            }

            if (this.notAnsweredQuestionsEnablementStatus.ContainsKey(entityKey))
            {
                return this.notAnsweredQuestionsEnablementStatus[entityKey];
            }

            var parentGroup = GetParentGroup(entityIdentity);
            while (parentGroup != null)
            {
                if (!IsEnabled(parentGroup))
                {
                    return false;
                }

                parentGroup = GetParentGroup(parentGroup);
            }

            return true;
        }

        public bool WasAnswered(Identity entityIdentity)
        {
            if (this.GetQuestionnaireOrThrow().IsStaticText(entityIdentity.Id))
            {
                return true;
            }

            var questionKey = ConversionHelper.ConvertIdentityToString(entityIdentity);

            if (!this.Answers.ContainsKey(questionKey))
                return false;

            if (!this.IsEnabled(entityIdentity))
                return false;

            var interviewAnswerModel = this.Answers[questionKey];
            return interviewAnswerModel.IsAnswered;
        }

        public List<QuestionComment> GetQuestionComments(Identity entityIdentity)
        {
            return this.questionsComments.ContainsKey(entityIdentity)
                ? this.questionsComments[entityIdentity]
                : null;
        }


        IEnumerable<CategoricalOption> IStatefulInterview.GetFilteredOptionsForQuestion(Identity question, int? parentQuestionValue, string filter)
        {
            return GetFilteredOptionsForQuestion(question, parentQuestionValue, filter);
        }

        CategoricalOption IStatefulInterview.GetOptionForQuestionWithoutFilter(Identity question, int value, int? parentQuestionValue)
        {
            return this.GetOptionForQuestionWithoutFilter(question, value, parentQuestionValue);
        }

        CategoricalOption IStatefulInterview.GetOptionForQuestionWithFilter(Identity question, string value, int? parentQuestionValue)
        {
            return this.GetOptionForQuestionWithFilter(question, value, parentQuestionValue);
        }

        public int CountCommentedQuestions()
        {
            var identitiesWithComments = this.GetCommentedBySupervisorQuestionsInInterview();
            return identitiesWithComments.Count();
        }

        private IEnumerable<Identity> GetGroupsAndRostersInGroup(Identity group)
        {
            return GetOrCalculate(
                group,
                (groupId) => this.GetGroupsAndRostersInGroupImpl(groupId).ToReadOnlyCollection(),
                this.calculated.GroupsAndRostersInGroup);
        }

        private int GetRosterSortIndex(Identity rosterIdentity)
        {
            return this.sortIndexesOfRosterInstanses.ContainsKey(rosterIdentity)
                ? this.sortIndexesOfRosterInstanses[rosterIdentity] ?? (int)rosterIdentity.RosterVector.Last()
                : (int)rosterIdentity.RosterVector.Last();
        }

        private IEnumerable<Identity> GetGroupsAndRostersInGroupImpl(Identity group)
        {
            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();

            IEnumerable<Guid> groupsAndRosters = questionnaire.GetAllUnderlyingChildGroupsAndRosters(group.Id);

            foreach (var entity in groupsAndRosters)
            {
                if (questionnaire.IsRosterGroup(entity))
                {
                    var childInstances = this.GetInstancesOfGroupsByGroupIdWithSameAndDeeperRosterLevelOrThrow(
                        this.interviewState, entity, @group.RosterVector, questionnaire)
                        .OrderBy(GetRosterSortIndex);
                    foreach (var rosterInstance in childInstances)
                    {
                        yield return rosterInstance;
                    }
                }
                else
                {
                    foreach (var entityInstance in this.GetInstancesOfEntitiesWithSameAndDeeperRosterLevelOrThrow(this.interviewState, entity, group.RosterVector, questionnaire))
                    {
                        yield return entityInstance;
                    }
                }
            }
        }

        private IEnumerable<Identity> GetEnabledInvalidChildStaticTexts(Identity parent)
        {
            return GetOrCalculate(
                parent,
                this.GetEnabledInterviewerChildStaticTextsImpl,
                this.calculated.EnabledStaticTextChildQuestions);
        }

        private ReadOnlyCollection<Identity> GetEnabledInterviewerChildQuestions(Identity group)
        {
            return GetOrCalculate(
                group,
                this.GetEnabledInterviewerChildQuestionsImpl,
                this.calculated.EnabledInterviewerChildQuestions);
        }

        private ReadOnlyCollection<Identity> GetEnabledInterviewerChildStaticTextsImpl(Identity group)
        {
            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();

            IEnumerable<Guid> staticTextIds = questionnaire.GetChildStaticTexts(group.Id);

            IEnumerable<Identity> questions = GetInstancesOfEntitiesWithSameAndDeeperRosterLevelOrThrow(
                this.interviewState, staticTextIds, group.RosterVector, questionnaire);

            return questions.Where(this.IsEnabled).ToReadOnlyCollection();
        }

        private ReadOnlyCollection<Identity> GetEnabledInterviewerChildQuestionsImpl(Identity group)
        {
            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();

            IEnumerable<Guid> questionIds = questionnaire.GetChildInterviewerQuestions(group.Id);

            IEnumerable<Identity> questions = this.GetInstancesOfEntitiesWithSameAndDeeperRosterLevelOrThrow(
                this.interviewState, questionIds, group.RosterVector, questionnaire);

            return questions.Where(this.IsEnabled).ToReadOnlyCollection();
        }

        private T GetQuestionAnswer<T>(Identity identity) where T : BaseInterviewAnswer, new()
        {
            var questionKey = ConversionHelper.ConvertIdentityToString(identity);
            if (this.Answers.ContainsKey(questionKey))
            {
                return (T)this.Answers[questionKey];
            }

            if (!this.notAnsweredQuestionsEnablementStatus.ContainsKey(questionKey)
                && !this.notAnsweredQuestionsValidityStatus.ContainsKey(questionKey))
            {
                return new T
                {
                    IsEnabled = true,
                    IsValid = true
                };
            }
            
            var isEnabled = !this.notAnsweredQuestionsEnablementStatus.ContainsKey(questionKey) || this.notAnsweredQuestionsEnablementStatus[questionKey];
            var isValid = !this.notAnsweredQuestionsValidityStatus.ContainsKey(questionKey) || this.notAnsweredQuestionsValidityStatus[questionKey];

            var answer = new T
            {
                IsEnabled = isEnabled,
                IsValid = isValid
            };

            return answer;
        }

        private T GetOrCreateAnswer<T>(QuestionActiveEvent @event) where T : BaseInterviewAnswer, new()
        {
            return this.GetOrCreateAnswer<T>(@event.QuestionId, @event.RosterVector);
        }

        private T GetOrCreateAnswer<T>(Guid questionId, decimal[] propagationVector) where T : BaseInterviewAnswer, new()
        {
            var questionKey = ConversionHelper.ConvertIdAndRosterVectorToString(questionId, propagationVector);

            T question;
            if (this.Answers.ContainsKey(questionKey))
            {
                question = (T)this.Answers[questionKey];
            }
            else
            {
                question = new T { Id = questionId, RosterVector = propagationVector };
                if (this.notAnsweredQuestionsEnablementStatus.ContainsKey(questionKey))
                {
                    question.IsEnabled = this.notAnsweredQuestionsEnablementStatus[questionKey];
                    bool removedItemEnablementStatus;
                    this.notAnsweredQuestionsEnablementStatus.TryRemove(questionKey, out removedItemEnablementStatus);
                }

                if (this.notAnsweredQuestionsValidityStatus.ContainsKey(questionKey))
                {
                    question.IsValid = this.notAnsweredQuestionsValidityStatus[questionKey];
                    if (!question.IsValid)
                    {
                        question.FailedValidations = this.notAnsweredFailedConditions[questionKey];
                    }
                    bool removedItemValidityStatus;
                    this.notAnsweredQuestionsValidityStatus.TryRemove(questionKey, out removedItemValidityStatus);
                }
            }

            this.answers[questionKey] = question;
            return question;
        }

        private InterviewEnablementState GetOrCreateGroupOrRoster(Guid id, RosterVector rosterVector)
        {
            var groupKey = ConversionHelper.ConvertIdAndRosterVectorToString(id, rosterVector);

            InterviewEnablementState groupOrRoster;
            if (this.groups.ContainsKey(groupKey))
            {
                groupOrRoster = this.groups[groupKey];
            }
            else
            {
                // rosters must be created with event RosterInstancesAdded. Groups has no such event, so
                // they should be created eventually. So if group model was not found that means that
                // we got event about group that was not created previously. Rosters should not be created here.
                groupOrRoster = new InterviewEnablementState { Id = id, RosterVector = rosterVector };
            }

            this.groups[groupKey] = groupOrRoster;
            return groupOrRoster;
        }

        private IQuestionnaire GetQuestionnaireOrThrow()
        {
            return this.cachedQuestionnaire ?? (this.cachedQuestionnaire = GetQuestionnaireOrThrow(
                this.QuestionnaireIdentity.QuestionnaireId,
                this.QuestionnaireIdentity.Version,
                this.Language));
        }

        private static decimal[] GetFullRosterVector(RosterInstance instance)
        {
            return instance.GetIdentity().RosterVector;
        }

        private BaseInterviewAnswer GetExistingAnswerOrNull(string questionKey)
        {
            return this.Answers.ContainsKey(questionKey) ? this.Answers[questionKey] : null;
        }

        private BaseInterviewAnswer GetExistingAnswerOrNull(Identity questionIdentity)
        {
            var questionKey = ConversionHelper.ConvertIdentityToString(questionIdentity);

            return this.GetExistingAnswerOrNull(questionKey);
        }

        private static TValue GetOrCalculate<TKey, TValue>(TKey key, Func<TKey, TValue> calculateValue, ConcurrentDictionary<TKey, TValue> calculatedValues)
        {
            return calculatedValues.GetOrAdd(key, calculateValue);
        }
    }
}