using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Ncqrs.Eventing;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Invariants;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Events;
using IEvent = WB.Core.Infrastructure.EventBus.IEvent;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    //stored in global cache
    //references to repositories should be resolved on demand 
    public class StatefulInterview : Interview, IStatefulInterview
    {
        public StatefulInterview(
            ISubstitutionTextFactory substitutionTextFactory,
            IInterviewTreeBuilder treeBuilder,
            IQuestionOptionsRepository optionsRepository)
            : base(substitutionTextFactory, treeBuilder, optionsRepository)
        {
        }

        #region Apply

        protected override void Apply(InterviewCreated @event)
        {
            base.Apply(@event);
            this.sourceInterview = this.Tree.Clone();
        }

        protected override void Apply(InterviewFromPreloadedDataCreated @event)
        {
            base.Apply(@event);
            this.sourceInterview = this.Tree.Clone();
        }

        protected override void Apply(InterviewOnClientCreated @event)
        {
            this.QuestionnaireIdentity = new QuestionnaireIdentity(@event.QuestionnaireId,
                @event.QuestionnaireVersion);

            this.properties.InterviewerId = @event.UserId;
            this.sourceInterview = this.Tree.Clone();
        }

        public void Apply(InterviewSynchronized @event)
        {
            this.QuestionnaireIdentity = new QuestionnaireIdentity(@event.InterviewData.QuestionnaireId,
                @event.InterviewData.QuestionnaireVersion);

            this.sourceInterview = this.Tree.Clone();
            this.Tree.ActualizeTree();

            this.properties.Status = @event.InterviewData.Status;
            this.properties.WasCompleted = @event.InterviewData.WasCompleted;
            this.properties.AssignmentId = @event.InterviewData.AssignmentId;

            foreach (var answerDto in @event.InterviewData.Answers.OrderBy(x => x.QuestionRosterVector.Length))
            {
                var question = this.Tree.GetQuestion(Identity.Create(answerDto.Id, answerDto.QuestionRosterVector));

                if (question == null)
                {
                    //now answers from removed rosters are in sync package.
                    continue;
                }

                if (answerDto.Answer != null)
                {
                    question.SetObjectAnswer(answerDto.Answer);
                }

                if (answerDto.ProtectedAnswer != null)
                {
                    question.SetObjectProtectedAnswer(answerDto.ProtectedAnswer);
                }

                if (answerDto.AllComments != null)
                    question.AnswerComments = answerDto.AllComments.Select(commentDto => ToAnswerComment(answerDto, commentDto)).ToList();

                this.ActualizeRostersIfQuestionIsRosterSize(answerDto.Id);
            }

            this.UpdateTreeWithDependentChanges(this.Tree, this.GetQuestionnaireOrThrow(), entityIdentity: null,
                DateTimeOffset.UtcNow, removeLinkedAnswers: false);

            foreach (var readonlyQuestion in @event.InterviewData.ReadonlyQuestions)
            {
                this.Tree.GetQuestion(Identity.Create(readonlyQuestion.Id, readonlyQuestion.InterviewItemRosterVector))?.MarkAsReadonly();
            }

            this.properties.SupervisorId = @event.InterviewData.SupervisorId;
            this.properties.InterviewerId = @event.InterviewData.UserId;
            this.SupervisorRejectComment = @event.InterviewData.Comments;

            this.interviewKey = @event.InterviewData.InterviewKey;
            this.sourceInterview = this.Tree.Clone();
        }

        public void Apply(InterviewAnswersFromSyncPackageRestored @event)
        {

        }

        protected override void Apply(InterviewCompleted @event)
        {
            base.Apply(@event);

            this.InterviewerCompleteComment = @event.Comment;
            this.IsCompleted = true;
        }

        protected override void Apply(InterviewRejected @event)
        {
            base.Apply(@event);
            this.SupervisorRejectComment = @event.Comment;
        }

        protected override void Apply(InterviewRestarted @event)
        {
            base.Apply(@event);
            this.IsCompleted = false;
        }

        protected override void Apply(InterviewDeclaredValid @event)
        {
            base.Apply(@event);
            this.HasErrors = false;
        }

        protected override void Apply(InterviewDeclaredInvalid @event)
        {
            base.Apply(@event);
            this.HasErrors = true;
        }

        #endregion

        private InterviewTree sourceInterview;

        public DateTimeOffset? StartedDate => this.properties.StartedDate;
        public DateTimeOffset? CompletedDate => this.properties.CompletedDate;
        public InterviewStatus Status => this.properties.Status;
        public InterviewMode Mode => this.properties.Mode;

        public bool IsDeleted => this.properties.IsHardDeleted || this.Status == InterviewStatus.Deleted;

        public Guid Id => this.EventSourceId;
        public string InterviewerCompleteComment { get; private set; }
        public string SupervisorRejectComment { get; private set; }

        public string GetAnswerAsString(Identity questionIdentity, CultureInfo cultureInfo = null)
        {
            var question = this.Tree.GetQuestion(questionIdentity);
            return question.GetAnswerAsString(cultureInfo ?? CultureInfo.InvariantCulture);
        }

        public string GetVariableValueAsString(Identity variableIdentity)
        {
            var variable = this.Tree.GetVariable(variableIdentity);
            return variable.GetValueAsString();
        }

        protected override void OnEventApplied(UncommittedEvent appliedEvent)
        {
            base.OnEventApplied(appliedEvent);
            if (appliedEvent.Payload is QuestionAnswered questionAnswered)
            {
                this.properties.LastAnswerDate = questionAnswered.OriginDate;
            }
        }

        public bool HasErrors { get; private set; }

        public bool IsCompleted { get; private set; }

        public InterviewTreeGroup GetGroup(Identity identity) => this.Tree.GetGroup(identity);
        public InterviewTreeRoster GetRoster(Identity identity) => this.Tree.GetRoster(identity);
        public InterviewTreeVariable GetVariable(Identity identity) => this.Tree.GetVariable(identity);

        public InterviewTreeGpsQuestion GetGpsQuestion(Identity identity) => this.Tree.GetQuestion(identity).GetAsInterviewTreeGpsQuestion();
        public InterviewTreeDateTimeQuestion GetDateTimeQuestion(Identity identity) => this.Tree.GetQuestion(identity).GetAsInterviewTreeDateTimeQuestion();
        public InterviewTreeMultimediaQuestion GetMultimediaQuestion(Identity identity) => this.Tree.GetQuestion(identity).GetAsInterviewTreeMultimediaQuestion();
        public InterviewTreeQRBarcodeQuestion GetQRBarcodeQuestion(Identity identity) => this.Tree.GetQuestion(identity).GetAsInterviewTreeQRBarcodeQuestion();
        public InterviewTreeTextListQuestion GetTextListQuestion(Identity identity) => this.Tree.GetQuestion(identity).GetAsInterviewTreeTextListQuestion();
        public InterviewTreeMultiOptionQuestion GetMultiOptionQuestion(Identity identity) => this.Tree.GetQuestion(identity).GetAsInterviewTreeMultiOptionQuestion();
        public InterviewTreeIntegerQuestion GetIntegerQuestion(Identity identity) => this.Tree.GetQuestion(identity).GetAsInterviewTreeIntegerQuestion();
        public InterviewTreeDoubleQuestion GetDoubleQuestion(Identity identity) => this.Tree.GetQuestion(identity).GetAsInterviewTreeDoubleQuestion();
        public InterviewTreeTextQuestion GetTextQuestion(Identity identity) => this.Tree.GetQuestion(identity).GetAsInterviewTreeTextQuestion();
        public InterviewTreeSingleOptionQuestion GetSingleOptionQuestion(Identity identity) => this.Tree.GetQuestion(identity).GetAsInterviewTreeSingleOptionQuestion();
        public InterviewTreeYesNoQuestion GetYesNoQuestion(Identity identity) => this.Tree.GetQuestion(identity).GetAsInterviewTreeYesNoQuestion();
        public InterviewTreeCascadingQuestion GetCascadingQuestion(Identity identity) => this.Tree.GetQuestion(identity).GetAsInterviewTreeCascadingQuestion();

        public InterviewTreeSingleOptionLinkedToListQuestion GetSingleOptionLinkedToListQuestion(Identity identity) => this.Tree.GetQuestion(identity).GetAsInterviewTreeSingleOptionLinkedToListQuestion();
        public InterviewTreeAudioQuestion GetAudioQuestion(Identity identity) => this.Tree.GetQuestion(identity).GetAsInterviewTreeAudioQuestion();

        public InterviewTreeQuestion GetQuestion(Identity identity) => this.Tree.GetQuestion(identity);
        public InterviewTreeStaticText GetStaticText(Identity identity) => this.Tree.GetStaticText(identity);

        public InterviewTreeAreaQuestion GetAreaQuestion(Identity identity) => this.Tree.GetQuestion(identity).GetAsInterviewTreeAreaQuestion();

        public IEnumerable<InterviewTreeSection> GetEnabledSections() => this.Tree.Sections.Where(s => !s.IsDisabled());

        #region Command handlers


        public void Complete(Guid userId, string comment, DateTimeOffset originDate)
        {
            Complete(userId, comment, originDate, true);
        }

        public void CompleteWithoutFirePassiveEvents(Guid userId, string comment, DateTimeOffset originDate)
        {
            Complete(userId, comment, originDate, false);
        }

        private void Complete(Guid userId, string comment, DateTimeOffset originDate, bool isNeedFirePassiveEvents)
        {
            var propertiesInvariants = new InterviewPropertiesInvariants(this.properties);

            propertiesInvariants.ThrowIfInterviewHardDeleted();
            propertiesInvariants.ThrowIfInterviewStatusIsNotOneOfExpected(
                InterviewStatus.SupervisorAssigned, InterviewStatus.InterviewerAssigned, InterviewStatus.Restarted, InterviewStatus.RejectedBySupervisor);

            if (isNeedFirePassiveEvents)
            {
                var treeDifference = FindDifferenceBetweenTrees(this.sourceInterview, this.Tree);
                this.ApplyPassiveEvents(treeDifference, originDate);
            }

            this.ApplyEvent(new InterviewCompleted(userId, originDate, comment));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.Completed, comment, previousStatus: this.properties.Status, originDate: originDate));
            this.ApplyEvent(new InterviewPaused(userId, originDate));

            var becomesValid = !(this.HasInvalidAnswers() || this.HasInvalidStaticTexts);
            if (this.properties.IsValid != becomesValid)
            {
                this.ApplyEvent(becomesValid
                    ? new InterviewDeclaredValid(originDate)
                    : new InterviewDeclaredInvalid(originDate) as IEvent);
            }
        }
        //Obsolete. Is used only in tests. Remove
        public void Synchronize(SynchronizeInterviewCommand command)
        {
            this.QuestionnaireIdentity = new QuestionnaireIdentity(command.SynchronizedInterview.QuestionnaireId, command.SynchronizedInterview.QuestionnaireVersion);

            new InterviewPropertiesInvariants(this.properties).ThrowIfInterviewHardDeleted();

            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();

            base.CreateInterviewFromSynchronizationMetadata(command.SynchronizedInterview.Id,
                command.UserId,
                command.SynchronizedInterview.QuestionnaireId,
                command.SynchronizedInterview.QuestionnaireVersion,
                command.InitialStatus,
                command.FeaturedQuestionsMeta,
                command.SynchronizedInterview.Comments,
                command.SynchronizedInterview.RejectDateTime,
                command.SynchronizedInterview.InterviewerAssignedDateTime,
                true,
                command.CreatedOnClient,
                command.OriginDate
            );

            if (command.SynchronizedInterview.Language != null)
                this.ApplyEvent(new TranslationSwitched(command.SynchronizedInterview.Language, command.UserId, originDate: command.OriginDate));

            var synchronizedInterviewInterviewKey = command.SynchronizedInterview.InterviewKey;
            if (synchronizedInterviewInterviewKey != null)
            {
                this.ApplyEvent(new InterviewKeyAssigned(synchronizedInterviewInterviewKey, command.OriginDate));
            }
            this.ApplyEvent(new InterviewSynchronized(command.SynchronizedInterview, command.OriginDate));
        }

        public void CreateInterviewFromSnapshot(CreateInterviewFromSnapshotCommand command)
        {
            var questionnaireIdentity = new QuestionnaireIdentity(command.SynchronizedInterview.QuestionnaireId, command.SynchronizedInterview.QuestionnaireVersion);
            this.QuestionnaireIdentity = questionnaireIdentity;
            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();

            new InterviewPropertiesInvariants(this.properties).ThrowIfInterviewHardDeleted();

            base.CreateInterviewFromSynchronizationMetadata(command.InterviewId,
                command.UserId,
                command.SynchronizedInterview.QuestionnaireId,
                command.SynchronizedInterview.QuestionnaireVersion,
                command.SynchronizedInterview.Status,
                new AnsweredQuestionSynchronizationDto[0],
                command.SynchronizedInterview.Comments,
                command.SynchronizedInterview.RejectDateTime,
                command.SynchronizedInterview.InterviewerAssignedDateTime,
                true,
                command.SynchronizedInterview.CreatedOnClient,
                command.OriginDate
            );

            if (command.SynchronizedInterview.Language != null)
                this.ApplyEvent(new TranslationSwitched(command.SynchronizedInterview.Language, command.UserId, command.OriginDate));

            this.ApplyEvent(new InterviewSynchronized(command.SynchronizedInterview, command.OriginDate));

            this.UpdateTreeWithDependentChanges(this.Tree, questionnaire, entityIdentity: null, command.OriginDate);
        }

        #endregion

        public bool HasEditableIdentifyingQuestions
        {
            get
            {
                IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();
                return questionnaire.GetPrefilledQuestions()
                    .Select(x => this.Tree.GetQuestion(new Identity(x, RosterVector.Empty)))
                    .Any(x => !x.IsReadonly);
            }
        }

        public bool ReceivedByInterviewer => this.properties.IsReceivedByInterviewer;

        public bool HasGroup(Identity group) => this.Tree.GetGroup(group) != null;

        public string GetRosterTitle(Identity rosterIdentity)
            => this.Tree.GetRoster(rosterIdentity)?.RosterTitle;

        public string GetTitleText(Identity entityIdentity)
            => this.GetTitleSubstitutionText(entityIdentity)?.Text ?? string.Empty;

        public string GetBrowserReadyTitleHtml(Identity entityIdentity)
            => this.GetTitleSubstitutionText(entityIdentity)?.BrowserReadyText ?? string.Empty;

        private SubstitutionText GetTitleSubstitutionText(Identity entityIdentity)
            => this.Tree.GetQuestion(entityIdentity)?.Title
            ?? this.Tree.GetGroup(entityIdentity)?.Title
            ?? this.Tree.GetStaticText(entityIdentity)?.Title;

        public string GetBrowserReadyInstructionsHtml(Identity entityIdentity)
            => this.Tree.GetQuestion(entityIdentity)?.Instructions?.BrowserReadyText ?? string.Empty;

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

        public IEnumerable<Identity> FindQuestionsFromSameOrDeeperLevel(Guid entityId, Identity questionIdentity)
            => this.Tree.FindEntitiesFromSameOrDeeperLevel(entityId, questionIdentity);

        public bool IsQuestionPrefilled(Identity entityIdentity)
        {
            return this.Tree.GetQuestion(entityIdentity)?.IsPrefilled ?? false;
        }

        public IEnumerable<Identity> GetUnderlyingEntitiesForReview(Identity sectionId)
        {
            var section = this.Tree.GetNodeByIdentity(sectionId);
            if (section == null)
            {
                throw new ArgumentException($"Section not found", nameof(sectionId))
                {
                    Data =
                    {
                        { "SectionId", sectionId },
                        { "InterviewId", Id }
                    }
                };
            }

            var isCover = this.sourceInterview.Questionnaire.IsCoverPage(sectionId.Id);

            return section.Children
                .Where(x => isCover || !(x is InterviewTreeVariable))
                .Select(x => x.Identity);
        }

        public IEnumerable<Identity> GetUnderlyingEntitiesForReviewRecursive(Identity sectionId)
        {
            var section = this.Tree.GetNodeByIdentity(sectionId);

            if (section == null)
            {
                throw new ArgumentException($"Section not found", nameof(sectionId))
                {
                    Data =
                    {
                        { "SectionId", sectionId },
                        { "InterviewId", Id }
                    }
                };
            }

            var isCover = this.sourceInterview.Questionnaire.IsCoverPage(sectionId.Id);

            return section
                .TreeToEnumerableDepthFirst(s => s.Children)
                .Where(x => isCover || !(x is InterviewTreeVariable))
                .Select(x => x.Identity);
        }

        public IEnumerable<Identity> FindEntity(Guid id) => this.Tree.FindEntity(id).Select(i => i.Identity);

        public IEnumerable<Identity> GetAllIdentitiesForEntityId(Guid id)
            => this.Tree.AllNodes.Where(node => node.Identity.Id == id).Select(node => node.Identity);

        public IEnumerable<string> GetParentRosterTitlesWithoutLast(Identity questionIdentity)
            => this.Tree.GetQuestion(questionIdentity).Parents
                .OfType<InterviewTreeRoster>()
                .Select(roster => roster.RosterTitle);

        public IEnumerable<string> GetParentRosterTitlesWithoutLastForRoster(Identity rosterIdentity)
            => this.Tree.GetRoster(rosterIdentity).Parents
                .OfType<InterviewTreeRoster>()
                .Select(roster => roster.RosterTitle);

        public int GetGroupsInGroupCount(Identity group) => this.GetGroupsAndRostersInGroup(group).Count();

        private IEnumerable<InterviewTreeQuestion> GetEnabledNotHiddenQuestions()
            => this.Tree.FindQuestions().Where(question => !question.IsDisabled() && !question.IsHidden);

        private IEnumerable<InterviewTreeQuestion> GetEnabledInterviewerQuestions()
            => this.GetEnabledNotHiddenQuestions().Where(question =>
                question.IsInterviewer && !question.IsReadonly);

        private IEnumerable<InterviewTreeQuestion> GetEnabledQuestionsForSupervisor()
            => this.GetEnabledNotHiddenQuestions().Where(question =>
                (question.IsInterviewer || question.IsSupervisors) && !question.IsReadonly);


        public int CountActiveAnsweredQuestionsInInterview() =>
            this.GetEnabledInterviewerQuestions().Count(question => question.IsAnswered());

        public int CountActiveQuestionsInInterview() =>
            this.GetEnabledInterviewerQuestions().Count();

        public int CountInvalidEntitiesInInterview() => this.GetInvalidEntitiesInInterview().Count();

        public int CountActiveAnsweredQuestionsInInterviewForSupervisor() =>
            this.GetEnabledQuestionsForSupervisor().Count(question => question.IsAnswered());

        public int CountActiveQuestionsInInterviewForSupervisor() =>
            this.GetEnabledQuestionsForSupervisor().Count();

        public int CountInvalidEntitiesInInterviewForSupervisor() => this.GetInvalidEntitiesInInterviewForSupervisor().Count();

        public int CountAllEnabledAnsweredQuestions()
            => this.GetEnabledNotHiddenQuestions().Count(question => question.IsAnswered());
        public int CountAllEnabledQuestions() => this.GetEnabledNotHiddenQuestions().Count();
        public int CountAllInvalidEntities() => this.GetAllInvalidEntitiesInInterview().Count();

        public IEnumerable<Identity> GetAllInvalidEntitiesInInterview()
            => this.GetEnabledInvalidStaticTexts()
                .Concat(this.GetEnabledInvalidQuestions(true).Select(question => question.Identity));

        public IEnumerable<Identity> GetInvalidEntitiesInInterview()
            => this.GetEnabledInvalidStaticTexts()
                .Concat(this.GetEnabledInvalidQuestions().Where(question => question.IsInterviewer)
                    .Select(question => question.Identity));

        private IEnumerable<Identity> GetInvalidEntitiesInInterviewForSupervisor()
            => this.GetEnabledInvalidStaticTexts()
                .Concat(this.GetEnabledInvalidQuestions().Where(question => question.IsInterviewer || question.IsSupervisors)
                    .Select(question => question.Identity));

        public bool IsFirstEntityBeforeSecond(Identity first, Identity second)
        {
            return first == this.Tree.GetAllNodesInEnumeratorOrder()
                               .Select(node => node.Identity)
                               .FirstOrDefault(identity => identity == first || identity == second);
        }

        private IEnumerable<Identity> GetEnabledInvalidStaticTexts()
            => this.Tree.FindStaticTexts()
                .Where(staticText => !staticText.IsDisabled() && !staticText.IsValid)
                .Select(staticText => staticText.Identity);

        private IEnumerable<InterviewTreeQuestion> GetEnabledInvalidQuestions(bool includeAllPrefilled = false)
            => this.Tree.FindQuestions()
                .Where(question => !question.IsDisabled() && !question.IsValid && (includeAllPrefilled || !question.IsReadonly));

        public int CountEnabledQuestions(Identity group)
            => this.Tree.GetGroup(group)?.CountEnabledQuestions() ?? 0;

        public int CountEnabledAnsweredQuestions(Identity group)
            => this.Tree.GetGroup(group)?.CountEnabledAnsweredQuestions() ?? 0;

        public int CountEnabledInvalidQuestionsAndStaticTexts(Identity group)
            => this.Tree.GetGroup(group)?.CountEnabledInvalidQuestionsAndStaticTexts() ?? 0;

        public bool HasEnabledInvalidQuestionsAndStaticTexts(Identity group)
            => this.CountEnabledInvalidQuestionsAndStaticTexts(group) > 0;

        public bool HasUnansweredQuestions(Identity group)
            => this.Tree.GetGroup(group)?.HasUnansweredQuestions() ?? false;

        public IEnumerable<Identity> GetCommentedBySupervisorQuestionsVisibleToInterviewer()
        {
            var allCommentedQuestions = this.GetCommentedBySupervisorNonResolvedQuestions();
            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();

            return allCommentedQuestions.Where(identity => questionnaire.IsInterviewierQuestion(identity.Id));
        }

        public IEnumerable<Identity> GetCommentedBySupervisorNonResolvedQuestions()
        {
            return this.Tree.FindQuestions()
                .Where(this.IsEnabledWithSupervisorNonResolvedComments)
                .Select(x => new
                {
                    Id = x.Identity,
                    HasInterviewerReply = this.HasSupervisorCommentInterviewerReply(x)
                })
                .OrderBy(x => x.HasInterviewerReply)
                .Select(x => x.Id);
        }

        public IEnumerable<Identity> GetAllCommentedEnabledQuestions()
        {
            return this.Tree.FindQuestions()
                .Where(question => !question.IsDisabled() && question.AnswerComments.Any() && question.AnswerComments.Any(x => !x.Resolved))
                .Select(x => x.Identity);
        }

        private bool IsEnabledWithSupervisorNonResolvedComments(InterviewTreeQuestion question)
            => !question.IsDisabled() &&
               question.AnswerComments.Any(y => y.UserId != this.properties.InterviewerId && question.AnswerComments.Any(x => !x.Resolved));

        public string GetLastSupervisorComment() => this.SupervisorRejectComment;

        private bool HasSupervisorCommentInterviewerReply(InterviewTreeQuestion question)
        {
            var indexOfLastNotInterviewerComment = question.AnswerComments.FindLastIndex(0, x => x.UserId != this.properties.InterviewerId);
            return question.AnswerComments.Skip(indexOfLastNotInterviewerComment + 1).Any();
        }

        public Identity GetParentGroup(Identity groupOrQuestion)
            => this.Tree.GetNodeByIdentity(groupOrQuestion)?.Parent?.Identity;

        public Identity[] GetParentGroups(Identity groupOrQuestion)
            => this.Tree.GetNodeByIdentity(groupOrQuestion)?.Parents?.Select(x => x.Identity).ToArray();

        public IEnumerable<Identity> GetChildQuestions(Identity groupIdentity)
            => this.GetAllChildrenOrEmptyList(groupIdentity)
                .OfType<InterviewTreeQuestion>()
                .Select(question => question.Identity);

        private IEnumerable<IInterviewTreeNode> GetAllChildrenOrEmptyList(Identity groupIdentity)
            => this.Tree.GetGroup(groupIdentity)?.Children ?? new List<IInterviewTreeNode>();

        public List<Identity> GetRosterInstances(Identity parentIdentity, Guid rosterId)
            => this.GetAllChildrenOrEmptyList(parentIdentity)
                .Where(roster => roster.Identity.Id == rosterId)
                .OfType<InterviewTreeRoster>()
                .OrderBy(roster => roster.SortIndex)
                .Select(roster => roster.Identity)
                .ToList();

        public IEnumerable<Identity> GetEnabledSubgroupsAndRosters(Identity group)
            => this.GetGroupsAndRostersInGroup(group)
                .Where(groupOrRoster => !groupOrRoster.IsDisabled())
                .Select(x => x.Identity);

        private IEnumerable<InterviewTreeGroup> GetGroupsAndRostersInGroup(Identity group)
            => this.Tree.GetGroup(group)?.Children?.OfType<InterviewTreeGroup>() ?? new InterviewTreeGroup[0];

        public IEnumerable<InterviewTreeGroup> GetAllGroupsAndRosters()
            => this.Tree.GetAllNodesInEnumeratorOrder().OfType<InterviewTreeGroup>();

        public IEnumerable<IInterviewTreeNode> GetAllInterviewNodes()
        {
            return this.Tree.GetAllNodesInEnumeratorOrder();
        }

        public Guid CurrentResponsibleId
        {
            get
            {
                var result = this.properties.InterviewerId ?? this.properties.SupervisorId;
                if (result == null) throw new InterviewException($"Interview has no responsible assigned. Interview key: {this.interviewKey}");
                return result.Value;
            }
        }

        public Guid SupervisorId
        {
            get
            {
                if (!this.properties.SupervisorId.HasValue)
                    throw new InterviewException($"Interview has no supervisor assigned. Interview key: {this.interviewKey}");
                return this.properties.SupervisorId.Value;
            }
        }

        public IEnumerable<InterviewTreeGroup> GetAllEnabledGroupsAndRosters()
            => this.Tree.GetAllNodesInEnumeratorOrder().OfType<InterviewTreeGroup>().Where(group => !group.IsDisabled());

        public bool IsEntityValid(Identity identity)
        {
            var question = this.Tree.GetQuestion(identity);
            if (question != null)
            {
                return !question.IsAnswered() || question.IsValid;
            }
            var staticText = this.Tree.GetStaticText(identity);
            return staticText?.IsValid ?? false;
        }

        public bool IsEntityPlausible(Identity identity)
        {
            var question = this.Tree.GetQuestion(identity);
            if (question != null)
            {
                return !question.IsAnswered() || question.IsPlausible;
            }
            var staticText = this.Tree.GetStaticText(identity);
            return staticText?.IsPlausible ?? false;
        }

        public IEnumerable<string> GetFailedWarningMessages(Identity questionOrStaticTextId,
            string defaltErrorMessageFallback)
        {
            var question = this.Tree.GetQuestion(questionOrStaticTextId);
            if (question?.FailedWarnings != null)
            {
                return GetValidationMessages(question.FailedWarnings, question.ValidationMessages, defaltErrorMessageFallback);
            }

            var staticText = this.Tree.GetStaticText(questionOrStaticTextId);
            if (staticText?.FailedWarnings != null)
            {
                return GetValidationMessages(staticText.FailedWarnings, staticText.ValidationMessages,
                    defaltErrorMessageFallback);
            }

            return Enumerable.Empty<string>();
        }

        public IEnumerable<string> GetFailedValidationMessages(Identity questionOrStaticTextId, string defaltErrorMessageFallback)
        {
            var question = this.Tree.GetQuestion(questionOrStaticTextId);
            IReadOnlyList<FailedValidationCondition> questionFailedErrorValidations = question?.FailedErrors;
            if (questionFailedErrorValidations != null)
            {
                return GetValidationMessages(questionFailedErrorValidations, question.ValidationMessages, defaltErrorMessageFallback);
            }

            var staticText = this.Tree.GetStaticText(questionOrStaticTextId);
            if (staticText?.FailedErrors != null)
            {
                return GetValidationMessages(staticText.FailedErrors, staticText.ValidationMessages,
                    defaltErrorMessageFallback);
            }

            return Enumerable.Empty<string>();
        }

        private IEnumerable<string> GetValidationMessages(IReadOnlyList<FailedValidationCondition> failedConditions,
            SubstitutionText[] texts,
            string defalutErrorMessageFallback)
        {
            var questionValidationMassages = texts
                .Select(substitutionText => string.IsNullOrWhiteSpace(substitutionText.BrowserReadyText)
                    ? defalutErrorMessageFallback
                    : substitutionText.BrowserReadyText)
                .ToList();

            if (failedConditions.Count > 0 && questionValidationMassages.Count == 1) return new[] { questionValidationMassages[0] };

            return failedConditions.Select(failedValidation =>
               $"{questionValidationMassages.ElementAt(failedValidation.FailedConditionIndex)} [{failedValidation.FailedConditionIndex + 1}]");
        }

        public bool IsEnabled(Identity entityIdentity)
        {
            var node = this.Tree.GetNodeByIdentity(entityIdentity);
            // Not being disposed of ViewModels can try to update their state, but they can be removed from the tree already if roster was removed.
            if (node == null) return false;
            return !node.IsDisabled();
        }

        [Obsolete("Replaced with HasEditableIdentifyingQuestions")]
        public bool CreatedOnClient { get; } = false;

        public bool WasAnswered(Identity entityIdentity)
        {
            var question = this.Tree.GetQuestion(entityIdentity);

            return question != null && !question.IsDisabled() && question.IsAnswered();
        }

        public List<AnswerComment> GetQuestionComments(Identity entityIdentity, bool includeResolved = false)
            => this.Tree.GetQuestion(entityIdentity).AnswerComments.Where(x => includeResolved || !x.Resolved).ToList();

        List<CategoricalOption> IStatefulInterview.GetTopFilteredOptionsForQuestion(Identity question,
            int? parentQuestionValue, string filter, int sliceSize, int[] excludedOptionIds)
            => this.GetFirstTopFilteredOptionsForQuestion(question, parentQuestionValue, filter, sliceSize, excludedOptionIds);

        public List<CategoricalOption> GetFirstTopFilteredOptionsForQuestion(Identity questionIdentity,
            int? parentQuestionValue, string filter, int itemsCount = 200, int[] excludedOptionIds = null)
        {
            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();

            if (questionnaire.IsLinkedToListQuestion(questionIdentity.Id))
            {
                return OptionsForLinkedToTextListQuestion(questionIdentity)
                    .Where(x => filter.IsNullOrEmpty() || x.Title.Contains(filter, StringComparison.CurrentCultureIgnoreCase))
                    .Where(x => excludedOptionIds == null || !excludedOptionIds.Contains(x.Value))
                    .Take(itemsCount)
                    .ToList();
            }

            var options = questionnaire.GetOptionsForQuestion(questionIdentity.Id, parentQuestionValue, filter, excludedOptionIds);

            if (!questionnaire.IsSupportFilteringForOptions(questionIdentity.Id))
                return options.Take(itemsCount).ToList();

            return this.FilteredCategoricalOptions(questionIdentity, itemsCount, options);
        }

        private IEnumerable<CategoricalOption> OptionsForLinkedToTextListQuestion(Identity questionId)
        {
            var questionnaire = GetQuestionnaireOrThrow(Language);
            var linkedToQuestionId = questionnaire.GetQuestionReferencedByLinkedQuestion(questionId.Id);
            var listQuestion = FindQuestionInQuestionBranch(linkedToQuestionId, questionId);
            if (listQuestion == null || listQuestion.IsDisabled()) yield break;

            var asInterviewTreeTextListQuestion = listQuestion.GetAsInterviewTreeTextListQuestion();
            var listOptions = asInterviewTreeTextListQuestion.GetAnswer()?.Rows;
            var filteredOptions = GetSingleOptionLinkedToListQuestion(questionId)?.Options;

            if (listOptions == null || filteredOptions == null) yield break;

            foreach (var textListAnswerRow in listOptions)
            {
                if (filteredOptions.Contains(textListAnswerRow.Value))
                    yield return new CategoricalOption
                    {
                        Title = textListAnswerRow.Text,
                        Value = textListAnswerRow.Value
                    };
            }
        }

        public bool DoesCascadingQuestionHaveMoreOptionsThanThreshold(Identity questionIdentity, int threshold)
        {
            var question = this.GetCascadingQuestion(questionIdentity);
            if (question == null)
                return false;
            var parentQuestion = question.GetCascadingParentQuestion();
            if (!parentQuestion.IsAnswered())
                return false;

            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();
            var optionsCount = questionnaire.GetOptionsForQuestion(questionIdentity.Id, parentQuestion.GetAnswer().SelectedValue, null, null).Take(threshold + 1).Count();

            if (optionsCount > threshold)
                return true;
            return false;
        }

        CategoricalOption IStatefulInterview.GetOptionForQuestionWithoutFilter(Identity question, int value,
            int? parentQuestionValue) => this.GetOptionForQuestionWithoutFilter(question, value, parentQuestionValue);

        public CategoricalOption GetOptionForQuestionWithoutFilter(Identity question, int value, int? parentQuestionValue = null)
        {
            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();
            if (questionnaire.IsLinkedToListQuestion(question.Id))
            {
                var linkedQuestion = GetSingleOptionLinkedToListQuestion(question);
                var answer = linkedQuestion.GetAnswer();
                if (answer == null) return null;


                var listQuestion = FindQuestionInQuestionBranch(linkedQuestion.LinkedSourceId, question)
                    .GetAsInterviewTreeTextListQuestion();

                var linkedToText = listQuestion.GetTitleByItemCode(answer.SelectedValue);

                return new CategoricalOption
                {
                    Title = linkedToText,
                    Value = answer.SelectedValue
                };
            }

            return questionnaire.GetOptionForQuestionByOptionValue(question.Id, value, parentQuestionValue);
        }

        CategoricalOption IStatefulInterview.GetOptionForQuestionWithFilter(Identity question, string value,
            int? parentQuestionValue) => this.GetOptionForQuestionWithFilter(question, value, parentQuestionValue);

        public CategoricalOption GetOptionForQuestionWithFilter(Identity question, string optionText, int? parentQuestionValue = null)
        {
            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();
            if (questionnaire.IsLinkedToListQuestion(question.Id))
            {
                return GetFirstTopFilteredOptionsForQuestion(question, null, optionText, 1, Array.Empty<int>())
                    .FirstOrDefault();
            }

            CategoricalOption filteredOption = questionnaire.GetOptionForQuestionByOptionText(question.Id, optionText, parentQuestionValue);

            if (filteredOption == null)
                return null;

            if (questionnaire.IsSupportFilteringForOptions(question.Id))
            {
                return FilteredCategoricalOptions(question, 1, filteredOption.ToEnumerable()).SingleOrDefault();
            }

            return filteredOption;
        }

        private static AnswerComment ToAnswerComment(AnsweredQuestionSynchronizationDto answerDto, CommentSynchronizationDto commentDto)
            => new AnswerComment(
                userId: commentDto.UserId,
                userRole: commentDto.UserRole,
                commentTime: commentDto.Date,
                comment: commentDto.Text,
                questionIdentity: Identity.Create(answerDto.Id, answerDto.QuestionRosterVector),
                commentDto.Id,
                false);

        public bool AcceptsInterviewerAnswers()
        {
            return !IsDeleted && (Status == InterviewStatus.InterviewerAssigned || Status == InterviewStatus.Restarted || Status == InterviewStatus.RejectedBySupervisor);
        }

        public bool AcceptsCAWIAnswers()
        {
            return !IsDeleted && properties.AcceptsCAWIAnswers;
        }

        public IReadOnlyCollection<IInterviewTreeNode> GetAllSections()
        {
            return this.Tree.Sections;
        }

        public bool IsReadOnlyQuestion(Identity identity)
        {
            return Tree.GetQuestion(identity).IsReadonly;
        }

        public InterviewKey GetInterviewKey()
        {
            return this.interviewKey;
        }

        public int? GetAssignmentId()
        {
            return this.properties.AssignmentId;
        }

        public bool? GetIsAudioRecordingEnabled()
        {
            return this.properties.IsAudioRecordingEnabled;
        }

        public Guid? GetAttachmentForEntity(Identity entityId)
        {
            var questionnaire = this.GetQuestionnaireOrThrow(this.Language);
            string attachmentName = questionnaire.GetAttachmentNameForEntity(entityId.Id);

            if (questionnaire.HasVariable(attachmentName))
            {
                var staticText = this.Tree.GetStaticText(entityId);

                Guid attachedVariable = questionnaire.GetVariableIdByVariableName(attachmentName);
                var interviewTreeGroup = (InterviewTreeGroup)staticText.Parent;
                InterviewTreeVariable variable = interviewTreeGroup.GetVariableFromThisOrUpperLevel(attachedVariable);
                var attachmentNameFromInterview = (string)variable?.Value;
                return attachmentNameFromInterview == null ? null : questionnaire.GetAttachmentIdByName(attachmentNameFromInterview);
            }

            return questionnaire.GetAttachmentIdByName(attachmentName);
        }

        public InterviewSimpleStatus GetInterviewSimpleStatus(bool includingSupervisorEntities)
        {
            int invalidEntities = includingSupervisorEntities
                ? this.CountInvalidEntitiesInInterviewForSupervisor()
                : this.CountInvalidEntitiesInInterview();

            int activeQuestionsCount = includingSupervisorEntities
                ? this.CountActiveQuestionsInInterviewForSupervisor()
                : this.CountActiveQuestionsInInterview();

            int answeredQuestionsCount = includingSupervisorEntities
                ? this.CountActiveAnsweredQuestionsInInterviewForSupervisor()
                : this.CountActiveAnsweredQuestionsInInterview();

            var simpleStatus = (invalidEntities > 0)
                ? SimpleGroupStatus.Invalid
                : ((activeQuestionsCount == answeredQuestionsCount)
                    ? SimpleGroupStatus.Completed
                    : SimpleGroupStatus.Other);

            var status = GetGroupStatus(simpleStatus, activeQuestionsCount, answeredQuestionsCount);

            return new InterviewSimpleStatus()
            {
                Status = status,
                SimpleStatus = simpleStatus,
                ActiveQuestionCount = activeQuestionsCount,
                AnsweredQuestionsCount = answeredQuestionsCount
            };
        }

        private GroupStatus GetGroupStatus(SimpleGroupStatus simpleStatus, int questionsCount, int answeredQuestionsCount)
        {
            switch (simpleStatus)
            {
                case SimpleGroupStatus.Completed:
                    return GroupStatus.Completed;

                case SimpleGroupStatus.Invalid:
                    return questionsCount == answeredQuestionsCount ? GroupStatus.CompletedInvalid : GroupStatus.StartedInvalid;

                case SimpleGroupStatus.Other:
                    return answeredQuestionsCount > 0 ? GroupStatus.Started : GroupStatus.NotStarted;

                default:
                    return GroupStatus.Started;
            }			
        }

        public bool IsParentOf(Identity parentIdentity, Identity childIdentity)
        {
            if ((parentIdentity ?? childIdentity) == null)
                return false;

            var childNode = this.Tree.GetNodeByIdentity(childIdentity);

            return childNode != null && childNode.Parents.Select(x => x.Identity).Any(x => x.Equals(parentIdentity));
        }

        /// <summary>
        /// Timespan within which two pause/resume sessions should be merged as a single one
        /// </summary>
        private readonly TimeSpan pauseResumeQuiteWindow = TimeSpan.FromMinutes(1);

        public void Resume(ResumeInterviewCommand command)
        {
            DateTimeOffset? lastResume = this.properties.LastResumed;
            if (lastResume.HasValue)
            {
                if (command.OriginDate - lastResume < pauseResumeQuiteWindow)
                {
                    return;
                }

                DateTimeOffset closePreviousNonClosedSessionDate =
                    lastResume.Value.AddMinutes(15);

                if (command.OriginDate < closePreviousNonClosedSessionDate)
                {
                    closePreviousNonClosedSessionDate = command.OriginDate;
                }

                if (this.properties.LastAnswerDate > closePreviousNonClosedSessionDate)
                {
                    closePreviousNonClosedSessionDate = this.properties.LastAnswerDate.Value;
                }

                ApplyEvent(new InterviewPaused(command.UserId, closePreviousNonClosedSessionDate));
            }

            ApplyEvent(new InterviewResumed(command.UserId, command.OriginDate, command.DeviceType));
        }

        public void Pause(PauseInterviewCommand command)
        {
            DateTimeOffset? lastOpen = this.properties.LastResumed;
            if (lastOpen.HasValue)
            {
                TimeSpan? afterLastResumeEvent = command.OriginDate - lastOpen;
                if (afterLastResumeEvent < pauseResumeQuiteWindow)
                {
                    return;
                }
            }
            else
            {
                return;
            }

            ApplyEvent(new InterviewPaused(command.UserId, command.OriginDate));
        }

        public void CloseBySupervisor(CloseInterviewBySupervisorCommand command)
        {
            DateTimeOffset? lastResume = this.properties.LastOpenedBySupervisor;
            if (lastResume.HasValue)
            {
                TimeSpan? afterLastResumeEvent = command.OriginDate - lastResume;
                if (afterLastResumeEvent < pauseResumeQuiteWindow)
                {
                    return;
                }
            }
            else
            {
                return;
            }

            ApplyEvent(new InterviewClosedBySupervisor(command.UserId, command.OriginDate));
        }

        public void OpenBySupervisor(OpenInterviewBySupervisorCommand command)
        {
            DateTimeOffset? lastOpenDate = this.properties.LastOpenedBySupervisor;
            if (lastOpenDate.HasValue)
            {
                if (command.OriginDate - lastOpenDate < pauseResumeQuiteWindow)
                {
                    return;
                }

                DateTimeOffset closePreviousNonClosedSessionDate =
                    lastOpenDate.Value.AddMinutes(15);

                if (command.OriginDate < closePreviousNonClosedSessionDate)
                {
                    closePreviousNonClosedSessionDate = command.OriginDate;
                }

                ApplyEvent(new InterviewClosedBySupervisor(command.UserId, closePreviousNonClosedSessionDate));
            }

            ApplyEvent(new InterviewOpenedBySupervisor(command.UserId, command.OriginDate));
        }

        private void Apply(InterviewPaused @event)
        {
            this.properties.LastPaused = @event.OriginDate;
            this.properties.LastResumed = null;
        }

        private void Apply(InterviewResumed @event)
        {
            this.properties.LastResumed = @event.OriginDate;
            this.properties.LastPaused = null;
        }

        private void Apply(InterviewOpenedBySupervisor @event)
        {
            this.properties.LastOpenedBySupervisor = @event.OriginDate;
            this.properties.LastClosedBySupervisor = null;
        }

        private void Apply(InterviewClosedBySupervisor @event)
        {
            this.properties.LastClosedBySupervisor = @event.OriginDate;
            this.properties.LastOpenedBySupervisor = null;
        }
    }
}
