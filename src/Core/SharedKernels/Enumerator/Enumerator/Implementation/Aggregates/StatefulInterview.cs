using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Exceptions;
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
        private IQuestionnaire cachedQuestionnaire = null;
        
        public StatefulInterview(IQuestionnaireStorage questionnaireRepository,
                                 IInterviewExpressionStatePrototypeProvider expressionProcessorStatePrototypeProvider)
            : base(questionnaireRepository, expressionProcessorStatePrototypeProvider)
        {
        }

        #region Apply

        protected new void Apply(InterviewOnClientCreated @event)
        {
            base.Apply(@event);
            this.CreatedOnClient = true;
            this.InterviewerId = @event.UserId;
        }

        public new void Apply(InterviewSynchronized @event)
        {
            base.Apply(@event);

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

        private QuestionnaireIdentity questionnaireIdentity;

        public QuestionnaireIdentity QuestionnaireIdentity
            => this.questionnaireIdentity ?? (this.questionnaireIdentity = new QuestionnaireIdentity(this.questionnaireId, this.questionnaireVersion));

        public string QuestionnaireId => this.QuestionnaireIdentity.ToString();
        public InterviewStatus Status => this.properties.Status;
        public Guid Id => this.EventSourceId;
        public string InterviewerCompleteComment { get; private set; }
        public string SupervisorRejectComment { get; private set; }

        public string GetAnswerAsString(Identity questionIdentity)
            => this.interviewState.GetQuestion(questionIdentity).GetAnswerAsString();

        public string Language => this.language;

        public bool HasErrors { get; private set; }

        public bool IsCompleted { get; private set; }

        public InterviewTreeRoster GetRoster(Identity identity) => this.interviewState.GetRoster(identity);
        public InterviewTreeGpsQuestion GetGpsQuestion(Identity identity) => this.interviewState.GetQuestion(identity).AsGps;
        public InterviewTreeDateTimeQuestion GetDateTimeQuestion(Identity identity) => this.interviewState.GetQuestion(identity).AsDateTime;
        public InterviewTreeMultimediaQuestion GetMultimediaQuestion(Identity identity) => this.interviewState.GetQuestion(identity).AsMultimedia;
        public InterviewTreeQRBarcodeQuestion GetQRBarcodeQuestion(Identity identity) => this.interviewState.GetQuestion(identity).AsQRBarcode;
        public InterviewTreeTextListQuestion GetTextListQuestion(Identity identity) => this.interviewState.GetQuestion(identity).AsTextList;
        public InterviewTreeSingleLinkedOptionQuestion GetLinkedSingleOptionQuestion(Identity identity) => this.interviewState.GetQuestion(identity).AsSingleLinkedOption;
        public InterviewTreeMultiOptionQuestion GetMultiOptionQuestion(Identity identity) => this.interviewState.GetQuestion(identity).AsMultiOption;
        public InterviewTreeMultiLinkedOptionQuestion GetLinkedMultiOptionQuestion(Identity identity) => this.interviewState.GetQuestion(identity).AsMultiLinkedOption;
        public InterviewTreeIntegerQuestion GetIntegerQuestion(Identity identity) => this.interviewState.GetQuestion(identity).AsInteger;
        public InterviewTreeDoubleQuestion GetDoubleQuestion(Identity identity) => this.interviewState.GetQuestion(identity).AsDouble;
        public InterviewTreeTextQuestion GetTextQuestion(Identity identity) => this.interviewState.GetQuestion(identity).AsText;
        public InterviewTreeSingleOptionQuestion GetSingleOptionQuestion(Identity identity) => this.interviewState.GetQuestion(identity).AsSingleOption;
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
            var propertiesInvariants = new InterviewPropertiesInvariants(this.properties);

            propertiesInvariants.ThrowIfInterviewHardDeleted();

            this.ApplyEvent(new InterviewSynchronized(synchronizedInterview));

            this.delta = this.interviewState.Clone();
        }

        [Obsolete("it should be removed when all clients has version 5.7 or higher")]
        public void MigrateLinkedOptionsToFiltered()
        {
            //IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();

            //var expressionProcessorState = this.ExpressionProcessorStatePrototype.Clone();

            //var linkedQuestionOptionsChanges = this.CreateChangedLinkedOptions(
            //    expressionProcessorState,
            //    this.interviewState,
            //    questionnaire, null,
            //    null, null, null).ToArray();

            //this.ApplyEvent(new LinkedOptionsChanged(linkedQuestionOptionsChanges));
        }

        //protected IEnumerable<ChangedLinkedOptions> CreateChangedLinkedOptions(
        //    ILatestInterviewExpressionState interviewExpressionState,
        //    IQuestionnaire questionnaire,
        //    List<AnswerChange> interviewByAnswerChanges,
        //    EnablementChanges enablementChanges,
        //    RosterCalculationData rosterCalculationData,
        //    Dictionary<Identity, string> rosterInstancesWithAffectedTitles)
        //{
        //    var currentLinkedOptions = currentState.LinkedQuestionOptions;

        //    var updatedState = currentState.Clone();

        //    if (enablementChanges != null)
        //        updatedState.ApplyEnablementChanges(enablementChanges);

        //    if (rosterCalculationData != null)
        //        updatedState.ApplyRosterData(rosterCalculationData);

        //    if (rosterInstancesWithAffectedTitles != null)
        //    {
        //        updatedState.ChangeRosterTitles(
        //            rosterInstancesWithAffectedTitles.Select(
        //                r =>
        //                    new ChangedRosterInstanceTitleDto(
        //                        new RosterInstance(r.Key.Id, r.Key.RosterVector.WithoutLast().ToArray(), r.Key.RosterVector.Last()), r.Value)).ToArray());
        //    }
        //    if (interviewByAnswerChanges != null)
        //    {
        //        foreach (var interviewByAnswerChange in interviewByAnswerChanges)
        //        {
        //            string questionKey =
        //                ConversionHelper.ConvertIdAndRosterVectorToString(interviewByAnswerChange.QuestionId,
        //                    interviewByAnswerChange.RosterVector);
        //            updatedState.AnswersSupportedInExpressions[questionKey] = interviewByAnswerChange.Answer;
        //            updatedState.AnsweredQuestions.Add(questionKey);
        //        }
        //    }
        //    var newCurrentLinkedOptions = GetLinkedQuestionOptionsChanges(interviewExpressionState, updatedState, questionnaire);

        //    foreach (var linkedQuestionConditionalExecutionResult in newCurrentLinkedOptions)
        //    {
        //        Identity instanceOfTheLinkedQuestionsQuestions = linkedQuestionConditionalExecutionResult.Key;
        //        RosterVector[] optionsForLinkedQuestion = linkedQuestionConditionalExecutionResult.Value;

        //        var linkedQuestionId = instanceOfTheLinkedQuestionsQuestions.Id;
        //        var referencedEntityId = questionnaire.IsQuestionLinkedToRoster(linkedQuestionId)
        //            ? questionnaire.GetRosterReferencedByLinkedQuestion(linkedQuestionId)
        //            : questionnaire.GetQuestionReferencedByLinkedQuestion(linkedQuestionId);

        //        var rosterVectorToStartFrom = this.CalculateStartRosterVectorForAnswersOfLinkedToQuestion(referencedEntityId, instanceOfTheLinkedQuestionsQuestions, questionnaire);

        //        var changedOptionAvaliableForInstanceOfTheQuestion = optionsForLinkedQuestion.Where(o => rosterVectorToStartFrom.SequenceEqual(o.Take(rosterVectorToStartFrom.Length))).ToArray();

        //        var questionIdentity = new Identity(instanceOfTheLinkedQuestionsQuestions.Id, instanceOfTheLinkedQuestionsQuestions.RosterVector);
        //        if (!currentLinkedOptions.ContainsKey(questionIdentity))
        //        {
        //            yield return new ChangedLinkedOptions(instanceOfTheLinkedQuestionsQuestions, changedOptionAvaliableForInstanceOfTheQuestion);
        //            continue;
        //        }

        //        var presentLinkedOptions = currentLinkedOptions[questionIdentity];

        //        bool hasNumberOfOptionsChanged = presentLinkedOptions.Length !=
        //                                        changedOptionAvaliableForInstanceOfTheQuestion.Length;

        //        bool doesNewOptionsListContainOptionsWhichWasNotPresentBefore =
        //            changedOptionAvaliableForInstanceOfTheQuestion.Any(o => !presentLinkedOptions.Contains(o));

        //        if (hasNumberOfOptionsChanged || doesNewOptionsListContainOptionsWhichWasNotPresentBefore)
        //            yield return new ChangedLinkedOptions(instanceOfTheLinkedQuestionsQuestions, changedOptionAvaliableForInstanceOfTheQuestion);
        //    }
        //}

        //private Dictionary<Identity, RosterVector[]> GetLinkedQuestionOptionsChanges(
        //    ILatestInterviewExpressionState interviewExpressionState,
        //    IQuestionnaire questionnaire)
        //{
        //    if (!interviewExpressionState.AreLinkedQuestionsSupported())
        //        return this.CalculateLinkedQuestionOptionsChangesWithLogicBeforeV7(updatedState, questionnaire);

        //    var processLinkedQuestionFilters = interviewExpressionState.ProcessLinkedQuestionFilters();

        //    if (processLinkedQuestionFilters == null)
        //        return new Dictionary<Identity, RosterVector[]>();

        //    if (processLinkedQuestionFilters.LinkedQuestionOptions.Count == 0)
        //        return processLinkedQuestionFilters.LinkedQuestionOptionsSet;

        //    //old v7 options handling 
        //    var linkedOptions = new Dictionary<Identity, RosterVector[]>();

        //    foreach (var linkedQuestionOption in processLinkedQuestionFilters.LinkedQuestionOptions)
        //    {
        //        IEnumerable<Identity> linkedQuestionInstances =
        //            this.GetInstancesOfEntitiesWithSameAndDeeperRosterLevelOrThrow(updatedState, linkedQuestionOption.Key, new decimal[0], questionnaire);
        //        linkedQuestionInstances.ForEach(x => linkedOptions.Add(x, linkedQuestionOption.Value));
        //    }

        //    return linkedOptions;
        //}

        //private Dictionary<Identity, RosterVector[]> CalculateLinkedQuestionOptionsChangesWithLogicBeforeV7(IQuestionnaire questionnaire)
        //{
        //    var questionsLinkedOnRoster = questionnaire.GetQuestionsLinkedToRoster();
        //    var questionsLinkedOnQuestion = questionnaire.GetQuestionsLinkedToQuestion();
        //    if (!questionsLinkedOnRoster.Any() && !questionsLinkedOnQuestion.Any())
        //        return new Dictionary<Identity, RosterVector[]>();

        //    var result = new Dictionary<Identity, RosterVector[]>();
        //    foreach (var questionLinkedOnRoster in questionsLinkedOnRoster)
        //    {
        //        var rosterId = questionnaire.GetRosterReferencedByLinkedQuestion(questionLinkedOnRoster);
        //        IEnumerable<Identity> targetRosters =
        //            this.GetInstancesOfGroupsWithSameAndDeeperRosterLevelOrThrow(updatedState,
        //                new[] { rosterId }, new decimal[0], questionnaire).ToArray();

        //        var optionRosterVectors =
        //            targetRosters.Where(
        //                r =>
        //                    !updatedState.IsGroupDisabled(r) && !string.IsNullOrEmpty(updatedState.GetRosterTitle(r.Id, r.RosterVector)))
        //                .Select(r => r.RosterVector)
        //                .ToArray();

        //        IEnumerable<Identity> linkedQuestionInstances =
        //            this.GetInstancesOfEntitiesWithSameAndDeeperRosterLevelOrThrow(updatedState, questionLinkedOnRoster, new decimal[0], questionnaire);

        //        foreach (var linkedQuestionInstance in linkedQuestionInstances)
        //        {
        //            result.Add(linkedQuestionInstance, optionRosterVectors);
        //        }
        //    }

        //    foreach (var questionLinkedOnQuestion in questionsLinkedOnQuestion)
        //    {
        //        var referencedQuestionId = questionnaire.GetQuestionReferencedByLinkedQuestion(questionLinkedOnQuestion);
        //        IEnumerable<Identity> targetQuestions =
        //            this.GetInstancesOfEntitiesWithSameAndDeeperRosterLevelOrThrow(updatedState,
        //                referencedQuestionId, new decimal[0], questionnaire);

        //        var optionRosterVectors =
        //            targetQuestions.Where(q => !updatedState.IsQuestionDisabled(q) && updatedState.GetAnswer(q) != null)
        //                .Select(q => q.RosterVector)
        //                .ToArray();

        //        IEnumerable<Identity> linkedQuestionInstances =
        //           this.GetInstancesOfEntitiesWithSameAndDeeperRosterLevelOrThrow(updatedState, questionLinkedOnQuestion, new decimal[0], questionnaire);

        //        foreach (var linkedQuestionInstance in linkedQuestionInstances)
        //        {
        //            result.Add(linkedQuestionInstance, optionRosterVectors);
        //        }
        //    }
        //    return result;
        //}

        ///// <remarks>
        ///// If roster vector should be extended, result will be a set of vectors depending on roster count of corresponding groups.
        ///// </remarks>
        //protected static IEnumerable<RosterVector> ExtendRosterVector(RosterVector rosterVector, int length, Guid[] rosterGroupsStartingFromTop)
        //{
        //    if (length < rosterVector.Length)
        //        throw new ArgumentException(string.Format(
        //            "Cannot extend vector with length {0} to smaller length {1}.", rosterVector.Length, length));

        //    if (length == rosterVector.Length)
        //    {
        //        yield return rosterVector;
        //        yield break;
        //    }

        //    var outerVectorsForExtend = GetOuterVectorForParentRoster(state, rosterGroupsStartingFromTop, rosterVector);

        //    foreach (var outerVectorForExtend in outerVectorsForExtend)
        //    {
        //        IEnumerable<decimal> rosterInstanceIds = state.GetRosterInstanceIds(rosterGroupsStartingFromTop.Last(), outerVectorForExtend);
        //        foreach (decimal rosterInstanceId in rosterInstanceIds)
        //        {
        //            yield return ((RosterVector)outerVectorForExtend).ExtendWithOneCoordinate(rosterInstanceId);
        //        }
        //    }
        //}

        //private static IEnumerable<decimal[]> GetOuterVectorForParentRoster(
        //    Guid[] rosterGroupsStartingFromTop, RosterVector rosterVector)
        //{
        //    if (rosterGroupsStartingFromTop.Length <= 1 || rosterGroupsStartingFromTop.Length - 1 == rosterVector.Length)
        //    {
        //        yield return rosterVector;
        //        yield break;
        //    }

        //    var indexOfPreviousRoster = rosterGroupsStartingFromTop.Length - 2;

        //    var previousRoster = rosterGroupsStartingFromTop[rosterVector.Length];
        //    var previousRosterInstances = state.GetRosterInstanceIds(previousRoster, rosterVector);
        //    foreach (var previousRosterInstance in previousRosterInstances)
        //    {
        //        var extendedRoster = rosterVector.ExtendWithOneCoordinate(previousRosterInstance);
        //        if (indexOfPreviousRoster == rosterVector.Length)
        //        {
        //            yield return extendedRoster;
        //            continue;
        //        }
        //        foreach (var nextVector in GetOuterVectorForParentRoster(state, rosterGroupsStartingFromTop, extendedRoster))
        //        {
        //            yield return nextVector;
        //        }
        //    }
        //}
        //protected IEnumerable<Identity> GetInstancesOfEntitiesWithSameAndDeeperRosterLevelOrThrow(IEnumerable<Guid> entityIds, RosterVector rosterVector, IQuestionnaire questionnare)
        //   => entityIds.SelectMany(entityId =>
        //       this.GetInstancesOfEntitiesWithSameAndDeeperRosterLevelOrThrow(entityId, rosterVector, questionnare));

        //protected IEnumerable<Identity> GetInstancesOfEntitiesWithSameAndDeeperRosterLevelOrThrow(
        //    Guid entityId,
        //    RosterVector rosterVector,
        //    IQuestionnaire questionnare)
        //{
        //    int vectorRosterLevel = rosterVector.Length;
        //    int entityRosterLevel = questionnare.GetRosterLevelForEntity(entityId);

        //    if (entityRosterLevel < vectorRosterLevel)
        //        throw new InterviewException(string.Format(
        //            "Entity {0} expected to have roster level not upper than {1} but it is {2}. InterviewId: {3}",
        //            FormatQuestionForException(entityId, questionnare), vectorRosterLevel, entityRosterLevel, EventSourceId));

        //    Guid[] parentRosterGroupsStartingFromTop =
        //        questionnare.GetRostersFromTopToSpecifiedEntity(entityId).ToArray();

        //    IEnumerable<RosterVector> entityRosterVectors = ExtendRosterVector(state,
        //        rosterVector, entityRosterLevel, parentRosterGroupsStartingFromTop);

        //    return entityRosterVectors.Select(entityRosterVector => new Identity(entityId, entityRosterVector));
        //}

        //protected IEnumerable<Identity> GetInstancesOfGroupsWithSameAndDeeperRosterLevelOrThrow(IEnumerable<Guid> groupIds, RosterVector rosterVector, IQuestionnaire questionnaire)
        //    => groupIds.SelectMany(groupId =>
        //        this.GetInstancesOfGroupsByGroupIdWithSameAndDeeperRosterLevelOrThrow(groupId, rosterVector, questionnaire));

        //protected IEnumerable<Identity> GetInstancesOfGroupsByGroupIdWithSameAndDeeperRosterLevelOrThrow(
        //    Guid groupId, RosterVector rosterVector, IQuestionnaire questionnaire)
        //{
        //    int vectorRosterLevel = rosterVector.Length;
        //    int groupRosterLevel = questionnaire.GetRosterLevelForGroup(groupId);

        //    if (groupRosterLevel < vectorRosterLevel)
        //        throw new InterviewException(string.Format(
        //            "Question {0} expected to have roster level not upper than {1} but it is {2}. InterviewId: {3}",
        //            FormatQuestionForException(groupId, questionnaire), vectorRosterLevel, groupRosterLevel, EventSourceId));

        //    Guid[] parentRosterGroupsStartingFromTop = questionnaire.GetRostersFromTopToSpecifiedGroup(groupId).ToArray();

        //    IEnumerable<RosterVector> groupRosterVectors = ExtendRosterVector(state,
        //        rosterVector, groupRosterLevel, parentRosterGroupsStartingFromTop);

        //    return groupRosterVectors.Select(groupRosterVector => new Identity(groupId, groupRosterVector));
        //}

        //protected Identity GetInstanceOfGroupWithSameAndUpperRosterLevelOrThrow(Guid groupId, RosterVector rosterVector, IQuestionnaire questionnaire)
        //{
        //    int vectorRosterLevel = rosterVector.Length;

        //    int groupRosterLevel = questionnaire.GetRosterLevelForGroup(groupId);

        //    if (groupRosterLevel > vectorRosterLevel)
        //        throw new InterviewException(string.Format(
        //            "Group {0} expected to have roster level not deeper than {1} but it is {2}. InterviewId: {3}",
        //            FormatGroupForException(groupId, questionnaire), vectorRosterLevel, groupRosterLevel, this.EventSourceId));

        //    decimal[] groupRosterVector = rosterVector.Shrink(groupRosterLevel);

        //    return new Identity(groupId, groupRosterVector);
        //}

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
        
        private IQuestionnaire GetQuestionnaireOrThrow()
        {
            return this.cachedQuestionnaire ?? (this.cachedQuestionnaire = GetQuestionnaireOrThrow(
                this.QuestionnaireIdentity.QuestionnaireId,
                this.QuestionnaireIdentity.Version,
                this.Language));
        }
    }
}