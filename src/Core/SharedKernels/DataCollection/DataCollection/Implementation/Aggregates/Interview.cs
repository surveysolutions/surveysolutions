using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Ncqrs.Domain;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Preloading;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Invariants;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    public partial class Interview : AggregateRootMappedByConvention
    {
        public Interview() { }

        private InterviewTree tree;
        protected InterviewTree Tree => this.tree ?? (this.tree = this.BuildInterviewTree(this.GetQuestionnaireOrThrow()));

        protected readonly InterviewEntities.InterviewProperties properties = new InterviewEntities.InterviewProperties();

        public override Guid EventSourceId
        {
            get { return base.EventSourceId; }

            protected set
            {
                base.EventSourceId = value;
                this.properties.Id = value.FormatGuid();
            }
        }

        private ILatestInterviewExpressionState expressionProcessorStatePrototype = null;
        protected ILatestInterviewExpressionState ExpressionProcessorStatePrototype
        {
            get
            {
                if (this.expressionProcessorStatePrototype == null)
                {
                    this.expressionProcessorStatePrototype = this.expressionProcessorStatePrototypeProvider.GetExpressionState(
                            this.QuestionnaireIdentity.QuestionnaireId, this.QuestionnaireIdentity.Version);

                    this.expressionProcessorStatePrototype.SetInterviewProperties(new InterviewProperties(EventSourceId));
                }

                return this.expressionProcessorStatePrototype;
            }

            set
            {
                expressionProcessorStatePrototype = value;
            }
        }
       
        /// <remarks>
        /// Repository operations are time-consuming.
        /// So this repository may be used only in command handlers.
        /// And should never be used in event handlers!!
        /// </remarks>
        private readonly IQuestionnaireStorage questionnaireRepository;

        private readonly IInterviewExpressionStatePrototypeProvider expressionProcessorStatePrototypeProvider;

        private readonly ISubstitionTextFactory substitionTextFactory;

        private InterviewKey interviewKey;

        public Interview(IQuestionnaireStorage questionnaireRepository,
            IInterviewExpressionStatePrototypeProvider expressionProcessorStatePrototypeProvider,
            ISubstitionTextFactory substitionTextFactory)
        {
            this.questionnaireRepository = questionnaireRepository;
            this.expressionProcessorStatePrototypeProvider = expressionProcessorStatePrototypeProvider;
            this.substitionTextFactory = substitionTextFactory;
        }

        #region Apply (state restore) methods

        public virtual void Apply(InterviewKeyAssigned @event)
        {
            this.interviewKey = @event.Key;
        }

        public virtual void Apply(InterviewReceivedByInterviewer @event)
        {
            this.properties.IsReceivedByInterviewer = true;
        }

        public virtual void Apply(InterviewReceivedBySupervisor @event)
        {
            this.properties.IsReceivedByInterviewer = false;
        }

        public virtual void Apply(InterviewCreated @event)
        {
            this.QuestionnaireIdentity = new QuestionnaireIdentity(@event.QuestionnaireId, @event.QuestionnaireVersion);
        }

        public virtual void Apply(InterviewOnClientCreated @event)
        {
            this.QuestionnaireIdentity = new QuestionnaireIdentity(@event.QuestionnaireId, @event.QuestionnaireVersion);
        }

        public virtual void Apply(InterviewFromPreloadedDataCreated @event)
        {
            this.QuestionnaireIdentity = new QuestionnaireIdentity(@event.QuestionnaireId, @event.QuestionnaireVersion);
        }

        public virtual void Apply(SynchronizationMetadataApplied @event)
        {
            this.QuestionnaireIdentity = new QuestionnaireIdentity(@event.QuestionnaireId, @event.QuestionnaireVersion);
            this.properties.Status = @event.Status;
        }

        public virtual void Apply(TextQuestionAnswered @event)
        {
            var questionIdentity = Identity.Create(@event.QuestionId, @event.RosterVector);
            this.SetStartDateOnFirstAnswerSet(questionIdentity, @event.AnswerTimeUtc);

            this.Tree.GetQuestion(questionIdentity).AsText.SetAnswer(TextAnswer.FromString(@event.Answer));
            this.ExpressionProcessorStatePrototype.UpdateTextAnswer(@event.QuestionId, @event.RosterVector, @event.Answer);
        }

        public virtual void Apply(QRBarcodeQuestionAnswered @event)
        {
            var questionIdentity = Identity.Create(@event.QuestionId, @event.RosterVector);
            this.SetStartDateOnFirstAnswerSet(questionIdentity, @event.AnswerTimeUtc);

            this.Tree.GetQuestion(questionIdentity).AsQRBarcode.SetAnswer(QRBarcodeAnswer.FromString(@event.Answer));
            this.ExpressionProcessorStatePrototype.UpdateQrBarcodeAnswer(@event.QuestionId, @event.RosterVector, @event.Answer);
        }

        public virtual void Apply(PictureQuestionAnswered @event)
        {
            var questionIdentity = Identity.Create(@event.QuestionId, @event.RosterVector);
            this.SetStartDateOnFirstAnswerSet(questionIdentity, @event.AnswerTimeUtc);

            this.Tree.GetQuestion(questionIdentity).AsMultimedia.SetAnswer(MultimediaAnswer.FromString(@event.PictureFileName));
            this.ExpressionProcessorStatePrototype.UpdateMediaAnswer(@event.QuestionId, @event.RosterVector, @event.PictureFileName);
        }

        public virtual void Apply(NumericRealQuestionAnswered @event)
        {
            var questionIdentity = Identity.Create(@event.QuestionId, @event.RosterVector);
            this.SetStartDateOnFirstAnswerSet(questionIdentity, @event.AnswerTimeUtc);

            this.Tree.GetQuestion(questionIdentity).AsDouble.SetAnswer(NumericRealAnswer.FromDecimal(@event.Answer));
            this.ExpressionProcessorStatePrototype.UpdateNumericRealAnswer(@event.QuestionId, @event.RosterVector, (double)@event.Answer);
        }

        public virtual void Apply(NumericIntegerQuestionAnswered @event)
        {
            var questionIdentity = Identity.Create(@event.QuestionId, @event.RosterVector);
            this.SetStartDateOnFirstAnswerSet(questionIdentity, @event.AnswerTimeUtc);

            this.Tree.GetQuestion(questionIdentity).AsInteger.SetAnswer(NumericIntegerAnswer.FromInt(@event.Answer));
            this.ActualizeRostersIfQuestionIsRosterSize(@event.QuestionId);
            this.ExpressionProcessorStatePrototype.UpdateNumericIntegerAnswer(@event.QuestionId, @event.RosterVector, @event.Answer);
        }

        public virtual void Apply(DateTimeQuestionAnswered @event)
        {
            var questionIdentity = Identity.Create(@event.QuestionId, @event.RosterVector);
            this.SetStartDateOnFirstAnswerSet(questionIdentity, @event.AnswerTimeUtc);

            this.Tree.GetQuestion(questionIdentity).AsDateTime.SetAnswer(DateTimeAnswer.FromDateTime(@event.Answer));
            this.ExpressionProcessorStatePrototype.UpdateDateAnswer(@event.QuestionId, @event.RosterVector, @event.Answer);
        }

        public virtual void Apply(SingleOptionQuestionAnswered @event)
        {
            var questionIdentity = Identity.Create(@event.QuestionId, @event.RosterVector);
            this.SetStartDateOnFirstAnswerSet(questionIdentity, @event.AnswerTimeUtc);

            var question = this.Tree.GetQuestion(questionIdentity);

            question.AsSingleFixedOption?.SetAnswer(CategoricalFixedSingleOptionAnswer.FromDecimal(@event.SelectedValue));
            question.AsSingleLinkedToList?.SetAnswer(CategoricalFixedSingleOptionAnswer.FromDecimal(@event.SelectedValue));

            this.ExpressionProcessorStatePrototype.UpdateSingleOptionAnswer(@event.QuestionId, @event.RosterVector, @event.SelectedValue);
        }

        public virtual void Apply(MultipleOptionsQuestionAnswered @event)
        {
            var questionIdentity = Identity.Create(@event.QuestionId, @event.RosterVector);
            this.SetStartDateOnFirstAnswerSet(questionIdentity, @event.AnswerTimeUtc);

            var question = this.Tree.GetQuestion(questionIdentity);

            question.AsMultiFixedOption?.SetAnswer(CategoricalFixedMultiOptionAnswer.FromDecimalArray(@event.SelectedValues));
            question.AsMultiLinkedToList?.SetAnswer(CategoricalFixedMultiOptionAnswer.FromDecimalArray(@event.SelectedValues));
            this.ActualizeRostersIfQuestionIsRosterSize(@event.QuestionId);
            this.ExpressionProcessorStatePrototype.UpdateMultiOptionAnswer(@event.QuestionId, @event.RosterVector, @event.SelectedValues);
        }

        public virtual void Apply(YesNoQuestionAnswered @event)
        {
            var questionIdentity = Identity.Create(@event.QuestionId, @event.RosterVector);
            this.SetStartDateOnFirstAnswerSet(questionIdentity, @event.AnswerTimeUtc);

            this.Tree.GetQuestion(questionIdentity).AsYesNo.SetAnswer(YesNoAnswer.FromAnsweredYesNoOptions(@event.AnsweredOptions));
            this.ActualizeRostersIfQuestionIsRosterSize(@event.QuestionId);
            this.ExpressionProcessorStatePrototype.UpdateYesNoAnswer(@event.QuestionId, @event.RosterVector, YesNoAnswer.FromAnsweredYesNoOptions(@event.AnsweredOptions).ToYesNoAnswersOnly());
        }

        public virtual void Apply(GeoLocationQuestionAnswered @event)
        {
            var questionIdentity = Identity.Create(@event.QuestionId, @event.RosterVector);
            this.SetStartDateOnFirstAnswerSet(questionIdentity, @event.AnswerTimeUtc);

            this.Tree.GetQuestion(questionIdentity).AsGps.SetAnswer(GpsAnswer.FromGeoPosition(new GeoPosition(
                    @event.Latitude, @event.Longitude, @event.Accuracy, @event.Altitude, @event.Timestamp)));

            this.ExpressionProcessorStatePrototype.UpdateGeoLocationAnswer(@event.QuestionId, @event.RosterVector, @event.Latitude,
                @event.Longitude, @event.Accuracy, @event.Altitude);
        }

        public virtual void Apply(TextListQuestionAnswered @event)
        {
            var questionIdentity = Identity.Create(@event.QuestionId, @event.RosterVector);
            this.SetStartDateOnFirstAnswerSet(questionIdentity, @event.AnswerTimeUtc);

            this.Tree.GetQuestion(questionIdentity).AsTextList.SetAnswer(TextListAnswer.FromTupleArray(@event.Answers));
            this.ActualizeRostersIfQuestionIsRosterSize(@event.QuestionId);
            this.ExpressionProcessorStatePrototype.UpdateTextListAnswer(@event.QuestionId, @event.RosterVector, @event.Answers);
        }

        public virtual void Apply(SingleOptionLinkedQuestionAnswered @event)
        {
            var questionIdentity = Identity.Create(@event.QuestionId, @event.RosterVector);
            this.SetStartDateOnFirstAnswerSet(questionIdentity, @event.AnswerTimeUtc);
            this.Tree.GetQuestion(questionIdentity).AsSingleLinkedOption.SetAnswer(CategoricalLinkedSingleOptionAnswer.FromRosterVector(@event.SelectedRosterVector));
            this.ExpressionProcessorStatePrototype.UpdateLinkedSingleOptionAnswer(@event.QuestionId, @event.RosterVector, @event.SelectedRosterVector);
        }

        public virtual void Apply(MultipleOptionsLinkedQuestionAnswered @event)
        {
            var questionIdentity = Identity.Create(@event.QuestionId, @event.RosterVector);
            this.SetStartDateOnFirstAnswerSet(questionIdentity, @event.AnswerTimeUtc);
            this.Tree.GetQuestion(questionIdentity).AsMultiLinkedOption.SetAnswer(CategoricalLinkedMultiOptionAnswer.FromRosterVectors(@event.SelectedRosterVectors.Select(x => new RosterVector(x)).ToArray()));
            this.ExpressionProcessorStatePrototype.UpdateLinkedMultiOptionAnswer(@event.QuestionId, @event.RosterVector, @event.SelectedRosterVectors);
        }

        public virtual void Apply(AnswersDeclaredValid @event)
        {
            foreach (var questionIdentity in @event.Questions)
                this.Tree.GetQuestion(questionIdentity).MarkValid();

            this.ExpressionProcessorStatePrototype.DeclareAnswersValid(@event.Questions);
        }

        public virtual void Apply(AnswersDeclaredInvalid @event)
        {
            if (@event.FailedValidationConditions.Count > 0)
            {
                foreach (var failedValidationCondition in @event.FailedValidationConditions)
                {
                    if (failedValidationCondition.Value?.Count > 0)
                        this.Tree.GetQuestion(failedValidationCondition.Key).MarkInvalid(failedValidationCondition.Value);
                    else
                        this.Tree.GetQuestion(failedValidationCondition.Key).MarkInvalid();
                }

                this.ExpressionProcessorStatePrototype.ApplyFailedValidations(@event.FailedValidationConditions);
            }
            else //handling of old events
            {
                foreach (var invalidQuestionIdentity in @event.Questions)
                    this.Tree.GetQuestion(invalidQuestionIdentity).MarkInvalid();

                this.ExpressionProcessorStatePrototype.DeclareAnswersInvalid(@event.Questions);
            }
        }

        public virtual void Apply(StaticTextsDeclaredValid @event)
        {
            foreach (var staticTextIdentity in @event.StaticTexts)
                this.Tree.GetStaticText(staticTextIdentity).MarkValid();
            this.ExpressionProcessorStatePrototype.DeclareStaticTextValid(@event.StaticTexts);
        }

        public virtual void Apply(StaticTextsDeclaredInvalid @event)
        {
            var staticTextsConditions = @event.GetFailedValidationConditionsDictionary();

            foreach (var staticTextIdentity in staticTextsConditions.Keys)
                this.Tree.GetStaticText(staticTextIdentity).MarkInvalid(staticTextsConditions[staticTextIdentity]);

            this.ExpressionProcessorStatePrototype.ApplyStaticTextFailedValidations(staticTextsConditions);
        }

        public void Apply(LinkedOptionsChanged @event)
        {
            foreach (var linkedQuestion in @event.ChangedLinkedQuestions)
                this.Tree.GetQuestion(linkedQuestion.QuestionId)?.AsLinked.SetOptions(linkedQuestion.Options);
        }

        public void Apply(LinkedToListOptionsChanged @event)
        {
            foreach (var linkedQuestion in @event.ChangedLinkedQuestions)
                this.Tree.GetQuestion(linkedQuestion.QuestionId).AsLinkedToList.SetOptions(linkedQuestion.Options);
        }

        public virtual void Apply(GroupsDisabled @event)
        {
            foreach (var groupIdentity in @event.Groups)
                this.Tree.GetGroup(groupIdentity).Disable();

            this.ExpressionProcessorStatePrototype.DisableGroups(@event.Groups);
        }

        public virtual void Apply(GroupsEnabled @event)
        {
            foreach (var groupIdentity in @event.Groups)
                this.Tree.GetGroup(groupIdentity)?.Enable();

            this.ExpressionProcessorStatePrototype.EnableGroups(@event.Groups);
        }

        public virtual void Apply(VariablesDisabled @event)
        {
            foreach (var variableIdentity in @event.Variables)
                this.Tree.GetVariable(variableIdentity).Disable();

            this.ExpressionProcessorStatePrototype.DisableVariables(@event.Variables);
        }

        public virtual void Apply(VariablesEnabled @event)
        {
            foreach (var variableIdentity in @event.Variables)
                this.Tree.GetVariable(variableIdentity)?.Enable();

            this.ExpressionProcessorStatePrototype.EnableVariables(@event.Variables);
        }

        public virtual void Apply(VariablesChanged @event)
        {
            foreach (var changedVariableValueDto in @event.ChangedVariables)
            {
                this.Tree.GetVariable(changedVariableValueDto.Identity)?.SetValue(changedVariableValueDto.NewValue);
                this.ExpressionProcessorStatePrototype.UpdateVariableValue(changedVariableValueDto.Identity, changedVariableValueDto.NewValue);
            }
        }

        public virtual void Apply(QuestionsDisabled @event)
        {
            foreach (var questionIdentity in @event.Questions)
                this.Tree.GetQuestion(questionIdentity).Disable();

            this.ExpressionProcessorStatePrototype.DisableQuestions(@event.Questions);
        }

        public virtual void Apply(QuestionsEnabled @event)
        {
            foreach (var questionIdentity in @event.Questions)
                this.Tree.GetQuestion(questionIdentity)?.Enable();

            this.ExpressionProcessorStatePrototype.EnableQuestions(@event.Questions);
        }

        public virtual void Apply(StaticTextsEnabled @event)
        {
            foreach (var staticTextIdentity in @event.StaticTexts)
                this.Tree.GetStaticText(staticTextIdentity)?.Enable();

            this.ExpressionProcessorStatePrototype.EnableStaticTexts(@event.StaticTexts);
        }

        public virtual void Apply(StaticTextsDisabled @event)
        {
            foreach (var staticTextIdentity in @event.StaticTexts)
                this.Tree.GetStaticText(staticTextIdentity).Disable();

            this.ExpressionProcessorStatePrototype.DisableStaticTexts(@event.StaticTexts);
        }

        public virtual void Apply(AnswerCommented @event)
        {
            var commentByQuestion = Identity.Create(@event.QuestionId, @event.RosterVector);

            var userRole = @event.UserId == this.properties.InterviewerId
                ? UserRoles.Interviewer
                : @event.UserId == this.properties.SupervisorId ? UserRoles.Supervisor : UserRoles.Headquarter;

            this.Tree.GetQuestion(commentByQuestion).AnswerComments.Add(new AnswerComment(@event.UserId, userRole, @event.CommentTime, @event.Comment,
                commentByQuestion));
        }

        public virtual void Apply(FlagSetToAnswer @event) { }

        public virtual void Apply(TranslationSwitched @event)
        {
            this.Language = @event.Language;

            var questionnaire = this.GetQuestionnaireOrThrow();

            this.Tree.SwitchQuestionnaire(questionnaire);
            this.UpdateTitlesAndTexts(questionnaire);
        }

        public virtual void Apply(FlagRemovedFromAnswer @event) { }

        public virtual void Apply(SubstitutionTitlesChanged @event)
        {
            foreach (var @group in @event.Groups)
                this.Tree.GetGroup(@group)?.ReplaceSubstitutions();

            foreach (var staticText in @event.StaticTexts)
                this.Tree.GetStaticText(staticText)?.ReplaceSubstitutions();

            foreach (var question in @event.Questions)
                this.Tree.GetQuestion(question)?.ReplaceSubstitutions();
        }

        public virtual void Apply(RosterInstancesTitleChanged @event)
        {
            foreach (var changedRosterTitle in @event.ChangedInstances)
                this.Tree.GetRoster(changedRosterTitle.RosterInstance.GetIdentity())?.SetRosterTitle(changedRosterTitle.Title);
        }

        private bool isFixedRostersInitialized = false;
        public virtual void Apply(RosterInstancesAdded @event)
        {
            // compatibility with previous versions < 5.16
            // for fixed rosters only
            if (!this.isFixedRostersInitialized)
            {
                this.Tree.ActualizeTree();
                this.isFixedRostersInitialized = true;
            }

            foreach (var instance in @event.Instances)
            {
                this.ExpressionProcessorStatePrototype.AddRoster(instance.GroupId, instance.OuterRosterVector,
                    instance.RosterInstanceId, instance.SortIndex);
            }
        }

        public virtual void Apply(RosterInstancesRemoved @event)
        {
            foreach (var instance in @event.Instances)
            {
                this.ExpressionProcessorStatePrototype.RemoveRoster(instance.GroupId, instance.OuterRosterVector, instance.RosterInstanceId);
            }
        }

        public virtual void Apply(InterviewStatusChanged @event)
        {
            this.properties.Status = @event.Status;
        }

        public virtual void Apply(SupervisorAssigned @event)
        {
            this.properties.SupervisorId = @event.SupervisorId;
        }

        public virtual void Apply(InterviewerAssigned @event)
        {
            this.properties.InterviewerId = @event.InterviewerId;
            this.properties.IsReceivedByInterviewer = false;
        }

        public virtual void Apply(InterviewDeleted @event) { }

        public virtual void Apply(InterviewHardDeleted @event)
        {
            this.properties.IsHardDeleted = true;
        }

        public virtual void Apply(InterviewSentToHeadquarters @event) { }

        public virtual void Apply(InterviewRestored @event) { }

        public virtual void Apply(InterviewCompleted @event)
        {
            this.properties.WasCompleted = true;
            this.properties.CompletedDate = @event.CompleteTime;
        }

        public virtual void Apply(InterviewRestarted @event) { }

        public virtual void Apply(InterviewApproved @event) { }

        public virtual void Apply(InterviewApprovedByHQ @event) { }

        public virtual void Apply(UnapprovedByHeadquarters @event) { }

        public virtual void Apply(InterviewRejected @event)
        {
            this.properties.WasCompleted = false;
        }

        public virtual void Apply(InterviewRejectedByHQ @event) { }

        public virtual void Apply(InterviewDeclaredValid @event) { }

        public virtual void Apply(InterviewDeclaredInvalid @event) { }

        public virtual void Apply(AnswersRemoved @event)
        {
            foreach (var identity in @event.Questions)
            {
                // can be removed from removed roster. No need for this event anymore
                this.Tree.GetQuestion(identity)?.RemoveAnswer();
                this.ActualizeRostersIfQuestionIsRosterSize(identity.Id);
                this.ExpressionProcessorStatePrototype.RemoveAnswer(new Identity(identity.Id, identity.RosterVector));
            }
        }

        public virtual void Apply(AnswerRemoved @event)
        {
            this.Tree.GetQuestion(Identity.Create(@event.QuestionId, @event.RosterVector)).RemoveAnswer();
            this.ActualizeRostersIfQuestionIsRosterSize(@event.QuestionId);

            this.ExpressionProcessorStatePrototype.RemoveAnswer(new Identity(@event.QuestionId, @event.RosterVector));
        }

        #endregion

        #region Questionnaire

        public string Language { get; private set; }
        public QuestionnaireIdentity QuestionnaireIdentity { get; protected set; }
        public string QuestionnaireId => this.QuestionnaireIdentity?.ToString();

        protected IQuestionnaire GetQuestionnaireOrThrow() => this.GetQuestionnaireOrThrow(this.Language);

        private IQuestionnaire GetQuestionnaireOrThrow(string language)
        {
            IQuestionnaire questionnaire = this.questionnaireRepository.GetQuestionnaire(this.QuestionnaireIdentity, language);

            if (questionnaire == null)
                throw new InterviewException(
                    $"Questionnaire '{this.QuestionnaireIdentity}' was not found. InterviewId {this.EventSourceId}",
                    InterviewDomainExceptionType.QuestionnaireIsMissing);

            return questionnaire;
        }

        protected static string FormatQuestionForException(Guid questionId, IQuestionnaire questionnaire)
            => $"'{GetQuestionTitleForException(questionId, questionnaire)} [{GetQuestionVariableNameForException(questionId, questionnaire)}]'";

        protected static string FormatGroupForException(Guid groupId, IQuestionnaire questionnaire)
            => $"'{GetGroupTitleForException(groupId, questionnaire)} ({groupId:N})'";

        private static string GetQuestionTitleForException(Guid questionId, IQuestionnaire questionnaire)
            => questionnaire.HasQuestion(questionId)
                ? questionnaire.GetQuestionTitle(questionId) ?? "<<NO QUESTION TITLE>>"
                : "<<MISSING QUESTION>>";

        private static string GetQuestionVariableNameForException(Guid questionId, IQuestionnaire questionnaire)
            => questionnaire.HasQuestion(questionId)
                ? questionnaire.GetQuestionVariableName(questionId) ?? "<<NO VARIABLE NAME>>"
                : "<<MISSING QUESTION>>";

        private static string GetGroupTitleForException(Guid groupId, IQuestionnaire questionnaire)
            => questionnaire.HasGroup(groupId)
                ? questionnaire.GetGroupTitle(groupId) ?? "<<NO GROUP TITLE>>"
                : "<<MISSING GROUP>>";

        public virtual List<CategoricalOption> GetFirstTopFilteredOptionsForQuestion(Identity question, int? parentQuestionValue, string filter, int itemsCount = 200)
        {
            itemsCount = itemsCount > 200 ? 200 : itemsCount;

            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();

            if (!questionnaire.IsSupportFilteringForOptions(question.Id))
                return questionnaire.GetOptionsForQuestion(question.Id, parentQuestionValue, filter).Take(itemsCount).ToList();

            return this.ExpressionProcessorStatePrototype.FilterOptionsForQuestion(question,
                questionnaire.GetOptionsForQuestion(question.Id, parentQuestionValue, filter)).Take(itemsCount).ToList();
        }

        public CategoricalOption GetOptionForQuestionWithoutFilter(Identity question, int value, int? parentQuestionValue = null)
        {
            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();
            return questionnaire.GetOptionForQuestionByOptionValue(question.Id, value);
        }

        public CategoricalOption GetOptionForQuestionWithFilter(Identity question, string optionText, int? parentQuestionValue = null)
        {
            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();
            var filteredOption = questionnaire.GetOptionForQuestionByOptionText(question.Id, optionText, parentQuestionValue);

            if (filteredOption == null)
                return null;

            if (questionnaire.IsSupportFilteringForOptions(question.Id))
                return this.ExpressionProcessorStatePrototype.FilterOptionsForQuestion(question, Enumerable.Repeat(filteredOption, 1)).SingleOrDefault();
            else
                return filteredOption;
        }

        #endregion

        #region Assembly

        protected void UpdateExpressionState(InterviewTree changedInterview, ILatestInterviewExpressionState expressionState)
            => this.UpdateExpressionState(this.Tree, changedInterview, expressionState);

        protected void UpdateExpressionState(InterviewTree sourceInterview, InterviewTree changedInterview, ILatestInterviewExpressionState expressionState)
        {
            var diff = sourceInterview.Compare(changedInterview);

            var diffByQuestions = diff.OfType<InterviewTreeQuestionDiff>().ToList();
            var questionsWithRemovedAnswer = diffByQuestions.Where(x => x.IsAnswerRemoved).ToArray();
            var questionsWithChangedAnswer = diffByQuestions.Where(x => x.IsAnswerChanged).ToArray();
            var changedRosters = diff.OfType<InterviewTreeRosterDiff>().ToArray();
            var changedVariables = diff.OfType<InterviewTreeVariableDiff>().ToArray();

            UpdateRostersInExpressionState(changedRosters, expressionState);
            UpdateAnswersInExpressionState(questionsWithChangedAnswer, expressionState);
            RemoveAnswersInExpressionState(questionsWithRemovedAnswer, expressionState);
            UpdateEnablementInExpressionState(diff, expressionState);
            UpdateValidityInExpressionState(diff, expressionState);
            UpdateVariablesInExpressionState(changedVariables, expressionState);
        }

        private void UpdateVariablesInExpressionState(InterviewTreeVariableDiff[] diffsByChangedVariables, ILatestInterviewExpressionState expressionState)
        {
            var changedVariables = diffsByChangedVariables.Where(x => x.ChangedNode != null && x.IsValueChanged)
                    .Select(x => x.ChangedNode)
                    .ToArray();

            foreach (var changedVariable in changedVariables)
            {
                expressionState.UpdateVariableValue(changedVariable.Identity, changedVariable.Value);
            }
        }

        private static void UpdateValidityInExpressionState(IReadOnlyCollection<InterviewTreeNodeDiff> diff, ILatestInterviewExpressionState expressionState)
        {
            var allNotNullableNodes = diff.Where(x => x.ChangedNode != null).ToList();

            var allChangedQuestionDiffs = allNotNullableNodes.OfType<InterviewTreeQuestionDiff>().ToList();
            var allChangedStaticTextDiffs = allNotNullableNodes.OfType<InterviewTreeStaticTextDiff>().ToList();

            var validQuestionIdentities = allChangedQuestionDiffs.Where(x => x.ChangedNodeBecameValid).Select(x => x.ChangedNode.Identity).ToArray();
            var invalidQuestionIdentities = allChangedQuestionDiffs.Where(x => x.ChangedNodeBecameInvalid).Select(x => x.ChangedNode)
                .ToDictionary(x => x.Identity, x => x.FailedValidations);

            var validStaticTextIdentities = allChangedStaticTextDiffs.Where(x => x.ChangedNodeBecameValid).Select(x => x.ChangedNode.Identity).ToArray();
            var invalidStaticTextIdentities = allChangedStaticTextDiffs.Where(x => x.ChangedNodeBecameInvalid).Select(x => x.ChangedNode)
                .ToDictionary(x => x.Identity, x => x.FailedValidations);

            if (validQuestionIdentities.Any()) expressionState.DeclareAnswersValid(validQuestionIdentities);
            if (invalidQuestionIdentities.Any()) expressionState.ApplyFailedValidations(invalidQuestionIdentities);

            if (validStaticTextIdentities.Any()) expressionState.DeclareStaticTextValid(validStaticTextIdentities);
            if (invalidStaticTextIdentities.Any()) expressionState.ApplyStaticTextFailedValidations(invalidStaticTextIdentities);
        }

        private static void UpdateEnablementInExpressionState(IReadOnlyCollection<InterviewTreeNodeDiff> diff, ILatestInterviewExpressionState expressionState)
        {
            var allNotNullableNodes = diff.Where(x => x.ChangedNode != null).ToList();

            var diffByGroups = allNotNullableNodes.OfType<InterviewTreeGroupDiff>().ToList();
            var diffByQuestions = allNotNullableNodes.OfType<InterviewTreeQuestionDiff>().ToList();
            var diffByStaticTexts = allNotNullableNodes.OfType<InterviewTreeStaticTextDiff>().ToList();
            var diffByVariables = allNotNullableNodes.OfType<InterviewTreeVariableDiff>().ToList();

            var disabledGroups = diffByGroups.Where(x => x.IsNodeDisabled).Select(x => x.ChangedNode.Identity).ToArray();
            var enabledGroups = diffByGroups.Where(x => x.IsNodeEnabled).Select(x => x.ChangedNode.Identity).ToArray();

            var disabledQuestions = diffByQuestions.Where(x => x.IsNodeDisabled).Select(x => x.ChangedNode.Identity).ToArray();
            var enabledQuestions = diffByQuestions.Where(x => x.IsNodeEnabled).Select(x => x.ChangedNode.Identity).ToArray();

            var disabledStaticTexts = diffByStaticTexts.Where(x => x.IsNodeDisabled).Select(x => x.ChangedNode.Identity).ToArray();
            var enabledStaticTexts = diffByStaticTexts.Where(x => x.IsNodeEnabled).Select(x => x.ChangedNode.Identity).ToArray();

            var disabledVariables = diffByVariables.Where(x => x.IsNodeDisabled).Select(x => x.ChangedNode.Identity).ToArray();
            var enabledVariables = diffByVariables.Where(x => x.IsNodeEnabled).Select(x => x.ChangedNode.Identity).ToArray();

            if (disabledGroups.Any()) expressionState.DisableGroups(disabledGroups);
            if (enabledGroups.Any()) expressionState.EnableGroups(enabledGroups);
            if (disabledQuestions.Any()) expressionState.DisableQuestions(disabledQuestions);
            if (enabledQuestions.Any()) expressionState.EnableQuestions(enabledQuestions);
            if (disabledStaticTexts.Any()) expressionState.DisableStaticTexts(disabledStaticTexts);
            if (enabledStaticTexts.Any()) expressionState.EnableStaticTexts(enabledStaticTexts);
            if (disabledVariables.Any()) expressionState.DisableVariables(disabledVariables);
            if (enabledVariables.Any()) expressionState.EnableVariables(enabledVariables);
        }

        private static void RemoveAnswersInExpressionState(IReadOnlyCollection<InterviewTreeNodeDiff> diffByQuestions, ILatestInterviewExpressionState expressionState)
        {
            foreach (var diffByQuestion in diffByQuestions)
            {
                expressionState.RemoveAnswer(diffByQuestion.SourceNode.Identity);
            }
        }

        private static void UpdateAnswersInExpressionState(InterviewTreeQuestionDiff[] diffByQuestions, ILatestInterviewExpressionState expressionState)
        {
            foreach (var diffByQuestion in diffByQuestions)
            {
                var changedQuestion = diffByQuestion.ChangedNode;

                if (changedQuestion == null) continue;

                if (!changedQuestion.IsAnswered()) continue;

                if (changedQuestion.IsText)
                {
                    expressionState.UpdateTextAnswer(changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, changedQuestion.AsText.GetAnswer().Value);
                }

                if (changedQuestion.IsTextList)
                {
                    expressionState.UpdateTextListAnswer(changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, changedQuestion.AsTextList.GetAnswer().ToTupleArray());
                }

                if (changedQuestion.IsDouble)
                {
                    expressionState.UpdateNumericRealAnswer(changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, changedQuestion.AsDouble.GetAnswer().Value);
                }

                if (changedQuestion.IsInteger)
                {
                    expressionState.UpdateNumericIntegerAnswer(changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, changedQuestion.AsInteger.GetAnswer().Value);
                }

                if (changedQuestion.IsDateTime)
                {
                    expressionState.UpdateDateAnswer(changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, changedQuestion.AsDateTime.GetAnswer().Value);
                }

                if (changedQuestion.IsGps)
                {
                    var gpsAnswer = changedQuestion.AsGps.GetAnswer().Value;
                    expressionState.UpdateGeoLocationAnswer(changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, gpsAnswer.Latitude, gpsAnswer.Longitude,
                        gpsAnswer.Accuracy, gpsAnswer.Altitude);
                }

                if (changedQuestion.IsQRBarcode)
                {
                    expressionState.UpdateQrBarcodeAnswer(changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, changedQuestion.AsQRBarcode.GetAnswer().DecodedText);
                }

                if (changedQuestion.IsMultimedia)
                {
                    expressionState.UpdateMediaAnswer(changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, changedQuestion.AsMultimedia.GetAnswer().FileName);
                }

                if (changedQuestion.IsYesNo)
                {
                    expressionState.UpdateYesNoAnswer(changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, changedQuestion.AsYesNo.GetAnswer().ToYesNoAnswersOnly());
                }

                if (changedQuestion.IsSingleFixedOption)
                {
                    expressionState.UpdateSingleOptionAnswer(changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, changedQuestion.AsSingleFixedOption.GetAnswer().SelectedValue);
                }

                if (changedQuestion.IsMultiFixedOption)
                {
                    expressionState.UpdateMultiOptionAnswer(changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, changedQuestion.AsMultiFixedOption.GetAnswer().ToDecimals().ToArray());
                }

                if (changedQuestion.IsSingleLinkedOption)
                {
                    expressionState.UpdateLinkedSingleOptionAnswer(changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, changedQuestion.AsSingleLinkedOption.GetAnswer().SelectedValue);
                }

                if (changedQuestion.IsMultiLinkedOption)
                {
                    expressionState.UpdateLinkedMultiOptionAnswer(changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector,
                        changedQuestion.AsMultiLinkedOption.GetAnswer().ToRosterVectorArray().Select(x => x.Select(Convert.ToDecimal).ToArray()).ToArray());
                }

                if (changedQuestion.IsSingleLinkedToList)
                {
                    expressionState.UpdateSingleOptionAnswer(changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, changedQuestion.AsSingleLinkedToList.GetAnswer().SelectedValue);
                }

                if (changedQuestion.IsMultiLinkedToList)
                {
                    expressionState.UpdateMultiOptionAnswer(changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, changedQuestion.AsMultiLinkedToList.GetAnswer().ToDecimals().ToArray());
                }
            }
        }

        private static void UpdateRostersInExpressionState(InterviewTreeRosterDiff[] diff, ILatestInterviewExpressionState expressionState)
        {
            var removedRosters = diff.Where(x => x.IsNodeRemoved).Select(x => x.SourceNode);
            var addedRosters = diff.Where(x => x.IsNodeAdded)
                .Select(x => x.ChangedNode)
                .OrderBy(x => x.RosterVector.Length)
                .ToList();

            foreach (var removedRosterIdentity in removedRosters.Select(ToRosterInstance))
            {
                expressionState.RemoveRoster(removedRosterIdentity.GroupId, removedRosterIdentity.OuterRosterVector, removedRosterIdentity.RosterInstanceId);
            }

            foreach (var rosterNode in addedRosters)
            {
                expressionState.AddRoster(rosterNode.Identity.Id, rosterNode.Identity.RosterVector.CoordinatesAsDecimals.Shrink(),
                    rosterNode.Identity.RosterVector.Last(), rosterNode.SortIndex);
            }
        }

        private ILatestInterviewExpressionState GetClonedExpressionState()
        {
            ILatestInterviewExpressionState expressionProcessorState = this.ExpressionProcessorStatePrototype.Clone();
            expressionProcessorState.SaveAllCurrentStatesAsPrevious();
            return expressionProcessorState;
        }

        #endregion

        #region Answer handlers

        public void AnswerTextQuestion(Guid userId, Guid questionId, RosterVector rosterVector, DateTime answerTime, string answer)
        {
            new InterviewPropertiesInvariants(this.properties)
                .RequireAnswerCanBeChanged();

            var questionIdentity = new Identity(questionId, rosterVector);

            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();

            new InterviewQuestionInvariants(questionIdentity, questionnaire, this.Tree)
                .RequireTextAnswerAllowed();

            var changedInterviewTree = this.Tree.Clone();

            changedInterviewTree.GetQuestion(questionIdentity).AsText.SetAnswer(TextAnswer.FromString(answer));

            this.UpdateTreeWithDependentChanges(changedInterviewTree, new[] { questionIdentity }, questionnaire);
            var treeDifference = FindDifferenceBetweenTrees(this.Tree, changedInterviewTree);

            this.ApplyEvents(treeDifference, userId);
        }

        public void AnswerDateTimeQuestion(Guid userId, Guid questionId, RosterVector rosterVector, DateTime answerTime, DateTime answer)
        {
            new InterviewPropertiesInvariants(this.properties)
                .RequireAnswerCanBeChanged();

            var questionIdentity = new Identity(questionId, rosterVector);

            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();

            new InterviewQuestionInvariants(questionIdentity, questionnaire, this.Tree)
                .RequireDateTimeAnswerAllowed();

            var changedInterviewTree = this.Tree.Clone();
            changedInterviewTree.GetQuestion(questionIdentity).AsDateTime.SetAnswer(DateTimeAnswer.FromDateTime(answer));

            this.UpdateTreeWithDependentChanges(changedInterviewTree, new[] { questionIdentity }, questionnaire);
            var treeDifference = FindDifferenceBetweenTrees(this.Tree, changedInterviewTree);

            this.ApplyEvents(treeDifference, userId);
        }

        public void AnswerNumericIntegerQuestion(AnswerNumericIntegerQuestionCommand command)
            => AnswerNumericIntegerQuestion(command.UserId, command.QuestionId, command.RosterVector, command.AnswerTime, command.Answer);

        internal void AnswerNumericIntegerQuestion(Guid userId, Guid questionId, RosterVector rosterVector, DateTime answerTime, int answer)
        {
            new InterviewPropertiesInvariants(this.properties)
                .RequireAnswerCanBeChanged();

            var questionIdentity = new Identity(questionId, rosterVector);

            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();

            new InterviewQuestionInvariants(questionIdentity, questionnaire, this.Tree)
                .RequireNumericIntegerAnswerAllowed(answer);

            var changedInterviewTree = this.Tree.Clone();

            changedInterviewTree.GetQuestion(questionIdentity).AsInteger.SetAnswer(NumericIntegerAnswer.FromInt(answer));

            changedInterviewTree.ActualizeTree();

            this.UpdateTreeWithDependentChanges(changedInterviewTree, new[] { questionIdentity }, questionnaire);
            var treeDifference = FindDifferenceBetweenTrees(this.Tree, changedInterviewTree);

            this.ApplyEvents(treeDifference, userId);
        }

        public void AnswerNumericRealQuestion(Guid userId, Guid questionId, RosterVector rosterVector, DateTime answerTime, double answer)
        {
            new InterviewPropertiesInvariants(this.properties)
                .RequireAnswerCanBeChanged();

            var questionIdentity = new Identity(questionId, rosterVector);
            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();

            new InterviewQuestionInvariants(questionIdentity, questionnaire, this.Tree)
                .RequireNumericRealAnswerAllowed(answer);

            var changedInterviewTree = this.Tree.Clone();
            changedInterviewTree.GetQuestion(questionIdentity).AsDouble.SetAnswer(NumericRealAnswer.FromDouble(answer));

            this.UpdateTreeWithDependentChanges(changedInterviewTree, new[] { questionIdentity }, questionnaire);
            var treeDifference = FindDifferenceBetweenTrees(this.Tree, changedInterviewTree);

            this.ApplyEvents(treeDifference, userId);
        }

        public void AnswerSingleOptionQuestion(Guid userId, Guid questionId, RosterVector rosterVector, DateTime answerTime, decimal selectedValue)
        {
            new InterviewPropertiesInvariants(this.properties)
                .RequireAnswerCanBeChanged();

            var questionIdentity = new Identity(questionId, rosterVector);

            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();

            var isLinkedToList = this.Tree.GetQuestion(questionIdentity).IsLinkedToListQuestion;

            if (isLinkedToList)
            {
                new InterviewQuestionInvariants(questionIdentity, questionnaire, this.Tree)
                    .RequireLinkedToListSingleOptionAnswerAllowed(selectedValue);
            }
            else
            {
                new InterviewQuestionInvariants(questionIdentity, questionnaire, this.Tree)
                    .RequireFixedSingleOptionAnswerAllowed(selectedValue, this.QuestionnaireIdentity);
            }

            var changedInterviewTree = this.Tree.Clone();

            var givenAndRemovedAnswers = new List<Identity> { questionIdentity };
            var singleQuestion = changedInterviewTree.GetQuestion(questionIdentity);

            if (isLinkedToList)
            {
                var question = singleQuestion.AsSingleLinkedToList;
                question.SetAnswer(CategoricalFixedSingleOptionAnswer.FromInt(Convert.ToInt32(selectedValue)));
            }
            else
            {
                var question = singleQuestion.AsSingleFixedOption;
                var questionWasAnsweredAndAnswerChanged = question.IsAnswered && question.GetAnswer().SelectedValue != selectedValue;
                question.SetAnswer(CategoricalFixedSingleOptionAnswer.FromInt(Convert.ToInt32(selectedValue)));

                if (questionWasAnsweredAndAnswerChanged)
                {
                    RemoveAnswersForDependendCascadingQuestions(questionIdentity, changedInterviewTree, questionnaire, givenAndRemovedAnswers);
                }
            }

            this.UpdateTreeWithDependentChanges(changedInterviewTree, givenAndRemovedAnswers, questionnaire);
            var treeDifference = FindDifferenceBetweenTrees(this.Tree, changedInterviewTree);

            this.ApplyEvents(treeDifference, userId);
        }

        public void AnswerSingleOptionLinkedQuestion(Guid userId, Guid questionId, RosterVector rosterVector, DateTime answerTime, decimal[] selectedRosterVector)
        {
            new InterviewPropertiesInvariants(this.properties)
                .RequireAnswerCanBeChanged();

            var questionIdentity = new Identity(questionId, rosterVector);

            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();

            new InterviewQuestionInvariants(questionIdentity, questionnaire, this.Tree)
                .RequireLinkedToRosterSingleOptionAnswerAllowed(selectedRosterVector);

            var changedInterviewTree = this.Tree.Clone();

            changedInterviewTree.GetQuestion(questionIdentity).AsSingleLinkedOption.SetAnswer(CategoricalLinkedSingleOptionAnswer.FromRosterVector(selectedRosterVector));

            this.UpdateTreeWithDependentChanges(changedInterviewTree, new[] { questionIdentity }, questionnaire);
            var treeDifference = FindDifferenceBetweenTrees(this.Tree, changedInterviewTree);

            this.ApplyEvents(treeDifference, userId);
        }

        public void AnswerMultipleOptionsQuestion(Guid userId, Guid questionId, RosterVector rosterVector, DateTime answerTime, int[] selectedValues)
        {
            new InterviewPropertiesInvariants(this.properties)
                .RequireAnswerCanBeChanged();

            var questionIdentity = new Identity(questionId, rosterVector);

            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();

            var isLinkedToList = this.Tree.GetQuestion(questionIdentity).IsLinkedToListQuestion;

            if (isLinkedToList)
            {
                new InterviewQuestionInvariants(questionIdentity, questionnaire, this.Tree)
                    .RequireLinkedToListMultipleOptionsAnswerAllowed(selectedValues);
            }
            else
            {
                new InterviewQuestionInvariants(questionIdentity, questionnaire, this.Tree)
                    .RequireFixedMultipleOptionsAnswerAllowed(selectedValues);
            }

            var changedInterviewTree = this.Tree.Clone();

            if (isLinkedToList)
            {
                changedInterviewTree.GetQuestion(questionIdentity).AsMultiLinkedToList.SetAnswer(CategoricalFixedMultiOptionAnswer.FromInts(selectedValues));
            }
            else
                changedInterviewTree.GetQuestion(questionIdentity).AsMultiFixedOption.SetAnswer(CategoricalFixedMultiOptionAnswer.FromInts(selectedValues));

            changedInterviewTree.ActualizeTree();

            this.UpdateTreeWithDependentChanges(changedInterviewTree, new[] { questionIdentity }, questionnaire);
            var treeDifference = FindDifferenceBetweenTrees(this.Tree, changedInterviewTree);

            this.ApplyEvents(treeDifference, userId);
        }

        public void AnswerMultipleOptionsLinkedQuestion(Guid userId, Guid questionId, RosterVector rosterVector,
            DateTime answerTime, RosterVector[] selectedRosterVectors)
        {
            new InterviewPropertiesInvariants(this.properties)
                .RequireAnswerCanBeChanged();

            var questionIdentity = new Identity(questionId, rosterVector);

            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();

            new InterviewQuestionInvariants(questionIdentity, questionnaire, this.Tree)
                .RequireLinkedToRosterMultipleOptionsAnswerAllowed(selectedRosterVectors);

            var changedInterviewTree = this.Tree.Clone();

            changedInterviewTree.GetQuestion(questionIdentity)
                .AsMultiLinkedOption
                .SetAnswer(CategoricalLinkedMultiOptionAnswer.FromRosterVectors(selectedRosterVectors));

            this.UpdateTreeWithDependentChanges(changedInterviewTree, new[] { questionIdentity }, questionnaire);
            var treeDifference = FindDifferenceBetweenTrees(this.Tree, changedInterviewTree);

            this.ApplyEvents(treeDifference, userId);
        }

        public void AnswerYesNoQuestion(AnswerYesNoQuestion command)
        {
            new InterviewPropertiesInvariants(this.properties)
                .RequireAnswerCanBeChanged();

            var questionIdentity = new Identity(command.QuestionId, command.RosterVector);

            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();

            new InterviewQuestionInvariants(command.Question, questionnaire, this.Tree)
                .RequireYesNoAnswerAllowed(YesNoAnswer.FromAnsweredYesNoOptions(command.AnsweredOptions));

            var changedInterviewTree = this.Tree.Clone();

            changedInterviewTree.GetQuestion(questionIdentity).AsYesNo.SetAnswer(YesNoAnswer.FromAnsweredYesNoOptions(command.AnsweredOptions));

            changedInterviewTree.ActualizeTree();

            this.UpdateTreeWithDependentChanges(changedInterviewTree, new[] { questionIdentity }, questionnaire);
            var treeDifference = FindDifferenceBetweenTrees(this.Tree, changedInterviewTree);

            this.ApplyEvents(treeDifference, command.UserId);
        }

        public void AnswerTextListQuestion(Guid userId, Guid questionId, RosterVector rosterVector, DateTime answerTime,
            Tuple<decimal, string>[] answers)
        {
            new InterviewPropertiesInvariants(this.properties)
                .RequireAnswerCanBeChanged();

            var questionIdentity = new Identity(questionId, rosterVector);

            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();

            new InterviewQuestionInvariants(questionIdentity, questionnaire, this.Tree)
                .RequireTextListAnswerAllowed(answers);

            var changedInterviewTree = this.Tree.Clone();

            changedInterviewTree.GetQuestion(questionIdentity).AsTextList.SetAnswer(TextListAnswer.FromTupleArray(answers));

            changedInterviewTree.ActualizeTree();

            this.UpdateTreeWithDependentChanges(changedInterviewTree, new[] { questionIdentity }, questionnaire);
            var treeDifference = FindDifferenceBetweenTrees(this.Tree, changedInterviewTree);

            this.ApplyEvents(treeDifference, userId);
        }

        public void AnswerGeoLocationQuestion(Guid userId, Guid questionId, RosterVector rosterVector, DateTime answerTime, double latitude, double longitude,
            double accuracy, double altitude, DateTimeOffset timestamp)
        {
            new InterviewPropertiesInvariants(this.properties)
                .RequireAnswerCanBeChanged();

            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();
            var questionIdentity = new Identity(questionId, rosterVector);

            new InterviewQuestionInvariants(questionIdentity, questionnaire, this.Tree)
                .RequireGpsCoordinatesAnswerAllowed();

            var changedInterviewTree = this.Tree.Clone();

            var answer = new GeoPosition(latitude, longitude, accuracy, altitude, timestamp);
            changedInterviewTree.GetQuestion(questionIdentity).AsGps.SetAnswer(GpsAnswer.FromGeoPosition(answer));

            this.UpdateTreeWithDependentChanges(changedInterviewTree, new[] { questionIdentity }, questionnaire);
            var treeDifference = FindDifferenceBetweenTrees(this.Tree, changedInterviewTree);

            this.ApplyEvents(treeDifference, userId);
        }

        public void AnswerQRBarcodeQuestion(Guid userId, Guid questionId, RosterVector rosterVector, DateTime answerTime, string answer)
        {
            new InterviewPropertiesInvariants(this.properties)
                .RequireAnswerCanBeChanged();

            var questionIdentity = new Identity(questionId, rosterVector);

            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();

            new InterviewQuestionInvariants(questionIdentity, questionnaire, this.Tree)
                .RequireQRBarcodeAnswerAllowed();

            var changedInterviewTree = this.Tree.Clone();
            changedInterviewTree.GetQuestion(questionIdentity).AsQRBarcode.SetAnswer(QRBarcodeAnswer.FromString(answer));

            this.UpdateTreeWithDependentChanges(changedInterviewTree, new [] { questionIdentity }, questionnaire);
            var treeDifference = FindDifferenceBetweenTrees(this.Tree, changedInterviewTree);

            this.ApplyEvents(treeDifference, userId);
        }

        public void AnswerPictureQuestion(Guid userId, Guid questionId, RosterVector rosterVector, DateTime answerTime, string pictureFileName)
        {
            new InterviewPropertiesInvariants(this.properties)
                .RequireAnswerCanBeChanged();

            var questionIdentity = new Identity(questionId, rosterVector);

            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();

            new InterviewQuestionInvariants(questionIdentity, questionnaire, this.Tree)
                .RequirePictureAnswerAllowed();

            var changedInterviewTree = this.Tree.Clone();

            changedInterviewTree.GetQuestion(questionIdentity).AsMultimedia.SetAnswer(MultimediaAnswer.FromString(pictureFileName));

            this.UpdateTreeWithDependentChanges(changedInterviewTree, new[] { questionIdentity }, questionnaire);
            var treeDifference = FindDifferenceBetweenTrees(this.Tree, changedInterviewTree);

            this.ApplyEvents(treeDifference, userId);
        }

        public void RemoveAnswer(Guid questionId, RosterVector rosterVector, Guid userId, DateTime removeTime)
        {
            new InterviewPropertiesInvariants(this.properties)
                .RequireAnswerCanBeChanged();

            var questionIdentity = new Identity(questionId, rosterVector);

            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();

            new InterviewQuestionInvariants(questionIdentity, questionnaire, this.Tree)
                .RequireQuestionExists()
                .RequireQuestionEnabled();

            var changedInterviewTree = this.Tree.Clone();

            var givenAndRemovedAnswers = new List<Identity> { questionIdentity };
            changedInterviewTree.GetQuestion(questionIdentity).RemoveAnswer();

            RemoveAnswersForDependendCascadingQuestions(questionIdentity, changedInterviewTree, questionnaire, givenAndRemovedAnswers);

            changedInterviewTree.ActualizeTree();

            this.UpdateTreeWithDependentChanges(changedInterviewTree, givenAndRemovedAnswers, questionnaire);
            var treeDifference = FindDifferenceBetweenTrees(this.Tree, changedInterviewTree);

            this.ApplyEvents(treeDifference, userId);
        }

        private static void RemoveAnswersForDependendCascadingQuestions(Identity questionIdentity, InterviewTree changedInterviewTree, IQuestionnaire questionnaire, List<Identity> givenAndRemovedAnswers)
        {
            IEnumerable<Guid> dependentQuestionIds = questionnaire.GetCascadingQuestionsThatDependUponQuestion(questionIdentity.Id);
            foreach (var dependentQuestionId in dependentQuestionIds)
            {
                var cascadingAnsweredQuestionsToRemoveAnswer = changedInterviewTree.FindQuestions(dependentQuestionId)
                    .Where(x => x.IsCascading && x.IsAnswered())
                    .Where(x => x.IsOnTheSameOrDeeperLevel(questionIdentity));

                foreach (var cascadingQuestion in cascadingAnsweredQuestionsToRemoveAnswer)
                {
                    cascadingQuestion.RemoveAnswer();
                    givenAndRemovedAnswers.Add(cascadingQuestion.Identity);
                }
            }
        }

        #endregion

        #region Other handlers

        public void CreateInterviewWithPreloadedData(CreateInterviewWithPreloadedData command)
        {
            this.QuestionnaireIdentity = new QuestionnaireIdentity(command.QuestionnaireId, command.Version);

            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();

            InterviewTree changedInterviewTree = this.Tree.Clone();

            PreloadedLevelDto[] orderedData = command.PreloadedData.Data.OrderBy(x => x.RosterVector.Length).ToArray();
            List<Identity> answeredQuestions = orderedData.SelectMany(x => x.Answers.Select(y => new Identity(y.Key, x.RosterVector))).ToList();

            for (int index = 0; index < orderedData.Length; index++)
            {
                PreloadedLevelDto preloadedLevel = orderedData[index];
                Dictionary<Guid, AbstractAnswer> answersToFeaturedQuestions = preloadedLevel.Answers;

                this.ValidatePreloadValues(changedInterviewTree, questionnaire, answersToFeaturedQuestions, preloadedLevel.RosterVector);

                Dictionary<Identity, AbstractAnswer> prefilledQuestionsWithAnswers = answersToFeaturedQuestions.ToDictionary(
                    answersToFeaturedQuestion =>
                        new Identity(answersToFeaturedQuestion.Key, preloadedLevel.RosterVector),
                    answersToFeaturedQuestion => answersToFeaturedQuestion.Value);

                foreach (KeyValuePair<Identity, AbstractAnswer> answer in prefilledQuestionsWithAnswers)
                {
                    changedInterviewTree.GetQuestion(answer.Key).SetAnswer(answer.Value);
                }

                bool isLastDataLevel = index == orderedData.Length - 1;
                bool nextDataLevelIsDeeperThanPrevious = preloadedLevel.RosterVector.Length != orderedData[Math.Min(index + 1, orderedData.Length - 1)].RosterVector.Length;
                if (isLastDataLevel || nextDataLevelIsDeeperThanPrevious)
                {
                    changedInterviewTree.ActualizeTree();
                }
            }

            this.UpdateTreeWithDependentChanges(changedInterviewTree, answeredQuestions, questionnaire);
            IReadOnlyCollection<InterviewTreeNodeDiff> treeDifference = FindDifferenceBetweenTrees(this.Tree, changedInterviewTree);

            //apply events
            this.ApplyEvent(new InterviewFromPreloadedDataCreated(command.UserId,
                this.QuestionnaireIdentity.QuestionnaireId, this.QuestionnaireIdentity.Version));

            this.ApplyEvents(treeDifference, command.UserId);

            this.ApplyEvent(new SupervisorAssigned(command.UserId, command.SupervisorId));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.SupervisorAssigned, comment: null));

            if (command.InterviewerId.HasValue)
            {
                this.ApplyEvent(new InterviewerAssigned(command.UserId, command.InterviewerId.Value, command.AnswersTime));
                this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.InterviewerAssigned, comment: null));
            }

            this.ApplyInterviewKey(command.InterviewKey);
        }

        public void CreateInterview(Guid questionnaireId, 
            long questionnaireVersion, 
            Guid supervisorId,
            Dictionary<Guid, AbstractAnswer> answersToFeaturedQuestions, 
            DateTime answersTime, 
            Guid userId)
        {
            this.CreateInterview(new CreateInterviewCommand(this.EventSourceId, userId, questionnaireId, 
                answersToFeaturedQuestions, answersTime, supervisorId, questionnaireVersion, null));
        }

        public void CreateInterview(CreateInterviewCommand command)
        {
            var userId = command.UserId;

            this.QuestionnaireIdentity = new QuestionnaireIdentity(command.QuestionnaireId, command.QuestionnaireVersion);

            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();

            InterviewTree changedInterviewTree = this.Tree.Clone();

            this.ValidatePrefilledAnswers(this.Tree, questionnaire, command.AnswersToFeaturedQuestions, RosterVector.Empty);

            Dictionary<Identity, AbstractAnswer> prefilledQuestionsWithAnswers = command.AnswersToFeaturedQuestions.ToDictionary(x => new Identity(x.Key, RosterVector.Empty), x => x.Value);
            foreach (KeyValuePair<Identity, AbstractAnswer> answer in prefilledQuestionsWithAnswers)
            {
                changedInterviewTree.GetQuestion(answer.Key).SetAnswer(answer.Value);
            }

            List<Identity> answeredQuestions = prefilledQuestionsWithAnswers.Keys.ToList();

            changedInterviewTree.ActualizeTree();

            this.UpdateTreeWithDependentChanges(changedInterviewTree, answeredQuestions, questionnaire);
            IReadOnlyCollection<InterviewTreeNodeDiff> treeDifference = FindDifferenceBetweenTrees(this.Tree, changedInterviewTree);

            //apply events
            this.ApplyEvent(new InterviewCreated(userId, command.QuestionnaireId, questionnaire.Version));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.Created, comment: null));

            this.ApplyEvents(treeDifference, userId);

            this.ApplyEvent(new SupervisorAssigned(userId, command.SupervisorId));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.SupervisorAssigned, comment: null));
            this.ApplyInterviewKey(command.InterviewKey);
        }

        private void ApplyInterviewKey(InterviewKey key)
        {
            if (this.interviewKey == null && key != null)
            {
                this.ApplyEvent(new InterviewKeyAssigned(key));
            }
        }

        //todo should respect changes calculated in ExpressionState
        public void ReevaluateSynchronizedInterview()
        {
            InterviewPropertiesInvariants propertiesInvariants = new InterviewPropertiesInvariants(this.properties);

            propertiesInvariants.ThrowIfInterviewHardDeleted();

            ILatestInterviewExpressionState expressionProcessorState = this.ExpressionProcessorStatePrototype.Clone();

            expressionProcessorState.SaveAllCurrentStatesAsPrevious();

            InterviewTree changedInterviewTree = this.Tree.Clone();

            EnablementChanges enablementChanges = expressionProcessorState.ProcessEnablementConditions();
            this.UpdateTreeWithEnablementChanges(changedInterviewTree, enablementChanges);

            ValidityChanges validationChanges = expressionProcessorState.ProcessValidationExpressions();
            this.UpdateTreeWithValidationChanges(changedInterviewTree, validationChanges);

            IReadOnlyCollection<InterviewTreeNodeDiff> treeDifference = FindDifferenceBetweenTrees(this.Tree, changedInterviewTree);

            this.ApplyEvents(treeDifference);

            if (!this.HasInvalidAnswers())
            {
                this.ApplyEvent(new InterviewDeclaredValid());
            }
        }

        public void RepeatLastInterviewStatus(RepeatLastInterviewStatus command)
        {
            this.ApplyEvent(new InterviewStatusChanged(this.properties.Status, command.Comment));
        }

        public void SwitchTranslation(SwitchTranslation command)
        {
            InterviewPropertiesInvariants propertiesInvariants = new InterviewPropertiesInvariants(this.properties);

            propertiesInvariants.ThrowIfInterviewHardDeleted();

            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();
            IReadOnlyCollection<string> availableLanguages = questionnaire.GetTranslationLanguages();

            if (command.Language != null)
            {
                if (availableLanguages.All(language => language != command.Language))
                    throw new InterviewException(
                        $"Questionnaire does not have translation. Language: {command.Language}. " +
                        $"Interview ID: {this.EventSourceId.FormatGuid()}. " +
                        $"Questionnaire ID: {this.QuestionnaireIdentity}.");
            }

            IQuestionnaire targetQuestionnaire = this.GetQuestionnaireOrThrow(command.Language);

            InterviewTree changedInterviewTree = this.Tree.Clone();
            changedInterviewTree.SwitchQuestionnaire(targetQuestionnaire);

            this.UpdateRosterTitles(changedInterviewTree, targetQuestionnaire);

            IReadOnlyCollection<InterviewTreeNodeDiff> treeDifference = FindDifferenceBetweenTrees(this.Tree, changedInterviewTree);

            this.ApplyEvents(treeDifference, command.UserId);
            this.ApplyEvent(new TranslationSwitched(command.Language, command.UserId));
            
        }

        public void CommentAnswer(Guid userId, Guid questionId, RosterVector rosterVector, DateTime commentTime, string comment)
        {
            new InterviewPropertiesInvariants(this.properties)
                .RequireAnswerCanBeChanged();

            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();

            new InterviewQuestionInvariants(new Identity(questionId, rosterVector), questionnaire, this.Tree)
                .RequireQuestionExists();

            this.ApplyEvent(new AnswerCommented(userId, questionId, rosterVector, commentTime, comment));
        }

        public void SetFlagToAnswer(Guid userId, Guid questionId, RosterVector rosterVector)
        {
            new InterviewPropertiesInvariants(this.properties)
                .RequireAnswerCanBeChanged();

            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();

            new InterviewQuestionInvariants(new Identity(questionId, rosterVector), questionnaire, this.Tree)
                .RequireQuestionExists();

            this.ApplyEvent(new FlagSetToAnswer(userId, questionId, rosterVector));
        }

        public void RemoveFlagFromAnswer(Guid userId, Guid questionId, RosterVector rosterVector)
        {
            new InterviewPropertiesInvariants(this.properties)
                .RequireAnswerCanBeChanged();

            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();

            new InterviewQuestionInvariants(new Identity(questionId, rosterVector), questionnaire, this.Tree)
                .RequireQuestionExists();

            this.ApplyEvent(new FlagRemovedFromAnswer(userId, questionId, rosterVector));
        }

        public void AssignSupervisor(Guid userId, Guid supervisorId)
        {
            InterviewPropertiesInvariants propertiesInvariants = new InterviewPropertiesInvariants(this.properties);

            propertiesInvariants.ThrowIfInterviewHardDeleted();
            propertiesInvariants.ThrowIfInterviewStatusIsNotOneOfExpected(InterviewStatus.Created, InterviewStatus.InterviewerAssigned, InterviewStatus.SupervisorAssigned, InterviewStatus.Completed, InterviewStatus.RejectedBySupervisor, InterviewStatus.RejectedByHeadquarters);

            this.ApplyEvent(new SupervisorAssigned(userId, supervisorId));

            if (this.properties.InterviewerId.HasValue)
            {
                this.ApplyEvent(new InterviewerAssigned(userId, null, null));
            }

            if (this.properties.Status == InterviewStatus.Created || this.properties.Status == InterviewStatus.InterviewerAssigned)
            {
                this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.SupervisorAssigned, comment: null));
            }
        }

        public void AssignInterviewer(Guid userId, Guid interviewerId, DateTime assignTime)
        {
            InterviewPropertiesInvariants propertiesInvariants = new InterviewPropertiesInvariants(this.properties);

            propertiesInvariants.ThrowIfInterviewHardDeleted();
            propertiesInvariants.ThrowIfInterviewStatusIsNotOneOfExpected(InterviewStatus.SupervisorAssigned, InterviewStatus.InterviewerAssigned, InterviewStatus.RejectedBySupervisor);
            propertiesInvariants.ThrowIfTryAssignToSameInterviewer(interviewerId);

            this.ApplyEvent(new InterviewerAssigned(userId, interviewerId, assignTime));
            if (!this.properties.WasCompleted && this.properties.Status == InterviewStatus.SupervisorAssigned)
            {
                this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.InterviewerAssigned, comment: null));
            }
        }

        public void Delete(Guid userId)
        {
            InterviewPropertiesInvariants propertiesInvariants = new InterviewPropertiesInvariants(this.properties);

            propertiesInvariants.ThrowIfInterviewHardDeleted();
            propertiesInvariants.ThrowIfInterviewWasCompleted();
            propertiesInvariants.ThrowIfInterviewStatusIsNotOneOfExpected(
                InterviewStatus.Created, InterviewStatus.SupervisorAssigned, InterviewStatus.InterviewerAssigned, InterviewStatus.Restored);

            this.ApplyEvent(new InterviewDeleted(userId));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.Deleted, comment: null));
        }

        public void HardDelete(Guid userId)
        {
            if (this.properties.IsHardDeleted)
                return;

            this.ApplyEvent(new InterviewHardDeleted(userId));
        }

        public void MarkInterviewAsReceivedByInterviwer(Guid userId)
        {
            InterviewPropertiesInvariants propertiesInvariants = new InterviewPropertiesInvariants(this.properties);

            propertiesInvariants.ThrowIfInterviewHardDeleted();

            this.ApplyEvent(new InterviewReceivedByInterviewer());
        }

        public void Restore(Guid userId)
        {
            InterviewPropertiesInvariants propertiesInvariants = new InterviewPropertiesInvariants(this.properties);

            propertiesInvariants.ThrowIfInterviewHardDeleted();
            propertiesInvariants.ThrowIfInterviewStatusIsNotOneOfExpected(InterviewStatus.Deleted);

            this.ApplyEvent(new InterviewRestored(userId));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.Restored, comment: null));
        }

        public void Restart(Guid userId, string comment, DateTime restartTime)
        {
            InterviewPropertiesInvariants propertiesInvariants = new InterviewPropertiesInvariants(this.properties);

            propertiesInvariants.ThrowIfInterviewHardDeleted();
            propertiesInvariants.ThrowIfInterviewStatusIsNotOneOfExpected(InterviewStatus.Completed);

            this.ApplyEvent(new InterviewRestarted(userId, restartTime, comment));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.Restarted, comment));
        }

        public void Approve(Guid userId, string comment, DateTime approveTime)
        {
            InterviewPropertiesInvariants propertiesInvariants = new InterviewPropertiesInvariants(this.properties);

            propertiesInvariants.ThrowIfInterviewHardDeleted();
            propertiesInvariants.ThrowIfInterviewStatusIsNotOneOfExpected(InterviewStatus.Completed,
                InterviewStatus.RejectedBySupervisor,
                InterviewStatus.RejectedByHeadquarters);

            this.ApplyEvent(new InterviewApproved(userId, comment, approveTime));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.ApprovedBySupervisor, comment));
        }

        public void Reject(Guid userId, string comment, DateTime rejectTime)
        {
            InterviewPropertiesInvariants propertiesInvariants = new InterviewPropertiesInvariants(this.properties);

            propertiesInvariants.ThrowIfInterviewHardDeleted();
            propertiesInvariants.ThrowIfInterviewStatusIsNotOneOfExpected(InterviewStatus.Completed,
                InterviewStatus.ApprovedBySupervisor,
                InterviewStatus.RejectedByHeadquarters);

            this.ApplyEvent(new InterviewRejected(userId, comment, rejectTime));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.RejectedBySupervisor, comment));
        }

        public void RejectToInterviewer(Guid userId, Guid interviewerId, string comment, DateTime rejectTime)
        {
            InterviewPropertiesInvariants propertiesInvariants = new InterviewPropertiesInvariants(this.properties);

            propertiesInvariants.ThrowIfInterviewHardDeleted();
            propertiesInvariants.ThrowIfInterviewStatusIsNotOneOfExpected(InterviewStatus.Completed,
                InterviewStatus.ApprovedBySupervisor,
                InterviewStatus.RejectedByHeadquarters);

            this.ApplyEvent(new InterviewRejected(userId, comment, rejectTime));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.RejectedBySupervisor, comment));
            this.ApplyEvent(new InterviewerAssigned(userId, interviewerId, rejectTime));
        }

        public void HqApprove(Guid userId, string comment)
        {
            InterviewPropertiesInvariants propertiesInvariants = new InterviewPropertiesInvariants(this.properties);

            propertiesInvariants.ThrowIfInterviewHardDeleted();
            propertiesInvariants.ThrowIfInterviewStatusIsNotOneOfExpected(InterviewStatus.Completed, InterviewStatus.ApprovedBySupervisor, InterviewStatus.RejectedByHeadquarters);

            this.ApplyEvent(new InterviewApprovedByHQ(userId, comment));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.ApprovedByHeadquarters, comment));
        }

        public void UnapproveByHeadquarters(Guid userId, string comment)
        {
            InterviewPropertiesInvariants propertiesInvariants = new InterviewPropertiesInvariants(this.properties);

            propertiesInvariants.ThrowIfInterviewHardDeleted();
            propertiesInvariants.ThrowIfInterviewStatusIsNotOneOfExpected(InterviewStatus.ApprovedByHeadquarters);

            string unapproveCommentMessage = "[Approved by Headquarters was revoked]";
            string unapproveComment = string.IsNullOrEmpty(comment)
                ? unapproveCommentMessage
                : string.Format("{0} \r\n {1}", unapproveCommentMessage, comment);
            this.ApplyEvent(new UnapprovedByHeadquarters(userId, unapproveComment));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.ApprovedBySupervisor, comment));
        }

        public void RejectInterviewFromHeadquarters(Guid userId,
            Guid supervisorId,
            Guid? interviewerId,
            InterviewSynchronizationDto interviewDto,
            DateTime synchronizationTime)
        {
            var commentedAnswers = (
                from answerDto in interviewDto.Answers
                from answerComment in answerDto.AllComments
                where !this.Tree.GetQuestion(Identity.Create(answerDto.Id, answerDto.QuestionRosterVector)).AnswerComments.Contains(ToAnswerComment(answerComment, answerDto))
                select new
                {
                    UserId = answerComment.UserId,
                    Date = answerComment.Date,
                    Text = answerComment.Text,
                    QuestionId = answerDto.Id,
                    RosterVector = answerDto.QuestionRosterVector
                });

            if (this.properties.Status == InterviewStatus.Deleted)
            {
                this.ApplyEvent(new InterviewRestored(userId));
            }


            this.ApplyEvent(new InterviewRejectedByHQ(userId, interviewDto.Comments));
            this.ApplyEvent(new InterviewStatusChanged(interviewDto.Status, comment: interviewDto.Comments));

            if (interviewerId.HasValue)
            {
                this.ApplyEvent(new InterviewerAssigned(userId, interviewerId.Value, synchronizationTime));
            }

            foreach (var commentedAnswer in commentedAnswers)
            {
                this.ApplyEvent(new AnswerCommented(commentedAnswer.UserId, commentedAnswer.QuestionId,
                    commentedAnswer.RosterVector, commentedAnswer.Date, commentedAnswer.Text));
            }
        }

        public void HqReject(Guid userId, string comment)
        {
            InterviewPropertiesInvariants propertiesInvariants = new InterviewPropertiesInvariants(this.properties);

            propertiesInvariants.ThrowIfInterviewHardDeleted();
            propertiesInvariants.ThrowIfInterviewStatusIsNotOneOfExpected(InterviewStatus.ApprovedBySupervisor, InterviewStatus.Deleted);

            this.ApplyEvent(new InterviewRejectedByHQ(userId, comment));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.RejectedByHeadquarters, comment));
        }

        public void SynchronizeInterviewEvents(SynchronizeInterviewEventsCommand command)
        {
            this.QuestionnaireIdentity = new QuestionnaireIdentity(command.QuestionnaireId, command.QuestionnaireVersion);

            InterviewPropertiesInvariants propertiesInvariants = new InterviewPropertiesInvariants(this.properties);

            bool isInterviewNeedToBeCreated = command.CreatedOnClient && this.Version == 0;

            if (isInterviewNeedToBeCreated)
            {
                this.ApplyEvent(new InterviewOnClientCreated(command.UserId, command.QuestionnaireId, command.QuestionnaireVersion));
            }
            else
            {
                propertiesInvariants.ThrowIfOtherInterviewerIsResponsible(command.UserId);

                if (this.properties.Status == InterviewStatus.Deleted)
                    this.Restore(command.UserId);
                else
                    propertiesInvariants.ThrowIfStatusNotAllowedToBeChangedWithMetadata(command.InterviewStatus);
            }

            foreach (IEvent synchronizedEvent in command.SynchronizedEvents)
            {
                this.ApplyEvent(synchronizedEvent);
            }

            this.ApplyInterviewKey(command.InterviewKey);
            this.ApplyEvent(new InterviewReceivedBySupervisor());
        }

        public void CreateInterviewFromSynchronizationMetadata(Guid id, Guid userId, Guid questionnaireId, long questionnaireVersion,
            InterviewStatus interviewStatus,
            AnsweredQuestionSynchronizationDto[] featuredQuestionsMeta,
            string comments,
            DateTime? rejectedDateTime,
            DateTime? interviewerAssignedDateTime,
            bool valid,
            bool createdOnClient)
        {
            this.QuestionnaireIdentity = new QuestionnaireIdentity(questionnaireId, questionnaireVersion);

            InterviewPropertiesInvariants propertiesInvariants = new InterviewPropertiesInvariants(this.properties);

            if (this.properties.Status == InterviewStatus.Deleted)
                this.Restore(userId);
            else
                propertiesInvariants.ThrowIfStatusNotAllowedToBeChangedWithMetadata(interviewStatus);

            this.ApplyEvent(new SynchronizationMetadataApplied(userId,
                questionnaireId,
                questionnaireVersion,
                interviewStatus,
                featuredQuestionsMeta,
                createdOnClient,
                comments,
                rejectedDateTime,
                interviewerAssignedDateTime));

            this.ApplyEvent(new InterviewStatusChanged(interviewStatus, comments));

            if (valid)
                this.ApplyEvent(new InterviewDeclaredValid());
            else
                this.ApplyEvent(new InterviewDeclaredInvalid());
        }

        #endregion

        #region Tree

        protected InterviewTree BuildInterviewTree(IQuestionnaire questionnaire)
        {
            var tree = new InterviewTree(this.EventSourceId, questionnaire, this.substitionTextFactory);
            var sections = this.BuildInterviewTreeSections(tree, questionnaire, this.substitionTextFactory).ToArray();

            tree.SetSections(sections);
            return tree;
        }

        private IEnumerable<InterviewTreeSection> BuildInterviewTreeSections(InterviewTree tree, IQuestionnaire questionnaire, ISubstitionTextFactory textFactory)
        {
            var sectionIds = questionnaire.GetAllSections();

            foreach (var sectionId in sectionIds)
            {
                var sectionIdentity = new Identity(sectionId, RosterVector.Empty);
                var section = this.BuildInterviewTreeSection(tree, sectionIdentity, questionnaire, textFactory);

                yield return section;
            }
        }

        private InterviewTreeSection BuildInterviewTreeSection(InterviewTree tree, Identity sectionIdentity, IQuestionnaire questionnaire, ISubstitionTextFactory textFactory)
        {
            var section = InterviewTree.CreateSection(tree, questionnaire, textFactory, sectionIdentity);

            section.AddChildren(this.BuildInterviewTreeGroupChildren(tree, sectionIdentity, questionnaire, textFactory).ToList());

            return section;
        }

        private InterviewTreeSubSection BuildInterviewTreeSubSection(InterviewTree tree, Identity groupIdentity, IQuestionnaire questionnaire, ISubstitionTextFactory textFactory)
        {
            var subSection = InterviewTree.CreateSubSection(tree, questionnaire, textFactory, groupIdentity);

            subSection.AddChildren(this.BuildInterviewTreeGroupChildren(tree, groupIdentity, questionnaire, textFactory).ToList());

            return subSection;
        }

        private IEnumerable<IInterviewTreeNode> BuildInterviewTreeGroupChildren(InterviewTree tree, Identity groupIdentity, IQuestionnaire questionnaire, ISubstitionTextFactory textFactory)
        {
            var childIds = questionnaire.GetChildEntityIds(groupIdentity.Id);

            foreach (var childId in childIds)
            {
                Identity entityIdentity = new Identity(childId, groupIdentity.RosterVector);

                if (questionnaire.IsRosterGroup(childId)) continue;

                if (questionnaire.HasGroup(childId))
                    yield return this.BuildInterviewTreeSubSection(tree, entityIdentity, questionnaire, textFactory);
                else if (questionnaire.HasQuestion(childId))
                    yield return InterviewTree.CreateQuestion(tree, questionnaire, textFactory, entityIdentity);
                else if (questionnaire.IsStaticText(childId))
                    yield return InterviewTree.CreateStaticText(tree, questionnaire, textFactory, entityIdentity);
                else if (questionnaire.IsVariable(childId))
                    yield return InterviewTree.CreateVariable(entityIdentity);

            }
        }

        #endregion

        #region Events raising

        protected void ApplyPassiveEvents(IReadOnlyCollection<InterviewTreeNodeDiff> diff)
        {
            var diffByQuestions = diff.OfType<InterviewTreeQuestionDiff>().ToList();
            var questionsWithChangedOptionsSet = diffByQuestions.Where(x => x.AreLinkedOptionsChanged).ToArray();
            var questionsWithChangedLinkedToListOptionsSet = diffByQuestions.Where(x => x.AreLinkedToListOptionsChanged).ToArray();
            var changedVariables = diff.OfType<InterviewTreeVariableDiff>().ToArray();

            this.ApplyEnablementEvents(diff);
            this.ApplyValidityEvents(diff);
            this.ApplyVariableEvents(changedVariables);
            this.ApplyLinkedOptionsChangesEvents(questionsWithChangedOptionsSet);
            this.ApplyLinkedToListOptionsChangesEvents(questionsWithChangedLinkedToListOptionsSet);
            this.ApplySubstitutionEvents(diff);
        }

        protected void ApplyEvents(IReadOnlyCollection<InterviewTreeNodeDiff> diff, Guid? responsibleId = null)
        {
            var diffByQuestions = diff.OfType<InterviewTreeQuestionDiff>().ToList();
            var questionsWithRemovedAnswer = diffByQuestions.Where(x => x.IsAnswerRemoved).ToArray();

            var questionsWithChangedAnswer = diffByQuestions.Where(x => x.IsAnswerChanged).ToArray();
            var changedRosters = diff.OfType<InterviewTreeRosterDiff>().ToArray();

            var questionsRosterLevels = questionsWithChangedAnswer.Select(x => x.Identity.RosterVector).ToHashSet();
            var rosterLevels = changedRosters.Select(x => x.Identity.RosterVector).ToHashSet();
            var intersection = questionsRosterLevels.Intersect(rosterLevels).ToList();

            var aggregatedEventsShouldBeFired = intersection.Count == 0;
            if (aggregatedEventsShouldBeFired)
            {
                this.ApplyUpdateAnswerEvents(questionsWithChangedAnswer, responsibleId.Value);
                this.ApplyRosterEvents(changedRosters);
            }
            else
            {
                var maxDepth = Math.Max(questionsRosterLevels.Max(x => x.Length), rosterLevels.Select(x => x.Length).Max());
                // events fired by levels (from the questionnaire level down to nested rosters)
                for (int i = 0; i <= maxDepth; i++)
                {
                    this.ApplyUpdateAnswerEvents(questionsWithChangedAnswer.Where(x => x.Identity.RosterVector.Length == i).ToArray(), responsibleId.Value);
                    this.ApplyRosterEvents(changedRosters.Where(x => x.Identity.RosterVector.Length == i + 1).ToArray());
                }
            }
            this.ApplyRemoveAnswerEvents(questionsWithRemovedAnswer);
            this.ApplyPassiveEvents(diff);
        }

        private void ApplySubstitutionEvents(IReadOnlyCollection<InterviewTreeNodeDiff> diff)
        {
            var groupsWithChangedTitles = diff.OfType<InterviewTreeGroupDiff>().Where(x => x.IsTitleChanged).Select(x => x.ChangedNode.Identity).ToArray();
            var questionsWithChangedTitles = diff.OfType<InterviewTreeQuestionDiff>().Where(x => x.IsTitleChanged || x.AreValidationMessagesChanged).Select(x => x.ChangedNode.Identity).ToArray();
            var staticTextsWithChangedTitles = diff.OfType<InterviewTreeStaticTextDiff>().Where(x => x.IsTitleChanged || x.AreValidationMessagesChanged).Select(x => x.ChangedNode.Identity).ToArray();

            if (groupsWithChangedTitles.Any() || questionsWithChangedTitles.Any() || staticTextsWithChangedTitles.Any())
            {
                this.ApplyEvent(new SubstitutionTitlesChanged(questionsWithChangedTitles, staticTextsWithChangedTitles, groupsWithChangedTitles));
            }
        }

        private void ApplyLinkedOptionsChangesEvents(InterviewTreeQuestionDiff[] questionsWithChangedOptionsSet)
        {
            var changedLinkedOptions = questionsWithChangedOptionsSet
                .Select(x => new ChangedLinkedOptions(x.ChangedNode.Identity, x.ChangedNode.AsLinked.Options.ToArray()))
                .ToArray();

            if (changedLinkedOptions.Any())
            {
                this.ApplyEvent(new LinkedOptionsChanged(changedLinkedOptions));
            }
        }

        private void ApplyLinkedToListOptionsChangesEvents(InterviewTreeQuestionDiff[] questionsWithChangedOptionsSet)
        {
            var changedLinkedOptions = questionsWithChangedOptionsSet
                .Select(x => new ChangedLinkedToListOptions(x.ChangedNode.Identity, x.ChangedNode.AsLinkedToList.Options))
                .ToArray();

            if (changedLinkedOptions.Any())
            {
                this.ApplyEvent(new LinkedToListOptionsChanged(changedLinkedOptions));
            }
        }

        private void ApplyVariableEvents(InterviewTreeVariableDiff[] diffsByChangedVariables)
        {
            var changedVariables = diffsByChangedVariables.Where(x => x.ChangedNode != null && x.IsValueChanged).Select(ToChangedVariable).ToArray();

            if (changedVariables.Any())
                this.ApplyEvent(new VariablesChanged(changedVariables));
        }

        private void ApplyValidityEvents(IReadOnlyCollection<InterviewTreeNodeDiff> diff)
        {
            var allNotNullableNodes = diff.Where(x => x.ChangedNode != null).ToList();

            var allChangedQuestionDiffs = allNotNullableNodes.OfType<InterviewTreeQuestionDiff>().ToList();
            var allChangedStaticTextDiffs = allNotNullableNodes.OfType<InterviewTreeStaticTextDiff>().ToList();

            var validQuestionIdentities = allChangedQuestionDiffs.Where(x => x.ChangedNodeBecameValid).Select(x => x.ChangedNode.Identity).ToArray();
            var invalidQuestionIdentities = allChangedQuestionDiffs.Where(x => x.ChangedNodeBecameInvalid || x.IsFailedValidationIndexChanged).Select(x => x.ChangedNode)
                .ToDictionary(x => x.Identity, x => x.FailedValidations);

            var validStaticTextIdentities = allChangedStaticTextDiffs.Where(x => x.ChangedNodeBecameValid).Select(x => x.ChangedNode.Identity).ToArray();
            var invalidStaticTextIdentities = allChangedStaticTextDiffs.Where(x => x.ChangedNodeBecameInvalid || x.IsFailedValidationIndexChanged).Select(x => x.ChangedNode)
                .Select(x => new KeyValuePair<Identity, IReadOnlyList<FailedValidationCondition>>(x.Identity, x.FailedValidations))
                .ToList();

            if (validQuestionIdentities.Any()) this.ApplyEvent(new AnswersDeclaredValid(validQuestionIdentities));
            if (invalidQuestionIdentities.Any()) this.ApplyEvent(new AnswersDeclaredInvalid(invalidQuestionIdentities));

            if (validStaticTextIdentities.Any()) this.ApplyEvent(new StaticTextsDeclaredValid(validStaticTextIdentities));
            if (invalidStaticTextIdentities.Any()) this.ApplyEvent(new StaticTextsDeclaredInvalid(invalidStaticTextIdentities));
        }

        private void ApplyEnablementEvents(IReadOnlyCollection<InterviewTreeNodeDiff> diff)
        {
            var allNotNullableNodes = diff.Where(x => x.ChangedNode != null).ToList();

            var diffByGroups = allNotNullableNodes.OfType<InterviewTreeGroupDiff>().ToList();
            var diffByQuestions = allNotNullableNodes.OfType<InterviewTreeQuestionDiff>().ToList();
            var diffByStaticTexts = allNotNullableNodes.OfType<InterviewTreeStaticTextDiff>().ToList();
            var diffByVariables = allNotNullableNodes.OfType<InterviewTreeVariableDiff>().ToList();

            var disabledGroups = diffByGroups.Where(x => x.IsNodeDisabled).Select(x => x.ChangedNode.Identity).ToArray();
            var enabledGroups = diff.OfType<InterviewTreeGroupDiff>().Where(x => x.IsNodeEnabled).Select(x => (x.ChangedNode ?? x.SourceNode).Identity).ToArray();

            var disabledQuestions = diffByQuestions.Where(x => x.IsNodeDisabled).Select(x => x.ChangedNode.Identity).ToArray();
            var enabledQuestions = diff.OfType<InterviewTreeQuestionDiff>().Where(x => x.IsNodeEnabled).Select(x => (x.ChangedNode ?? x.SourceNode).Identity).ToArray();

            var disabledStaticTexts = diffByStaticTexts.Where(x => x.IsNodeDisabled).Select(x => x.ChangedNode.Identity).ToArray();
            var enabledStaticTexts = diffByStaticTexts.Where(x => x.IsNodeEnabled).Select(x => x.ChangedNode.Identity).ToArray();

            var disabledVariables = diffByVariables.Where(x => x.IsNodeDisabled).Select(x => x.ChangedNode.Identity).ToArray();
            var enabledVariables = diffByVariables.Where(x => x.IsNodeEnabled).Select(x => x.ChangedNode.Identity).ToArray();

            if (disabledGroups.Any()) this.ApplyEvent(new GroupsDisabled(disabledGroups));
            if (enabledGroups.Any()) this.ApplyEvent(new GroupsEnabled(enabledGroups));
            if (disabledQuestions.Any()) this.ApplyEvent(new QuestionsDisabled(disabledQuestions));
            if (enabledQuestions.Any()) this.ApplyEvent(new QuestionsEnabled(enabledQuestions));
            if (disabledStaticTexts.Any()) this.ApplyEvent(new StaticTextsDisabled(disabledStaticTexts));
            if (enabledStaticTexts.Any()) this.ApplyEvent(new StaticTextsEnabled(enabledStaticTexts));
            if (disabledVariables.Any()) this.ApplyEvent(new VariablesDisabled(disabledVariables));
            if (enabledVariables.Any()) this.ApplyEvent(new VariablesEnabled(enabledVariables));
        }

        private void ApplyUpdateAnswerEvents(InterviewTreeQuestionDiff[] diffByQuestions, Guid responsibleId)
        {
            foreach (var diffByQuestion in diffByQuestions)
            {
                var changedQuestion = diffByQuestion.ChangedNode;

                if (changedQuestion == null) continue;

                if (!changedQuestion.IsAnswered()) continue;

                if (changedQuestion.IsText)
                {
                    this.ApplyEvent(new TextQuestionAnswered(responsibleId, changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, DateTime.UtcNow, changedQuestion.AsText.GetAnswer().Value));
                }

                if (changedQuestion.IsTextList)
                {
                    this.ApplyEvent(new TextListQuestionAnswered(responsibleId, changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, DateTime.UtcNow, changedQuestion.AsTextList.GetAnswer().ToTupleArray()));
                }

                if (changedQuestion.IsDouble)
                {
                    this.ApplyEvent(new NumericRealQuestionAnswered(responsibleId, changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, DateTime.UtcNow, (decimal)changedQuestion.AsDouble.GetAnswer().Value));
                }

                if (changedQuestion.IsInteger)
                {
                    this.ApplyEvent(new NumericIntegerQuestionAnswered(responsibleId, changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, DateTime.UtcNow, changedQuestion.AsInteger.GetAnswer().Value));
                }

                if (changedQuestion.IsDateTime)
                {
                    this.ApplyEvent(new DateTimeQuestionAnswered(responsibleId, changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, DateTime.UtcNow, changedQuestion.AsDateTime.GetAnswer().Value));
                }

                if (changedQuestion.IsGps)
                {
                    var gpsAnswer = changedQuestion.AsGps.GetAnswer().Value;
                    this.ApplyEvent(new GeoLocationQuestionAnswered(responsibleId, changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, DateTime.UtcNow, gpsAnswer.Latitude, gpsAnswer.Longitude,
                        gpsAnswer.Accuracy, gpsAnswer.Altitude, gpsAnswer.Timestamp));
                }

                if (changedQuestion.IsQRBarcode)
                {
                    this.ApplyEvent(new QRBarcodeQuestionAnswered(responsibleId, changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, DateTime.UtcNow, changedQuestion.AsQRBarcode.GetAnswer().DecodedText));
                }

                if (changedQuestion.IsMultimedia)
                {
                    this.ApplyEvent(new PictureQuestionAnswered(responsibleId, changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, DateTime.UtcNow, changedQuestion.AsMultimedia.GetAnswer().FileName));
                }

                if (changedQuestion.IsYesNo)
                {
                    this.ApplyEvent(new YesNoQuestionAnswered(responsibleId, changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, DateTime.UtcNow, changedQuestion.AsYesNo.GetAnswer().ToAnsweredYesNoOptions().ToArray()));
                }

                if (changedQuestion.IsSingleFixedOption)
                {
                    this.ApplyEvent(new SingleOptionQuestionAnswered(responsibleId, changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, DateTime.UtcNow, changedQuestion.AsSingleFixedOption.GetAnswer().SelectedValue));
                }

                if (changedQuestion.IsMultiFixedOption)
                {
                    this.ApplyEvent(new MultipleOptionsQuestionAnswered(responsibleId, changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, DateTime.UtcNow, changedQuestion.AsMultiFixedOption.GetAnswer().ToDecimals().ToArray()));
                }

                if (changedQuestion.IsSingleLinkedOption)
                {
                    this.ApplyEvent(new SingleOptionLinkedQuestionAnswered(responsibleId, changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, DateTime.UtcNow, changedQuestion.AsSingleLinkedOption.GetAnswer().SelectedValue));
                }

                if (changedQuestion.IsMultiLinkedOption)
                {
                    this.ApplyEvent(new MultipleOptionsLinkedQuestionAnswered(responsibleId, changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, DateTime.UtcNow,
                        changedQuestion.AsMultiLinkedOption.GetAnswer().ToRosterVectorArray().Select(x => x.Select(Convert.ToDecimal).ToArray()).ToArray()));
                }

                if (changedQuestion.IsSingleLinkedToList)
                {
                    this.ApplyEvent(new SingleOptionQuestionAnswered(responsibleId, changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, DateTime.UtcNow, changedQuestion.AsSingleLinkedToList.GetAnswer().SelectedValue));
                }

                if (changedQuestion.IsMultiLinkedToList)
                {
                    this.ApplyEvent(new MultipleOptionsQuestionAnswered(responsibleId, changedQuestion.Identity.Id,
                        changedQuestion.Identity.RosterVector, DateTime.UtcNow, changedQuestion.AsMultiLinkedToList.GetAnswer().ToDecimals().ToArray()));
                }
            }
        }

        private void ApplyRemoveAnswerEvents(InterviewTreeQuestionDiff[] diffByQuestions)
        {
            var questionIdentittiesWithRemovedAnswer = diffByQuestions.Select(x => x.SourceNode.Identity).ToArray();

            if (questionIdentittiesWithRemovedAnswer.Any())
                this.ApplyEvent(new AnswersRemoved(questionIdentittiesWithRemovedAnswer));
        }

        private void ApplyRosterEvents(InterviewTreeRosterDiff[] diff)
        {
            var removedRosters = diff.Where(x => x.IsNodeRemoved).Select(x => x.SourceNode).ToArray();
            var addedRosters = diff.Where(x => x.IsNodeAdded).Select(x => x.ChangedNode).ToArray();
            var changedRosterTitles = diff.Where(x => x.IsRosterTitleChanged).Select(x => x.ChangedNode).ToArray();

            if (removedRosters.Any())
                this.ApplyEvent(new RosterInstancesRemoved(removedRosters.Select(ToRosterInstance).ToArray()));

            if (addedRosters.Any())
                this.ApplyEvent(new RosterInstancesAdded(addedRosters.OrderBy(x => x.RosterVector.Length).Select(ToAddedRosterInstance).ToArray()));

            if (changedRosterTitles.Any())
                this.ApplyEvent(new RosterInstancesTitleChanged(changedRosterTitles.Select(ToChangedRosterInstanceTitleDto).ToArray()));
        }

        private static ChangedRosterInstanceTitleDto ToChangedRosterInstanceTitleDto(InterviewTreeRoster roster)
            => new ChangedRosterInstanceTitleDto(ToRosterInstance(roster), roster.RosterTitle);

        private static AddedRosterInstance ToAddedRosterInstance(IInterviewTreeNode rosterNode)
            => AddedRosterInstance.CreateFromIdentityAndSortIndex(rosterNode.Identity, (rosterNode as InterviewTreeRoster)?.SortIndex);

        private static RosterInstance ToRosterInstance(IInterviewTreeNode rosterNode)
            => RosterInstance.CreateFromIdentity(rosterNode.Identity);

        private static ChangedVariable ToChangedVariable(InterviewTreeVariableDiff variable)
            => new ChangedVariable(variable.ChangedNode.Identity, variable.ChangedNode.Value);

        #endregion

        private void ValidatePreloadValues(InterviewTree tree, IQuestionnaire questionnaire, Dictionary<Guid, AbstractAnswer> answersToFeaturedQuestions, RosterVector rosterVector = null)
        {
            var currentRosterVector = rosterVector ?? (decimal[])RosterVector.Empty;
            foreach (KeyValuePair<Guid, AbstractAnswer> answerToFeaturedQuestion in answersToFeaturedQuestions)
            {
                Guid questionId = answerToFeaturedQuestion.Key;
                AbstractAnswer answer = answerToFeaturedQuestion.Value;

                var questionIdentity = new Identity(questionId, currentRosterVector);

                QuestionType questionType = questionnaire.GetQuestionType(questionId);

                var questionInvariants = new InterviewQuestionInvariants(questionIdentity, questionnaire, tree);

                switch (questionType)
                {
                    case QuestionType.Text:
                        questionInvariants.RequireTextPreloadValueAllowed();
                        break;

                    case QuestionType.Numeric:
                        if (questionnaire.IsQuestionInteger(questionId))
                            questionInvariants.RequireNumericIntegerPreloadValueAllowed(((NumericIntegerAnswer)answer).Value);
                        else
                            questionInvariants.RequireNumericRealPreloadValueAllowed();
                        break;

                    case QuestionType.DateTime:
                        questionInvariants.RequireDateTimePreloadValueAllowed();
                        break;

                    case QuestionType.SingleOption:
                        questionInvariants.RequireFixedSingleOptionPreloadValueAllowed(((CategoricalFixedSingleOptionAnswer)answer).SelectedValue);
                        break;

                    case QuestionType.MultyOption:
                        if (questionnaire.IsQuestionYesNo(questionId))
                            questionInvariants.RequireYesNoPreloadValueAllowed((YesNoAnswer)answer);
                        else
                            questionInvariants.RequireFixedMultipleOptionsPreloadValueAllowed(((CategoricalFixedMultiOptionAnswer)answer).CheckedValues);
                        break;

                    case QuestionType.QRBarcode:
                        questionInvariants.RequireQRBarcodePreloadValueAllowed();
                        break;

                    case QuestionType.GpsCoordinates:
                        questionInvariants.RequireGpsCoordinatesPreloadValueAllowed();
                        break;

                    case QuestionType.TextList:
                        questionInvariants.RequireTextListPreloadValueAllowed(((TextListAnswer)answer).ToTupleArray());
                        break;

                    default:
                        throw new InterviewException(
                            $"Question {questionId} has type {questionType} which is not supported as initial pre-filled question. InterviewId: {this.EventSourceId}");
                }
            }
        }

        private void ValidatePrefilledAnswers(InterviewTree tree, IQuestionnaire questionnaire, Dictionary<Guid, AbstractAnswer> answersToFeaturedQuestions, RosterVector rosterVector = null)
        {
            var currentRosterVector = rosterVector ?? (decimal[])RosterVector.Empty;
            foreach (KeyValuePair<Guid, AbstractAnswer> answerToFeaturedQuestion in answersToFeaturedQuestions)
            {
                Guid questionId = answerToFeaturedQuestion.Key;
                AbstractAnswer answer = answerToFeaturedQuestion.Value;

                var questionIdentity = new Identity(questionId, currentRosterVector);

                QuestionType questionType = questionnaire.GetQuestionType(questionId);

                var questionInvariants = new InterviewQuestionInvariants(questionIdentity, questionnaire, tree);

                switch (questionType)
                {
                    case QuestionType.Text:
                        questionInvariants.RequireTextAnswerAllowed();
                        break;

                    case QuestionType.Numeric:
                        if (questionnaire.IsQuestionInteger(questionId))
                            questionInvariants.RequireNumericIntegerAnswerAllowed(((NumericIntegerAnswer)answer).Value);
                        else
                            questionInvariants.RequireNumericRealAnswerAllowed(((NumericRealAnswer)answer).Value);
                        break;

                    case QuestionType.DateTime:
                        questionInvariants.RequireDateTimeAnswerAllowed();
                        break;

                    case QuestionType.SingleOption:
                        questionInvariants.RequireFixedSingleOptionAnswerAllowed(((CategoricalFixedSingleOptionAnswer)answer).SelectedValue, this.QuestionnaireIdentity);
                        break;

                    case QuestionType.MultyOption:
                        if (questionnaire.IsQuestionYesNo(questionId))
                            questionInvariants.RequireYesNoAnswerAllowed((YesNoAnswer)answer);
                        else
                            questionInvariants.RequireFixedMultipleOptionsAnswerAllowed(((CategoricalFixedMultiOptionAnswer)answer).CheckedValues);
                        break;

                    case QuestionType.QRBarcode:
                        questionInvariants.RequireQRBarcodeAnswerAllowed();
                        break;

                    case QuestionType.GpsCoordinates:
                        questionInvariants.RequireGpsCoordinatesAnswerAllowed();
                        break;

                    case QuestionType.TextList:
                        questionInvariants.RequireTextListAnswerAllowed(((TextListAnswer)answer).ToTupleArray());
                        break;

                    default:
                        throw new InterviewException(
                            $"Question {questionId} has type {questionType} which is not supported as initial pre-filled question. InterviewId: {this.EventSourceId}");
                }
            }
        }

        private static AnswerComment ToAnswerComment(CommentSynchronizationDto answerComment,
            AnsweredQuestionSynchronizationDto answerDto)
            => new AnswerComment(answerComment.UserId, answerComment.UserRole, answerComment.Date, answerComment.Text,
                Identity.Create(answerDto.Id, answerDto.QuestionRosterVector));

        protected decimal[] CalculateStartRosterVectorForAnswersOfLinkedToQuestion(
            Guid linkedToEntityId, Identity linkedQuestion, IQuestionnaire questionnaire)
        {
            Guid[] linkSourceRosterVector = questionnaire.GetRosterSizeSourcesForEntity(linkedToEntityId);
            Guid[] linkedQuestionRosterSources = questionnaire.GetRosterSizeSourcesForEntity(linkedQuestion.Id);

            int commonRosterSourcesPartLength = Enumerable
                .Zip(linkSourceRosterVector, linkedQuestionRosterSources, (a, b) => a == b)
                .TakeWhile(areEqual => areEqual)
                .Count();

            int linkedQuestionRosterLevel = linkedQuestion.RosterVector.Length;

            int targetRosterLevel = Math.Min(commonRosterSourcesPartLength, Math.Min(linkSourceRosterVector.Length - 1, linkedQuestionRosterLevel));

            return linkedQuestion.RosterVector.Shrink(targetRosterLevel);
        }

        public InterviewTreeMultiLinkedToRosterQuestion GetLinkedMultiOptionQuestion(Identity identity) => this.Tree.GetQuestion(identity).AsMultiLinkedOption;
        public InterviewTreeSingleLinkedToRosterQuestion GetLinkedSingleOptionQuestion(Identity identity) => this.Tree.GetQuestion(identity).AsSingleLinkedOption;

        public string GetLinkedOptionTitle(Identity linkedQuestionIdentity, RosterVector option)
        {
            InterviewTreeQuestion linkedQuestion = this.Tree.GetQuestion(linkedQuestionIdentity);
            if (!linkedQuestion.IsLinked) return string.Empty;

            Identity sourceIdentity = Identity.Create(linkedQuestion.AsLinked.LinkedSourceId, option);

            IInterviewTreeNode sourceNode = this.Tree.GetNodeByIdentity(sourceIdentity);

            string optionTitle = string.Empty;
            int skipBreadcrumsThreshold = 1;
            if (sourceNode is InterviewTreeRoster)
            {
                InterviewTreeRoster sourceRoster = sourceNode as InterviewTreeRoster;
                optionTitle = sourceRoster.RosterTitle;
                skipBreadcrumsThreshold = 0;
            }
            if (sourceNode is InterviewTreeQuestion)
            {
                InterviewTreeQuestion sourceQuestion = sourceNode as InterviewTreeQuestion;
                optionTitle = sourceQuestion.GetAnswerAsString();
            }

            InterviewTreeRoster[] sourceBreadcrumbsOfRosterTitles = sourceNode.Parents.OfType<InterviewTreeRoster>().ToArray();
            InterviewTreeRoster[] linkedBreadcrumbsOfRosterTitles = linkedQuestion.Parents.OfType<InterviewTreeRoster>().ToArray();

            int common = sourceBreadcrumbsOfRosterTitles.Zip(linkedBreadcrumbsOfRosterTitles, (x, y) => x.RosterSizeId.Equals(y.RosterSizeId) ? x : null).TakeWhile(x => x != null).Count();

            string[] breadcrumbsOfRosterTitles = sourceBreadcrumbsOfRosterTitles.Skip(common).Select(x => x.RosterTitle).ToArray();

            string breadcrumbs = string.Join(": ", breadcrumbsOfRosterTitles);

            return breadcrumbsOfRosterTitles.Length > skipBreadcrumsThreshold
                ? $"{breadcrumbs}: {optionTitle}"
                : optionTitle;
        }

        public IEnumerable<Identity> GetUnderlyingInterviewerEntities(Identity sectionId)
        {
            InterviewTreeGroup interviewTreeGroup = this.Tree.GetGroup(sectionId);
            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();
            IEnumerable<IInterviewTreeNode> result = interviewTreeGroup.Children
                     .Except(x => questionnaire.IsQuestion(x.Identity.Id) && !questionnaire.IsInterviewierQuestion(x.Identity.Id));

            return result.Select(x => x.Identity);
        }

        protected bool HasInvalidAnswers()
            => this.Tree.FindQuestions().Any(question => !question.IsValid && !question.IsDisabled());

        protected bool HasInvalidStaticTexts
            => this.Tree.FindStaticTexts().Any(staticText => !staticText.IsValid && !staticText.IsDisabled());

        protected static IReadOnlyCollection<InterviewTreeNodeDiff> FindDifferenceBetweenTrees(InterviewTree sourceInterview, InterviewTree changedInterview)
            => sourceInterview.Clone().Compare(changedInterview);

        protected void UpdateTreeWithDependentChanges(InterviewTree changedInterviewTree, IEnumerable<Identity> changedQuestions, IQuestionnaire questionnaire)
        {
            ILatestInterviewExpressionState expressionProcessorState = this.GetClonedExpressionState();

            this.UpdateExpressionState(changedInterviewTree, expressionProcessorState);

            EnablementChanges enablementChanges = expressionProcessorState.ProcessEnablementConditions();

            this.UpdateTreeWithEnablementChanges(changedInterviewTree, enablementChanges);

            StructuralChanges structuralChanges = expressionProcessorState.GetStructuralChanges();
            this.UpdateTreeWithStructuralChanges(changedInterviewTree, structuralChanges);

            this.UpdateRosterTitles(changedInterviewTree, questionnaire);

            this.UpdateLinkedQuestions(changedInterviewTree, expressionProcessorState);

            VariableValueChanges variableValueChanges = expressionProcessorState.ProcessVariables();
            this.UpdateTreeWithVariableChanges(changedInterviewTree, variableValueChanges);

            ValidityChanges validationChanges = expressionProcessorState.ProcessValidationExpressions();
            this.UpdateTreeWithValidationChanges(changedInterviewTree, validationChanges);

            changedInterviewTree.ReplaceSubstitutions();
        }

        private void UpdateTreeWithVariableChanges(InterviewTree tree, VariableValueChanges variableValueChanges)
            => variableValueChanges?.ChangedVariableValues.ForEach(x => tree.GetVariable(x.Key).SetValue(x.Value));

        private void UpdateTreeWithValidationChanges(InterviewTree tree, ValidityChanges validationChanges)
        {
            if (validationChanges == null) return; // can be in tests only.

            validationChanges.AnswersDeclaredValid.ForEach(x => tree.GetQuestion(x).MarkValid());
            validationChanges.AnswersDeclaredInvalid.ForEach(x => tree.GetQuestion(x).MarkInvalid(new FailedValidationCondition(0).ToEnumerable()));
            validationChanges.FailedValidationConditionsForQuestions.ForEach(x => tree.GetQuestion(x.Key).MarkInvalid(x.Value));

            validationChanges.StaticTextsDeclaredValid.ForEach(x => tree.GetStaticText(x).MarkValid());
            validationChanges.FailedValidationConditionsForStaticTexts.ForEach(x => tree.GetStaticText(x.Key).MarkInvalid(x.Value));
        }

        private void UpdateTreeWithEnablementChanges(InterviewTree tree, EnablementChanges enablementChanges)
        {
            if (enablementChanges == null) return; // can be in tests only.

            enablementChanges.QuestionsToBeDisabled.ForEach(x => tree.GetQuestion(x).Disable());
            enablementChanges.QuestionsToBeEnabled.ForEach(x => tree.GetQuestion(x).Enable());

            enablementChanges.GroupsToBeDisabled.ForEach(x => tree.GetGroup(x).Disable());
            enablementChanges.GroupsToBeEnabled.ForEach(x => tree.GetGroup(x).Enable());

            enablementChanges.StaticTextsToBeDisabled.ForEach(x => tree.GetStaticText(x).Disable());
            enablementChanges.StaticTextsToBeEnabled.ForEach(x => tree.GetStaticText(x).Enable());

            enablementChanges.VariablesToBeDisabled.ForEach(x => tree.GetVariable(x).Disable());
            enablementChanges.VariablesToBeEnabled.ForEach(x => tree.GetVariable(x).Enable());
        }

        protected void UpdateRosterTitles(InterviewTree tree, IQuestionnaire questionnaire)
        {
            foreach (InterviewTreeRoster roster in tree.FindRosters())
            {
                if (roster.IsFixed)
                {
                    string changedRosterTitle = questionnaire.GetFixedRosterTitle(roster.Identity.Id,
                        roster.Identity.RosterVector.Coordinates.Last());

                    roster.SetRosterTitle(changedRosterTitle);
                }
                else
                {
                    roster.UpdateRosterTitle((questionId, answerOptionValue) =>
                        questionnaire.GetOptionForQuestionByOptionValue(questionId, answerOptionValue).Title);
                }
            }
        }

        protected void UpdateLinkedQuestions(InterviewTree interviewTree, ILatestInterviewExpressionState interviewExpressionState, bool removeAnswersIfOptionsSetChanged = true)
        {
            bool expressionStateSupportLinkedOptionsCalculation = interviewExpressionState.AreLinkedQuestionsSupported();
            if (expressionStateSupportLinkedOptionsCalculation)
            {
                LinkedQuestionOptionsChanges processLinkedQuestionFilters = interviewExpressionState.ProcessLinkedQuestionFilters();

                foreach (KeyValuePair<Identity, RosterVector[]> linkedQuestionWithOptions in processLinkedQuestionFilters.LinkedQuestionOptionsSet)
                {
                    InterviewTreeQuestion linkedQuestion = interviewTree.GetQuestion(linkedQuestionWithOptions.Key);
                    linkedQuestion.UpdateLinkedOptionsAndResetAnswerIfNeeded(linkedQuestionWithOptions.Value, removeAnswersIfOptionsSetChanged);
                }

                // backward compatibility with old assemblies
                UpdateLinkedQuestionsCalculatedByObsoleteAlgorythm(interviewTree, processLinkedQuestionFilters);
            }
            else
            {
                // backward compatibility if assembly cannot process linked questions
                CalculateLinkedOptionsOnTree(interviewTree);
            }
            
            CalculateLinkedToListOptionsOnTree(interviewTree);
        }

        [Obsolete("v 5.10, release 01 jul 16")]
        private static void UpdateLinkedQuestionsCalculatedByObsoleteAlgorythm(InterviewTree tree, LinkedQuestionOptionsChanges processLinkedQuestionFilters)
        {
            foreach (KeyValuePair<Guid, RosterVector[]> linkedQuestionWithOptions in processLinkedQuestionFilters.LinkedQuestionOptions)
            {
                tree.FindQuestions(linkedQuestionWithOptions.Key)
                    .ForEach(x => x.AsLinked.SetOptions(linkedQuestionWithOptions.Value));
            }
        }

        private static void CalculateLinkedOptionsOnTree(InterviewTree tree)
        {
            IEnumerable<InterviewTreeQuestion> linkedQuestions = tree.FindQuestions().Where(x => x.IsLinked);
            foreach (InterviewTreeQuestion linkedQuestion in linkedQuestions)
            {
                linkedQuestion.CalculateLinkedOptions();
            }
        }

        protected static void CalculateLinkedToListOptionsOnTree(InterviewTree tree, bool resetAnswerOnOptionChange = true)
        {
            IEnumerable<InterviewTreeQuestion> linkedToListQuestions = tree.FindQuestions().Where(x => x.IsLinkedToListQuestion);
            foreach (InterviewTreeQuestion linkedQuestion in linkedToListQuestions)
            {
                linkedQuestion.CalculateLinkedToListOptions(resetAnswerOnOptionChange);
            }
        }

        private void UpdateTreeWithStructuralChanges(InterviewTree tree, StructuralChanges structuralChanges)
        {
            foreach (KeyValuePair<Identity, int[]> changedMultiQuestion in structuralChanges.ChangedMultiQuestions)
            {
                tree.GetQuestion(changedMultiQuestion.Key).AsMultiFixedOption?.SetAnswer(CategoricalFixedMultiOptionAnswer.FromInts(changedMultiQuestion.Value));
                tree.GetQuestion(changedMultiQuestion.Key).AsMultiLinkedToList?.SetAnswer(CategoricalFixedMultiOptionAnswer.FromInts(changedMultiQuestion.Value));
            }

            foreach (KeyValuePair<Identity, int?> changedSingleQuestion in structuralChanges.ChangedSingleQuestions)
            {
                InterviewTreeQuestion interviewTreeQuestion = tree.GetQuestion(changedSingleQuestion.Key);

                if (interviewTreeQuestion.IsSingleFixedOption)
                {
                    InterviewTreeSingleOptionQuestion question = interviewTreeQuestion.AsSingleFixedOption;
                    if (changedSingleQuestion.Value.HasValue)
                        question.SetAnswer(CategoricalFixedSingleOptionAnswer.FromInt(changedSingleQuestion.Value.Value));
                    else
                        question.RemoveAnswer();
                }

                if (interviewTreeQuestion.IsSingleLinkedToList)
                {
                    InterviewTreeSingleOptionLinkedToListQuestion question = interviewTreeQuestion.AsSingleLinkedToList;
                    if (changedSingleQuestion.Value.HasValue)
                        question.SetAnswer(CategoricalFixedSingleOptionAnswer.FromInt(changedSingleQuestion.Value.Value));
                    else
                        question.RemoveAnswer();
                }
            }

            foreach (KeyValuePair<Identity, YesNoAnswersOnly> changedYesNoQuestion in structuralChanges.ChangedYesNoQuestions)
            {
                tree.GetQuestion(changedYesNoQuestion.Key).AsYesNo.SetAnswer(YesNoAnswer.FromYesNoAnswersOnly(changedYesNoQuestion.Value));
            }

            foreach (Identity removedRosterIdentity in structuralChanges.RemovedRosters)
            {
                tree.RemoveNode(removedRosterIdentity);
            }
        }

        private void UpdateTitlesAndTexts(IQuestionnaire questionnaire)
        {
            foreach (IInterviewTreeNode node in this.Tree.AllNodes)
            {
                InterviewTreeQuestion question = node as InterviewTreeQuestion;
                if (question != null)
                {
                    SubstitionText title = this.substitionTextFactory.CreateText(question.Identity,
                        questionnaire.GetQuestionTitle(question.Identity.Id), questionnaire);

                    SubstitionText[] validationMessages = questionnaire.GetValidationMessages(question.Identity.Id)
                        .Select(x => this.substitionTextFactory.CreateText(question.Identity, x, questionnaire))
                        .ToArray();

                    question.SetTitle(title);
                    question.SetValidationMessages(validationMessages);
                    question.ReplaceSubstitutions();
                }

                InterviewTreeGroup groupOrRoster = node as InterviewTreeGroup;
                if (groupOrRoster != null)
                {
                    SubstitionText title = this.substitionTextFactory.CreateText(groupOrRoster.Identity,
                        questionnaire.GetGroupTitle(groupOrRoster.Identity.Id), questionnaire);

                    groupOrRoster.SetTitle(title);
                    groupOrRoster.ReplaceSubstitutions();
                }

                InterviewTreeStaticText staticText = node as InterviewTreeStaticText;
                if (staticText != null)
                {
                    SubstitionText title = this.substitionTextFactory.CreateText(staticText.Identity,
                       questionnaire.GetStaticText(staticText.Identity.Id), questionnaire);

                    SubstitionText[] validationMessages = questionnaire.GetValidationMessages(staticText.Identity.Id)
                        .Select(x => this.substitionTextFactory.CreateText(staticText.Identity, x, questionnaire))
                        .ToArray();

                    staticText.SetTitle(title);
                    staticText.SetValidationMessages(validationMessages);
                    staticText.ReplaceSubstitutions();
                }
            }
        }

        protected void ActualizeRostersIfQuestionIsRosterSize(Guid questionId)
        {
            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();

            foreach (Guid rosterId in questionnaire.GetRosterGroupsByRosterSizeQuestion(questionId))
            {
                Guid? parentOfRoster = questionnaire.GetParentGroup(rosterId);
                if (!parentOfRoster.HasValue) continue;

                List<InterviewTreeGroup> parentsOfRosters = this.Tree.FindEntity(parentOfRoster.Value).OfType<InterviewTreeGroup>().ToList();

                foreach (InterviewTreeGroup parentRoster in parentsOfRosters)
                    parentRoster.ActualizeChildren();
            }
        }

        private void SetStartDateOnFirstAnswerSet(Identity questionIdentity, DateTime answerDate)
        {
            if (this.properties.StartedDate.HasValue) return;

            InterviewTreeQuestion question = this.Tree.GetQuestion(questionIdentity);
            if (question == null) return;

            if (question.IsPrefilled) return;
            if (!question.IsInterviewer) return;

            this.properties.StartedDate = answerDate;
        }
    }
}