using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Ncqrs.Domain;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.CustomCollections;
using WB.Core.Infrastructure.EventBus;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
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

        protected readonly InterviewEntities.InterviewProperties properties = new InterviewEntities.InterviewProperties();

        protected Guid questionnaireId;
        protected long questionnaireVersion;
        protected string language;

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
                    this.expressionProcessorStatePrototype = this.expressionProcessorStatePrototypeProvider.GetExpressionState(this.questionnaireId, this.questionnaireVersion);
                    this.expressionProcessorStatePrototype.SetInterviewProperties(new InterviewProperties(EventSourceId));
                }

                return this.expressionProcessorStatePrototype;
            }

            set
            {
                expressionProcessorStatePrototype = value;
            }
        }

        protected InterviewStateDependentOnAnswers interviewState = new InterviewStateDependentOnAnswers();

        /// <remarks>
        /// Repository operations are time-consuming.
        /// So this repository may be used only in command handlers.
        /// And should never be used in event handlers!!
        /// </remarks>
        private readonly IQuestionnaireStorage questionnaireRepository;

        private readonly IInterviewExpressionStatePrototypeProvider expressionProcessorStatePrototypeProvider;


        public Interview(IQuestionnaireStorage questionnaireRepository, IInterviewExpressionStatePrototypeProvider expressionProcessorStatePrototypeProvider)
        {
            this.questionnaireRepository = questionnaireRepository;
            this.expressionProcessorStatePrototypeProvider = expressionProcessorStatePrototypeProvider;
        }

        private void SetQuestionnaireProperties(Guid questionnaireId, long questionnaireVersion)
        {
            this.questionnaireId = questionnaireId;
            this.questionnaireVersion = questionnaireVersion;
        }
        
        #region StaticMethods

        private static ConcurrentDictionary<string, ConcurrentDistinctList<decimal>> BuildRosterInstanceIdsFromSynchronizationDto(InterviewSynchronizationDto synchronizationDto)
        {
            return synchronizationDto.RosterGroupInstances.ToConcurrentDictionary(
                pair => ConversionHelper.ConvertIdAndRosterVectorToString(pair.Key.Id, pair.Key.InterviewItemRosterVector),
                pair => new ConcurrentDistinctList<decimal>(pair.Value.Select(rosterInstance => rosterInstance.RosterInstanceId).ToList()));
        }

        /// <remarks>
        /// If roster vector should be extended, result will be a set of vectors depending on roster count of corresponding groups.
        /// </remarks>
        protected static IEnumerable<RosterVector> ExtendRosterVector(IReadOnlyInterviewStateDependentOnAnswers state, RosterVector rosterVector, int length, Guid[] rosterGroupsStartingFromTop)
        {
            if (length < rosterVector.Length)
                throw new ArgumentException(string.Format(
                    "Cannot extend vector with length {0} to smaller length {1}.", rosterVector.Length, length));

            if (length == rosterVector.Length)
            {
                yield return rosterVector;
                yield break;
            }

            var outerVectorsForExtend = GetOuterVectorForParentRoster(state, rosterGroupsStartingFromTop, rosterVector);

            foreach (var outerVectorForExtend in outerVectorsForExtend)
            {
                IEnumerable<decimal> rosterInstanceIds = state.GetRosterInstanceIds(rosterGroupsStartingFromTop.Last(), outerVectorForExtend);
                foreach (decimal rosterInstanceId in rosterInstanceIds)
                {
                    yield return ((RosterVector)outerVectorForExtend).ExtendWithOneCoordinate(rosterInstanceId);
                }
            }
        }

        private static IEnumerable<decimal[]> GetOuterVectorForParentRoster(IReadOnlyInterviewStateDependentOnAnswers state,
            Guid[] rosterGroupsStartingFromTop, RosterVector rosterVector)
        {
            if (rosterGroupsStartingFromTop.Length <= 1 || rosterGroupsStartingFromTop.Length - 1 == rosterVector.Length)
            {
                yield return rosterVector;
                yield break;
            }

            var indexOfPreviousRoster = rosterGroupsStartingFromTop.Length - 2;

            var previousRoster = rosterGroupsStartingFromTop[rosterVector.Length];
            var previousRosterInstances = state.GetRosterInstanceIds(previousRoster, rosterVector);
            foreach (var previousRosterInstance in previousRosterInstances)
            {
                var extendedRoster = rosterVector.ExtendWithOneCoordinate(previousRosterInstance);
                if (indexOfPreviousRoster == rosterVector.Length)
                {
                    yield return extendedRoster;
                    continue;
                }
                foreach (var nextVector in GetOuterVectorForParentRoster(state, rosterGroupsStartingFromTop, extendedRoster))
                {
                    yield return nextVector;
                }
            }
        }

        private static string JoinDecimalsWithComma(IEnumerable<decimal> values)
        {
            return string.Join(", ", values.Select(value => value.ToString(CultureInfo.InvariantCulture)));
        }

        private static ConcurrentHashSet<string> ToHashSetOfIdAndRosterVectorStrings(IEnumerable<InterviewItemId> synchronizationIdentities)
        {
            return new ConcurrentHashSet<string>(
                synchronizationIdentities.Select(
                    question => ConversionHelper.ConvertIdAndRosterVectorToString(question.Id, question.InterviewItemRosterVector)));
        }
        
        private static Identity GetInstanceOfQuestionWithSameAndUpperRosterLevelOrThrow(Guid questionId,
            RosterVector rosterVector, IQuestionnaire questionnare)
        {
            int vectorRosterLevel = rosterVector.Length;
            int questionRosterLevel = questionnare.GetRosterLevelForQuestion(questionId);

            if (questionRosterLevel > vectorRosterLevel)
                throw new InterviewException(string.Format(
                    "Question {0} expected to have roster level not deeper than {1} but it is {2}.",
                    FormatQuestionForException(questionId, questionnare), vectorRosterLevel, questionRosterLevel));

            decimal[] questionRosterVector = rosterVector.Shrink(questionRosterLevel);

            return new Identity(questionId, questionRosterVector);
        }

        protected IEnumerable<Identity> GetInstancesOfEntitiesWithSameAndDeeperRosterLevelOrThrow(
            IReadOnlyInterviewStateDependentOnAnswers state,
            IEnumerable<Guid> entityIds, RosterVector rosterVector, IQuestionnaire questionnare)
        {
            return entityIds.SelectMany(entityId =>
                GetInstancesOfEntitiesWithSameAndDeeperRosterLevelOrThrow(state, entityId, rosterVector, questionnare));
        }

        protected IEnumerable<Identity> GetInstancesOfEntitiesWithSameAndDeeperRosterLevelOrThrow(
            IReadOnlyInterviewStateDependentOnAnswers state,
            Guid entityId,
            RosterVector rosterVector,
            IQuestionnaire questionnare)
        {
            int vectorRosterLevel = rosterVector.Length;
            int entityRosterLevel = questionnare.GetRosterLevelForEntity(entityId);

            if (entityRosterLevel < vectorRosterLevel)
                throw new InterviewException(string.Format(
                    "Entity {0} expected to have roster level not upper than {1} but it is {2}. InterviewId: {3}",
                    FormatQuestionForException(entityId, questionnare), vectorRosterLevel, entityRosterLevel, EventSourceId));

            Guid[] parentRosterGroupsStartingFromTop =
                questionnare.GetRostersFromTopToSpecifiedEntity(entityId).ToArray();

            IEnumerable<RosterVector> entityRosterVectors = ExtendRosterVector(state,
                rosterVector, entityRosterLevel, parentRosterGroupsStartingFromTop);

            return entityRosterVectors.Select(entityRosterVector => new Identity(entityId, entityRosterVector));
        }

        protected IEnumerable<Identity> GetInstancesOfGroupsWithSameAndDeeperRosterLevelOrThrow(IReadOnlyInterviewStateDependentOnAnswers state,
            IEnumerable<Guid> groupIds, RosterVector rosterVector, IQuestionnaire questionnaire)
        {
            return groupIds.SelectMany(groupId =>
                GetInstancesOfGroupsByGroupIdWithSameAndDeeperRosterLevelOrThrow(state, groupId, rosterVector, questionnaire));
        }

        protected IEnumerable<Identity> GetInstancesOfGroupsByGroupIdWithSameAndDeeperRosterLevelOrThrow(IReadOnlyInterviewStateDependentOnAnswers state,
            Guid groupId, RosterVector rosterVector, IQuestionnaire questionnaire)
        {
            int vectorRosterLevel = rosterVector.Length;
            int groupRosterLevel = questionnaire.GetRosterLevelForGroup(groupId);

            if (groupRosterLevel < vectorRosterLevel)
                throw new InterviewException(string.Format(
                    "Question {0} expected to have roster level not upper than {1} but it is {2}. InterviewId: {3}",
                    FormatQuestionForException(groupId, questionnaire), vectorRosterLevel, groupRosterLevel, EventSourceId));

            Guid[] parentRosterGroupsStartingFromTop = questionnaire.GetRostersFromTopToSpecifiedGroup(groupId).ToArray();

            IEnumerable<RosterVector> groupRosterVectors = ExtendRosterVector(state,
                rosterVector, groupRosterLevel, parentRosterGroupsStartingFromTop);

            return groupRosterVectors.Select(groupRosterVector => new Identity(groupId, groupRosterVector));
        }

        protected Identity GetInstanceOfGroupWithSameAndUpperRosterLevelOrThrow(Guid groupId, RosterVector rosterVector, IQuestionnaire questionnaire)
        {
            int vectorRosterLevel = rosterVector.Length;

            int groupRosterLevel = questionnaire.GetRosterLevelForGroup(groupId);

            if (groupRosterLevel > vectorRosterLevel)
                throw new InterviewException(string.Format(
                    "Group {0} expected to have roster level not deeper than {1} but it is {2}. InterviewId: {3}",
                    FormatGroupForException(groupId, questionnaire), vectorRosterLevel, groupRosterLevel, this.EventSourceId));

            decimal[] groupRosterVector = rosterVector.Shrink(groupRosterLevel);

            return new Identity(groupId, groupRosterVector);
        }

        #endregion

        #region Handlers

        public void CreateInterviewWithPreloadedData(CreateInterviewWithPreloadedData command)
        {
            this.SetQuestionnaireProperties(command.QuestionnaireId, command.Version);

            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow(command.QuestionnaireId, command.Version, language: null);

            var state = new InterviewStateDependentOnAnswers();

            var sourceInterviewTree = this.BuildInterviewTree(questionnaire, state);
            var changedInterviewTree = sourceInterviewTree.Clone();

            var orderedData = command.PreloadedData.Data.OrderBy(x => x.RosterVector.Length).ToArray();
            var changedQuestionIdentities = orderedData.SelectMany(x => x.Answers.Select(y => new Identity(y.Key, x.RosterVector))).ToList();

            foreach (var preloadedLevel in orderedData)
            {
                var answersToFeaturedQuestions = preloadedLevel.Answers;

                this.ValidatePrefilledQuestions(sourceInterviewTree, questionnaire, answersToFeaturedQuestions, preloadedLevel.RosterVector, state, false);

                var prefilledQuestionsWithAnswers = answersToFeaturedQuestions.ToDictionary(
                    answersToFeaturedQuestion => new Identity(answersToFeaturedQuestion.Key, preloadedLevel.RosterVector),
                    answersToFeaturedQuestion => answersToFeaturedQuestion.Value);

                foreach (var answer in prefilledQuestionsWithAnswers)
                {
                    changedInterviewTree.GetQuestion(answer.Key).SetAnswer(answer.Value);
                }
                changedInterviewTree.ActualizeTree();
            }

            //apply events
            this.ApplyEvent(new InterviewFromPreloadedDataCreated(command.UserId, questionnaireId, questionnaire.Version));

            this.ApplyTreeDiffChanges(userId: command.UserId, questionnaire: questionnaire,
                sourceInterviewTree: sourceInterviewTree, changedInterviewTree: changedInterviewTree,
                changedQuestionIdentities: changedQuestionIdentities);

            this.ApplyEvent(new SupervisorAssigned(command.UserId, command.SupervisorId));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.SupervisorAssigned, comment: null));

            if (command.InterviewerId.HasValue)
            {
                this.ApplyEvent(new InterviewerAssigned(command.UserId, command.InterviewerId.Value, command.AnswersTime));
                this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.InterviewerAssigned, comment: null));
            }
        }

        public void CreateInterview(Guid questionnaireId, long questionnaireVersion, Guid supervisorId,
            Dictionary<Guid, object> answersToFeaturedQuestions, DateTime answersTime, Guid userId)
        {
            this.SetQuestionnaireProperties(questionnaireId, questionnaireVersion);

            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow(questionnaireId, questionnaireVersion, language: null);

            var state = new InterviewStateDependentOnAnswers();

            var sourceInterviewTree = this.BuildInterviewTree(questionnaire, state);
            this.ValidatePrefilledQuestions(sourceInterviewTree, questionnaire, answersToFeaturedQuestions, RosterVector.Empty, state);
            
            var changedInterviewTree = sourceInterviewTree.Clone();

            var prefilledQuestionsWithAnswers = answersToFeaturedQuestions.ToDictionary(x => new Identity(x.Key, RosterVector.Empty), x => x.Value);
            foreach (var answer in prefilledQuestionsWithAnswers)
            {
                changedInterviewTree.GetQuestion(answer.Key).SetAnswer(answer.Value);
            }

            //apply events
            this.ApplyEvent(new InterviewCreated(userId, questionnaireId, questionnaire.Version));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.Created, comment: null));

            changedInterviewTree.ActualizeTree();
            
            this.ApplyTreeDiffChanges(userId: userId, questionnaire: questionnaire,
                sourceInterviewTree: sourceInterviewTree, changedInterviewTree: changedInterviewTree,
                changedQuestionIdentities: prefilledQuestionsWithAnswers.Keys.ToList());

            this.ApplyEvent(new SupervisorAssigned(userId, supervisorId));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.SupervisorAssigned, comment: null));
        }

        public void CreateInterviewOnClient(QuestionnaireIdentity questionnaireIdentity, Guid supervisorId, DateTime answersTime, Guid userId)
        {
            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version, language: null);
            this.SetQuestionnaireProperties(questionnaireIdentity.QuestionnaireId, questionnaire.Version);

            var state = new InterviewStateDependentOnAnswers();

            var sourceInterviewTree = this.BuildInterviewTree(questionnaire, state);
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

        //todo should respect changes calculated in ExpressionState
        public void ReevaluateSynchronizedInterview()
        {
            var propertiesInvariants = new InterviewPropertiesInvariants(this.properties);

            propertiesInvariants.ThrowIfInterviewHardDeleted();

            var expressionProcessorState = this.ExpressionProcessorStatePrototype.Clone();

            expressionProcessorState.SaveAllCurrentStatesAsPrevious();

            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion, language: this.language);

            var sourceInterviewTree = this.BuildInterviewTree(questionnaire, this.interviewState);
            var changedInterviewTree = sourceInterviewTree.Clone();

            EnablementChanges enablementChanges = expressionProcessorState.ProcessEnablementConditions();
            this.UpdateTreeWithEnablementChanges(changedInterviewTree, enablementChanges);

            ValidityChanges validationChanges = expressionProcessorState.ProcessValidationExpressions();
            this.UpdateTreeWithValidationChanges(changedInterviewTree, validationChanges);

            this.ApplyEvents(sourceInterviewTree, changedInterviewTree);

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
            var propertiesInvariants = new InterviewPropertiesInvariants(this.properties);

            propertiesInvariants.ThrowIfInterviewHardDeleted();

            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion, this.language);
            IReadOnlyCollection<string> availableLanguages = questionnaire.GetTranslationLanguages();

            if (command.Language != null)
            {
                if (availableLanguages.All(language => language != command.Language))
                    throw new InterviewException(
                        $"Questionnaire does not have translation. Language: {command.Language}. Interview ID: {this.EventSourceId.FormatGuid()}. Questionnaire ID: {new QuestionnaireIdentity(this.questionnaireId, this.questionnaireVersion)}.");
            }

            var targetQuestionnaire = this.GetQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion, command.Language);
            
            this.ApplyEvent(new TranslationSwitched(command.Language, command.UserId));

            var sourceInterviewTree = this.BuildInterviewTree(questionnaire, new InterviewStateDependentOnAnswers());

            var changedInterviewTree = sourceInterviewTree.Clone();

            this.UpdateRosterTitles(changedInterviewTree, targetQuestionnaire);

            this.ApplyEvents(sourceInterviewTree, changedInterviewTree, command.UserId);
        }

        public void CommentAnswer(Guid userId, Guid questionId, RosterVector rosterVector, DateTime commentTime, string comment)
        {
            new InterviewPropertiesInvariants(this.properties).RequireAnswerCanBeChanged();

            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion, this.language);

            var tree = this.BuildInterviewTree(questionnaire, this.interviewState);
            var treeInvariants = new InterviewTreeInvariants(tree);

            this.ThrowIfQuestionDoesNotExist(questionId, questionnaire);
            treeInvariants.RequireRosterVectorQuestionInstanceExists(questionId, rosterVector);

            this.ApplyEvent(new AnswerCommented(userId, questionId, rosterVector, commentTime, comment));
        }

        public void SetFlagToAnswer(Guid userId, Guid questionId, RosterVector rosterVector)
        {
            new InterviewPropertiesInvariants(this.properties).RequireAnswerCanBeChanged();

            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion, this.language);

            var tree = this.BuildInterviewTree(questionnaire, this.interviewState);
            var treeInvariants = new InterviewTreeInvariants(tree);

            this.ThrowIfQuestionDoesNotExist(questionId, questionnaire);
            treeInvariants.RequireRosterVectorQuestionInstanceExists(questionId, rosterVector);

            this.ApplyEvent(new FlagSetToAnswer(userId, questionId, rosterVector));
        }

        public void RemoveFlagFromAnswer(Guid userId, Guid questionId, RosterVector rosterVector)
        {
            new InterviewPropertiesInvariants(this.properties).RequireAnswerCanBeChanged();

            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion, this.language);

            var tree = this.BuildInterviewTree(questionnaire, this.interviewState);
            var treeInvariants = new InterviewTreeInvariants(tree);

            this.ThrowIfQuestionDoesNotExist(questionId, questionnaire);
            treeInvariants.RequireRosterVectorQuestionInstanceExists(questionId, rosterVector);

            this.ApplyEvent(new FlagRemovedFromAnswer(userId, questionId, rosterVector));
        }

        public void AssignSupervisor(Guid userId, Guid supervisorId)
        {
            var propertiesInvariants = new InterviewPropertiesInvariants(this.properties);

            propertiesInvariants.ThrowIfInterviewHardDeleted();
            propertiesInvariants.ThrowIfInterviewStatusIsNotOneOfExpected(InterviewStatus.Created, InterviewStatus.SupervisorAssigned);

            this.ApplyEvent(new SupervisorAssigned(userId, supervisorId));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.SupervisorAssigned, comment: null));
        }

        public void AssignInterviewer(Guid userId, Guid interviewerId, DateTime assignTime)
        {
            var propertiesInvariants = new InterviewPropertiesInvariants(this.properties);

            propertiesInvariants.ThrowIfInterviewHardDeleted();
            propertiesInvariants.ThrowIfInterviewStatusIsNotOneOfExpected(InterviewStatus.SupervisorAssigned, InterviewStatus.InterviewerAssigned, InterviewStatus.RejectedBySupervisor);
            propertiesInvariants.ThrowIfTryAssignToSameInterviewer(interviewerId);

            this.ApplyEvent(new InterviewerAssigned(userId, interviewerId, assignTime));
            if (!this.properties.WasCompleted && this.properties.Status != InterviewStatus.InterviewerAssigned)
            {
                this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.InterviewerAssigned, comment: null));
            }
        }

        public void Delete(Guid userId)
        {
            var propertiesInvariants = new InterviewPropertiesInvariants(this.properties);

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
            var propertiesInvariants = new InterviewPropertiesInvariants(this.properties);

            propertiesInvariants.ThrowIfInterviewHardDeleted();

            this.ApplyEvent(new InterviewReceivedByInterviewer());
        }

        public void Restore(Guid userId)
        {
            var propertiesInvariants = new InterviewPropertiesInvariants(this.properties);

            propertiesInvariants.ThrowIfInterviewHardDeleted();
            propertiesInvariants.ThrowIfInterviewStatusIsNotOneOfExpected(InterviewStatus.Deleted);

            this.ApplyEvent(new InterviewRestored(userId));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.Restored, comment: null));
        }

        public void Restart(Guid userId, string comment, DateTime restartTime)
        {
            var propertiesInvariants = new InterviewPropertiesInvariants(this.properties);

            propertiesInvariants.ThrowIfInterviewHardDeleted();
            propertiesInvariants.ThrowIfInterviewStatusIsNotOneOfExpected(InterviewStatus.Completed);

            this.ApplyEvent(new InterviewRestarted(userId, restartTime, comment));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.Restarted, comment));
        }

        public void Approve(Guid userId, string comment, DateTime approveTime)
        {
            var propertiesInvariants = new InterviewPropertiesInvariants(this.properties);

            propertiesInvariants.ThrowIfInterviewHardDeleted();
            propertiesInvariants.ThrowIfInterviewStatusIsNotOneOfExpected(InterviewStatus.Completed,
                InterviewStatus.RejectedBySupervisor,
                InterviewStatus.RejectedByHeadquarters);

            this.ApplyEvent(new InterviewApproved(userId, comment, approveTime));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.ApprovedBySupervisor, comment));
        }

        public void Reject(Guid userId, string comment, DateTime rejectTime)
        {
            var propertiesInvariants = new InterviewPropertiesInvariants(this.properties);

            propertiesInvariants.ThrowIfInterviewHardDeleted();
            propertiesInvariants.ThrowIfInterviewStatusIsNotOneOfExpected(InterviewStatus.Completed,
                InterviewStatus.ApprovedBySupervisor,
                InterviewStatus.RejectedByHeadquarters);

            this.ApplyEvent(new InterviewRejected(userId, comment, rejectTime));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.RejectedBySupervisor, comment));
        }

        public void HqApprove(Guid userId, string comment)
        {
            var propertiesInvariants = new InterviewPropertiesInvariants(this.properties);

            propertiesInvariants.ThrowIfInterviewHardDeleted();
            propertiesInvariants.ThrowIfInterviewStatusIsNotOneOfExpected(InterviewStatus.ApprovedBySupervisor, InterviewStatus.RejectedByHeadquarters);

            this.ApplyEvent(new InterviewApprovedByHQ(userId, comment));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.ApprovedByHeadquarters, comment));
        }

        public void UnapproveByHeadquarters(Guid userId, string comment)
        {
            var propertiesInvariants = new InterviewPropertiesInvariants(this.properties);

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
                where !this.interviewState.AnswerComments.Contains(new AnswerComment(answerComment.UserId, answerComment.Date, answerComment.Text, answerDto.Id, answerDto.QuestionRosterVector))
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
                this.ApplyEvent(new AnswerCommented(commentedAnswer.UserId, commentedAnswer.QuestionId, commentedAnswer.RosterVector, commentedAnswer.Date, commentedAnswer.Text));
            }
        }

        public void HqReject(Guid userId, string comment)
        {
            var propertiesInvariants = new InterviewPropertiesInvariants(this.properties);

            propertiesInvariants.ThrowIfInterviewHardDeleted();
            propertiesInvariants.ThrowIfInterviewStatusIsNotOneOfExpected(InterviewStatus.ApprovedBySupervisor, InterviewStatus.Deleted);

            this.ApplyEvent(new InterviewRejectedByHQ(userId, comment));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.RejectedByHeadquarters, comment));
        }

        public void SynchronizeInterview(Guid userId, InterviewSynchronizationDto synchronizedInterview)
        {
            var propertiesInvariants = new InterviewPropertiesInvariants(this.properties);

            propertiesInvariants.ThrowIfInterviewHardDeleted();

            this.ApplyEvent(new InterviewSynchronized(synchronizedInterview));
        }

        public void SynchronizeInterviewEvents(Guid userId, Guid questionnaireId, long questionnaireVersion,
            InterviewStatus interviewStatus, IEvent[] synchronizedEvents, bool createdOnClient)
        {
            this.SetQuestionnaireProperties(questionnaireId, questionnaireVersion);

            var propertiesInvariants = new InterviewPropertiesInvariants(this.properties);

            propertiesInvariants.ThrowIfOtherInterviewerIsResponsible(userId);

            this.GetQuestionnaireOrThrow(questionnaireId, questionnaireVersion, language: null);

            var isInterviewNeedToBeCreated = createdOnClient && this.Version == 0;

            if (isInterviewNeedToBeCreated)
            {
                this.ApplyEvent(new InterviewOnClientCreated(userId, questionnaireId, questionnaireVersion));
            }
            else
            {
                if (this.properties.Status == InterviewStatus.Deleted)
                    this.Restore(userId);
                else
                    propertiesInvariants.ThrowIfStatusNotAllowedToBeChangedWithMetadata(interviewStatus);
            }

            foreach (var synchronizedEvent in synchronizedEvents)
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
            this.SetQuestionnaireProperties(questionnaireId, questionnaireVersion);

            var propertiesInvariants = new InterviewPropertiesInvariants(this.properties);

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

        private Dictionary<Identity, RosterVector[]> GetLinkedQuestionOptionsChanges(
            ILatestInterviewExpressionState interviewExpressionState,
            InterviewStateDependentOnAnswers updatedState,
            IQuestionnaire questionnaire)
        {
            if (!interviewExpressionState.AreLinkedQuestionsSupported())
                return this.CalculateLinkedQuestionOptionsChangesWithLogicBeforeV7(updatedState, questionnaire);

            var processLinkedQuestionFilters = interviewExpressionState.ProcessLinkedQuestionFilters();

            if (processLinkedQuestionFilters == null)
                return new Dictionary<Identity, RosterVector[]>();

            if (processLinkedQuestionFilters.LinkedQuestionOptions.Count == 0)
                return processLinkedQuestionFilters.LinkedQuestionOptionsSet;

            //old v7 options handling 
            var linkedOptions = new Dictionary<Identity, RosterVector[]>();

            foreach (var linkedQuestionOption in processLinkedQuestionFilters.LinkedQuestionOptions)
            {
                IEnumerable<Identity> linkedQuestionInstances =
                    this.GetInstancesOfEntitiesWithSameAndDeeperRosterLevelOrThrow(updatedState, linkedQuestionOption.Key, new decimal[0], questionnaire);
                linkedQuestionInstances.ForEach(x => linkedOptions.Add(x, linkedQuestionOption.Value));
            }

            return linkedOptions;
        }

        private Dictionary<Identity, RosterVector[]> CalculateLinkedQuestionOptionsChangesWithLogicBeforeV7(
            InterviewStateDependentOnAnswers updatedState,
            IQuestionnaire questionnaire)
        {
            var questionsLinkedOnRoster = questionnaire.GetQuestionsLinkedToRoster();
            var questionsLinkedOnQuestion = questionnaire.GetQuestionsLinkedToQuestion();
            if (!questionsLinkedOnRoster.Any() && !questionsLinkedOnQuestion.Any())
                return new Dictionary<Identity, RosterVector[]>();

            var result = new Dictionary<Identity, RosterVector[]>();
            foreach (var questionLinkedOnRoster in questionsLinkedOnRoster)
            {
                var rosterId = questionnaire.GetRosterReferencedByLinkedQuestion(questionLinkedOnRoster);
                IEnumerable<Identity> targetRosters =
                    this.GetInstancesOfGroupsWithSameAndDeeperRosterLevelOrThrow(updatedState,
                        new[] { rosterId }, new decimal[0], questionnaire).ToArray();

                var optionRosterVectors =
                    targetRosters.Where(
                        r =>
                            !updatedState.IsGroupDisabled(r) && !string.IsNullOrEmpty(updatedState.GetRosterTitle(r.Id, r.RosterVector)))
                        .Select(r => r.RosterVector)
                        .ToArray();

                IEnumerable<Identity> linkedQuestionInstances =
                    this.GetInstancesOfEntitiesWithSameAndDeeperRosterLevelOrThrow(updatedState, questionLinkedOnRoster, new decimal[0], questionnaire);

                foreach (var linkedQuestionInstance in linkedQuestionInstances)
                {
                    result.Add(linkedQuestionInstance, optionRosterVectors);
                }
            }

            foreach (var questionLinkedOnQuestion in questionsLinkedOnQuestion)
            {
                var referencedQuestionId = questionnaire.GetQuestionReferencedByLinkedQuestion(questionLinkedOnQuestion);
                IEnumerable<Identity> targetQuestions =
                    this.GetInstancesOfEntitiesWithSameAndDeeperRosterLevelOrThrow(updatedState,
                        referencedQuestionId, new decimal[0], questionnaire);

                var optionRosterVectors =
                    targetQuestions.Where(q => !updatedState.IsQuestionDisabled(q) && updatedState.GetAnswerSupportedInExpressions(q) != null)
                        .Select(q => q.RosterVector)
                        .ToArray();

                IEnumerable<Identity> linkedQuestionInstances =
                   this.GetInstancesOfEntitiesWithSameAndDeeperRosterLevelOrThrow(updatedState, questionLinkedOnQuestion, new decimal[0], questionnaire);

                foreach (var linkedQuestionInstance in linkedQuestionInstances)
                {
                    result.Add(linkedQuestionInstance, optionRosterVectors);
                }
            }
            return result;
        }

        protected IEnumerable<ChangedLinkedOptions> CreateChangedLinkedOptions(
            ILatestInterviewExpressionState interviewExpressionState,
            InterviewStateDependentOnAnswers currentState,
            IQuestionnaire questionnaire,
            List<AnswerChange> interviewByAnswerChanges,
            EnablementChanges enablementChanges,
            RosterCalculationData rosterCalculationData,
            Dictionary<Identity, string> rosterInstancesWithAffectedTitles)
        {
            var currentLinkedOptions = currentState.LinkedQuestionOptions;

            var updatedState = currentState.Clone();

            if (enablementChanges != null)
                updatedState.ApplyEnablementChanges(enablementChanges);

            if (rosterCalculationData != null)
                updatedState.ApplyRosterData(rosterCalculationData);

            if (rosterInstancesWithAffectedTitles != null)
            {
                updatedState.ChangeRosterTitles(
                    rosterInstancesWithAffectedTitles.Select(
                        r =>
                            new ChangedRosterInstanceTitleDto(
                                new RosterInstance(r.Key.Id, r.Key.RosterVector.WithoutLast().ToArray(), r.Key.RosterVector.Last()), r.Value)).ToArray());
            }
            if (interviewByAnswerChanges != null)
            {
                foreach (var interviewByAnswerChange in interviewByAnswerChanges)
                {
                    string questionKey =
                        ConversionHelper.ConvertIdAndRosterVectorToString(interviewByAnswerChange.QuestionId,
                            interviewByAnswerChange.RosterVector);
                    updatedState.AnswersSupportedInExpressions[questionKey] = interviewByAnswerChange.Answer;
                    updatedState.AnsweredQuestions.Add(questionKey);
                }
            }
            var newCurrentLinkedOptions = GetLinkedQuestionOptionsChanges(interviewExpressionState, updatedState, questionnaire);

            foreach (var linkedQuestionConditionalExecutionResult in newCurrentLinkedOptions)
            {
                Identity instanceOfTheLinkedQuestionsQuestions = linkedQuestionConditionalExecutionResult.Key;
                RosterVector[] optionsForLinkedQuestion = linkedQuestionConditionalExecutionResult.Value;

                var linkedQuestionId = instanceOfTheLinkedQuestionsQuestions.Id;
                var referencedEntityId = questionnaire.IsQuestionLinkedToRoster(linkedQuestionId)
                    ? questionnaire.GetRosterReferencedByLinkedQuestion(linkedQuestionId)
                    : questionnaire.GetQuestionReferencedByLinkedQuestion(linkedQuestionId);

                var rosterVectorToStartFrom = this.CalculateStartRosterVectorForAnswersOfLinkedToQuestion(referencedEntityId, instanceOfTheLinkedQuestionsQuestions, questionnaire);

                var changedOptionAvaliableForInstanceOfTheQuestion = optionsForLinkedQuestion.Where(o => rosterVectorToStartFrom.SequenceEqual(o.Take(rosterVectorToStartFrom.Length))).ToArray();

                var questionIdentity = new Identity(instanceOfTheLinkedQuestionsQuestions.Id, instanceOfTheLinkedQuestionsQuestions.RosterVector);
                if (!currentLinkedOptions.ContainsKey(questionIdentity))
                {
                    yield return new ChangedLinkedOptions(instanceOfTheLinkedQuestionsQuestions, changedOptionAvaliableForInstanceOfTheQuestion);
                    continue;
                }

                var presentLinkedOptions = currentLinkedOptions[questionIdentity];

                bool hasNumberOfOptionsChanged = presentLinkedOptions.Length !=
                                                changedOptionAvaliableForInstanceOfTheQuestion.Length;

                bool doesNewOptionsListContainOptionsWhichWasNotPresentBefore =
                    changedOptionAvaliableForInstanceOfTheQuestion.Any(o => !presentLinkedOptions.Contains(o));

                if (hasNumberOfOptionsChanged || doesNewOptionsListContainOptionsWhichWasNotPresentBefore)
                    yield return new ChangedLinkedOptions(instanceOfTheLinkedQuestionsQuestions, changedOptionAvaliableForInstanceOfTheQuestion);
            }
        }

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

        protected bool HasInvalidAnswers() => this.interviewState.InvalidAnsweredQuestions.Any(x => !this.interviewState.DisabledQuestions.Contains(ConversionHelper.ConvertIdentityToString(x.Key)));
        protected bool HasInvalidStaticTexts => this.interviewState.InvalidStaticTexts.Any(x => !this.interviewState.DisabledStaticTexts.Contains(x.Key));

        private void ApplyTreeDiffChanges(Guid userId, InterviewTree changedInterviewTree, IQuestionnaire questionnaire,
            List<Identity> changedQuestionIdentities, InterviewTree sourceInterviewTree)
        {
            var expressionProcessorState = this.GetClonedExpressionState();
            
            this.UpdateExpressionState(sourceInterviewTree, changedInterviewTree, expressionProcessorState);

            EnablementChanges enablementChanges = expressionProcessorState.ProcessEnablementConditions();

            this.UpdateTreeWithEnablementChanges(changedInterviewTree, enablementChanges);

            var structuralChanges = expressionProcessorState.GetStructuralChanges();
            this.UpdateTreeWithStructuralChanges(changedInterviewTree, structuralChanges);

            changedQuestionIdentities.AddRange(structuralChanges.ChangedMultiQuestions.Keys);
            changedQuestionIdentities.AddRange(structuralChanges.ChangedSingleQuestions.Keys);
            changedQuestionIdentities.AddRange(structuralChanges.ChangedYesNoQuestions.Keys);

            this.UpdateRosterTitles(changedInterviewTree, questionnaire);

            this.UpdateLinkedQuestions(changedInterviewTree, expressionProcessorState);

            VariableValueChanges variableValueChanges = expressionProcessorState.ProcessVariables();
            this.UpdateTreeWithVariableChanges(changedInterviewTree, variableValueChanges);

            ValidityChanges validationChanges = expressionProcessorState.ProcessValidationExpressions();
            this.UpdateTreeWithValidationChanges(changedInterviewTree, validationChanges);

            this.ApplySubstitutionEvents(changedInterviewTree, questionnaire, changedQuestionIdentities);

            this.ApplyEvents(sourceInterviewTree, changedInterviewTree, userId);
        }

        private void UpdateTreeWithVariableChanges(InterviewTree tree, VariableValueChanges variableValueChanges)
            => variableValueChanges?.ChangedVariableValues.ForEach(x => tree.GetVariable(x.Key).SetValue(x.Value));

        private void UpdateTreeWithValidationChanges(InterviewTree tree, ValidityChanges validationChanges)
        {
            if (validationChanges == null) return; // can be in tests only.

            validationChanges.AnswersDeclaredValid.ForEach(x => tree.GetQuestion(x).MarkAsValid());
            validationChanges.AnswersDeclaredInvalid.ForEach(x => tree.GetQuestion(x).MarkAsInvalid(new FailedValidationCondition(0).ToEnumerable()));
            validationChanges.FailedValidationConditionsForQuestions.ForEach(x => tree.GetQuestion(x.Key).MarkAsInvalid(x.Value));

            validationChanges.StaticTextsDeclaredValid.ForEach(x => tree.GetStaticText(x).MarkAsValid());
            validationChanges.FailedValidationConditionsForStaticTexts.ForEach(x => tree.GetStaticText(x.Key).MarkAsInvalid(x.Value));
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
            foreach (var roster in tree.FindRosters())
            {
                if (roster.IsFixed)
                {
                    var changedRosterTitle = questionnaire.GetFixedRosterTitle(roster.Identity.Id,
                        roster.Identity.RosterVector.Last());

                    roster.SetRosterTitle(changedRosterTitle);
                }
                else
                {
                    roster.UpdateRosterTitle((questionId, answerOptionValue) =>
                        questionnaire.GetOptionsForQuestion(questionId, null, string.Empty)
                            .FirstOrDefault(x => x.Value == Convert.ToInt32(answerOptionValue))
                            .Title);
                }
            }
        }

        private void UpdateLinkedQuestions(InterviewTree tree, ILatestInterviewExpressionState interviewExpressionState)
        {
            if (!interviewExpressionState.AreLinkedQuestionsSupported())
            {
                var linkedQuestions = tree.FindQuestions().Where(x => x.IsLinked);
                foreach (InterviewTreeQuestion linkedQuestion in linkedQuestions)
                {
                    linkedQuestion.CalculateLinkedOptions();
                }
            }
            else
            {
                var processLinkedQuestionFilters = interviewExpressionState.ProcessLinkedQuestionFilters();
                foreach (var linkedQuestionWithOptions in processLinkedQuestionFilters.LinkedQuestionOptions)
                {
                    tree.FindQuestions(linkedQuestionWithOptions.Key).ForEach(x => x.AsLinked.SetOptions(linkedQuestionWithOptions.Value));
                }
                foreach (var linkedQuestionWithOptions in processLinkedQuestionFilters.LinkedQuestionOptionsSet)
                {
                    var linkedQuestion = tree.GetQuestion(linkedQuestionWithOptions.Key);
                    linkedQuestion.UpdateLinkedOptionsAndResetAnswerIfNeeded(linkedQuestionWithOptions.Value);
                }
            }
        }

        private void UpdateTreeWithStructuralChanges(InterviewTree tree, StructuralChanges structuralChanges)
        {
            foreach (var changedMultiQuestion in structuralChanges.ChangedMultiQuestions)
            {
                tree.GetQuestion(changedMultiQuestion.Key).AsMultiOption.SetAnswer(changedMultiQuestion.Value);
            }

            foreach (var changedSingleQuestion in structuralChanges.ChangedSingleQuestions)
            {
                var question = tree.GetQuestion(changedSingleQuestion.Key).AsSingleOption;
                if (changedSingleQuestion.Value.HasValue)
                    question.SetAnswer(changedSingleQuestion.Value.Value);
                else
                    question.RemoveAnswer();
            }

            foreach (var changedYesNoQuestion in structuralChanges.ChangedYesNoQuestions)
            {
                tree.GetQuestion(changedYesNoQuestion.Key).AsYesNo.SetAnswer(changedYesNoQuestion.Value);
            }

            foreach (var removedRosterIdentity in structuralChanges.RemovedRosters)
            {
                tree.RemoveNode(removedRosterIdentity);
            }
        }
    }
}