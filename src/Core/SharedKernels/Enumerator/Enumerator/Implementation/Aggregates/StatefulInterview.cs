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

        public override void Apply(InterviewOnClientCreated @event)
        {
            this.QuestionnaireIdentity = new QuestionnaireIdentity(@event.QuestionnaireId,
                @event.QuestionnaireVersion);

            this.CreatedOnClient = true;
            this.properties.InterviewerId = @event.UserId;

            this.sourceInterview = this.Tree.Clone();

            this.Tree.ActualizeTree();
        }

        public void SynchronizeInterview(Guid userId, InterviewSynchronizationDto synchronizedInterview)
        {
            var propertiesInvariants = new InterviewPropertiesInvariants(this.properties);

            propertiesInvariants.ThrowIfInterviewHardDeleted();

            this.ApplyEvent(new InterviewSynchronized(synchronizedInterview));
        }

        public void Apply(InterviewSynchronized @event)
        {
            this.QuestionnaireIdentity = new QuestionnaireIdentity(@event.InterviewData.QuestionnaireId,
                @event.InterviewData.QuestionnaireVersion);

            this.sourceInterview = this.Tree.Clone();

            this.properties.Status = @event.InterviewData.Status;
            this.properties.WasCompleted = @event.InterviewData.WasCompleted;

            var maxRosterDepth = @event.InterviewData.RosterGroupInstances.SelectMany(x => x.Value).Select(x => x.OuterScopeRosterVector.Length).DefaultIfEmpty(0).Max();
            var maxQuestionDepth = @event.InterviewData.Answers.Select(x => x.QuestionRosterVector.Length).DefaultIfEmpty(0).Max();

            int maxDepth = Math.Max(maxRosterDepth, maxQuestionDepth);

            for (int i = 0; i <= maxDepth; i++)
            {
                var answersByScope = @event.InterviewData.Answers
                    .Where(x => x.QuestionRosterVector.Length == i);

                foreach (var answerDto in answersByScope)
                {
                    var question = this.Tree.GetQuestion(Identity.Create(answerDto.Id, answerDto.QuestionRosterVector));

                    if (answerDto.Answer != null)
                    {
                        question.SetObjectAnswer(answerDto.Answer);
                    }

                    if (answerDto.AllComments != null)
                        question.AnswerComments = answerDto.AllComments.Select(commentDto => ToAnswerComment(answerDto, commentDto)).ToList();
                }

                foreach (var rosterDto in @event.InterviewData.RosterGroupInstances.SelectMany(x => x.Value).Where(x => x.OuterScopeRosterVector.Length == i))
                {
                    var rosterIdentity = new RosterIdentity(rosterDto.RosterId, rosterDto.OuterScopeRosterVector, rosterDto.RosterInstanceId, rosterDto.SortIndex);
                    this.AddRosterToTree(rosterIdentity);
                    this.Tree.GetRoster(rosterIdentity.ToIdentity()).SetRosterTitle(rosterDto.RosterTitle);
                }
            }

            foreach (var disabledGroup in @event.InterviewData.DisabledGroups)
                this.Tree.GetGroup(Identity.Create(disabledGroup.Id, disabledGroup.InterviewItemRosterVector)).Disable();

            foreach (var disabledQuestion in @event.InterviewData.DisabledQuestions)
                this.Tree.GetQuestion(Identity.Create(disabledQuestion.Id, disabledQuestion.InterviewItemRosterVector)).Disable();

            foreach (var invalidQuestion in @event.InterviewData.FailedValidationConditions)
                this.Tree.GetQuestion(invalidQuestion.Key).MarkInvalid(invalidQuestion.Value);

            foreach (var disabledStaticText in @event.InterviewData.DisabledStaticTexts)
                this.Tree.GetStaticText(disabledStaticText).Disable();

            foreach (var invalidStaticText in @event.InterviewData.InvalidStaticTexts)
                this.Tree.GetStaticText(invalidStaticText.Key).MarkInvalid(invalidStaticText.Value);

            foreach (var validStaticText in @event.InterviewData.ValidStaticTexts)
                this.Tree.GetStaticText(validStaticText).MarkValid();

            foreach (var variable in @event.InterviewData.Variables)
                this.Tree.GetVariable(Identity.Create(variable.Key.Id, variable.Key.InterviewItemRosterVector)).SetValue(variable.Value);

            foreach (var disabledVariable in @event.InterviewData.DisabledVariables)
                this.Tree.GetVariable(Identity.Create(disabledVariable.Id, disabledVariable.InterviewItemRosterVector)).Disable();

            foreach (var linkedQuestion in @event.InterviewData.LinkedQuestionOptions)
                this.Tree.GetQuestion(Identity.Create(linkedQuestion.Key.Id, linkedQuestion.Key.InterviewItemRosterVector)).AsLinked.SetOptions(linkedQuestion.Value);

            base.ReplaceSubstitutions(this.Tree, this.GetQuestionnaireOrThrow(),
                @event.InterviewData.Answers.Select(questionWithAnswer =>
                    Identity.Create(questionWithAnswer.Id, questionWithAnswer.QuestionRosterVector)).ToList());

            CalculateLinkedToListOptionsOnTree(this.Tree, false);

            base.UpdateExpressionState(this.sourceInterview, this.Tree, this.ExpressionProcessorStatePrototype);

            this.CreatedOnClient = @event.InterviewData.CreatedOnClient;
            this.properties.SupervisorId = @event.InterviewData.SupervisorId;
            this.properties.InterviewerId = @event.InterviewData.UserId;
            this.SupervisorRejectComment = @event.InterviewData.Comments;

            this.sourceInterview = this.Tree.Clone();
        }

        public void Apply(InterviewAnswersFromSyncPackageRestored @event)
        {
            
        }

        public new void Apply(SubstitutionTitlesChanged @event)
        {
            foreach (var @group in @event.Groups)
                this.Tree.GetGroup(@group).ReplaceSubstitutions();

            foreach (var @question in @event.Questions)
                this.Tree.GetQuestion(@question).ReplaceSubstitutions();

            foreach (var @staticText in @event.StaticTexts)
                this.Tree.GetStaticText(@staticText).ReplaceSubstitutions();
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

        private InterviewTree sourceInterview;
        
        public InterviewStatus Status => this.properties.Status;
        public Guid Id => this.EventSourceId;
        public string InterviewerCompleteComment { get; private set; }
        public string SupervisorRejectComment { get; private set; }

        public string GetAnswerAsString(Identity questionIdentity)
        {
            var questionnaire = this.GetQuestionnaireOrThrow();
            var question = this.Tree.GetQuestion(questionIdentity);

            return question.GetAnswerAsString(answerOptionValue =>
                    questionnaire.GetOptionForQuestionByOptionValue(question.Identity.Id, answerOptionValue).Title);
        }

        public bool HasErrors { get; private set; }

        public bool IsCompleted { get; private set; }

        public InterviewTreeRoster GetRoster(Identity identity) => this.Tree.GetRoster(identity);
        public InterviewTreeGpsQuestion GetGpsQuestion(Identity identity) => this.Tree.GetQuestion(identity).AsGps;
        public InterviewTreeDateTimeQuestion GetDateTimeQuestion(Identity identity) => this.Tree.GetQuestion(identity).AsDateTime;
        public InterviewTreeMultimediaQuestion GetMultimediaQuestion(Identity identity) => this.Tree.GetQuestion(identity).AsMultimedia;
        public InterviewTreeQRBarcodeQuestion GetQRBarcodeQuestion(Identity identity) => this.Tree.GetQuestion(identity).AsQRBarcode;
        public InterviewTreeTextListQuestion GetTextListQuestion(Identity identity) => this.Tree.GetQuestion(identity).AsTextList;
        public InterviewTreeSingleLinkedToRosterQuestion GetLinkedSingleOptionQuestion(Identity identity) => this.Tree.GetQuestion(identity).AsSingleLinkedOption;
        public InterviewTreeMultiOptionQuestion GetMultiOptionQuestion(Identity identity) => this.Tree.GetQuestion(identity).AsMultiFixedOption;
        public InterviewTreeMultiLinkedToRosterQuestion GetLinkedMultiOptionQuestion(Identity identity) => this.Tree.GetQuestion(identity).AsMultiLinkedOption;
        public InterviewTreeIntegerQuestion GetIntegerQuestion(Identity identity) => this.Tree.GetQuestion(identity).AsInteger;
        public InterviewTreeDoubleQuestion GetDoubleQuestion(Identity identity) => this.Tree.GetQuestion(identity).AsDouble;
        public InterviewTreeTextQuestion GetTextQuestion(Identity identity) => this.Tree.GetQuestion(identity).AsText;
        public InterviewTreeSingleOptionQuestion GetSingleOptionQuestion(Identity identity) => this.Tree.GetQuestion(identity).AsSingleFixedOption;
        public InterviewTreeYesNoQuestion GetYesNoQuestion(Identity identity) => this.Tree.GetQuestion(identity).AsYesNo;

        public InterviewTreeSingleOptionLinkedToListQuestion GetSingleOptionLinkedToListQuestion(Identity identity) => this.Tree.GetQuestion(identity).AsSingleLinkedToList;
        public InterviewTreeMultiOptionLinkedToListQuestion GetMultiOptionLinkedToListQuestion(Identity identity) => this.Tree.GetQuestion(identity).AsMultiLinkedToList;

        public string GetLinkedOptionTitle(Identity linkedQuestionIdentity, RosterVector option)
        {
            var linkedQuestion = this.Tree.GetQuestion(linkedQuestionIdentity);
            if (!linkedQuestion.IsLinked) return string.Empty;

            Identity sourceIdentity = Identity.Create(linkedQuestion.AsLinked.LinkedSourceId, option);

            IInterviewTreeNode sourceNode = this.Tree.GetNodeByIdentity(sourceIdentity);

            string optionTitle = string.Empty;
            var skipBreadcrumsThreshold = 1;
            if (sourceNode is InterviewTreeRoster)
            {
                var sourceRoster = sourceNode as InterviewTreeRoster;
                optionTitle = sourceRoster.RosterTitle;
                skipBreadcrumsThreshold = 0;
            }
            if (sourceNode is InterviewTreeQuestion)
            {
                var sourceQuestion = sourceNode as InterviewTreeQuestion;
                optionTitle = sourceQuestion.GetAnswerAsString();
            }
            
            var sourceBreadcrumbsOfRosterTitles = sourceNode.Parents.OfType<InterviewTreeRoster>().ToArray();
            var linkedBreadcrumbsOfRosterTitles = linkedQuestion.Parents.OfType<InterviewTreeRoster>().ToArray();
            
            var common = sourceBreadcrumbsOfRosterTitles.Zip(linkedBreadcrumbsOfRosterTitles, (x, y) => x.RosterSizeId.Equals(y.RosterSizeId) ? x : null).TakeWhile(x => x != null).Count();

            string[] breadcrumbsOfRosterTitles = sourceBreadcrumbsOfRosterTitles.Skip(common).Select(x => x.RosterTitle).ToArray();

            var breadcrumbs = string.Join(": ", breadcrumbsOfRosterTitles);

            return breadcrumbsOfRosterTitles.Length > skipBreadcrumsThreshold
                ?  $"{breadcrumbs}: {optionTitle}"
                : optionTitle;
        }

        #region Command handlers

        public void CreateInterviewOnClient(QuestionnaireIdentity questionnaireIdentity, Guid supervisorId, DateTime answersTime, Guid userId)
        {
            this.QuestionnaireIdentity = questionnaireIdentity;
            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();

            var changedInterviewTree = this.Tree.Clone();

            changedInterviewTree.ActualizeTree();

            this.CalculateTreeDiffChanges(changedInterviewTree, questionnaire, new List<Identity>());

            //apply events
            this.ApplyEvent(new InterviewOnClientCreated(userId, questionnaireIdentity.QuestionnaireId, questionnaire.Version));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.Created, comment: null));

            this.ApplyEvents(changedInterviewTree, userId);

            this.ApplyEvent(new SupervisorAssigned(userId, supervisorId));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.SupervisorAssigned, comment: null));

            this.ApplyEvent(new InterviewerAssigned(userId, userId, answersTime));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.InterviewerAssigned, comment: null));
        }

        public void Complete(Guid userId, string comment, DateTime completeTime)
        {
            var propertiesInvariants = new InterviewPropertiesInvariants(this.properties);

            propertiesInvariants.ThrowIfInterviewHardDeleted();
            propertiesInvariants.ThrowIfInterviewStatusIsNotOneOfExpected(
                InterviewStatus.InterviewerAssigned, InterviewStatus.Restarted, InterviewStatus.RejectedBySupervisor);

            this.ApplyEvents(this.sourceInterview, this.Tree, userId);

            this.ApplyEvent(new InterviewCompleted(userId, completeTime, comment));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.Completed, comment));

            this.ApplyEvent(this.HasInvalidAnswers() || this.HasInvalidStaticTexts
                ? new InterviewDeclaredInvalid() as IEvent
                : new InterviewDeclaredValid());
        }

        public void RestoreInterviewStateFromSyncPackage(Guid userId, InterviewSynchronizationDto synchronizedInterview)
        {
            this.QuestionnaireIdentity = new QuestionnaireIdentity(synchronizedInterview.QuestionnaireId, synchronizedInterview.QuestionnaireVersion);

            new InterviewPropertiesInvariants(this.properties).ThrowIfInterviewHardDeleted();
            
            this.ApplyEvent(new InterviewSynchronized(synchronizedInterview));
        }

        #endregion

        public bool HasGroup(Identity group) => this.Tree.GetGroup(group) != null;

        public string GetRosterTitle(Identity rosterIdentity)
            => this.Tree.GetRoster(rosterIdentity)?.RosterTitle;

        public string GetTitleText(Identity entityIdentity)
        {
            var question = this.Tree.GetQuestion(entityIdentity);
            if (question != null) return question.Title.Text;

            var group = this.Tree.GetGroup(entityIdentity);
            if(group != null) return group.Title.Text;

            var staticText = this.Tree.GetStaticText(entityIdentity);
            if (staticText != null) return staticText.Title.Text;

            return string.Empty;
        }

        public object GetVariableValueByOrDeeperRosterLevel(Guid variableId, RosterVector variableRosterVector)
        {
            do
            {
                var variableIdentity = new Identity(variableId, variableRosterVector);

                var variable = this.Tree.GetVariable(variableIdentity);
                if (variable != null) return variable.Value;

                if (variableRosterVector.Length == 0) break;

                variableRosterVector = variableRosterVector.Shrink(variableRosterVector.Length - 1);
            } while (variableRosterVector.Length >= 0);

            return null;
        }

        public InterviewTreeQuestion FindQuestionInQuestionBranch(Guid entityId, Identity questionIdentity)
            => this.Tree.FindEntityInQuestionBranch(entityId, questionIdentity) as InterviewTreeQuestion;

        public IEnumerable<string> GetParentRosterTitlesWithoutLast(Identity questionIdentity)
            => this.Tree.GetQuestion(questionIdentity).Parents
                .OfType<InterviewTreeRoster>()
                .Select(roster => roster.RosterTitle);

        public IEnumerable<string> GetParentRosterTitlesWithoutLastForRoster(Identity rosterIdentity)
            => this.Tree.GetRoster(rosterIdentity).Parents
                .OfType<InterviewTreeRoster>()
                .Select(roster => roster.RosterTitle);

        public int GetGroupsInGroupCount(Identity group) => this.GetGroupsAndRostersInGroup(group).Count();

        public int CountAnsweredQuestionsInInterview()
            => this.Tree.FindQuestions().Count(question => !question.IsDisabled() && question.IsAnswered());

        public int CountActiveQuestionsInInterview()
            => this.Tree.FindQuestions().Count(question => !question.IsDisabled());

        public int CountInvalidEntitiesInInterview() => this.GetInvalidEntitiesInInterview().Count();

        public IEnumerable<Identity> GetInvalidEntitiesInInterview()
            => this.GetEnabledInvalidStaticTexts().Concat(this.GetEnabledInvalidQuestions());

        private IEnumerable<Identity> GetEnabledInvalidStaticTexts()
            => this.Tree.FindStaticTexts()
                .Where(staticText => !staticText.IsDisabled() && !staticText.IsValid)
                .Select(staticText => staticText.Identity);

        private IEnumerable<Identity> GetEnabledInvalidQuestions()
            => this.Tree.FindQuestions()
                .Where(question => !question.IsDisabled() && !question.IsValid)
                .Select(question => question.Identity);

        public int CountEnabledQuestions(Identity group)
            => this.Tree.FindQuestions(group).Count(question => !question.IsDisabled());

        public int CountEnabledAnsweredQuestions(Identity group)
            => this.Tree.FindQuestions(group).Count(question => !question.IsDisabled() && question.IsAnswered());

        public int CountEnabledInvalidQuestionsAndStaticTexts(Identity group)
            => this.Tree.FindQuestions(group).Count(question => !question.IsDisabled() && !question.IsValid) +
               this.Tree.FindStaticTexts(group).Count(staticText => !staticText.IsDisabled() && !staticText.IsValid);

        public bool HasEnabledInvalidQuestionsAndStaticTexts(Identity group)
            => this.CountEnabledInvalidQuestionsAndStaticTexts(group) > 0;

        public bool HasUnansweredQuestions(Identity group) 
            => this.Tree.FindQuestions(group).Any(question => !question.IsDisabled() && !question.IsAnswered());

        public IEnumerable<Identity> GetCommentedBySupervisorQuestionsInInterview()
        {
            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();

            return this.Tree.FindQuestions().Where(
                question => this.IsEnabledWithSupervisorComments(question, questionnaire))
                .Select(x => new
                {
                    Id = x.Identity,
                    HasInterviewerReply = this.HasSupervisorCommentInterviewerReply(x)
                })
                .OrderBy(x => x.HasInterviewerReply)
                .Select(x => x.Id);
        }

        private bool IsEnabledWithSupervisorComments(InterviewTreeQuestion question, IQuestionnaire questionnaire)
            => !question.IsDisabled() &&
               questionnaire.IsInterviewierQuestion(question.Identity.Id) &&
               question.AnswerComments.Any(y => y.UserId != this.properties.InterviewerId);

        public string GetLastSupervisorComment() => this.SupervisorRejectComment;

        private bool HasSupervisorCommentInterviewerReply(InterviewTreeQuestion question)
        {
            var indexOfLastNotInterviewerComment = question.AnswerComments.FindLastIndex(0, x => x.UserId != this.properties.InterviewerId);
            return question.AnswerComments.Skip(indexOfLastNotInterviewerComment + 1).Any();
        }

        public Identity GetParentGroup(Identity groupOrQuestion)
            => this.Tree.GetNodeByIdentity(groupOrQuestion).Parent.Identity;

        public IEnumerable<Identity> GetChildQuestions(Identity groupIdentity)
            => this.Tree.GetGroup(groupIdentity).Children
                .OfType<InterviewTreeQuestion>()
                .Select(question => question.Identity);

        public IReadOnlyList<Identity> GetRosterInstances(Identity parentIdentity, Guid rosterId)
            => this.Tree.FindRosters()
                .Where(roster => roster.Identity.Id == rosterId && roster.Parent.Identity.Equals(parentIdentity))
                .OrderBy(roster => roster.SortIndex)
                .Select(roster => roster.Identity)
                .ToList();

        public IEnumerable<Identity> GetEnabledSubgroups(Identity group)
            => this.GetGroupsAndRostersInGroup(group)
                .Where(groupOrRoster => !groupOrRoster.IsDisabled())
                .Select(groupOrRoster => groupOrRoster.Identity);

        private IEnumerable<InterviewTreeGroup> GetGroupsAndRostersInGroup(Identity group)
            => this.Tree.GetGroup(group)?.Children?.OfType<InterviewTreeGroup>() ?? new InterviewTreeGroup[0];

        public bool IsValid(Identity identity)
            => (this.Tree.GetQuestion(identity)?.IsValid ?? false) ||
               (this.Tree.GetStaticText(identity)?.IsValid ?? false);

        public IEnumerable<string> GetFailedValidationMessages(Identity questionOrStaticTextId)
        {
            var question = this.Tree.GetQuestion(questionOrStaticTextId);
            if (question?.FailedValidations != null)
            {
                var questionValidationMassages = question.ValidationMessages
                    .Select(substitutionText => substitutionText.Text)
                    .ToList();

                if (questionValidationMassages.Count == 1) return new[] {questionValidationMassages[0]};

                return question.FailedValidations.Select(failedValidation =>
                    $"{questionValidationMassages.ElementAt(failedValidation.FailedConditionIndex)} " +
                    $"[{failedValidation.FailedConditionIndex + 1}]");

            }

            var staticText =  this.Tree.GetStaticText(questionOrStaticTextId);
            if (staticText?.FailedValidations != null)
            {
                var staticTextValidationMassages = staticText.ValidationMessages
                    .Select(substitutionText => substitutionText.Text)
                    .ToList();

                if (staticTextValidationMassages.Count == 1) return new[] {staticTextValidationMassages[0]};

                return staticText.FailedValidations.Select(failedValidation =>
                    $"{staticTextValidationMassages.ElementAt(failedValidation.FailedConditionIndex)} " +
                    $"[{failedValidation.FailedConditionIndex + 1}]");
            }

            return Enumerable.Empty<string>();
        }

        public bool IsEnabled(Identity entityIdentity)
        {
            var node = this.Tree.GetNodeByIdentity(entityIdentity);
            // Not being disposed of ViewModels can try to update their state, but they can be removed from the tree already if roster was removed.
            if (node == null) return false;
            return !node.IsDisabled();
        }

        public bool CreatedOnClient { get; private set; } = false;

        public bool WasAnswered(Identity entityIdentity)
        {
            var question = this.Tree.GetQuestion(entityIdentity);

            return question != null && !question.IsDisabled() && question.IsAnswered();
        }

        public IEnumerable<AnswerComment> GetQuestionComments(Identity entityIdentity)
            => this.Tree.GetQuestion(entityIdentity).AnswerComments;

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