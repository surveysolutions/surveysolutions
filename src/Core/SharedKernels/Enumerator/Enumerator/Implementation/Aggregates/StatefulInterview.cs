using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;
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

        public void Apply(InterviewOnClientCreated @event)
        {
            this.QuestionnaireIdentity = new QuestionnaireIdentity(@event.QuestionnaireId,
                @event.QuestionnaireVersion);

            this.CreatedOnClient = true;
            this.properties.InterviewerId = @event.UserId;

            this.sourceInterview = this.changedInterview.Clone();

            this.changedInterview.ActualizeTree();
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

            this.properties.Status = @event.InterviewData.Status;
            this.properties.WasCompleted = @event.InterviewData.WasCompleted;

            var sourceInterviewTree = this.changedInterview.Clone();

            var maxRosterDepth = @event.InterviewData.RosterGroupInstances.SelectMany(x => x.Value).Select(x => x.OuterScopeRosterVector.Length).DefaultIfEmpty(0).Max();
            var maxQuestionDepth = @event.InterviewData.Answers.Select(x => x.QuestionRosterVector.Length).DefaultIfEmpty(0).Max();

            int maxDepth = Math.Max(maxRosterDepth, maxQuestionDepth);

            for (int i = 0; i <= maxDepth; i++)
            {
                foreach (var question in @event.InterviewData.Answers .Where(questionWithAnswer => questionWithAnswer.Answer != null).Where(x => x.QuestionRosterVector.Length == i))
                {
                    this.changedInterview.GetQuestion(Identity.Create(question.Id, question.QuestionRosterVector)).SetObjectAnswer(question.Answer);
                }

                foreach (var rosterDto in @event.InterviewData.RosterGroupInstances.SelectMany(x => x.Value).Where(x => x.OuterScopeRosterVector.Length == i))
                {
                    var rosterIdentity = new RosterIdentity(rosterDto.RosterId, rosterDto.OuterScopeRosterVector, rosterDto.RosterInstanceId, rosterDto.SortIndex);
                    this.AddRosterToChangedTree(rosterIdentity);
                    this.changedInterview.GetRoster(rosterIdentity.ToIdentity()).SetRosterTitle(rosterDto.RosterTitle);
                }
            }

            foreach (var disabledGroup in @event.InterviewData.DisabledGroups)
                this.changedInterview.GetGroup(Identity.Create(disabledGroup.Id, disabledGroup.InterviewItemRosterVector)).Disable();

            foreach (var disabledQuestion in @event.InterviewData.DisabledQuestions)
                this.changedInterview.GetQuestion(Identity.Create(disabledQuestion.Id, disabledQuestion.InterviewItemRosterVector)).Disable();

            foreach (var invalidQuestion in @event.InterviewData.FailedValidationConditions)
                this.changedInterview.GetQuestion(invalidQuestion.Key).MarkInvalid(invalidQuestion.Value);

            foreach (var disabledStaticText in @event.InterviewData.DisabledStaticTexts)
                this.changedInterview.GetStaticText(disabledStaticText).Disable();

            foreach (var invalidStaticText in @event.InterviewData.InvalidStaticTexts)
                this.changedInterview.GetStaticText(invalidStaticText.Key).MarkInvalid(invalidStaticText.Value);

            foreach (var validStaticText in @event.InterviewData.ValidStaticTexts)
                this.changedInterview.GetStaticText(validStaticText).MarkValid();

            foreach (var variable in @event.InterviewData.Variables)
                this.changedInterview.GetVariable(Identity.Create(variable.Key.Id, variable.Key.InterviewItemRosterVector)).SetValue(variable.Value);

            foreach (var disabledVariable in @event.InterviewData.DisabledVariables)
                this.changedInterview.GetVariable(Identity.Create(disabledVariable.Id, disabledVariable.InterviewItemRosterVector)).Disable();

            foreach (var linkedQuestion in @event.InterviewData.LinkedQuestionOptions)
                this.changedInterview.GetQuestion(Identity.Create(linkedQuestion.Key.Id, linkedQuestion.Key.InterviewItemRosterVector)).AsLinked.SetOptions(linkedQuestion.Value);

            this.changedInterview.AnswerComments = @event.InterviewData.Answers
                ?.SelectMany(answerDto => answerDto.AllComments?.Select(commentDto => ToAnswerComment(answerDto, commentDto)))
                ?.ToList() ?? new List<AnswerComment>();

            //base.ReplaceSubstitutions(this.changedInterview, this.GetQuestionnaireOrThrow(), changedQeustionIdentities);

            base.UpdateExpressionState(sourceInterviewTree, this.changedInterview, this.ExpressionProcessorStatePrototype);

            this.CreatedOnClient = @event.InterviewData.CreatedOnClient;
            this.properties.SupervisorId = @event.InterviewData.SupervisorId;
            this.properties.InterviewerId = @event.InterviewData.UserId;
            this.SupervisorRejectComment = @event.InterviewData.Comments;

            this.sourceInterview = this.changedInterview.Clone();
        }

        public void Apply(InterviewAnswersFromSyncPackageRestored @event)
        {
            
        }

        public new void Apply(SubstitutionTitlesChanged @event)
        {
            foreach (var @group in @event.Groups)
                this.changedInterview.GetGroup(@group).ReplaceSubstitutions();

            foreach (var @question in @event.Questions)
                this.changedInterview.GetQuestion(@question).ReplaceSubstitutions();

            foreach (var @staticText in @event.StaticTexts)
                this.changedInterview.GetStaticText(@staticText).ReplaceSubstitutions();
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
            => this.changedInterview.GetQuestion(questionIdentity).GetAnswerAsString();

        public bool HasErrors { get; private set; }

        public bool IsCompleted { get; private set; }

        public InterviewTreeRoster GetRoster(Identity identity) => this.changedInterview.GetRoster(identity);
        public InterviewTreeGpsQuestion GetGpsQuestion(Identity identity) => this.changedInterview.GetQuestion(identity).AsGps;
        public InterviewTreeDateTimeQuestion GetDateTimeQuestion(Identity identity) => this.changedInterview.GetQuestion(identity).AsDateTime;
        public InterviewTreeMultimediaQuestion GetMultimediaQuestion(Identity identity) => this.changedInterview.GetQuestion(identity).AsMultimedia;
        public InterviewTreeQRBarcodeQuestion GetQRBarcodeQuestion(Identity identity) => this.changedInterview.GetQuestion(identity).AsQRBarcode;
        public InterviewTreeTextListQuestion GetTextListQuestion(Identity identity) => this.changedInterview.GetQuestion(identity).AsTextList;
        public InterviewTreeSingleLinkedToRosterQuestion GetLinkedSingleOptionQuestion(Identity identity) => this.changedInterview.GetQuestion(identity).AsSingleLinkedOption;
        public InterviewTreeMultiOptionQuestion GetMultiOptionQuestion(Identity identity) => this.changedInterview.GetQuestion(identity).AsMultiFixedOption;
        public InterviewTreeMultiLinkedToRosterQuestion GetLinkedMultiOptionQuestion(Identity identity) => this.changedInterview.GetQuestion(identity).AsMultiLinkedOption;
        public InterviewTreeIntegerQuestion GetIntegerQuestion(Identity identity) => this.changedInterview.GetQuestion(identity).AsInteger;
        public InterviewTreeDoubleQuestion GetDoubleQuestion(Identity identity) => this.changedInterview.GetQuestion(identity).AsDouble;
        public InterviewTreeTextQuestion GetTextQuestion(Identity identity) => this.changedInterview.GetQuestion(identity).AsText;
        public InterviewTreeSingleOptionQuestion GetSingleOptionQuestion(Identity identity) => this.changedInterview.GetQuestion(identity).AsSingleFixedOption;
        public InterviewTreeYesNoQuestion GetYesNoQuestion(Identity identity) => this.changedInterview.GetQuestion(identity).AsYesNo;

        public InterviewTreeSingleOptionLinkedToListQuestion GetSingleOptionLinkedToListQuestion(Identity identity) => this.changedInterview.GetQuestion(identity).AsSingleLinkedToList;
        public InterviewTreeMultiOptionLinkedToListQuestion GetMultiOptionLinkedToListQuestion(Identity identity) => this.changedInterview.GetQuestion(identity).AsMultiLinkedToList;

        public string GetLinkedOptionTitle(Identity linkedQuestionIdentity, RosterVector option)
        {
            var linkedQuestion = this.changedInterview.GetQuestion(linkedQuestionIdentity);
            if (!linkedQuestion.IsLinked) return string.Empty;

            Identity sourceIdentity = Identity.Create(linkedQuestion.AsLinked.LinkedSourceId, option);

            IInterviewTreeNode sourceNode = changedInterview.GetNodeByIdentity(sourceIdentity);

            string optionTitle = string.Empty;
            if (sourceNode is InterviewTreeRoster)
            {
                var sourceRoster = sourceNode as InterviewTreeRoster;
                optionTitle = sourceRoster.RosterTitle;
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

            return breadcrumbsOfRosterTitles.Length > 1
                ?  $"{breadcrumbs}: {optionTitle}"
                : optionTitle;
        }

        #region Command handlers

        public void CreateInterviewOnClient(QuestionnaireIdentity questionnaireIdentity, Guid supervisorId, DateTime answersTime, Guid userId)
        {
            this.QuestionnaireIdentity = questionnaireIdentity;
            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();

            var sourceInterviewTree = this.changedInterview;
            var changedInterviewTree = sourceInterviewTree.Clone();

            //apply events
            this.ApplyEvent(new InterviewOnClientCreated(userId, questionnaireIdentity.QuestionnaireId, questionnaire.Version));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.Created, comment: null));

            changedInterviewTree.ActualizeTree();

            this.ApplyTreeDiffChanges(userId: userId, questionnaire: questionnaire,
                sourceInterviewTree: sourceInterviewTree, changedInterviewTree: changedInterviewTree,
                changedQuestionIdentities: new List<Identity>());

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

            this.ApplyEvents(this.sourceInterview, this.changedInterview, userId);

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

        public bool HasGroup(Identity group) => this.changedInterview.GetGroup(group) != null;

        public string GetRosterTitle(Identity rosterIdentity)
            => this.changedInterview.GetRoster(rosterIdentity)?.RosterTitle;

        public string GetTitleText(Identity entityIdentity)
        {
            var question = this.changedInterview.GetQuestion(entityIdentity);
            if (question != null) return question.Title.Text;

            var group = this.changedInterview.GetGroup(entityIdentity);
            if(group != null) return group.Title.Text;

            var staticText = this.changedInterview.GetStaticText(entityIdentity);
            if (staticText != null) return staticText.Title.Text;

            return string.Empty;
        }

        public object GetVariableValueByOrDeeperRosterLevel(Guid variableId, RosterVector variableRosterVector)
        {
            do
            {
                var variableIdentity = new Identity(variableId, variableRosterVector);

                var variable = this.changedInterview.GetVariable(variableIdentity);
                if (variable != null) return variable.Value;

                if (variableRosterVector.Length == 0) break;

                variableRosterVector = variableRosterVector.Shrink(variableRosterVector.Length - 1);
            } while (variableRosterVector.Length >= 0);

            return null;
        }

        public InterviewTreeTextListQuestion FindTextListQuestionInQuestionBranch(Guid entityId, Identity questionIdentity)
        {
            return (this.changedInterview.FindEntityInQuestionBranch(entityId, questionIdentity) as InterviewTreeQuestion)?.AsTextList;
        }

        public IEnumerable<string> GetParentRosterTitlesWithoutLast(Identity questionIdentity)
            => this.changedInterview.GetQuestion(questionIdentity).Parents
                .OfType<InterviewTreeRoster>()
                .Select(roster => roster.RosterTitle);

        public IEnumerable<string> GetParentRosterTitlesWithoutLastForRoster(Identity rosterIdentity)
            => this.changedInterview.GetRoster(rosterIdentity).Parents
                .OfType<InterviewTreeRoster>()
                .Select(roster => roster.RosterTitle);

        public int GetGroupsInGroupCount(Identity group) => this.GetGroupsAndRostersInGroup(group).Count();

        public int CountAnsweredQuestionsInInterview()
            => this.changedInterview.FindQuestions().Count(question => !question.IsDisabled() && question.IsAnswered());

        public int CountActiveQuestionsInInterview()
            => this.changedInterview.FindQuestions().Count(question => !question.IsDisabled());

        public int CountInvalidEntitiesInInterview() => this.GetInvalidEntitiesInInterview().Count();

        public IEnumerable<Identity> GetInvalidEntitiesInInterview()
            => this.GetEnabledInvalidStaticTexts().Concat(this.GetEnabledInvalidQuestions());

        private IEnumerable<Identity> GetEnabledInvalidStaticTexts()
            => this.changedInterview.FindStaticTexts()
                .Where(staticText => !staticText.IsDisabled() && !staticText.IsValid)
                .Select(staticText => staticText.Identity);

        private IEnumerable<Identity> GetEnabledInvalidQuestions()
            => this.changedInterview.FindQuestions()
                .Where(question => !question.IsDisabled() && !question.IsValid)
                .Select(question => question.Identity);

        public int CountEnabledQuestions(Identity group)
            => this.changedInterview.FindQuestions(group).Count(question => !question.IsDisabled());

        public int CountEnabledAnsweredQuestions(Identity group)
            => this.changedInterview.FindQuestions(group).Count(question => !question.IsDisabled() && question.IsAnswered());

        public int CountEnabledInvalidQuestionsAndStaticTexts(Identity group)
            => this.changedInterview.FindQuestions(group).Count(question => !question.IsDisabled() && !question.IsValid) +
               this.changedInterview.FindStaticTexts(group).Count(staticText => !staticText.IsDisabled() && !staticText.IsValid);

        public bool HasEnabledInvalidQuestionsAndStaticTexts(Identity group)
            => this.CountEnabledInvalidQuestionsAndStaticTexts(group) > 0;

        public bool HasUnansweredQuestions(Identity group) 
            => this.changedInterview.FindQuestions(group).Any(question => !question.IsDisabled() && !question.IsAnswered());

        public IEnumerable<Identity> GetCommentedBySupervisorQuestionsInInterview()
        {
            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();
            
            var commentedEnabledInterviewerQuestionIds = this.changedInterview.AnswerComments
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
            => this.changedInterview.GetNodeByIdentity(groupOrQuestion).Parent.Identity;

        public IEnumerable<Identity> GetChildQuestions(Identity groupIdentity)
            => this.changedInterview.GetGroup(groupIdentity).Children
                .OfType<InterviewTreeQuestion>()
                .Select(question => question.Identity);

        public IReadOnlyList<Identity> GetRosterInstances(Identity parentIdentity, Guid rosterId)
            => this.changedInterview.FindRosters()
                .Where(roster => roster.Identity.Id == rosterId && roster.Parent.Identity.Equals(parentIdentity))
                .Select(roster => roster.Identity)
                .ToList();

        public IEnumerable<Identity> GetEnabledSubgroups(Identity group)
            => this.GetGroupsAndRostersInGroup(group)
                .Where(groupOrRoster => !groupOrRoster.IsDisabled())
                .Select(groupOrRoster => groupOrRoster.Identity);

        private IEnumerable<InterviewTreeGroup> GetGroupsAndRostersInGroup(Identity group)
            => this.changedInterview.GetGroup(group)?.Children?.OfType<InterviewTreeGroup>() ?? new InterviewTreeGroup[0];

        public bool IsValid(Identity identity)
            => (this.changedInterview.GetQuestion(identity)?.IsValid ?? false) ||
               (this.changedInterview.GetStaticText(identity)?.IsValid ?? false);

        public IEnumerable<string> GetFailedValidationMessages(Identity questionOrStaticTextId)
        {
            var question = this.changedInterview.GetQuestion(questionOrStaticTextId);
            if (question?.FailedValidations != null)
            {
                var questionValidationMassages = question.ValidationMessages
                    .Select(substitutionText => substitutionText.Text)
                    .ToList();

                return question.FailedValidations.Select((failedValidation) => questionValidationMassages.ElementAt(failedValidation.FailedConditionIndex));
            }

            var staticText =  this.changedInterview.GetStaticText(questionOrStaticTextId);
            if (staticText?.FailedValidations != null)
            {
                var staticTextValidationMassages = staticText.ValidationMessages
                    .Select(substitutionText => substitutionText.Text)
                    .ToList();

                return staticText.FailedValidations.Select((failedValidation, index) => staticTextValidationMassages.ElementAt(index));
            }

            return Enumerable.Empty<string>();
        }

        public bool IsEnabled(Identity entityIdentity)
        {
            var node = this.changedInterview.GetNodeByIdentity(entityIdentity);
            // Not being disposed of ViewModels can try to update their state, but they can be removed from the tree already if roster was removed.
            if (node == null) return false;
            return !node.IsDisabled();
        }

        public bool CreatedOnClient { get; private set; } = false;

        public bool WasAnswered(Identity entityIdentity)
        {
            var question = this.changedInterview.GetQuestion(entityIdentity);

            return question != null && !question.IsDisabled() && question.IsAnswered();
        }

        public IEnumerable<AnswerComment> GetQuestionComments(Identity entityIdentity)
            => this.changedInterview.AnswerComments.Where(comment => comment.QuestionIdentity.Equals(entityIdentity));

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