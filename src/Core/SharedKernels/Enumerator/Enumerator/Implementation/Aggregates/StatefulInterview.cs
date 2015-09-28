﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
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
        }

        private CalculatedState calculated = null;
        private IQuestionnaire cachedQuestionnaire = null;

        private readonly Dictionary<string, BaseInterviewAnswer> answers;
        private readonly Dictionary<string, InterviewGroup> groups;
        private readonly Dictionary<string, List<Identity>> rosterInstancesIds;
        private readonly Dictionary<string, bool> notAnsweredQuestionsValidityStatus;
        private readonly Dictionary<string, bool> notAnsweredQuestionsEnablementStatus;
        private readonly Dictionary<string, string> notAnsweredQuestionsInterviewerComments;
        private bool createdOnClient;

        public StatefulInterview(ILogger logger, IQuestionnaireRepository questionnaireRepository, IInterviewExpressionStatePrototypeProvider expressionProcessorStatePrototypeProvider)
            : base(logger, questionnaireRepository, expressionProcessorStatePrototypeProvider)
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
       
        protected new void Apply(SynchronizationMetadataApplied @event)
        {
            base.Apply(@event);
            this.InitializeCreatedInterview(this.EventSourceId, @event.QuestionnaireId, @event.QuestionnaireVersion);
        }

        protected new void Apply(InterviewOnClientCreated @event)
        {
            base.Apply(@event);
            this.InitializeCreatedInterview(this.EventSourceId, @event.QuestionnaireId,  @event.QuestionnaireVersion);
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

            var orderedRosterInstances = @event.InterviewData.RosterGroupInstances.SelectMany(x => x.Value).OrderBy(x => x.OuterScopeRosterVector.Length).ToList();
            foreach (RosterSynchronizationDto roster in orderedRosterInstances)
            {
                AddRosterInstance(new AddedRosterInstance(roster.RosterId, roster.OuterScopeRosterVector, roster.RosterInstanceId, roster.SortIndex));
                ChangeRosterTitle(new RosterInstance(roster.RosterId, roster.OuterScopeRosterVector, roster.RosterInstanceId), roster.RosterTitle);
            }

            @event.InterviewData.ValidAnsweredQuestions.ForEach(x => DeclareAnswerAsValid(x.Id, x.InterviewItemRosterVector));
            @event.InterviewData.InvalidAnsweredQuestions.ForEach(x => DeclareAnswerAsInvalid(x.Id, x.InterviewItemRosterVector));

            @event.InterviewData.DisabledQuestions.ForEach(x => DisableQuestion(x.Id, x.InterviewItemRosterVector));
            @event.InterviewData.DisabledGroups.ForEach(x => DisableGroup(x.Id, x.InterviewItemRosterVector));
            @event.InterviewData.Answers.ForEach(x => CommentQuestion(x.Id, x.QuestionRosterVector,x.Comments));
        }

        public void Apply(InterviewAnswersFromSyncPackageRestored @event)
        {
            foreach (var answerDto in @event.Answers)
            {
                this.SaveAnswerFromAnswerDto(answerDto);
            }
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
            this.CommentQuestion(@event.QuestionId, @event.RosterVector, @event.Comment);
        }

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

        public new void Apply(AnswersDeclaredValid @event)
        {
            base.Apply(@event);
            this.ResetCalculatedState();

            @event.Questions.ForEach(x => this.DeclareAnswerAsValid(x.Id, x.RosterVector));
        }

        public new void Apply(AnswersDeclaredInvalid @event)
        {
            base.Apply(@event);
            this.ResetCalculatedState();

            @event.Questions.ForEach(x => this.DeclareAnswerAsInvalid(x.Id, x.RosterVector));
        }

        public new void Apply(GroupsDisabled @event)
        {
            base.Apply(@event);
            this.ResetCalculatedState();

            @event.Groups.ForEach(x => this.DisableGroup(x.Id, x.RosterVector));
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

            @event.Questions.ForEach(x => this.DisableQuestion(x.Id, x.RosterVector));
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
        public string QuestionnaireId { get { return this.QuestionnaireIdentity.ToString(); } }
        public Guid InterviewerId { get { return this.interviewerId; } }
        public InterviewStatus Status { get { return status; } }
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

        public void RestoreInterviewStateFromSyncPackage(Guid userId, InterviewSynchronizationDto synchronizedInterview)
        {
            ThrowIfInterviewHardDeleted();
            IQuestionnaire questionnaire = GetHistoricalQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);
            var answerDtos = synchronizedInterview
                .Answers
                .Select(answerDto => new InterviewAnswerDto(answerDto.Id, answerDto.QuestionRosterVector, questionnaire.GetAnswerType(answerDto.Id), answerDto.Answer))
                .ToArray();

            this.ApplyEvent(new InterviewSynchronized(synchronizedInterview));
            this.ApplyEvent(new InterviewAnswersFromSyncPackageRestored(answerDtos, synchronizedInterview.UserId));
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
                .Select(this.GetExistingAnswerOrNull)
                .Where(answer => answer != null);

            return answers;
        }

        private void SaveAnswerFromAnswerDto(InterviewAnswerDto answerDto)
        {
            switch (answerDto.Type)
            {
                case AnswerType.Integer:
                    this.GetOrCreateAnswer<IntegerNumericAnswer>(answerDto.Id, answerDto.RosterVector)
                        .SetAnswer(answerDto.Answer == null ? null : (int?) Convert.ToInt32(answerDto.Answer));
                    break;
                case AnswerType.Decimal:
                    this.GetOrCreateAnswer<RealNumericAnswer>(answerDto.Id, answerDto.RosterVector)
                        .SetAnswer(answerDto.Answer == null ? null : (decimal?)Convert.ToDecimal(answerDto.Answer));
                    break;
                case AnswerType.DateTime:
                    this.GetOrCreateAnswer<DateTimeAnswer>(answerDto.Id, answerDto.RosterVector)
                        .SetAnswer(answerDto.Answer == null ? null : (DateTime?)Convert.ToDateTime(answerDto.Answer));
                    break;
                case AnswerType.OptionCodeArray:
                    this.GetOrCreateAnswer<MultiOptionAnswer>(answerDto.Id, answerDto.RosterVector)
                        .SetAnswers(answerDto.Answer as decimal[]);
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
                    if (geoAnswer!=null)
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

        private void DeclareAnswerAsValid(Guid id, decimal[] rosterVector)
        {
            var questionKey = ConversionHelper.ConvertIdAndRosterVectorToString(id, rosterVector);
            var answer = this.GetExistingAnswerOrNull(questionKey);
            if (answer != null)
            {
                answer.IsValid = true;
            }
            else
            {
                this.notAnsweredQuestionsValidityStatus[questionKey] = true;
            }
        }

        private void DeclareAnswerAsInvalid(Guid id, decimal[] rosterVector)
        {
            var questionKey = ConversionHelper.ConvertIdAndRosterVectorToString(id, rosterVector);
            var answer = this.GetExistingAnswerOrNull(questionKey);
            if (answer != null)
            {
                answer.IsValid = false;
            }
            else
            {
                this.notAnsweredQuestionsValidityStatus[questionKey] = false;
            }
        }

        private void DisableGroup(Guid id, decimal[] rosterVector)
        {
            var groupOrRoster = this.GetOrCreateGroupOrRoster(id, rosterVector);
            groupOrRoster.IsDisabled = true;
        }

        private void CommentQuestion(Guid id, decimal[] rosterVector, string comment)
        {
            var questionKey = ConversionHelper.ConvertIdAndRosterVectorToString(id, rosterVector);
            var answer = this.GetExistingAnswerOrNull(questionKey);
            if (answer != null)
            {
                answer.InterviewerComment = comment;
            }
            else
            {
                this.notAnsweredQuestionsInterviewerComments[questionKey] = comment;
            }
        }

        private void DisableQuestion(Guid id, decimal[] rosterVector)
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
            var rosterParentKey = ConversionHelper.ConvertIdAndRosterVectorToString(
                rosterInstance.GroupId,
                rosterInstance.OuterRosterVector);

            this.groups[rosterKey] = new InterviewRoster
            {
                Id = rosterIdentity.Id,
                RosterVector = rosterIdentity.RosterVector,
                ParentRosterVector = rosterInstance.OuterRosterVector,
                RowCode = rosterInstance.RosterInstanceId
            };
            if (!this.rosterInstancesIds.ContainsKey(rosterParentKey))
            {
                this.rosterInstancesIds.Add(rosterParentKey, new List<Identity>());
            }

            this.rosterInstancesIds[rosterParentKey].Add(rosterIdentity);
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
                yield return this.GetRosterTitle(new Identity(parentRosterId, parentRosterVector));
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

        public int CountActiveInterviewerQuestionsInGroupRecursively(Identity groupIdentity)
        {
            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();
            IEnumerable<Guid> allQuestionsInGroup = questionnaire.GetAllUnderlyingInterviewerQuestions(groupIdentity.Id);

            var questionInstances = this.GetInstancesOfQuestionsWithSameAndDeeperRosterLevelOrThrow(this.interviewState,
               allQuestionsInGroup,
               groupIdentity.RosterVector,
               questionnaire,
               GetRosterInstanceIds);

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
                .Count;
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

        public int CountInvalidQuestionsInInterview()
        {
            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();
            var sectionInstances = questionnaire.GetAllSections().Select(x => new Identity(x, new decimal[0]));

            return sectionInstances.Sum(section => this.CountInvalidInterviewerAnswersInGroupRecursively(section));
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

            return this.Answers.Where(x => questionInstances.Contains(x.Key)).Count(
                x => x.Value != null 
                && x.Value.IsEnabled 
                && x.Value.IsAnswered 
                && !x.Value.IsValid);
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

        public IEnumerable<Identity> GetInterviewerEntities(Identity groupIdentity)
        {
            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();
            IEnumerable<Guid> allEntitiesInGroup = questionnaire.GetChildEntityIds(groupIdentity.Id);

            foreach (var entity in allEntitiesInGroup)
            {
                if (questionnaire.IsRosterGroup(entity))
                {
                    foreach (var rosterInstance in this.GetInstancesOfGroupsByGroupIdWithSameAndDeeperRosterLevelOrThrow(this.interviewState, entity,
                            groupIdentity.RosterVector, questionnaire, GetRosterInstanceIds))
                    {
                        yield return rosterInstance;
                    }
                }
                else
                {
                    if (questionnaire.IsQuestion(entity) && !questionnaire.IsInterviewierQuestion(entity)) continue;

                    foreach (var entityInstance in this.GetInstancesOfEntitiesWithSameAndDeeperRosterLevelOrThrow(this.interviewState, entity,
                            groupIdentity.RosterVector, questionnaire, GetRosterInstanceIds))
                    {
                        yield return entityInstance;
                    }
                }
            }
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

            if (this.notAnsweredQuestionsEnablementStatus.ContainsKey(entityKey))
            {
                return this.notAnsweredQuestionsEnablementStatus[entityKey];
            }

            return true;
        }

        public bool WasAnswered(Identity entityIdentity)
        {
            var questionKey = ConversionHelper.ConvertIdentityToString(entityIdentity);
            if (!this.Answers.ContainsKey(questionKey))
                return false;

            if (!this.IsEnabled(entityIdentity))
                return false;

            var interviewAnswerModel = this.Answers[questionKey];
            return interviewAnswerModel.IsAnswered;
        }

        public string GetInterviewerAnswerComment(Identity entityIdentity)
        {
            var questionKey = ConversionHelper.ConvertIdentityToString(entityIdentity);
            if (!this.Answers.ContainsKey(questionKey))
            {
                return this.notAnsweredQuestionsInterviewerComments.ContainsKey(questionKey)
                    ? this.notAnsweredQuestionsInterviewerComments[questionKey]
                    : null;
            }

            var interviewAnswerModel = this.Answers[questionKey];
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
            return this.cachedQuestionnaire ?? (this.cachedQuestionnaire = GetHistoricalQuestionnaireOrThrow(
                this.QuestionnaireIdentity.QuestionnaireId, 
                this.QuestionnaireIdentity.Version));
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