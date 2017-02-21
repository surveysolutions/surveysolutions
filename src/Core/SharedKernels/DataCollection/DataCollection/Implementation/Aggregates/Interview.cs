using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Ncqrs.Domain;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Preloading;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Invariants;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
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

        public Interview(IQuestionnaireStorage questionnaireRepository,
            IInterviewExpressionStatePrototypeProvider expressionProcessorStatePrototypeProvider,
            ISubstitionTextFactory substitionTextFactory)
        {
            this.questionnaireRepository = questionnaireRepository;
            this.expressionProcessorStatePrototypeProvider = expressionProcessorStatePrototypeProvider;
            this.substitionTextFactory = substitionTextFactory;
        }

        #region StaticMethods

        private static AnswerComment ToAnswerComment(CommentSynchronizationDto answerComment,
            AnsweredQuestionSynchronizationDto answerDto)
            => new AnswerComment(answerComment.UserId, answerComment.UserRole, answerComment.Date, answerComment.Text,
                Identity.Create(answerDto.Id, answerDto.QuestionRosterVector));

        private static string JoinDecimalsWithComma(IEnumerable<decimal> values) => string.Join(", ", values.Select(value => value.ToString(CultureInfo.InvariantCulture)));

        private static string JoinIntsWithComma(IEnumerable<int> values) => string.Join(", ", values.Select(value => value.ToString(CultureInfo.InvariantCulture)));

        #endregion

        #region Handlers

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
        }

        public void CreateInterview(Guid questionnaireId, long questionnaireVersion, Guid supervisorId,
            Dictionary<Guid, AbstractAnswer> answersToFeaturedQuestions, DateTime answersTime, Guid userId)
        {
            this.QuestionnaireIdentity = new QuestionnaireIdentity(questionnaireId, questionnaireVersion);

            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();

            InterviewTree changedInterviewTree = this.Tree.Clone();

            this.ValidatePrefilledAnswers(this.Tree, questionnaire, answersToFeaturedQuestions, RosterVector.Empty);

            Dictionary<Identity, AbstractAnswer> prefilledQuestionsWithAnswers = answersToFeaturedQuestions.ToDictionary(x => new Identity(x.Key, RosterVector.Empty), x => x.Value);
            foreach (KeyValuePair<Identity, AbstractAnswer> answer in prefilledQuestionsWithAnswers)
            {
                changedInterviewTree.GetQuestion(answer.Key).SetAnswer(answer.Value);
            }

            List<Identity> answeredQuestions = prefilledQuestionsWithAnswers.Keys.ToList();

            changedInterviewTree.ActualizeTree();

            this.UpdateTreeWithDependentChanges(changedInterviewTree, answeredQuestions, questionnaire);
            IReadOnlyCollection<InterviewTreeNodeDiff> treeDifference = FindDifferenceBetweenTrees(this.Tree, changedInterviewTree);

            //apply events
            this.ApplyEvent(new InterviewCreated(userId, questionnaireId, questionnaire.Version));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.Created, comment: null));

            this.ApplyEvents(treeDifference, userId);

            this.ApplyEvent(new SupervisorAssigned(userId, supervisorId));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.SupervisorAssigned, comment: null));
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

            this.ApplyEvent(new TranslationSwitched(command.Language, command.UserId));
            this.ApplyEvents(treeDifference, command.UserId);
        }

        public void CommentAnswer(Guid userId, Guid questionId, RosterVector rosterVector, DateTime commentTime, string comment)
        {
            new InterviewPropertiesInvariants(this.properties)
                .RequireAnswerCanBeChanged();

            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();

            new InterviewQuestionInvariants(new Identity(questionId, rosterVector), questionnaire, this.Tree)
                .RequireQuestion()
                .RequireQuestionInstanceExists();

            this.ApplyEvent(new AnswerCommented(userId, questionId, rosterVector, commentTime, comment));
        }

        public void SetFlagToAnswer(Guid userId, Guid questionId, RosterVector rosterVector)
        {
            new InterviewPropertiesInvariants(this.properties)
                .RequireAnswerCanBeChanged();

            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();

            new InterviewQuestionInvariants(new Identity(questionId, rosterVector), questionnaire, this.Tree)
                .RequireQuestion()
                .RequireQuestionInstanceExists();

            this.ApplyEvent(new FlagSetToAnswer(userId, questionId, rosterVector));
        }

        public void RemoveFlagFromAnswer(Guid userId, Guid questionId, RosterVector rosterVector)
        {
            new InterviewPropertiesInvariants(this.properties)
                .RequireAnswerCanBeChanged();

            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();

            new InterviewQuestionInvariants(new Identity(questionId, rosterVector), questionnaire, this.Tree)
                .RequireQuestion()
                .RequireQuestionInstanceExists();

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

        public void SynchronizeInterviewEvents(Guid userId, Guid questionnaireId, long questionnaireVersion,
            InterviewStatus interviewStatus, IEvent[] synchronizedEvents, bool createdOnClient)
        {
            this.QuestionnaireIdentity = new QuestionnaireIdentity(questionnaireId, questionnaireVersion);

            InterviewPropertiesInvariants propertiesInvariants = new InterviewPropertiesInvariants(this.properties);

            bool isInterviewNeedToBeCreated = createdOnClient && this.Version == 0;

            if (isInterviewNeedToBeCreated)
            {
                this.ApplyEvent(new InterviewOnClientCreated(userId, questionnaireId, questionnaireVersion));
            }
            else
            {
                propertiesInvariants.ThrowIfOtherInterviewerIsResponsible(userId);

                if (this.properties.Status == InterviewStatus.Deleted)
                    this.Restore(userId);
                else
                    propertiesInvariants.ThrowIfStatusNotAllowedToBeChangedWithMetadata(interviewStatus);
            }

            foreach (IEvent synchronizedEvent in synchronizedEvents)
            {
                this.ApplyEvent(synchronizedEvent);
            }

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
                        roster.Identity.RosterVector.Last());

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

        protected static void CalculateLinkedToListOptionsOnTree(InterviewTree tree)
        {
            IEnumerable<InterviewTreeQuestion> linkedToListQuestions = tree.FindQuestions().Where(x => x.IsLinkedToListQuestion);
            foreach (InterviewTreeQuestion linkedQuestion in linkedToListQuestions)
            {
                linkedQuestion.CalculateLinkedToListOptions();
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