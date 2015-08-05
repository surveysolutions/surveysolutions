﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using WB.Core.BoundedContexts.Tester.Implementation.Entities;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;

using Identity = WB.Core.SharedKernels.DataCollection.Identity;

namespace WB.Core.BoundedContexts.Tester.Implementation.Aggregates
{
    internal class StatefulInterview : Interview, IStatefulInterview
    {
        private class CalculatedState
        {
            public ConcurrentDictionary<Identity, ReadOnlyCollection<Identity>> EnabledInterviewerChildQuestions = new ConcurrentDictionary<Identity, ReadOnlyCollection<Identity>>();
            public ConcurrentDictionary<Identity, ReadOnlyCollection<Identity>> GroupsAndRostersInGroup = new ConcurrentDictionary<Identity, ReadOnlyCollection<Identity>>();
            public ConcurrentDictionary<Identity, bool> IsEnabled = new ConcurrentDictionary<Identity, bool>();
        }

        private CalculatedState calculated = null;
        private IQuestionnaire cachedQuestionnaire = null;

        private readonly Dictionary<string, BaseInterviewAnswer> answers;
        private readonly Dictionary<string, InterviewGroup> groups;
        private readonly Dictionary<string, List<Identity>> rosterInstancesIds;
        private readonly Dictionary<string, bool> notAnsweredQuestionsValidityStatus;
        private readonly Dictionary<string, bool> notAnsweredQuestionsEnablementStatus;
        private readonly Dictionary<string, string> notAnsweredQuestionsInterviewerComments;

        public StatefulInterview()
        {
            this.answers = new Dictionary<string, BaseInterviewAnswer>();
            this.groups = new Dictionary<string, InterviewGroup>();
            this.rosterInstancesIds = new Dictionary<string, List<Identity>>();
            this.notAnsweredQuestionsValidityStatus = new Dictionary<string, bool>();
            this.notAnsweredQuestionsEnablementStatus = new Dictionary<string, bool>();
            this.notAnsweredQuestionsInterviewerComments = new Dictionary<string, string>();
        }

        private void ResetCalculatedState()
        {
            this.calculated = new CalculatedState();
        }

        protected new void Apply(InterviewOnClientCreated @event)
        {
            base.Apply(@event);
            this.ResetCalculatedState();

            this.Id = this.EventSourceId;
            this.QuestionnaireId = Helpers.CreateHistoricId(@event.QuestionnaireId, @event.QuestionnaireVersion);
            this.QuestionnaireVersion = @event.QuestionnaireVersion;
            this.QuestionnaireGuid = @event.QuestionnaireId;
        }

        #region Applying answers

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

            var answer = this.GetOrCreateAnswer<QRBarcodeAnswer>(@event);
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
            answer.SetAnswer(@event.SelectedPropagationVector);
        }

        internal new void Apply(MultipleOptionsLinkedQuestionAnswered @event)
        {
            base.Apply(@event);
            this.ResetCalculatedState();

            var answer = this.GetOrCreateAnswer<LinkedMultiOptionAnswer>(@event);
            answer.SetAnswers(@event.SelectedPropagationVectors);
        }

        #endregion

        internal new void Apply(AnswerCommented @event)
        {
            base.Apply(@event);
            this.ResetCalculatedState();

            var questionKey = ConversionHelper.ConvertIdAndRosterVectorToString(@event.QuestionId, @event.PropagationVector);
            var answer = this.GetExistingAnswerOrNull(questionKey);
            if (answer != null)
            {
                answer.InterviewerComment = @event.Comment;
            }
            else
            {
                this.notAnsweredQuestionsInterviewerComments[questionKey] = @event.Comment;
            }
        }

        #region Group and question status and validity

        public new void Apply(AnswersRemoved @event)
        {
            base.Apply(@event);
            this.ResetCalculatedState();

            @event.Questions.ForEach(x =>
                this.Answers[ConversionHelper.ConvertIdAndRosterVectorToString(x.Id, x.RosterVector)].RemoveAnswer()
            );
        }

