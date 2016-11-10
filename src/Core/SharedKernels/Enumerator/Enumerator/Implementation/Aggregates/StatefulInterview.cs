using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.Infrastructure.EventBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Invariants;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Events;

using Identity = WB.Core.SharedKernels.DataCollection.Identity;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Aggregates
{
    internal class StatefulInterview : Interview, IStatefulInterview
    {
        public StatefulInterview(IQuestionnaireStorage questionnaireRepository,
                                 IInterviewExpressionStatePrototypeProvider expressionProcessorStatePrototypeProvider,
                                 ISubstitionTextFactory substitionTextFactory)
            : base(questionnaireRepository, expressionProcessorStatePrototypeProvider, substitionTextFactory)
        {
        }

        #region Apply

        protected void Apply(InterviewOnClientCreated @event)
        {
            this.questionnaireId = @event.QuestionnaireId;
            this.questionnaireVersion = @event.QuestionnaireVersion;

            this.CreatedOnClient = true;
            this.InterviewerId = @event.UserId;
            
            this.delta = this.BuildInterviewTree(this.GetQuestionnaireOrThrow());
            this.interviewState = this.delta.Clone();
        }

        public void Apply(InterviewSynchronized @event)
        {
            this.questionnaireId = @event.InterviewData.QuestionnaireId;
            this.questionnaireVersion = @event.InterviewData.QuestionnaireVersion;
            this.properties.Status = @event.InterviewData.Status;
            this.properties.WasCompleted = @event.InterviewData.WasCompleted;

            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();

            var sourceInterviewTree = this.BuildInterviewTree(questionnaire);
            this.interviewState = sourceInterviewTree.Clone();
            
            var orderedRosters = @event.InterviewData.RosterGroupInstances
                .SelectMany(x => x.Value)
                .OrderBy(x => x.OuterScopeRosterVector.Length)
                .ToList();

            foreach (var rosterDto in orderedRosters)
            {
                Guid? parentGroupId = questionnaire.GetParentGroup(rosterDto.RosterId);
                if (!parentGroupId.HasValue) continue;

                Identity parentGroupIdentity = Identity.Create(parentGroupId.Value, rosterDto.OuterScopeRosterVector);
                RosterIdentity rosterIdentity = new RosterIdentity(rosterDto.RosterId, rosterDto.OuterScopeRosterVector,
                    rosterDto.RosterInstanceId, rosterDto.SortIndex);

                InterviewTreeRoster roster = this.interviewState.GetRosterManager(rosterIdentity.GroupId)
                    .CreateRoster(parentGroupIdentity, rosterIdentity.ToIdentity(), rosterIdentity.SortIndex ?? 0);

                roster.SetRosterTitle(rosterDto.RosterTitle);
            }

            foreach (var question in @event.InterviewData.Answers)
                this.interviewState.GetQuestion(Identity.Create(question.Id, question.QuestionRosterVector)).SetObjectAnswer(question.Answer);

            foreach (var disabledGroup in @event.InterviewData.DisabledGroups)
                this.interviewState.GetGroup(Identity.Create(disabledGroup.Id, disabledGroup.InterviewItemRosterVector)).Disable();

            foreach (var disabledQuestion in @event.InterviewData.DisabledQuestions)
                this.interviewState.GetQuestion(Identity.Create(disabledQuestion.Id, disabledQuestion.InterviewItemRosterVector)).Disable();

            foreach (var invalidQuestion in @event.InterviewData.FailedValidationConditions)
                this.interviewState.GetQuestion(invalidQuestion.Key).MarkAsInvalid(invalidQuestion.Value);

            foreach (var disabledStaticText in @event.InterviewData.DisabledStaticTexts)
                this.interviewState.GetStaticText(disabledStaticText).Disable();

            foreach (var invalidStaticText in @event.InterviewData.InvalidStaticTexts)
                this.interviewState.GetStaticText(invalidStaticText.Key).MarkAsInvalid(invalidStaticText.Value);

            foreach (var validStaticText in @event.InterviewData.ValidStaticTexts)
                this.interviewState.GetStaticText(validStaticText).MarkAsValid();

            foreach (var variable in @event.InterviewData.Variables)
                this.interviewState.GetVariable(Identity.Create(variable.Key.Id, variable.Key.InterviewItemRosterVector)).SetValue(variable.Value);

            foreach (var disabledVariable in @event.InterviewData.DisabledVariables)
                this.interviewState.GetVariable(Identity.Create(disabledVariable.Id, disabledVariable.InterviewItemRosterVector)).Disable();

            foreach (var linkedQuestion in @event.InterviewData.LinkedQuestionOptions)
                this.interviewState.GetQuestion(Identity.Create(linkedQuestion.Key.Id, linkedQuestion.Key.InterviewItemRosterVector)).AsLinked.SetOptions(linkedQuestion.Value);

            this.interviewState.AnswerComments = @event.InterviewData.Answers
                .SelectMany(answerDto => answerDto.AllComments.Select(commentDto => ToAnswerComment(answerDto, commentDto)))
                .ToList();

            base.UpdateRosterTitles(this.interviewState, questionnaire);
            base.UpdateExpressionState(sourceInterviewTree, this.interviewState, this.ExpressionProcessorStatePrototype);

            this.delta = this.interviewState.Clone();

            this.CreatedOnClient = @event.InterviewData.CreatedOnClient;
            this.SupervisorId = @event.InterviewData.SupervisorId;
            this.InterviewerId = @event.InterviewData.UserId;
            this.SupervisorRejectComment = @event.InterviewData.Comments;
        }

        public void Apply(InterviewAnswersFromSyncPackageRestored @event) { }

        public new void Apply(LinkedOptionsChanged @event)
        {
            base.Apply(@event);
            this.HasLinkedOptionsChangedEvents = true;
        }

        public new void Apply(SubstitutionTitlesChanged @event)
        {
            foreach (var @group in @event.Groups)
                this.interviewState.GetGroup(@group).ReplaceSubstitutions();

            foreach (var @question in @event.Questions)
                this.interviewState.GetQuestion(@question).ReplaceSubstitutions();

            foreach (var @staticText in @event.StaticTexts)
                this.interviewState.GetStaticText(@staticText).ReplaceSubstitutions();
        }

        public new void Apply(InterviewCompleted @event)
        {
            base.Apply(@event);

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
            this.IsCompleted = false;
        }

        public new void Apply(InterviewDeclaredValid @event)
        {
            base.Apply(@event);
            this.HasErrors = false;
        }

        public new void Apply(InterviewDeclaredInvalid @event)
        {
            base.Apply(@event);
            this.HasErrors = true;
        }

        #endregion

        public bool HasLinkedOptionsChangedEvents { get; private set; } = false;
        public Guid? SupervisorId { get; private set; }
        public Guid? InterviewerId { get; private set; }
        
        public InterviewStatus Status => this.properties.Status;
        public Guid Id => this.EventSourceId;
        public string InterviewerCompleteComment { get; private set; }
        public string SupervisorRejectComment { get; private set; }

        public string GetAnswerAsString(Identity questionIdentity)
            => this.interviewState.GetQuestion(questionIdentity).GetAnswerAsString();

        public bool HasErrors { get; private set; }

        public bool IsCompleted { get; private set; }

        public InterviewTreeRoster GetRoster(Identity identity) => this.interviewState.GetRoster(identity);
        public InterviewTreeGpsQuestion GetGpsQuestion(Identity identity) => this.interviewState.GetQuestion(identity).AsGps;
        public InterviewTreeDateTimeQuestion GetDateTimeQuestion(Identity identity) => this.interviewState.GetQuestion(identity).AsDateTime;
        public InterviewTreeMultimediaQuestion GetMultimediaQuestion(Identity identity) => this.interviewState.GetQuestion(identity).AsMultimedia;
        public InterviewTreeQRBarcodeQuestion GetQRBarcodeQuestion(Identity identity) => this.interviewState.GetQuestion(identity).AsQRBarcode;
        public InterviewTreeTextListQuestion GetTextListQuestion(Identity identity) => this.interviewState.GetQuestion(identity).AsTextList;
        public InterviewTreeSingleLinkedOptionQuestion GetLinkedSingleOptionQuestion(Identity identity) => this.interviewState.GetQuestion(identity).AsSingleLinkedOption;
        public InterviewTreeMultiOptionQuestion GetMultiOptionQuestion(Identity identity) => this.interviewState.GetQuestion(identity).AsMultiFixedOption;
        public InterviewTreeMultiLinkedOptionQuestion GetLinkedMultiOptionQuestion(Identity identity) => this.interviewState.GetQuestion(identity).AsMultiLinkedOption;
        public InterviewTreeIntegerQuestion GetIntegerQuestion(Identity identity) => this.interviewState.GetQuestion(identity).AsInteger;
        public InterviewTreeDoubleQuestion GetDoubleQuestion(Identity identity) => this.interviewState.GetQuestion(identity).AsDouble;
        public InterviewTreeTextQuestion GetTextQuestion(Identity identity) => this.interviewState.GetQuestion(identity).AsText;
        public InterviewTreeSingleOptionQuestion GetSingleOptionQuestion(Identity identity) => this.interviewState.GetQuestion(identity).AsSingleFixedOption;
        public InterviewTreeYesNoQuestion GetYesNoQuestion(Identity identity) => this.interviewState.GetQuestion(identity).AsYesNo;

        #region Command handlers

        public void Complete(Guid userId, string comment, DateTime completeTime)
        {
            var propertiesInvariants = new InterviewPropertiesInvariants(this.properties);

            propertiesInvariants.ThrowIfInterviewHardDeleted();
            propertiesInvariants.ThrowIfInterviewStatusIsNotOneOfExpected(
                InterviewStatus.InterviewerAssigned, InterviewStatus.Restarted, InterviewStatus.RejectedBySupervisor);

            this.ApplyEvents(this.delta, this.interviewState, userId);

            this.ApplyEvent(new InterviewCompleted(userId, completeTime, comment));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.Completed, comment));

            this.ApplyEvent(this.HasInvalidAnswers() || this.HasInvalidStaticTexts
                ? new InterviewDeclaredInvalid() as IEvent
                : new InterviewDeclaredValid());
        }

        public void RestoreInterviewStateFromSyncPackage(Guid userId, InterviewSynchronizationDto synchronizedInterview)
        {
            new InterviewPropertiesInvariants(this.properties).ThrowIfInterviewHardDeleted();
            
            this.ApplyEvent(new InterviewSynchronized(synchronizedInterview));
        }

        #endregion

        public bool HasGroup(Identity group) => this.interviewState.GetGroup(group) != null;

        public string GetRosterTitle(Identity rosterIdentity)
            => this.interviewState.GetRoster(rosterIdentity)?.RosterTitle;

        public object GetVariableValueByOrDeeperRosterLevel(Guid variableId, RosterVector variableRosterVector)
        {
            do
            {
                var variableIdentity = new Identity(variableId, variableRosterVector);

                var variable = this.interviewState.GetVariable(variableIdentity);
                if (variable != null) return variable.Value;

                if (variableRosterVector.Length == 0) break;

                variableRosterVector = variableRosterVector.Shrink(variableRosterVector.Length - 1);
            } while (variableRosterVector.Length >= 0);

            return null;
        }

        public IEnumerable<string> GetParentRosterTitlesWithoutLast(Identity questionIdentity)
            => this.interviewState.GetQuestion(questionIdentity).Parents
                .OfType<InterviewTreeRoster>()
                .Select(roster => roster.RosterTitle);

        public IEnumerable<string> GetParentRosterTitlesWithoutLastForRoster(Identity rosterIdentity)
            => this.interviewState.GetRoster(rosterIdentity).Parents
                .OfType<InterviewTreeRoster>()
                .Select(roster => roster.RosterTitle);

        public int GetGroupsInGroupCount(Identity group) => this.GetGroupsAndRostersInGroup(group).Count();

        public int CountAnsweredQuestionsInInterview()
            => this.interviewState.FindQuestions().Count(question => !question.IsDisabled() && question.IsAnswered());

        public int CountActiveQuestionsInInterview()
            => this.interviewState.FindQuestions().Count(question => !question.IsDisabled());

        public int CountInvalidEntitiesInInterview() => this.GetInvalidEntitiesInInterview().Count();

        public IEnumerable<Identity> GetInvalidEntitiesInInterview()
            => this.GetEnabledInvalidStaticTexts().Concat(this.GetEnabledInvalidQuestions());

        private IEnumerable<Identity> GetEnabledInvalidStaticTexts()
            => this.interviewState.FindStaticTexts()
                .Where(staticText => !staticText.IsDisabled() && !staticText.IsValid)
                .Select(staticText => staticText.Identity);

        private IEnumerable<Identity> GetEnabledInvalidQuestions()
            => this.interviewState.FindQuestions()
                .Where(question => !question.IsDisabled() && !question.IsValid)
                .Select(question => question.Identity);

        public int CountEnabledQuestions(Identity group)
            => this.interviewState.FindQuestions(group).Count(question => !question.IsDisabled());

        public int CountEnabledAnsweredQuestions(Identity group)
            => this.interviewState.FindQuestions(group).Count(question => !question.IsDisabled() && question.IsAnswered());

        public int CountEnabledInvalidQuestionsAndStaticTexts(Identity group)
            => this.interviewState.FindQuestions(group).Count(question => !question.IsDisabled() && !question.IsValid) +
               this.interviewState.FindStaticTexts(group).Count(staticText => !staticText.IsDisabled() && !staticText.IsValid);

        public bool HasEnabledInvalidQuestionsAndStaticTexts(Identity group)
            => this.CountEnabledInvalidQuestionsAndStaticTexts(group) > 0;

        public bool HasUnansweredQuestions(Identity group) 
            => this.interviewState.FindQuestions(group).Any(question => !question.IsDisabled() && !question.IsAnswered());

        public IEnumerable<Identity> GetCommentedBySupervisorQuestionsInInterview()
        {
            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();
            
            var commentedEnabledInterviewerQuestionIds = this.interviewState.AnswerComments
                  .Where(x => x.UserId != this.InterviewerId)
                  .Select(x => x.QuestionIdentity)
                  .Where(this.IsEnabled)
                  .Where(x => questionnaire.IsInterviewierQuestion(x.Id))
                  .Where(x => HasGroup(GetParentGroup(x)));

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

        public string GetLastSupervisorComment() => this.SupervisorRejectComment;

        private bool HasSupervisorCommentInterviewerReply(Identity questionId)
        {
            var interviewerAnswerComments = this.GetQuestionComments(questionId).ToList();
            var indexOfLastNotInterviewerComment = interviewerAnswerComments.FindLastIndex(0, x => x.UserId != this.InterviewerId);
            return interviewerAnswerComments.Skip(indexOfLastNotInterviewerComment + 1).Any();
        }

        public Identity GetParentGroup(Identity groupOrQuestion)
            => this.interviewState.GetNodeByIdentity(groupOrQuestion).Parent.Identity;

        public IEnumerable<Identity> GetChildQuestions(Identity groupIdentity)
            => this.interviewState.GetGroup(groupIdentity).Children
                .OfType<InterviewTreeQuestion>()
                .Select(question => question.Identity);

        public IReadOnlyList<Identity> GetRosterInstances(Identity parentIdentity, Guid rosterId)
            => this.interviewState.FindRosters()
                .Where(roster => roster.Identity.Id == rosterId && roster.Parent.Identity.Equals(parentIdentity))
                .Select(roster => roster.Identity)
                .ToList();

        public IEnumerable<Identity> GetEnabledSubgroups(Identity group)
            => this.GetGroupsAndRostersInGroup(group)
                .Where(groupOrRoster => !groupOrRoster.IsDisabled())
                .Select(groupOrRoster => groupOrRoster.Identity);

        private IEnumerable<InterviewTreeGroup> GetGroupsAndRostersInGroup(Identity group)
            => this.interviewState.GetGroup(group).Children.OfType<InterviewTreeGroup>();

        public bool IsValid(Identity identity)
            => (this.interviewState.GetQuestion(identity)?.IsValid ?? false) ||
               (this.interviewState.GetStaticText(identity)?.IsValid ?? false);

        public IReadOnlyList<FailedValidationCondition> GetFailedValidationConditions(Identity questionId)
            => this.interviewState.GetQuestion(questionId)?.FailedValidations ?? 
               this.interviewState.GetStaticText(questionId)?.FailedValidations;

        public bool IsEnabled(Identity entityIdentity) => !this.interviewState.GetNodeByIdentity(entityIdentity).IsDisabled();

        public bool CreatedOnClient { get; private set; } = false;

        public bool WasAnswered(Identity entityIdentity)
        {
            var question = this.interviewState.GetQuestion(entityIdentity);

            return !question.IsDisabled() && question.IsAnswered();
        }

        public IEnumerable<AnswerComment> GetQuestionComments(Identity entityIdentity)
            => this.interviewState.AnswerComments.Where(comment => comment.QuestionIdentity.Equals(entityIdentity));

        List<CategoricalOption> IStatefulInterview.GetTopFilteredOptionsForQuestion(Identity question, int? parentQuestionValue, string filter, int sliceSize)
            => this.GetFirstTopFilteredOptionsForQuestion(question, parentQuestionValue, filter, sliceSize);

        CategoricalOption IStatefulInterview.GetOptionForQuestionWithoutFilter(Identity question, int value,
            int? parentQuestionValue) => this.GetOptionForQuestionWithoutFilter(question, value, parentQuestionValue);

        CategoricalOption IStatefulInterview.GetOptionForQuestionWithFilter(Identity question, string value,
            int? parentQuestionValue) => this.GetOptionForQuestionWithFilter(question, value, parentQuestionValue);

        public int CountCommentedQuestions() => this.GetCommentedBySupervisorQuestionsInInterview().Count();

        private static AnswerComment ToAnswerComment(AnsweredQuestionSynchronizationDto answerDto, CommentSynchronizationDto commentDto)
            => new AnswerComment(
                userId: commentDto.UserId,
                commentTime: commentDto.Date,
                comment: commentDto.Text,
                questionIdentity: Identity.Create(answerDto.Id, answerDto.QuestionRosterVector));
    }
}