        public new void Apply(AnswersDeclaredValid @event)
        {
            base.Apply(@event);
            this.ResetCalculatedState();

            @event.Questions.ForEach(x =>
            {
                var questionKey = ConversionHelper.ConvertIdAndRosterVectorToString(x.Id, x.RosterVector);
                var answer = this.GetExistingAnswerOrNull(questionKey);
                if (answer != null)
                {
                    answer.IsValid = true;
                }
                else
                {
                    this.notAnsweredQuestionsValidityStatus[questionKey] = true;
                }
            });
        }

        public new void Apply(AnswersDeclaredInvalid @event)
        {
            base.Apply(@event);
            this.ResetCalculatedState();

            @event.Questions.ForEach(x =>
            {
                var questionKey = ConversionHelper.ConvertIdAndRosterVectorToString(x.Id, x.RosterVector);
                var answer = this.GetExistingAnswerOrNull(questionKey);
                if (answer != null)
                {
                    answer.IsValid = false;
                }
                else
                {
                    this.notAnsweredQuestionsValidityStatus[questionKey] = false;
                }
            });
        }

        public new void Apply(GroupsDisabled @event)
        {
            base.Apply(@event);
            this.ResetCalculatedState();

            @event.Groups.ForEach(x =>
            {
                var groupOrRoster = this.GetOrCreateGroupOrRoster(x.Id, x.RosterVector);
                groupOrRoster.IsDisabled = true;
            });
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
        }

        public new void Apply(QuestionsDisabled @event)
        {
            base.Apply(@event);
            this.ResetCalculatedState();

            @event.Questions.ForEach(x =>
            {
                var questionKey = ConversionHelper.ConvertIdAndRosterVectorToString(x.Id, x.RosterVector);
                var answer = this.GetExistingAnswerOrNull(questionKey);
                if (answer != null)
                {
                    answer.IsEnabled = false;
                }
                else
                {
                    this.notAnsweredQuestionsEnablementStatus[questionKey] = false;
                }
            });
        }


        public new void Apply(QuestionsEnabled @event)
        {
            base.Apply(@event);
            this.ResetCalculatedState();

            @event.Questions.ForEach(x =>
            {
                var questionKey = ConversionHelper.ConvertIdAndRosterVectorToString(x.Id, x.RosterVector);
                var answer = GetExistingAnswerOrNull(questionKey);
                if (answer != null)
                {
                    answer.IsEnabled = true;
                }
                else
                {
                    this.notAnsweredQuestionsEnablementStatus[questionKey] = true;
                }
            });
        }

        #endregion

        #region Roster instances and titles

        public new void Apply(RosterInstancesTitleChanged @event)
        {
            base.Apply(@event);
            this.ResetCalculatedState();

            foreach (var changedRosterInstanceTitle in @event.ChangedInstances)
            {
                var rosterKey = ConversionHelper.ConvertIdAndRosterVectorToString(changedRosterInstanceTitle.RosterInstance.GroupId, GetFullRosterVector(changedRosterInstanceTitle.RosterInstance));
                var roster = (InterviewRoster)this.groups[rosterKey];
                roster.Title = changedRosterInstanceTitle.Title;
            }
        }

        public new void Apply(RosterInstancesAdded @event)
        {
            base.Apply(@event);
            this.ResetCalculatedState();

            foreach (var rosterInstance in @event.Instances)
            {
                var rosterIdentity = new Identity(rosterInstance.GroupId, GetFullRosterVector(rosterInstance));

                var rosterKey = ConversionHelper.ConvertIdentityToString(rosterIdentity);
                var rosterParentKey = ConversionHelper.ConvertIdAndRosterVectorToString(rosterInstance.GroupId, rosterInstance.OuterRosterVector);

                this.groups[rosterKey] = new InterviewRoster
                {
                    Id = rosterIdentity.Id,
                    RosterVector = rosterIdentity.RosterVector,
                    ParentRosterVector = rosterInstance.OuterRosterVector,
                    RowCode = rosterInstance.RosterInstanceId
                };
                if (!this.rosterInstancesIds.ContainsKey(rosterParentKey))
                    this.rosterInstancesIds.Add(rosterParentKey, new List<Identity>());

                this.rosterInstancesIds[rosterParentKey].Add(rosterIdentity);
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
                var rosterParentKey = ConversionHelper.ConvertIdAndRosterVectorToString(rosterInstance.GroupId, rosterInstance.OuterRosterVector);

                var rosterIdentity = this.RosterInstancesIds[rosterParentKey].Find(roster => roster.Id == rosterInstance.GroupId &&
                                                                                             roster.RosterVector.SequenceEqual(fullRosterVector));

                this.groups.Remove(rosterKey);
                this.RosterInstancesIds[rosterParentKey].Remove(rosterIdentity);
            }
        }

        #endregion

        #region Interview status and validity

        public new void Apply(InterviewCompleted @event)
        {
            base.Apply(@event);
            this.ResetCalculatedState();

            this.IsCompleted = true;
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

        public string QuestionnaireId { get; set; }

        public long QuestionnaireVersion { get; set; }
        public Guid QuestionnaireGuid { get; set; }

        public Guid Id { get; set; }

        public IReadOnlyDictionary<string, BaseInterviewAnswer> Answers
        {
            get
            {
                return new ReadOnlyDictionary<string, BaseInterviewAnswer>(this.answers);
            }
        }

        public IReadOnlyDictionary<string, List<Identity>> RosterInstancesIds
        {
            get
            {
                return new ReadOnlyDictionary<string, List<Identity>>(this.rosterInstancesIds);
            }
        }

        public bool HasErrors { get; set; }

        public bool IsCompleted { get; set; }

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

        public QRBarcodeAnswer GetQRBarcodeAnswer(Identity identity)
        {
            return this.GetQuestionAnswer<QRBarcodeAnswer>(identity);
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

        public bool HasGroup(Identity group)
        {
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

        public BaseInterviewAnswer FindBaseAnswerByOrDeeperRosterLevel(Guid questionId, decimal[] targetRosterVector)
        {
            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();

            int questionRosterLevel = questionnaire.GetRosterLevelForQuestion(questionId);
            var rosterVector = this.ShrinkRosterVector(targetRosterVector, questionRosterLevel);
            var questionKey = ConversionHelper.ConvertIdAndRosterVectorToString(questionId, rosterVector);

            return this.Answers.ContainsKey(questionKey) ? this.Answers[questionKey] : null;
        }

        public IEnumerable<BaseInterviewAnswer> FindAnswersOfReferencedQuestionForLinkedQuestion(Guid referencedQuestionId, Identity linkedQuestion)
        {
            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();

            var rosterVectorToStartFrom = this.CalculateStartRosterVectorForAnswersOfLinkedToQuestion(referencedQuestionId, linkedQuestion, questionnaire);

            IEnumerable<Identity> targetQuestions =
               this.GetInstancesOfQuestionsWithSameAndDeeperRosterLevelOrThrow(this.interviewState, referencedQuestionId, rosterVectorToStartFrom, questionnaire, GetRosterInstanceIds);

            var answers = targetQuestions
                .Select(GetExistingAnswerOrNull)
                .Where(answer => answer != null);

            return answers;
        }

        private decimal[] CalculateStartRosterVectorForAnswersOfLinkedToQuestion(
            Guid linkedToQuestionId, Identity linkedQuestion, IQuestionnaire questionnaire)
        {
            Guid[] linkedToQuestionRosterSources = questionnaire.GetRosterSizeSourcesForQuestion(linkedToQuestionId);
            Guid[] linkedQuestionRosterSources = questionnaire.GetRosterSizeSourcesForQuestion(linkedQuestion.Id);

            int commonRosterSourcesPartLength = Enumerable
                .Zip(linkedToQuestionRosterSources, linkedQuestionRosterSources, AreEqual)
                .TakeWhile(areEqual => areEqual)
                .Count();

            int linkedToQuestionRosterLevel = questionnaire.GetRosterLevelForQuestion(linkedToQuestionId);
            int linkedQuestionRosterLevel = linkedQuestion.RosterVector.Length;

            int targetRosterLevel = Math.Min(commonRosterSourcesPartLength, Math.Min(linkedToQuestionRosterLevel - 1, linkedQuestionRosterLevel));

            return this.ShrinkRosterVector(linkedQuestion.RosterVector, targetRosterLevel);
        }

        public InterviewRoster FindRosterByOrDeeperRosterLevel(Guid rosterId, decimal[] targetRosterVector)
        {
            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();

            int grosterLevel = questionnaire.GetRosterLevelForGroup(rosterId);
            var rosterVector = this.ShrinkRosterVector(targetRosterVector, grosterLevel);
            var rosterKey = ConversionHelper.ConvertIdAndRosterVectorToString(rosterId, rosterVector);

            return this.groups.ContainsKey(rosterKey) ? this.groups[rosterKey] as InterviewRoster : null;
        }

        public IEnumerable<string> GetParentRosterTitlesWithoutLast(Guid questionId, decimal[] rosterVector)
        {
            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();

            IEnumerable<Guid> parentRosters = questionnaire.GetRostersFromTopToSpecifiedQuestion(questionId).WithoutLast();

            foreach (var parentRosterId in parentRosters)
            {
                int parentRosterLevel = questionnaire.GetRosterLevelForGroup(parentRosterId);
                var parentRosterVector = this.ShrinkRosterVector(rosterVector, parentRosterLevel);
                yield return GetRosterTitle(new Identity(parentRosterId, parentRosterVector));
            }
        }

        public int CountInterviewerQuestionsInGroupRecursively(Identity groupIdentity)
        {
            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();
            IEnumerable<Guid> allQuestionsInGroup = questionnaire.GetAllUnderlyingInterviewerQuestions(groupIdentity.Id);

            return this.GetInstancesOfQuestionsWithSameAndDeeperRosterLevelOrThrow(this.interviewState,
                allQuestionsInGroup,
                groupIdentity.RosterVector,
                questionnaire,
                GetRosterInstanceIds).Count();
        }

        public int CountActiveInterviewerQuestionsInGroupOnly(Identity group)
        {
            return this
                .GetEnabledInterviewerChildQuestions(group)
                .Count;
        }

        public int GetGroupsInGroupCount(Identity group)
        {
            return this
                .GetGroupsAndRostersInGroup(group)
                .Count;
        }

        public int CountAnsweredInterviewerQuestionsInGroupRecursively(Identity groupIdentity)
        {
            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();
            IEnumerable<Guid> allQuestionsInGroup = questionnaire.GetAllUnderlyingInterviewerQuestions(groupIdentity.Id);

            var questionInstances = this.GetInstancesOfQuestionsWithSameAndDeeperRosterLevelOrThrow(this.interviewState,
                allQuestionsInGroup,
                groupIdentity.RosterVector,
                questionnaire,
                GetRosterInstanceIds);

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

            var questionInstances = this.GetInstancesOfQuestionsWithSameAndDeeperRosterLevelOrThrow(this.interviewState,
                allQuestionsInGroup,
                groupIdentity.RosterVector,
                questionnaire,
                GetRosterInstanceIds).Select(ConversionHelper.ConvertIdentityToString);
            return this.Answers.Where(x => questionInstances.Contains(x.Key)).Count(x => x.Value != null && x.Value.IsAnswered && !x.Value.IsValid);
        }

        public int CountInvalidInterviewerQuestionsInGroupOnly(Identity group)
        {
            return this
                .GetEnabledInterviewerChildQuestions(group)
                .Count(question => !this.IsValid(question));
        }

        public bool HasInvalidInterviewerQuestionsInGroupOnly(Identity group)
        {
            return this
                .GetEnabledInterviewerChildQuestions(group)
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

            IEnumerable<Identity> questionInstances = this.GetInstancesOfQuestionsWithSameAndDeeperRosterLevelOrThrow(this.interviewState,
                allQuestionsInGroup,
                groupIdentity.RosterVector,
                questionnaire,
                GetRosterInstanceIds);
            return questionInstances;
        }

        public IEnumerable<Identity> GetChildEntities(Identity groupIdentity)
        {
            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();
            IEnumerable<Guid> allEntitiesInGroup = questionnaire.GetChildEntityIds(groupIdentity.Id);

            var list = new List<Identity>();
            foreach (var entity in allEntitiesInGroup)
            {
                var isQuestion = questionnaire.IsQuestion(entity);
                if (isQuestion && !questionnaire.IsInterviewierQuestion(entity)) continue;

                if (!isQuestion && questionnaire.IsRosterGroup(entity))
                {
                    foreach (var rosterInstance in GetInstancesOfGroupsByGroupIdWithSameAndDeeperRosterLevelOrThrow(this.interviewState, entity,
                            groupIdentity.RosterVector, questionnaire, GetRosterInstanceIds))
                    {
                        list.Add(rosterInstance);
                    }
                }
                else
                {
                    foreach (var entityInstance in GetInstancesOfEntitiesWithSameAndDeeperRosterLevelOrThrow(this.interviewState, entity,
                            groupIdentity.RosterVector, questionnaire, GetRosterInstanceIds))
                    {
                        list.Add(entityInstance);
                    }
                }
            }
            return list;
        }

        public IEnumerable<Identity> GetEnabledGroupInstances(Guid groupId, decimal[] parentRosterVector)
        {
            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();
            var resultInstances = this.GetInstancesOfGroupsByGroupIdWithSameAndDeeperRosterLevelOrThrow(this.interviewState, 
                groupId, 
                parentRosterVector, 
                questionnaire, 
                GetRosterInstanceIds);

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
            if (!this.Answers.ContainsKey(questionKey))
            {
                return !this.notAnsweredQuestionsValidityStatus.ContainsKey(questionKey) || this.notAnsweredQuestionsValidityStatus[questionKey];
            }

            var interviewAnswerModel = this.Answers[questionKey];
            return interviewAnswerModel.IsValid;
        }

        public bool IsEnabled(Identity entityIdentity)
        {
            return GetOrCalculate(
                entityIdentity,
                this.IsEnabledImpl,
                this.calculated.IsEnabled);
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

            if (this.notAnsweredQuestionsEnablementStatus.ContainsKey(entityKey))
            {
                return this.notAnsweredQuestionsEnablementStatus[entityKey];
            }

            return true;
        }

        public bool WasAnswered(Identity entityIdentity)
        {
            var questionKey = ConversionHelper.ConvertIdentityToString(entityIdentity);
            if (!Answers.ContainsKey(questionKey))
                return false;

            if (!IsEnabled(entityIdentity))
                return false;

            var interviewAnswerModel = Answers[questionKey];
            return interviewAnswerModel.IsAnswered;
        }

        public string GetInterviewerAnswerComment(Identity entityIdentity)
        {
            var questionKey = ConversionHelper.ConvertIdentityToString(entityIdentity);
            if (!Answers.ContainsKey(questionKey))
            {
                return this.notAnsweredQuestionsInterviewerComments.ContainsKey(questionKey)
                    ? this.notAnsweredQuestionsInterviewerComments[questionKey]
                    : null;
            }

            var interviewAnswerModel = Answers[questionKey];
            return interviewAnswerModel.InterviewerComment;
        }

        private ReadOnlyCollection<Identity> GetGroupsAndRostersInGroup(Identity group)
        {
            return GetOrCalculate(
                group,
                this.GetGroupsAndRostersInGroupImpl,
                this.calculated.GroupsAndRostersInGroup);
        }

        private ReadOnlyCollection<Identity> GetGroupsAndRostersInGroupImpl(Identity group)
        {
            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();

            IEnumerable<Guid> groupsAndRosters = questionnaire.GetAllUnderlyingChildGroupsAndRosters(group.Id);

            return this.GetInstancesOfGroupsWithSameAndDeeperRosterLevelOrThrow(
                this.interviewState, groupsAndRosters, group.RosterVector, questionnaire, GetRosterInstanceIds).ToReadOnlyCollection();
        }

        private ReadOnlyCollection<Identity> GetEnabledInterviewerChildQuestions(Identity group)
        {
            return GetOrCalculate(
                group,
                this.GetEnabledInterviewerChildQuestionsImpl,
                this.calculated.EnabledInterviewerChildQuestions);
        }

        private ReadOnlyCollection<Identity> GetEnabledInterviewerChildQuestionsImpl(Identity group)
        {
            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();

            IEnumerable<Guid> questionIds = questionnaire.GetChildInterviewerQuestions(group.Id);

            IEnumerable<Identity> questions = this.GetInstancesOfQuestionsWithSameAndDeeperRosterLevelOrThrow(
                this.interviewState, questionIds, group.RosterVector, questionnaire, GetRosterInstanceIds);

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
                && !this.notAnsweredQuestionsInterviewerComments.ContainsKey(questionKey)
                && !this.notAnsweredQuestionsValidityStatus.ContainsKey(questionKey))
            {
                return new T
                       {
                           InterviewerComment = null,
                           IsEnabled = true,
                           IsValid = true
                       };
            }

            var interviewerComment = this.notAnsweredQuestionsInterviewerComments.ContainsKey(questionKey)
                ? this.notAnsweredQuestionsInterviewerComments[questionKey]
                : null;

            var isEnabled = !this.notAnsweredQuestionsEnablementStatus.ContainsKey(questionKey) || this.notAnsweredQuestionsEnablementStatus[questionKey];
            var isValid = !this.notAnsweredQuestionsValidityStatus.ContainsKey(questionKey) || this.notAnsweredQuestionsValidityStatus[questionKey];

            var answer = new T
            {
                InterviewerComment = interviewerComment,
                IsEnabled = isEnabled,
                IsValid = isValid
            };

            return answer;
        }

        private T GetOrCreateAnswer<T>(QuestionActiveEvent @event) where T : BaseInterviewAnswer, new()
        {
            var questionKey = ConversionHelper.ConvertIdAndRosterVectorToString(@event.QuestionId, @event.PropagationVector);

            T question;
            if (this.Answers.ContainsKey(questionKey))
            {
                question = (T)this.Answers[questionKey];
            }
            else
            {
                question = new T { Id = @event.QuestionId, RosterVector = @event.PropagationVector };
                if (this.notAnsweredQuestionsEnablementStatus.ContainsKey(questionKey))
                {
                    question.IsEnabled = this.notAnsweredQuestionsEnablementStatus[questionKey];
                    this.notAnsweredQuestionsEnablementStatus.Remove(questionKey);
                }

                if (this.notAnsweredQuestionsValidityStatus.ContainsKey(questionKey))
                {
                    question.IsValid = this.notAnsweredQuestionsValidityStatus[questionKey];
                    this.notAnsweredQuestionsValidityStatus.Remove(questionKey);
                }

                if (this.notAnsweredQuestionsInterviewerComments.ContainsKey(questionKey))
                {
                    question.InterviewerComment = this.notAnsweredQuestionsInterviewerComments[questionKey];
                    this.notAnsweredQuestionsInterviewerComments.Remove(questionKey);
                }
            }

            this.answers[questionKey] = question;
            return question;
        }

        private InterviewGroup GetOrCreateGroupOrRoster(Guid id, decimal[] rosterVector)
        {
            var groupKey = ConversionHelper.ConvertIdAndRosterVectorToString(id, rosterVector);

            InterviewGroup groupOrRoster;
            if (this.groups.ContainsKey(groupKey))
            {
                groupOrRoster = this.groups[groupKey];
            }
            else
            {
                // rosters must be created with event RosterInstancesAdded. Groups has no such event, so
                // they should be created eventually. So if group model was not found that means that
                // we got event about group that was not created previously. Rosters should not be created here.
                groupOrRoster = new InterviewGroup { Id = id, RosterVector = rosterVector };
            }

            this.groups[groupKey] = groupOrRoster;
            return groupOrRoster;
        }

        private IQuestionnaire GetQuestionnaireOrThrow()
        {
            return this.cachedQuestionnaire ?? (this.cachedQuestionnaire = GetHistoricalQuestionnaireOrThrow(this.QuestionnaireGuid, this.QuestionnaireVersion));
        }

        private static decimal[] GetFullRosterVector(RosterInstance instance)
        {
            return instance.OuterRosterVector.Concat(new[] { instance.RosterInstanceId }).ToArray();
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

        private static bool AreEqual(Guid first, Guid second)
        {
            return first == second;
        }
    }
}