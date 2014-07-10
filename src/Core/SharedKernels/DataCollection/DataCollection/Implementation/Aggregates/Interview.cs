using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Main.Core.Entities.SubEntities;
using Microsoft.Practices.ServiceLocation;
using Ncqrs.Domain;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Preloading;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Snapshots;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.ExpressionProcessing;
using Identity = WB.Core.SharedKernels.ExpressionProcessing.Identity;
using IExpressionProcessor = WB.Core.SharedKernels.ExpressionProcessor.Services.IExpressionProcessor;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    internal class Interview : AggregateRootMappedByConvention, ISnapshotable<InterviewState>
    {
        #region Constants

        private static readonly decimal[] EmptyRosterVector = { };

        #endregion

        #region State

        private Guid questionnaireId;
        private long questionnaireVersion;
        private bool wasCompleted;
        private InterviewStatus status;

        private IInterviewExpressionState expressionProcessorStatePrototype = new StronglyTypedInterviewEvaluator();
        private ExpressionProcessing.ExpressionProcessor expressionSharpProcessor = new ExpressionProcessing.ExpressionProcessor();

        private readonly InterviewStateDependentOnAnswers interviewState = new InterviewStateDependentOnAnswers();

        private void Apply(InterviewCreated @event)
        {
            this.questionnaireId = @event.QuestionnaireId;
            this.questionnaireVersion = @event.QuestionnaireVersion;
        }

        private void Apply(InterviewFromPreloadedDataCreated @event)
        {
            this.questionnaireId = @event.QuestionnaireId;
            this.questionnaireVersion = @event.QuestionnaireVersion;
        }

        private void Apply(InterviewForTestingCreated @event)
        {
            this.questionnaireId = @event.QuestionnaireId;
            this.questionnaireVersion = @event.QuestionnaireVersion;
        }

        private void Apply(InterviewOnClientCreated @event)
        {
            this.questionnaireId = @event.QuestionnaireId;
            this.questionnaireVersion = @event.QuestionnaireVersion;
        }

        internal void Apply(InterviewSynchronized @event)
        {
            this.questionnaireId = @event.InterviewData.QuestionnaireId;
            this.questionnaireVersion = @event.InterviewData.QuestionnaireVersion;
            this.status = @event.InterviewData.Status;
            this.wasCompleted = @event.InterviewData.WasCompleted;

            this.interviewState.AnswersSupportedInExpressions = @event.InterviewData.Answers == null
                ? new Dictionary<string, object>()
                : @event.InterviewData.Answers
                    .Where(
                        question =>
                            !(question.Answer is GeoPosition || question.Answer is decimal[] || question.Answer is decimal[][] ||
                                question.Answer is Tuple<decimal, string>[]))
                    .ToDictionary(
                        question => ConversionHelper.ConvertIdAndRosterVectorToString(question.Id, question.QuestionPropagationVector),
                        question => question.Answer);

            this.interviewState.LinkedSingleOptionAnswersBuggy = @event.InterviewData.Answers == null
                ? new Dictionary<string, Tuple<Guid, decimal[], decimal[]>>()
                : @event.InterviewData.Answers
                    .Where(question => question.Answer is decimal[]) // bug: here we get multioption questions as well
                    .ToDictionary(
                        question => ConversionHelper.ConvertIdAndRosterVectorToString(question.Id, question.QuestionPropagationVector),
                        question => Tuple.Create(question.Id, question.QuestionPropagationVector, (decimal[])question.Answer));

            this.interviewState.LinkedMultipleOptionsAnswers = @event.InterviewData.Answers == null
                ? new Dictionary<string, Tuple<Guid, decimal[], decimal[][]>>()
                : @event.InterviewData.Answers
                    .Where(question => question.Answer is decimal[][])
                    .ToDictionary(
                        question => ConversionHelper.ConvertIdAndRosterVectorToString(question.Id, question.QuestionPropagationVector),
                        question => Tuple.Create(question.Id, question.QuestionPropagationVector, (decimal[][])question.Answer));

            this.interviewState.TextListAnswers = @event.InterviewData.Answers == null
                ? new Dictionary<string, Tuple<decimal, string>[]>()
                : @event.InterviewData.Answers
                    .Where(question => question.Answer is Tuple<decimal, string>[])
                    .ToDictionary(
                        question => ConversionHelper.ConvertIdAndRosterVectorToString(question.Id, question.QuestionPropagationVector),
                        question => (Tuple<decimal, string>[])question.Answer
                    );

            this.interviewState.AnsweredQuestions = new HashSet<string>(
                @event.InterviewData.Answers.Select(
                    question => ConversionHelper.ConvertIdAndRosterVectorToString(question.Id, question.QuestionPropagationVector)));

            this.interviewState.DisabledGroups = ToHashSetOfIdAndRosterVectorStrings(@event.InterviewData.DisabledGroups);
            this.interviewState.DisabledQuestions = ToHashSetOfIdAndRosterVectorStrings(@event.InterviewData.DisabledQuestions);

            this.interviewState.RosterGroupInstanceIds = BuildRosterInstanceIdsFromSynchronizationDto(@event.InterviewData);

            var orderedRosterInstances = @event.InterviewData.RosterGroupInstances.OrderBy(x => ConversionHelper.ConvertIdAndRosterVectorToString(x.Key.Id, x.Key.InterviewItemPropagationVector)).Select(x => x.Value).ToList();

            foreach (RosterSynchronizationDto roster in orderedRosterInstances.SelectMany(rosters => rosters))
            {
                this.expressionProcessorStatePrototype.AddRoster(roster.RosterId, roster.OuterScopePropagationVector, roster.RosterInstanceId, roster.SortIndex);
            }

            if (@event.InterviewData.Answers != null)
            {
                foreach (var question in @event.InterviewData.Answers)
                {
                    decimal[] questionPropagationVector = question.QuestionPropagationVector;
                    if (question.Answer is int)
                    {
                        this.expressionProcessorStatePrototype.UpdateIntAnswer(question.Id, questionPropagationVector, Convert.ToInt32(question.Answer));
                    }
                    if (question.Answer is decimal)
                    {
                        this.expressionProcessorStatePrototype.UpdateDecimalAnswer(question.Id, questionPropagationVector, Convert.ToDecimal(question.Answer));
                        this.expressionProcessorStatePrototype.UpdateSingleOptionAnswer(question.Id, questionPropagationVector, Convert.ToDecimal(question.Answer));
                    }
                    var answer = question.Answer as string;
                    if (answer != null)
                    {
                        this.expressionProcessorStatePrototype.UpdateTextAnswer(question.Id, questionPropagationVector, answer);
                        this.expressionProcessorStatePrototype.UpdateQrBarcodeAnswer(question.Id, questionPropagationVector, answer);
                    }

                    if (question.Answer is decimal[])
                    {
                        this.expressionProcessorStatePrototype.UpdateMultiOptionAnswer(question.Id, questionPropagationVector, (decimal[])(question.Answer));
                        this.expressionProcessorStatePrototype.UpdateLinkedSingleOptionAnswer(question.Id, questionPropagationVector, (decimal[])(question.Answer));
                    }
                    var geoAnswer = question.Answer as GeoPosition;
                    if (geoAnswer != null)
                    {
                        this.expressionProcessorStatePrototype.UpdateGeoLocationAnswer(question.Id, questionPropagationVector, geoAnswer.Latitude, geoAnswer.Longitude);
                    }
                    if (question.Answer is DateTime)
                    {
                        this.expressionProcessorStatePrototype.UpdateDateAnswer(question.Id, questionPropagationVector, (DateTime)question.Answer);
                    }
                    if (question.Answer is decimal[][])
                    {
                        this.expressionProcessorStatePrototype.UpdateLinkedMultiOptionAnswer(question.Id, questionPropagationVector, (decimal[][])(question.Answer));
                    }
                    if (question.Answer is Tuple<decimal, string>[])
                    {
                        this.expressionProcessorStatePrototype.UpdateTextListAnswer(question.Id, questionPropagationVector, (Tuple<decimal, string>[])(question.Answer));
                    }
                }
            }

            this.interviewState.ValidAnsweredQuestions = ToHashSetOfIdAndRosterVectorStrings(@event.InterviewData.ValidAnsweredQuestions);
            this.interviewState.InvalidAnsweredQuestions = ToHashSetOfIdAndRosterVectorStrings(@event.InterviewData.InvalidAnsweredQuestions);
        }

        private void Apply(SynchronizationMetadataApplied @event)
        {
            this.questionnaireId = @event.QuestionnaireId;
            this.status = @event.Status;
        }

        internal void Apply(TextQuestionAnswered @event)
        {
            string questionKey = ConversionHelper.ConvertIdAndRosterVectorToString(@event.QuestionId, @event.PropagationVector);

            this.interviewState.AnswersSupportedInExpressions[questionKey] = @event.Answer;
            this.interviewState.AnsweredQuestions.Add(questionKey);

            expressionProcessorStatePrototype.UpdateTextAnswer(@event.QuestionId, @event.PropagationVector, @event.Answer);
        }

        internal void Apply(QRBarcodeQuestionAnswered @event)
        {
            string questionKey = ConversionHelper.ConvertIdAndRosterVectorToString(@event.QuestionId, @event.PropagationVector);

            this.interviewState.AnswersSupportedInExpressions[questionKey] = @event.Answer;
            this.interviewState.AnsweredQuestions.Add(questionKey);

            expressionProcessorStatePrototype.UpdateQrBarcodeAnswer(@event.QuestionId, @event.PropagationVector, @event.Answer);
        }

        private void Apply(NumericQuestionAnswered @event)
        {
            string questionKey = ConversionHelper.ConvertIdAndRosterVectorToString(@event.QuestionId, @event.PropagationVector);

            this.interviewState.AnswersSupportedInExpressions[questionKey] = @event.Answer;
            this.interviewState.AnsweredQuestions.Add(questionKey);

            expressionProcessorStatePrototype.UpdateDecimalAnswer(@event.QuestionId, @event.PropagationVector, @event.Answer);
        }

        internal void Apply(NumericRealQuestionAnswered @event)
        {
            string questionKey = ConversionHelper.ConvertIdAndRosterVectorToString(@event.QuestionId, @event.PropagationVector);

            this.interviewState.AnswersSupportedInExpressions[questionKey] = @event.Answer;
            this.interviewState.AnsweredQuestions.Add(questionKey);

            expressionProcessorStatePrototype.UpdateDecimalAnswer(@event.QuestionId, @event.PropagationVector, @event.Answer);
        }

        internal void Apply(NumericIntegerQuestionAnswered @event)
        {
            string questionKey = ConversionHelper.ConvertIdAndRosterVectorToString(@event.QuestionId, @event.PropagationVector);

            this.interviewState.AnswersSupportedInExpressions[questionKey] = @event.Answer;
            this.interviewState.AnsweredQuestions.Add(questionKey);

            expressionProcessorStatePrototype.UpdateIntAnswer(@event.QuestionId, @event.PropagationVector, @event.Answer);
        }

        internal void Apply(DateTimeQuestionAnswered @event)
        {
            string questionKey = ConversionHelper.ConvertIdAndRosterVectorToString(@event.QuestionId, @event.PropagationVector);

            this.interviewState.AnswersSupportedInExpressions[questionKey] = @event.Answer;
            this.interviewState.AnsweredQuestions.Add(questionKey);

            expressionProcessorStatePrototype.UpdateDateAnswer(@event.QuestionId, @event.PropagationVector, @event.Answer);
        }

        internal void Apply(SingleOptionQuestionAnswered @event)
        {
            string questionKey = ConversionHelper.ConvertIdAndRosterVectorToString(@event.QuestionId, @event.PropagationVector);

            this.interviewState.AnswersSupportedInExpressions[questionKey] = @event.SelectedValue;
            this.interviewState.AnsweredQuestions.Add(questionKey);

            expressionProcessorStatePrototype.UpdateSingleOptionAnswer(@event.QuestionId, @event.PropagationVector, @event.SelectedValue);
        }

        internal void Apply(MultipleOptionsQuestionAnswered @event)
        {
            string questionKey = ConversionHelper.ConvertIdAndRosterVectorToString(@event.QuestionId, @event.PropagationVector);

            this.interviewState.AnswersSupportedInExpressions[questionKey] = @event.SelectedValues;

            if (@event.SelectedValues.Length != 0)
            {
                this.interviewState.AnsweredQuestions.Add(questionKey);
            }
            else
            {
                this.interviewState.AnsweredQuestions.Remove(questionKey);
            }
            expressionProcessorStatePrototype.UpdateMultiOptionAnswer(@event.QuestionId, @event.PropagationVector, @event.SelectedValues);
        }

        internal void Apply(GeoLocationQuestionAnswered @event)
        {
            string questionKey = ConversionHelper.ConvertIdAndRosterVectorToString(@event.QuestionId, @event.PropagationVector);

            this.interviewState.AnsweredQuestions.Add(questionKey);

            expressionProcessorStatePrototype.UpdateGeoLocationAnswer(@event.QuestionId, @event.PropagationVector, @event.Latitude, @event.Longitude);
        }

        internal void Apply(TextListQuestionAnswered @event)
        {
            string questionKey = ConversionHelper.ConvertIdAndRosterVectorToString(@event.QuestionId, @event.PropagationVector);
            this.interviewState.TextListAnswers[questionKey] = @event.Answers;
            this.interviewState.AnsweredQuestions.Add(questionKey);

            expressionProcessorStatePrototype.UpdateTextListAnswer(@event.QuestionId, @event.PropagationVector, @event.Answers);
        }

        private void Apply(SingleOptionLinkedQuestionAnswered @event)
        {
            string questionKey = ConversionHelper.ConvertIdAndRosterVectorToString(@event.QuestionId, @event.PropagationVector);

            this.interviewState.LinkedSingleOptionAnswersBuggy[questionKey] = Tuple.Create(@event.QuestionId, @event.PropagationVector,
                @event.SelectedPropagationVector);
            this.interviewState.AnsweredQuestions.Add(questionKey);

            expressionProcessorStatePrototype.UpdateLinkedSingleOptionAnswer(@event.QuestionId, @event.PropagationVector, @event.SelectedPropagationVector);
        }

        internal void Apply(MultipleOptionsLinkedQuestionAnswered @event)
        {
            string questionKey = ConversionHelper.ConvertIdAndRosterVectorToString(@event.QuestionId, @event.PropagationVector);

            this.interviewState.LinkedMultipleOptionsAnswers[questionKey] = Tuple.Create(@event.QuestionId, @event.PropagationVector,
                @event.SelectedPropagationVectors);

            if (@event.SelectedPropagationVectors.Length != 0)
            {
                this.interviewState.AnsweredQuestions.Add(questionKey);
            }
            else
            {
                this.interviewState.AnsweredQuestions.Remove(questionKey);
            }

            expressionProcessorStatePrototype.UpdateLinkedMultiOptionAnswer(@event.QuestionId, @event.PropagationVector, @event.SelectedPropagationVectors);
        }

        private void Apply(AnswerDeclaredValid @event)
        {
            string questionKey = ConversionHelper.ConvertIdAndRosterVectorToString(@event.QuestionId, @event.PropagationVector);

            this.interviewState.ValidAnsweredQuestions.Add(questionKey);
            this.interviewState.InvalidAnsweredQuestions.Remove(questionKey);
        }

        private void Apply(AnswerDeclaredInvalid @event)
        {
            string questionKey = ConversionHelper.ConvertIdAndRosterVectorToString(@event.QuestionId, @event.PropagationVector);

            this.interviewState.ValidAnsweredQuestions.Remove(questionKey);
            this.interviewState.InvalidAnsweredQuestions.Add(questionKey);
        }

        private void Apply(AnswersDeclaredValid @event)
        {
            this.interviewState.DeclareAnswersValid(@event.Questions);
        }

        internal void Apply(AnswersDeclaredInvalid @event)
        {
            this.interviewState.DeclareAnswersInvalid(@event.Questions);
        }

        internal void Apply(GroupDisabled @event)
        {
            string groupKey = ConversionHelper.ConvertIdAndRosterVectorToString(@event.GroupId, @event.PropagationVector);

            this.interviewState.DisabledGroups.Add(groupKey);
        }

        internal void Apply(GroupEnabled @event)
        {
            string groupKey = ConversionHelper.ConvertIdAndRosterVectorToString(@event.GroupId, @event.PropagationVector);

            this.interviewState.DisabledGroups.Remove(groupKey);
        }

        internal void Apply(GroupsDisabled @event)
        {
            this.interviewState.DisableGroups(@event.Groups);

            expressionProcessorStatePrototype.DisableGroups(@event.Groups.ToIdentities());
        }

        internal void Apply(GroupsEnabled @event)
        {
            this.interviewState.EnableGroups(@event.Groups);

            expressionProcessorStatePrototype.EnableGroups(@event.Groups.ToIdentities());
        }

        internal void Apply(QuestionDisabled @event)
        {
            string questionKey = ConversionHelper.ConvertIdAndRosterVectorToString(@event.QuestionId, @event.PropagationVector);

            this.interviewState.DisabledQuestions.Add(questionKey);
        }

        internal void Apply(QuestionEnabled @event)
        {
            string questionKey = ConversionHelper.ConvertIdAndRosterVectorToString(@event.QuestionId, @event.PropagationVector);

            this.interviewState.DisabledQuestions.Remove(questionKey);
        }

        internal void Apply(QuestionsDisabled @event)
        {
            this.interviewState.DisableQuestions(@event.Questions);

            expressionProcessorStatePrototype.DisableQuestions(@event.Questions.ToIdentities());
        }

        internal void Apply(QuestionsEnabled @event)
        {
            this.interviewState.EnableQuestions(@event.Questions);

            expressionProcessorStatePrototype.EnableQuestions(@event.Questions.ToIdentities());

        }

        internal void Apply(AnswerCommented @event)
        {
            this.interviewState.AnswerComments.Add(new AnswerComment(@event.UserId, @event.CommentTime, @event.Comment, @event.QuestionId, @event.PropagationVector));
        }

        private void Apply(FlagSetToAnswer @event) { }

        private void Apply(FlagRemovedFromAnswer @event) { }

        private void Apply(GroupPropagated @event)
        {
            string rosterGroupKey = ConversionHelper.ConvertIdAndRosterVectorToString(@event.GroupId, @event.OuterScopePropagationVector);
            var rosterRowInstances = new DistinctDecimalList();

            for (int i = 0; i < @event.Count; i++)
            {
                rosterRowInstances.Add(i);
            }

            this.interviewState.RosterGroupInstanceIds[rosterGroupKey] = rosterRowInstances;
        }

        internal void Apply(RosterRowAdded @event)
        {
            string rosterGroupKey = ConversionHelper.ConvertIdAndRosterVectorToString(@event.GroupId, @event.OuterRosterVector);
            DistinctDecimalList rosterRowInstances = this.interviewState.RosterGroupInstanceIds.ContainsKey(rosterGroupKey)
                ? this.interviewState.RosterGroupInstanceIds[rosterGroupKey]
                : new DistinctDecimalList();

            rosterRowInstances.Add(@event.RosterInstanceId);

            this.interviewState.RosterGroupInstanceIds[rosterGroupKey] = rosterRowInstances;

            expressionProcessorStatePrototype.AddRoster(@event.GroupId, @event.OuterRosterVector, @event.RosterInstanceId, @event.SortIndex);
        }

        private void Apply(RosterRowRemoved @event)
        {
            string rosterGroupKey = ConversionHelper.ConvertIdAndRosterVectorToString(@event.GroupId, @event.OuterRosterVector);

            var rosterRowInstances = this.interviewState.RosterGroupInstanceIds.ContainsKey(rosterGroupKey)
                ? this.interviewState.RosterGroupInstanceIds[rosterGroupKey]
                : new DistinctDecimalList();
            rosterRowInstances.Remove(@event.RosterInstanceId);

            this.interviewState.RosterGroupInstanceIds[rosterGroupKey] = rosterRowInstances;
        }

        private void Apply(RosterRowTitleChanged @event) { }

        private void Apply(RosterInstancesTitleChanged @event) { }

        internal void Apply(RosterInstancesAdded @event)
        {
            this.interviewState.AddRosterInstances(@event.Instances);
            foreach (var instance in @event.Instances)
            {
                expressionProcessorStatePrototype.AddRoster(instance.GroupId, instance.OuterRosterVector, instance.RosterInstanceId, instance.SortIndex);
            }
        }

        private void Apply(RosterInstancesRemoved @event)
        {
            this.interviewState.RemoveRosterInstances(@event.Instances);
            foreach (var instance in @event.Instances)
            {
                expressionProcessorStatePrototype.RemoveRoster(instance.GroupId, instance.OuterRosterVector, instance.RosterInstanceId);
            }
        }

        internal void Apply(InterviewStatusChanged @event)
        {
            this.status = @event.Status;
        }

        private void Apply(SupervisorAssigned @event) { }

        private void Apply(InterviewerAssigned @event) { }

        private void Apply(InterviewDeleted @event) { }

        private void Apply(InterviewSentToHeadquarters @event) { }

        private void Apply(InterviewRestored @event) { }

        private void Apply(InterviewCompleted @event)
        {
            this.wasCompleted = true;
        }

        private void Apply(InterviewRestarted @event) { }

        private void Apply(InterviewApproved @event) { }

        private void Apply(InterviewApprovedByHQ @event) { }

        private void Apply(InterviewRejected @event)
        {
            this.wasCompleted = false;
        }

        private void Apply(InterviewRejectedByHQ @event) { }

        private void Apply(InterviewDeclaredValid @event) { }

        private void Apply(InterviewDeclaredInvalid @event) { }

        private void Apply(AnswerRemoved @event)
        {
            string questionKey = ConversionHelper.ConvertIdAndRosterVectorToString(@event.QuestionId, @event.PropagationVector);

            this.interviewState.AnswersSupportedInExpressions.Remove(questionKey);
            this.interviewState.LinkedSingleOptionAnswersBuggy.Remove(questionKey);
            this.interviewState.LinkedMultipleOptionsAnswers.Remove(questionKey);
            this.interviewState.TextListAnswers.Remove(questionKey);
            this.interviewState.AnsweredQuestions.Remove(questionKey);
            this.interviewState.DisabledQuestions.Remove(questionKey);
            this.interviewState.ValidAnsweredQuestions.Remove(questionKey);
            this.interviewState.InvalidAnsweredQuestions.Remove(questionKey);
        }

        private void Apply(AnswersRemoved @event)
        {
            this.interviewState.RemoveAnswers(@event.Questions);
        }


        public InterviewState CreateSnapshot()
        {
            return new InterviewState(
                this.questionnaireId,
                this.questionnaireVersion,
                this.status,
                this.interviewState.AnswersSupportedInExpressions,
                this.interviewState.LinkedSingleOptionAnswersBuggy,
                this.interviewState.LinkedMultipleOptionsAnswers,
                this.interviewState.TextListAnswers,
                this.interviewState.AnsweredQuestions,
                this.interviewState.DisabledGroups,
                this.interviewState.DisabledQuestions,
                this.interviewState.RosterGroupInstanceIds,
                this.interviewState.ValidAnsweredQuestions,
                this.interviewState.InvalidAnsweredQuestions,
                this.wasCompleted,
                this.expressionProcessorStatePrototype);
        }

        public void RestoreFromSnapshot(InterviewState snapshot)
        {
            this.expressionProcessorStatePrototype = snapshot.ExpressionProcessorState;
            this.questionnaireId = snapshot.QuestionnaireId;
            this.questionnaireVersion = snapshot.QuestionnaireVersion;
            this.status = snapshot.Status;
            this.interviewState.AnswersSupportedInExpressions = snapshot.AnswersSupportedInExpressions;
            this.interviewState.LinkedSingleOptionAnswersBuggy = snapshot.LinkedSingleOptionAnswers;
            this.interviewState.LinkedMultipleOptionsAnswers = snapshot.LinkedMultipleOptionsAnswers;
            this.interviewState.TextListAnswers = snapshot.TextListAnswers;
            this.interviewState.AnsweredQuestions = snapshot.AnsweredQuestions;
            this.interviewState.DisabledGroups = snapshot.DisabledGroups;
            this.interviewState.DisabledQuestions = snapshot.DisabledQuestions;
            this.interviewState.RosterGroupInstanceIds = snapshot.RosterGroupInstanceIds;
            this.interviewState.ValidAnsweredQuestions = snapshot.ValidAnsweredQuestions;
            this.interviewState.InvalidAnsweredQuestions = snapshot.InvalidAnsweredQuestions;
            this.wasCompleted = snapshot.WasCompleted;
        }

        #endregion

        #region Dependencies

        private ILogger Logger
        {
            get { return ServiceLocator.Current.GetInstance<ILogger>(); }
        }

        /// <remarks>
        /// Repository operations are time-consuming.
        /// So this repository may be used only in command handlers.
        /// And should never be used in event handlers!!
        /// </remarks>
        private IQuestionnaireRepository QuestionnaireRepository
        {
            get { return ServiceLocator.Current.GetInstance<IQuestionnaireRepository>(); }
        }

        /// <remarks>
        /// All operations with expressions are time-consuming.
        /// So this processor may be used only in command handlers.
        /// And should never be used in event handlers!!
        /// </remarks>
        private IExpressionProcessor ExpressionProcessor
        {
            get { return ServiceLocator.Current.GetInstance<IExpressionProcessor>(); }
        }

        #endregion

        #region .ctors

        /// <remarks>Is used to restore aggregate from event stream.</remarks>
        public Interview() { }

        public Interview(Guid id, Guid userId, Guid questionnaireId, long version, PreloadedDataDto preloadedData, DateTime answersTime,
            Guid supervisorId)
            : base(id)
        {
            IQuestionnaire questionnaire = this.GetHistoricalQuestionnaireOrThrow(questionnaireId, version);

            var interviewChangeStructures = new InterviewChangeStructures();

            InitInterview(questionnaire, interviewChangeStructures);

            var fixedRosterCalculationDatas = this.CalculateFixedRostersData(interviewChangeStructures.State, questionnaire);

            foreach (var fixedRosterCalculationData in fixedRosterCalculationDatas)
            {
                var fixedRosterChanges = new InterviewChanges(null, null, null, fixedRosterCalculationData, null, null, null);
                interviewChangeStructures.State.ApplyInterviewChanges(fixedRosterChanges);
                interviewChangeStructures.Changes.Add(fixedRosterChanges);
            }

            var orderedData = preloadedData.Data.OrderBy(x => x.RosterVector.Length).ToArray();
            foreach (var preloadedLevel in orderedData)
            {
                var answersToFeaturedQuestions = preloadedLevel.Answers;
                this.ValidatePrefilledQuestions(questionnaire, answersToFeaturedQuestions, preloadedLevel.RosterVector,
                    interviewChangeStructures.State, false);

                var newAnswers =
                    answersToFeaturedQuestions.ToDictionary(
                        answersToFeaturedQuestion => new Identity(answersToFeaturedQuestion.Key, preloadedLevel.RosterVector),
                        answersToFeaturedQuestion => answersToFeaturedQuestion.Value);

                foreach (var newAnswer in answersToFeaturedQuestions)
                {
                    string key = ConversionHelper.ConvertIdAndRosterVectorToString(newAnswer.Key, preloadedLevel.RosterVector);

                    interviewChangeStructures.State.AnswersSupportedInExpressions[key] = newAnswer.Value;
                    interviewChangeStructures.State.AnsweredQuestions.Add(key);
                }

                this.CalculateChangesByFeaturedQuestion(interviewChangeStructures, userId, questionnaire, answersToFeaturedQuestions,
                    answersTime,
                    newAnswers, preloadedLevel.RosterVector);
            }

            //apply events
            this.ApplyEvent(new InterviewFromPreloadedDataCreated(userId, questionnaireId, questionnaire.Version));
            this.ApplyInterviewChanges(interviewChangeStructures.Changes);
            this.ApplyEvent(new SupervisorAssigned(userId, supervisorId));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.SupervisorAssigned, comment: null));
        }

        public Interview(Guid id, Guid userId, Guid questionnaireId, Dictionary<Guid, object> answersToFeaturedQuestions,
            DateTime answersTime, Guid supervisorId)
            : base(id)
        {
            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow(questionnaireId);

            var interviewChangeStructures = new InterviewChangeStructures();
            var newAnswers =
                answersToFeaturedQuestions.ToDictionary(
                    answersToFeaturedQuestion => new Identity(answersToFeaturedQuestion.Key, EmptyRosterVector),
                    answersToFeaturedQuestion => answersToFeaturedQuestion.Value);

            this.ValidatePrefilledQuestions(questionnaire, answersToFeaturedQuestions, EmptyRosterVector, interviewChangeStructures.State);
            foreach (var newAnswer in answersToFeaturedQuestions)
            {
                string key = ConversionHelper.ConvertIdAndRosterVectorToString(newAnswer.Key, EmptyRosterVector);

                interviewChangeStructures.State.AnswersSupportedInExpressions[key] = newAnswer.Value;
                interviewChangeStructures.State.AnsweredQuestions.Add(key);
            }

            InitInterview(questionnaire, interviewChangeStructures);

            this.CalculateChangesByFeaturedQuestion(interviewChangeStructures, userId, questionnaire, answersToFeaturedQuestions,
                answersTime, newAnswers);

            var fixedRosterCalculationDatas = this.CalculateFixedRostersData(interviewChangeStructures.State, questionnaire);

            //apply events
            this.ApplyEvent(new InterviewCreated(userId, questionnaireId, questionnaire.Version));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.Created, comment: null));
            this.ApplyInterviewChanges(interviewChangeStructures.Changes);
            this.ApplyRostersEvents(fixedRosterCalculationDatas.ToArray());
            this.ApplyEvent(new SupervisorAssigned(userId, supervisorId));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.SupervisorAssigned, comment: null));
        }

        public Interview(Guid id, Guid userId, Guid questionnaireId, Dictionary<Guid, object> answersToFeaturedQuestions,
            DateTime answersTime)
            : base(id)
        {
            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow(questionnaireId);

            var interviewChangeStructures = new InterviewChangeStructures();

            this.ValidatePrefilledQuestions(questionnaire, answersToFeaturedQuestions, EmptyRosterVector, interviewChangeStructures.State);
            var newAnswers =
                answersToFeaturedQuestions.ToDictionary(
                    answersToFeaturedQuestion => new Identity(answersToFeaturedQuestion.Key, EmptyRosterVector),
                    answersToFeaturedQuestion => answersToFeaturedQuestion.Value);

            foreach (var newAnswer in answersToFeaturedQuestions)
            {
                string key = ConversionHelper.ConvertIdAndRosterVectorToString(newAnswer.Key, EmptyRosterVector);

                interviewChangeStructures.State.AnswersSupportedInExpressions[key] = newAnswer.Value;
                interviewChangeStructures.State.AnsweredQuestions.Add(key);
            }

            InitInterview(questionnaire, interviewChangeStructures);
            this.CalculateChangesByFeaturedQuestion(interviewChangeStructures, userId, questionnaire, answersToFeaturedQuestions,
                answersTime, newAnswers);
            var fixedRosterCalculationDatas = this.CalculateFixedRostersData(interviewChangeStructures.State, questionnaire);

            //apply events
            this.ApplyEvent(new InterviewForTestingCreated(userId, questionnaireId, questionnaire.Version));

            this.ApplyInterviewChanges(interviewChangeStructures.Changes);
            this.ApplyRostersEvents(fixedRosterCalculationDatas.ToArray());
        }

        public Interview(Guid id, Guid userId, Guid questionnaireId, long? questionnaireVersion, DateTime answersTime, Guid supervisorId)
            : base(id)
        {
            IQuestionnaire questionnaire = questionnaireVersion.HasValue
                ? this.GetHistoricalQuestionnaireOrThrow(questionnaireId, questionnaireVersion.Value)
                : this.GetQuestionnaireOrThrow(questionnaireId);

            InterviewChangeStructures interviewChangeStructures = new InterviewChangeStructures();

            InitInterview(questionnaire, interviewChangeStructures);

            var fixedRosterCalculationDatas = this.CalculateFixedRostersData(interviewChangeStructures.State, questionnaire);

            //apply events
            this.ApplyEvent(new InterviewOnClientCreated(userId, questionnaireId, questionnaire.Version));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.Created, comment: null));

            this.ApplyInterviewChanges(interviewChangeStructures.Changes);
            this.ApplyRostersEvents(fixedRosterCalculationDatas.ToArray());
            this.ApplyEvent(new SupervisorAssigned(userId, supervisorId));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.SupervisorAssigned, comment: null));

            this.ApplyEvent(new InterviewerAssigned(userId, userId));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.InterviewerAssigned, comment: null));
        }

        public Interview(Guid id, Guid userId, Guid questionnaireId, long questionnaireVersion,
            InterviewStatus interviewStatus, AnsweredQuestionSynchronizationDto[] featuredQuestionsMeta, bool isValid)
            : base(id)
        {
            this.ApplyEvent(new InterviewOnClientCreated(userId, questionnaireId, questionnaireVersion));
            this.ApplyEvent(new SynchronizationMetadataApplied(userId, questionnaireId, interviewStatus, featuredQuestionsMeta, true, null));
            this.ApplyValidationEvent(isValid);
        }

        public Interview(Guid id, Guid userId, Guid questionnaireId, InterviewStatus interviewStatus,
            AnsweredQuestionSynchronizationDto[] featuredQuestionsMeta, string comments, bool valid, bool createdOnClient)
            : base(id)
        {
            this.ApplySynchronizationMetadata(id, userId, questionnaireId, interviewStatus, featuredQuestionsMeta, comments, valid, createdOnClient);
        }

        public Interview(Guid id, Guid userId, Guid supervisorId, InterviewSynchronizationDto interviewDto, DateTime synchronizationTime)
            : base(id)
        {
            this.SynchronizeInterviewFromHeadquarters(id, userId, supervisorId, interviewDto, synchronizationTime);
        }

        #endregion

        #region StaticMethods

        private static void InitInterview(IQuestionnaire questionnaire, InterviewChangeStructures interviewState)
        {
            List<Identity> initiallyDisabledGroups = GetGroupsToBeDisabledInJustCreatedInterview(questionnaire);
            List<Identity> initiallyDisabledQuestions = GetQuestionsToBeDisabledInJustCreatedInterview(questionnaire);
            List<Identity> initiallyInvalidQuestions = GetQuestionsToBeInvalidInJustCreatedInterview(questionnaire, initiallyDisabledGroups,
                initiallyDisabledQuestions);

            var enablementChanges = new EnablementChanges(initiallyDisabledGroups, null, initiallyDisabledQuestions, null);
            var validityChanges = new ValidityChanges(null, initiallyInvalidQuestions);

            var interviewChanges = new InterviewChanges(null, enablementChanges, validityChanges, null, null, null, null);

            interviewState.State.ApplyInterviewChanges(interviewChanges);

            interviewState.Changes.Add(interviewChanges);
        }

        private static Dictionary<string, DistinctDecimalList> BuildRosterInstanceIdsFromSynchronizationDto(InterviewSynchronizationDto synchronizationDto)
        {
            return synchronizationDto.RosterGroupInstances.ToDictionary(
                pair => ConversionHelper.ConvertIdAndRosterVectorToString(pair.Key.Id, pair.Key.InterviewItemPropagationVector),
                pair => new DistinctDecimalList(pair.Value.Select(rosterInstance => rosterInstance.RosterInstanceId).ToList()));
        }

        private static string GetLinkedQuestionAnswerFormattedAsRosterTitle(InterviewStateDependentOnAnswers state, Identity linkedQuestion, IQuestionnaire questionnaire)
        {
            // set of answers that support expressions includes set of answers that may be linked to, so following line is correct
            object answer = GetEnabledQuestionAnswerSupportedInExpressions(state, linkedQuestion, questionnaire);

            return AnswerUtils.AnswerToString(answer);
        }

        private static Events.Interview.Dtos.Identity[] ToEventIdentities(IEnumerable<Identity> answersDeclaredValid)
        {
            return answersDeclaredValid.Select(Events.Interview.Dtos.Identity.ToEventIdentity).ToArray();
        }

        private static bool IsQuestionOrParentGroupDisabled(Identity question, IQuestionnaire questionnaire, Func<Identity, bool> isGroupDisabled, Func<Identity, bool> isQuestionDisabled)
        {
            if (isQuestionDisabled(question))
                return true;

            IEnumerable<Guid> parentGroupIds = questionnaire.GetAllParentGroupsForQuestion(question.Id);
            IEnumerable<Identity> parentGroups = GetInstancesOfGroupsWithSameAndUpperRosterLevelOrThrow(parentGroupIds,
                question.RosterVector, questionnaire).ToList();

            var result = parentGroups.Any(isGroupDisabled);
            return result;
        }

        private static bool IsGroupDisabled(InterviewStateDependentOnAnswers state, Identity group)
        {
            string groupKey = ConversionHelper.ConvertIdAndRosterVectorToString(group.Id, group.RosterVector);

            return state.DisabledGroups.Contains(groupKey);
        }

        private static bool IsQuestionDisabled(InterviewStateDependentOnAnswers state, Identity question)
        {
            string questionKey = ConversionHelper.ConvertIdAndRosterVectorToString(question.Id, question.RosterVector);

            return state.DisabledQuestions.Contains(questionKey);
        }

        private static bool WasQuestionAnswered(InterviewStateDependentOnAnswers state, Identity question)
        {
            string questionKey = ConversionHelper.ConvertIdAndRosterVectorToString(question.Id, question.RosterVector);

            return state.AnsweredQuestions.Contains(questionKey);
        }

        private static object GetAnswerSupportedInExpressionsForEnabledOrNull(InterviewStateDependentOnAnswers state, Identity question,
            Func<Identity, bool?> getNewQuestionState)
        {
            bool? newQuestionState = getNewQuestionState(question);

            //no changes after dis/enable but already marked as disabled
            if (!newQuestionState.HasValue && IsQuestionDisabled(state, question))
                return null;
            //new state of question is disabled
            if (newQuestionState.HasValue && !newQuestionState.Value)
            {
                return null;
            }

            string questionKey = ConversionHelper.ConvertIdAndRosterVectorToString(question.Id, question.RosterVector);

            return state.AnswersSupportedInExpressions.ContainsKey(questionKey)
                ? state.AnswersSupportedInExpressions[questionKey]
                : null;
        }

        private static object GetEnabledQuestionAnswerSupportedInExpressions(InterviewStateDependentOnAnswers state, Identity question, IQuestionnaire questionnaire)
        {
            return GetEnabledQuestionAnswerSupportedInExpressions(state, question, IsQuestionDisabled, IsGroupDisabled, questionnaire);
        }

        private static object GetEnabledQuestionAnswerSupportedInExpressions(InterviewStateDependentOnAnswers state,
            Identity question,
            Func<InterviewStateDependentOnAnswers, Identity, bool> isQuestionDisabled,
            Func<InterviewStateDependentOnAnswers, Identity, bool> isGroupDisabled,
            IQuestionnaire questionnaire,
            Identity questionBeingAnswered = null,
            object answerBeingApplied = null)
        {
            if (IsQuestionOrParentGroupDisabled(question,
                questionnaire,
                isGroupDisabled: (g) => isGroupDisabled(state, g),
                isQuestionDisabled: (q) => isQuestionDisabled(state, q)))
                return null;

            if (questionBeingAnswered != null && AreEqual(question, questionBeingAnswered))
                return answerBeingApplied;

            string questionKey = ConversionHelper.ConvertIdAndRosterVectorToString(question.Id, question.RosterVector);

            return state.AnswersSupportedInExpressions.ContainsKey(questionKey)
                ? state.AnswersSupportedInExpressions[questionKey]
                : null;
        }

        private static decimal[] ShrinkRosterVector(decimal[] rosterVector, int length)
        {
            if (length == 0)
                return EmptyRosterVector;

            if (length == rosterVector.Length)
                return rosterVector;

            if (length > rosterVector.Length)
                throw new ArgumentException(string.Format("Cannot shrink vector with length {0} to bigger length {1}.", rosterVector.Length,
                    length));

            return rosterVector.Take(length).ToArray();
        }

        /// <remarks>
        /// If roster vector should be extended, result will be a set of vectors depending on roster count of corresponding groups.
        /// </remarks>
        private static IEnumerable<decimal[]> ExtendRosterVector(InterviewStateDependentOnAnswers state, decimal[] rosterVector, int length,
            Guid[] rosterGroupsStartingFromTop,
            Func<InterviewStateDependentOnAnswers, Guid, decimal[], DistinctDecimalList> getRosterInstanceIds)
        {
            if (length < rosterVector.Length)
                throw new ArgumentException(string.Format(
                    "Cannot extend vector with length {0} to smaller length {1}.", rosterVector.Length, length));

            if (length == rosterVector.Length)
            {
                yield return rosterVector;
                yield break;
            }

            var outerVectorsForExtend =
                GetOuterVectorForParentRoster(state, rosterGroupsStartingFromTop,
                    rosterVector);

            foreach (var outerVectorForExtend in outerVectorsForExtend)
            {
                DistinctDecimalList rosterInstanceIds = getRosterInstanceIds(state, rosterGroupsStartingFromTop.Last(), outerVectorForExtend);
                foreach (decimal rosterInstanceId in rosterInstanceIds)
                {
                    yield return ExtendRosterVectorWithOneValue(outerVectorForExtend, rosterInstanceId);
                }
            }
        }

        private static IEnumerable<decimal[]> GetOuterVectorForParentRoster(InterviewStateDependentOnAnswers state,
            Guid[] rosterGroupsStartingFromTop, decimal[] rosterVector)
        {
            if (rosterGroupsStartingFromTop.Count() <= 1 || rosterGroupsStartingFromTop.Length - 1 == rosterVector.Length)
            {
                yield return rosterVector;
                yield break;
            }

            var indexOfPreviousRoster = rosterGroupsStartingFromTop.Length - 2;

            var previousRoster = rosterGroupsStartingFromTop[rosterVector.Length];
            var previousRosterInstances = GetRosterInstanceIds(state, previousRoster, rosterVector);
            foreach (var previousRosterInstance in previousRosterInstances)
            {
                var extendedRoster = ExtendRosterVectorWithOneValue(rosterVector, previousRosterInstance);
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

        private static decimal[] ExtendRosterVectorWithOneValue(decimal[] rosterVector, decimal value)
        {
            return new List<decimal>(rosterVector) { value }.ToArray();
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
                where !this.interviewState.AnswerComments.Contains(new AnswerComment(answerComment.UserId, answerComment.Date, answerComment.Text, answerDto.Id, answerDto.QuestionPropagationVector))
                select new
                {
                    UserId = answerComment.UserId,
                    Date = answerComment.Date,
                    Text = answerComment.Text,
                    QuestionId = answerDto.Id,
                    RosterVector = answerDto.QuestionPropagationVector
                });

            if (this.status == InterviewStatus.Deleted)
            {
                this.ApplyEvent(new InterviewRestored(userId));
            }

            this.ApplyEvent(new InterviewRejectedByHQ(userId, interviewDto.Comments));
            this.ApplyEvent(new InterviewStatusChanged(interviewDto.Status, comment: interviewDto.Comments));

            if (interviewerId.HasValue)
            {
                this.ApplyEvent(new InterviewerAssigned(userId, interviewerId.Value));
            }

            foreach (var commentedAnswer in commentedAnswers)
            {
                this.ApplyEvent(new AnswerCommented(commentedAnswer.UserId, commentedAnswer.QuestionId, commentedAnswer.RosterVector, commentedAnswer.Date, commentedAnswer.Text));
            }
        }

        private static bool AreEqual(Identity identityA, Identity identityB)
        {
            return identityA.Id == identityB.Id
                && AreEqualRosterVectors(identityA.RosterVector, identityB.RosterVector);
        }

        private static bool AreEqualRosterVectors(decimal[] rosterVectorA, decimal[] rosterVectorB)
        {
            return rosterVectorA.SequenceEqual(rosterVectorB);
        }

        private static Tuple<decimal[], decimal> SplitRosterVectorOntoOuterVectorAndRosterInstanceId(decimal[] rosterVector)
        {
            return Tuple.Create(
                rosterVector.Take(rosterVector.Length - 1).ToArray(),
                rosterVector[rosterVector.Length - 1]);
        }

        private static int ToRosterSize(decimal decimalValue)
        {
            return (int)decimalValue;
        }

        private static string JoinDecimalsWithComma(IEnumerable<decimal> values)
        {
            return string.Join(", ", values.Select(value => value.ToString(CultureInfo.InvariantCulture)));
        }

        private static string FormatQuestionForException(Identity question, IQuestionnaire questionnaire)
        {
            return string.Format("'{0} [{1}] ({2:N} <{3}>)'",
                GetQuestionTitleForException(question.Id, questionnaire),
                GetQuestionVariableNameForException(question.Id, questionnaire),
                question.Id,
                string.Join("-", question.RosterVector));
        }

        private static string FormatQuestionForException(Guid questionId, IQuestionnaire questionnaire)
        {
            return string.Format("'{0} [{1}]'",
                GetQuestionTitleForException(questionId, questionnaire),
                GetQuestionVariableNameForException(questionId, questionnaire));
        }

        private static string FormatGroupForException(Identity group, IQuestionnaire questionnaire)
        {
            return string.Format("'{0} ({1:N} <{2}>)'",
                GetGroupTitleForException(group.Id, questionnaire),
                group.Id,
                string.Join("-", group.RosterVector));
        }

        private static string FormatGroupForException(Guid groupId, IQuestionnaire questionnaire)
        {
            return string.Format("'{0} ({1:N})'",
                GetGroupTitleForException(groupId, questionnaire),
                groupId);
        }

        private static string GetQuestionTitleForException(Guid questionId, IQuestionnaire questionnaire)
        {
            return questionnaire.HasQuestion(questionId)
                ? questionnaire.GetQuestionTitle(questionId) ?? "<<NO QUESTION TITLE>>"
                : "<<MISSING QUESTION>>";
        }

        private InterviewChanges CalculateInterviewChangesOnAnswerQRBarcodeQuestion(InterviewStateDependentOnAnswers state, Guid userId,
            Guid questionId, decimal[] rosterVector, DateTime answerTime, string answer,
            Identity answeredQuestion, IQuestionnaire questionnaire)
        {
            List<RosterIdentity> rosterInstancesWithAffectedTitles = CalculateRosterInstancesWhichTitlesAreAffected(
              questionId, rosterVector, questionnaire);

            var answerChanges = new List<AnswerChange>()
            {
                new AnswerChange(AnswerChangeType.QRBarcode, userId, questionId, rosterVector, answerTime, answer)
            };

            return new InterviewChanges(answerChanges, null, null,
                null,
                null, rosterInstancesWithAffectedTitles, AnswerUtils.AnswerToString(answer));
        }

        private void CheckQRBarcodeInvariants(Guid questionId, decimal[] rosterVector, IQuestionnaire questionnaire,
            Identity answeredQuestion, InterviewStateDependentOnAnswers currentInterviewState, bool applyStrongChecks = true)
        {
            ThrowIfQuestionDoesNotExist(questionId, questionnaire);
            this.ThrowIfRosterVectorIsIncorrect(currentInterviewState, questionId, rosterVector, questionnaire);
            ThrowIfQuestionTypeIsNotOneOfExpected(questionId, questionnaire, QuestionType.QRBarcode);
            if (applyStrongChecks)
                ThrowIfQuestionOrParentGroupIsDisabled(currentInterviewState, answeredQuestion, questionnaire);
        }


        private static string GetQuestionVariableNameForException(Guid questionId, IQuestionnaire questionnaire)
        {
            return questionnaire.HasQuestion(questionId)
                ? questionnaire.GetQuestionVariableName(questionId) ?? "<<NO VARIABLE NAME>>"
                : "<<MISSING QUESTION>>";
        }

        private static string GetGroupTitleForException(Guid groupId, IQuestionnaire questionnaire)
        {
            return questionnaire.HasGroup(groupId)
                ? questionnaire.GetGroupTitle(groupId) ?? "<<NO GROUP TITLE>>"
                : "<<MISSING GROUP>>";
        }

        private static HashSet<string> ToHashSetOfIdAndRosterVectorStrings(IEnumerable<InterviewItemId> synchronizationIdentities)
        {
            return new HashSet<string>(
                synchronizationIdentities.Select(
                    question => ConversionHelper.ConvertIdAndRosterVectorToString(question.Id, question.InterviewItemPropagationVector)));
        }

        private static Identity ToIdentity(InterviewItemId synchronizationIdentity)
        {
            return new Identity(synchronizationIdentity.Id, synchronizationIdentity.InterviewItemPropagationVector);
        }

        private static void PutToCorrespondingListAccordingToEnablementStateChange(
            Identity entity, List<Identity> entitiesToBeEnabled, List<Identity> entitiesToBeDisabled, bool isNewStateEnabled,
            bool isOldStateEnabled)
        {
            if (isNewStateEnabled && !isOldStateEnabled)
            {
                entitiesToBeEnabled.Add(entity);
            }

            if (!isNewStateEnabled && isOldStateEnabled)
            {
                entitiesToBeDisabled.Add(entity);
            }
        }

        private static IEnumerable<KeyValuePair<string, Identity>> GetInstancesOfQuestionsWithSameAndUpperRosterLevelOrThrow(
            IEnumerable<Guid> questionIds, decimal[] rosterVector, IQuestionnaire questionnare)
        {
            return questionIds.Select(
                questionId => GetInstanceOfQuestionWithSameAndUpperRosterLevelOrThrow(questionId, rosterVector, questionnare));
        }

        private static KeyValuePair<string, Identity> GetInstanceOfQuestionWithSameAndUpperRosterLevelOrThrow(Guid questionId,
            decimal[] rosterVector, IQuestionnaire questionnare)
        {
            int vectorRosterLevel = rosterVector.Length;
            int questionRosterLevel = questionnare.GetRosterLevelForQuestion(questionId);

            if (questionRosterLevel > vectorRosterLevel)
                throw new InterviewException(string.Format(
                    "Question {0} expected to have roster level not deeper than {1} but it is {2}.",
                    FormatQuestionForException(questionId, questionnare), vectorRosterLevel, questionRosterLevel));

            decimal[] questionRosterVector = ShrinkRosterVector(rosterVector, questionRosterLevel);

            return new KeyValuePair<string, Identity>(questionnare.GetQuestionVariableName(questionId),
                new Identity(questionId, questionRosterVector));
        }

        private static IEnumerable<Identity> GetInstancesOfQuestionsWithSameAndDeeperRosterLevelOrThrow(
            InterviewStateDependentOnAnswers state,
            IEnumerable<Guid> questionIds, decimal[] rosterVector, IQuestionnaire questionnare,
            Func<InterviewStateDependentOnAnswers, Guid, decimal[], DistinctDecimalList> getRosterInstanceIds)
        {
            return questionIds.SelectMany(questionId =>
                GetInstancesOfQuestionsWithSameAndDeeperRosterLevelOrThrow(state, questionId, rosterVector, questionnare,
                    getRosterInstanceIds));
        }

        private static IEnumerable<Identity> GetInstancesOfQuestionsWithSameAndDeeperRosterLevelOrThrow(
            InterviewStateDependentOnAnswers state,
            Guid questionId, decimal[] rosterVector, IQuestionnaire questionnare,
            Func<InterviewStateDependentOnAnswers, Guid, decimal[], DistinctDecimalList> getRosterInstanceIds)
        {
            int vectorRosterLevel = rosterVector.Length;
            int questionRosterLevel = questionnare.GetRosterLevelForQuestion(questionId);

            if (questionRosterLevel < vectorRosterLevel)
                throw new InterviewException(string.Format(
                    "Question {0} expected to have roster level not upper than {1} but it is {2}.",
                    FormatQuestionForException(questionId, questionnare), vectorRosterLevel, questionRosterLevel));

            Guid[] parentRosterGroupsStartingFromTop =
                questionnare.GetRostersFromTopToSpecifiedQuestion(questionId).ToArray();

            IEnumerable<decimal[]> questionRosterVectors = ExtendRosterVector(state,
                rosterVector, questionRosterLevel, parentRosterGroupsStartingFromTop, getRosterInstanceIds);

            foreach (decimal[] questionRosterVector in questionRosterVectors)
            {
                yield return new Identity(questionId, questionRosterVector);
            }
        }

        private static IEnumerable<Identity> GetInstancesOfQuestionsInAllRosterLevels(
            InterviewStateDependentOnAnswers state,
            Guid questionId, decimal[] rosterVector, IQuestionnaire questionnare,
            Func<InterviewStateDependentOnAnswers, Guid, decimal[], DistinctDecimalList> getRosterInstanceIds)
        {
            int vectorRosterLevel = rosterVector.Length;
            int questionRosterLevel = questionnare.GetRosterLevelForQuestion(questionId);

            if (questionRosterLevel > vectorRosterLevel)
            {
                Guid[] parentRosterGroupsStartingFromTop =
                    questionnare.GetRostersFromTopToSpecifiedQuestion(questionId).ToArray();

                IEnumerable<decimal[]> questionRosterVectors = ExtendRosterVector(state,
                    rosterVector, questionRosterLevel, parentRosterGroupsStartingFromTop, getRosterInstanceIds);

                foreach (decimal[] questionRosterVector in questionRosterVectors)
                {
                    yield return new Identity(questionId, questionRosterVector);
                }
            }

            else if (questionRosterLevel <= vectorRosterLevel)
            {
                decimal[] questionRosterVector = ShrinkRosterVector(rosterVector, questionRosterLevel);

                yield return new Identity(questionId, questionRosterVector);
            }
        }

        private static IEnumerable<Identity> GetInstancesOfGroupsWithSameAndUpperRosterLevelOrThrow(
            IEnumerable<Guid> groupIds, decimal[] rosterVector, IQuestionnaire questionnare)
        {
            int vectorRosterLevel = rosterVector.Length;

            foreach (Guid groupId in groupIds)
            {
                int groupRosterLevel = questionnare.GetRosterLevelForGroup(groupId);

                if (groupRosterLevel > vectorRosterLevel)
                    throw new InterviewException(string.Format(
                        "Group {0} expected to have roster level not deeper than {1} but it is {2}.",
                        FormatGroupForException(groupId, questionnare), vectorRosterLevel, groupRosterLevel));

                decimal[] groupRosterVector = ShrinkRosterVector(rosterVector, groupRosterLevel);

                yield return new Identity(groupId, groupRosterVector);
            }
        }

        private static IEnumerable<Identity> GetInstancesOfGroupsWithSameAndDeeperRosterLevelOrThrow(
            InterviewStateDependentOnAnswers state,
            IEnumerable<Guid> groupIds, decimal[] rosterVector, IQuestionnaire questionnare,
            Func<InterviewStateDependentOnAnswers, Guid, decimal[], DistinctDecimalList> getRosterInstanceIds)
        {
            int vectorRosterLevel = rosterVector.Length;

            foreach (Guid groupId in groupIds)
            {
                int groupRosterLevel = questionnare.GetRosterLevelForGroup(groupId);

                if (groupRosterLevel < vectorRosterLevel)
                    throw new InterviewException(string.Format(
                        "Group {0} expected to have roster level not upper than {1} but it is {2}.",
                        FormatGroupForException(groupId, questionnare), vectorRosterLevel, groupRosterLevel));

                Guid[] rosterGroupsStartingFromTop = questionnare.GetRostersFromTopToSpecifiedGroup(groupId).ToArray();
                IEnumerable<decimal[]> groupRosterVectors = ExtendRosterVector(state,
                    rosterVector, groupRosterLevel, rosterGroupsStartingFromTop, getRosterInstanceIds);

                foreach (decimal[] groupRosterVector in groupRosterVectors)
                {
                    yield return new Identity(groupId, groupRosterVector);
                }
            }
        }

        private static List<Identity> GetGroupsToBeDisabledInJustCreatedInterview(IQuestionnaire questionnaire)
        {
            return questionnaire
                .GetAllGroupsWithNotEmptyCustomEnablementConditions()
                .Where(groupId => !questionnaire.IsRosterGroup(groupId))
                .Where(groupId => !IsGroupUnderRosterGroup(questionnaire, groupId))
                .Select(groupId => new Identity(groupId, EmptyRosterVector))
                .ToList();
        }

        private static List<Identity> GetQuestionsToBeDisabledInJustCreatedInterview(IQuestionnaire questionnaire)
        {
            return questionnaire
                .GetAllQuestionsWithNotEmptyCustomEnablementConditions()
                .Where(questionId => !IsQuestionUnderRosterGroup(questionnaire, questionId))
                .Select(questionId => new Identity(questionId, EmptyRosterVector))
                .ToList();
        }

        private static List<Identity> GetQuestionsToBeInvalidInJustCreatedInterview(IQuestionnaire questionnaire,
            List<Identity> groupsToBeDisabled, List<Identity> questionsToBeDisabled)
        {
            return questionnaire
                .GetAllMandatoryQuestions()
                .Where(
                    questionId =>
                        !IsQuestionUnderRosterGroup(questionnaire, questionId) &&
                            !IsQuestionOrParentGroupDisabled(new Identity(questionId, new decimal[0]), questionnaire,
                                (question) => groupsToBeDisabled.Any(q => AreEqual(q, question)),
                                (question) => questionsToBeDisabled.Any(q => AreEqual(q, question))))
                .Select(questionId => new Identity(questionId, EmptyRosterVector))
                .ToList();
        }

        private static bool IsGroupUnderRosterGroup(IQuestionnaire questionnaire, Guid groupId)
        {
            return questionnaire.GetRostersFromTopToSpecifiedGroup(groupId).Any();
        }

        private static bool IsQuestionUnderRosterGroup(IQuestionnaire questionnaire, Guid questionId)
        {
            return questionnaire.GetRostersFromTopToSpecifiedQuestion(questionId).Any();
        }

        #endregion

        #region Handlers

        #region AnsweringQuestions

        public void AnswerTextQuestion(Guid userId, Guid questionId, decimal[] rosterVector, DateTime answerTime, string answer)
        {
            var answeredQuestion = new Identity(questionId, rosterVector);
            IQuestionnaire questionnaire = this.GetHistoricalQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);

            this.CheckTextQuestionInvariants(questionId, rosterVector, questionnaire, answeredQuestion, this.interviewState);

            Func<InterviewStateDependentOnAnswers, Identity, object> getAnswer =
                (currentState, question) => AreEqual(question, answeredQuestion)
                    ? answer
                    : GetEnabledQuestionAnswerSupportedInExpressions(this.interviewState, question, questionnaire);

            InterviewChanges interviewChanges = this.CalculateInterviewChangesOnAnswerTextQuestion(this.interviewState, userId, questionId,
                rosterVector, answerTime, answer, answeredQuestion, getAnswer, questionnaire);

            this.ApplyInterviewChanges(interviewChanges);
        }

        public void AnswerNumericRealQuestion(Guid userId, Guid questionId, decimal[] rosterVector, DateTime answerTime, decimal answer)
        {
            var answeredQuestion = new Identity(questionId, rosterVector);

            IQuestionnaire questionnaire = this.GetHistoricalQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);

            this.CheckNumericRealQuestionInvariants(questionId, rosterVector, answer, questionnaire, answeredQuestion, this.interviewState);

            Func<InterviewStateDependentOnAnswers, Identity, object> getAnswer =
                (currentState, question) => AreEqual(question, answeredQuestion)
                    ? answer
                    : GetEnabledQuestionAnswerSupportedInExpressions(this.interviewState, question, questionnaire);

            InterviewChanges interviewChanges = this.CalculateInterviewChangesOnAnswerNumericRealQuestion(this.interviewState, userId,
                questionId, rosterVector, answerTime, answer, answeredQuestion, getAnswer, questionnaire);

            this.ApplyInterviewChanges(interviewChanges);
        }

        public void AnswerQRBarcodeQuestion(Guid userId, Guid questionId, decimal[] rosterVector, DateTime answerTime, string answer)
        {
            var answeredQuestion = new Identity(questionId, rosterVector);

            IQuestionnaire questionnaire = this.GetHistoricalQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);
            ThrowIfQuestionDoesNotExist(questionId, questionnaire);
            this.ThrowIfRosterVectorIsIncorrect(this.interviewState, questionId, rosterVector, questionnaire);
            this.ThrowIfQuestionTypeIsNotOneOfExpected(questionId, questionnaire, QuestionType.QRBarcode);
            ThrowIfQuestionOrParentGroupIsDisabled(this.interviewState, answeredQuestion, questionnaire);

            InterviewChanges interviewChanges = this.CalculateInterviewChangesOnAnswerQrBarcodeQuestion(userId,
                questionId, rosterVector, answerTime, answer, questionnaire);

            this.ApplyInterviewChanges(interviewChanges);
        }

        public void AnswerNumericIntegerQuestion(Guid userId, Guid questionId, decimal[] rosterVector, DateTime answerTime, int answer)
        {
            var answeredQuestion = new Identity(questionId, rosterVector);
            IQuestionnaire questionnaire = this.GetHistoricalQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);
            this.CheckNumericIntegerQuestionInvariants(questionId, rosterVector, answer, questionnaire, answeredQuestion,
                this.interviewState);

            Func<InterviewStateDependentOnAnswers, Identity, object> getAnswer =
                (currentState, question) => AreEqual(question, answeredQuestion)
                    ? answer
                    : GetEnabledQuestionAnswerSupportedInExpressions(this.interviewState, question, questionnaire);

            InterviewChanges interviewChanges = this.CalculateInterviewChangesOnAnswerNumericIntegerQuestion(this.interviewState, userId,
                questionId, rosterVector, answerTime, answer,
                answeredQuestion, getAnswer, questionnaire);

            this.ApplyInterviewChanges(interviewChanges);
        }

        public void AnswerMultipleOptionsQuestion(Guid userId, Guid questionId, decimal[] rosterVector, DateTime answerTime, decimal[] selectedValues)
        {
            var answeredQuestion = new Identity(questionId, rosterVector);

            IQuestionnaire questionnaire = this.GetHistoricalQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);
            this.CheckMultipleOptionQuestionInvariants(questionId, rosterVector, selectedValues, questionnaire, answeredQuestion,
                this.interviewState);

            Func<InterviewStateDependentOnAnswers, Identity, object> getAnswer =
                (currentState, question) => AreEqual(question, answeredQuestion)
                    ? selectedValues
                    : GetEnabledQuestionAnswerSupportedInExpressions(this.interviewState, question, questionnaire);

            InterviewChanges interviewChanges = this.CalculateInterviewChangesOnAnswerMultipleOptionsQuestion(this.interviewState, userId,
                questionId, rosterVector, answerTime, selectedValues, answeredQuestion, getAnswer, questionnaire);

            this.ApplyInterviewChanges(interviewChanges);
        }

        public void AnswerMultipleOptionsLinkedQuestion(Guid userId, Guid questionId, decimal[] rosterVector, DateTime answerTime, decimal[][] selectedPropagationVectors)
        {
            var answeredQuestion = new Identity(questionId, rosterVector);

            IQuestionnaire questionnaire = this.GetHistoricalQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);
            ThrowIfQuestionDoesNotExist(questionId, questionnaire);
            this.ThrowIfRosterVectorIsIncorrect(this.interviewState, questionId, rosterVector, questionnaire);
            this.ThrowIfQuestionTypeIsNotOneOfExpected(questionId, questionnaire, QuestionType.MultyOption);
            ThrowIfQuestionOrParentGroupIsDisabled(this.interviewState, answeredQuestion, questionnaire);

            Guid linkedQuestionId = this.GetLinkedQuestionIdOrThrow(questionId, questionnaire);
            var answeredLinkedQuestions =
                selectedPropagationVectors.Select(selectedRosterVector => new Identity(linkedQuestionId, selectedRosterVector));
            foreach (var answeredLinkedQuestion in answeredLinkedQuestions)
            {
                this.ThrowIfRosterVectorIsIncorrect(this.interviewState, linkedQuestionId, answeredLinkedQuestion.RosterVector,
                    questionnaire);
                ThrowIfQuestionOrParentGroupIsDisabled(this.interviewState, answeredLinkedQuestion, questionnaire);
                this.ThrowIfLinkedQuestionDoesNotHaveAnswer(this.interviewState, answeredQuestion, answeredLinkedQuestion, questionnaire);
            }
            ThrowIfLengthOfSelectedValuesMoreThanMaxForSelectedAnswerOptions(questionId, selectedPropagationVectors.Length, questionnaire);

            List<RosterIdentity> rosterInstancesWithAffectedTitles = CalculateRosterInstancesWhichTitlesAreAffected(
                questionId, rosterVector, questionnaire);
            string answerFormattedAsRosterTitle = string.Join(", ",
                answeredLinkedQuestions.Select(q => GetLinkedQuestionAnswerFormattedAsRosterTitle(this.interviewState, q, questionnaire)));

            this.ApplyEvent(new MultipleOptionsLinkedQuestionAnswered(userId, questionId, rosterVector, answerTime,
                     selectedPropagationVectors));

            bool questionIsMandatoryAndLinkedAndNotAnswered = questionnaire.IsQuestionMandatory(questionId) && !answeredLinkedQuestions.Any();
            if (questionIsMandatoryAndLinkedAndNotAnswered)
            {
                this.ApplySingleAnswerDeclaredInvalidEvent(questionId, rosterVector);
            }
            else
            {
                this.ApplySingleAnswerDeclaredValidEvent(questionId, rosterVector);
            }

            this.ApplyRosterRowsTitleChangedEvents(rosterInstancesWithAffectedTitles, answerFormattedAsRosterTitle);
        }

        public void AnswerDateTimeQuestion(Guid userId, Guid questionId, decimal[] rosterVector, DateTime answerTime, DateTime answer)
        {
            var answeredQuestion = new Identity(questionId, rosterVector);

            IQuestionnaire questionnaire = this.GetHistoricalQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);
            this.CheckDateTimeQuestionInvariants(questionId, rosterVector, questionnaire, answeredQuestion, this.interviewState);

            Func<InterviewStateDependentOnAnswers, Identity, object> getAnswer =
                (currentState, question) => AreEqual(question, answeredQuestion)
                    ? answer
                    : GetEnabledQuestionAnswerSupportedInExpressions(this.interviewState, question, questionnaire);

            InterviewChanges interviewChanges = this.CalculateInterviewChangesOnAnswerDateTimeQuestion(this.interviewState, userId,
                questionId, rosterVector, answerTime, answer, answeredQuestion, getAnswer, questionnaire);

            this.ApplyInterviewChanges(interviewChanges);
        }

        public void AnswerSingleOptionQuestion(Guid userId, Guid questionId, decimal[] rosterVector, DateTime answerTime, decimal selectedValue)
        {
            var answeredQuestion = new Identity(questionId, rosterVector);

            IQuestionnaire questionnaire = this.GetHistoricalQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);
            this.CheckSingleOptionQuestionInvariants(questionId, rosterVector, selectedValue, questionnaire, answeredQuestion,
                this.interviewState);

            Func<InterviewStateDependentOnAnswers, Identity, object> getAnswer =
                (currentState, question) => AreEqual(question, answeredQuestion)
                    ? selectedValue
                    : GetEnabledQuestionAnswerSupportedInExpressions(this.interviewState, question, questionnaire);

            InterviewChanges interviewChanges = this.CalculateInterviewChangesOnAnswerSingleOptionQuestion(this.interviewState, userId,
                questionId, rosterVector, answerTime, selectedValue, answeredQuestion, getAnswer, questionnaire);

            this.ApplyInterviewChanges(interviewChanges);
        }


        private void CheckTextListInvariants(Guid questionId, decimal[] rosterVector, IQuestionnaire questionnaire, Identity answeredQuestion,
            InterviewStateDependentOnAnswers currentInterviewState, Tuple<decimal, string>[] answers, bool applyStrongChecks = true)
        {
            ThrowIfQuestionDoesNotExist(questionId, questionnaire);
            this.ThrowIfRosterVectorIsIncorrect(currentInterviewState, questionId, rosterVector, questionnaire);
            this.ThrowIfQuestionTypeIsNotOneOfExpected(questionId, questionnaire, QuestionType.TextList);

            if (applyStrongChecks)
            {
                ThrowIfQuestionOrParentGroupIsDisabled(currentInterviewState, answeredQuestion, questionnaire);
                ThrowIfDecimalValuesAreNotUnique(answers, questionId, questionnaire);
                ThrowIfStringValueAreEmptyOrWhitespaces(answers, questionId, questionnaire);
                var maxAnswersCountLimit = questionnaire.GetListSizeForListQuestion(questionId);
                ThrowIfAnswersExceedsMaxAnswerCountLimit(answers, maxAnswersCountLimit, questionId, questionnaire);
            }
        }

        public void AnswerTextListQuestion(Guid userId, Guid questionId, decimal[] rosterVector, DateTime answerTime,
            Tuple<decimal, string>[] answers)
        {
            var answeredQuestion = new Identity(questionId, rosterVector);

            IQuestionnaire questionnaire = this.GetHistoricalQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);

            CheckTextListInvariants(questionId, rosterVector, questionnaire, answeredQuestion, this.interviewState, answers);

            Func<InterviewStateDependentOnAnswers, Identity, object> getAnswer = (currentState, question) => AreEqual(question, answeredQuestion) ?
                answers :
                GetEnabledQuestionAnswerSupportedInExpressions(this.interviewState, question, questionnaire);

            InterviewChanges interviewChanges = CalculateInterviewChangesOnAnswerTextListQuestion(this.interviewState, userId, questionId,
                rosterVector, answerTime, answers, answeredQuestion, getAnswer, questionnaire);

            this.ApplyInterviewChanges(interviewChanges);
        }

        private InterviewChanges CalculateInterviewChangesOnAnswerTextListQuestion(InterviewStateDependentOnAnswers state, Guid userId,
            Guid questionId, decimal[] rosterVector, DateTime answerTime, Tuple<decimal, string>[] answers, Identity answeredQuestion,
            Func<InterviewStateDependentOnAnswers, Identity, object> getAnswer, IQuestionnaire questionnaire)
        {
            var selectedValues = answers.Select(x => x.Item1).ToArray();
            DistinctDecimalList rosterInstanceIds = new DistinctDecimalList(selectedValues.ToList());
            Dictionary<decimal, int?> rosterInstanceIdsWithSortIndexes = selectedValues.ToDictionary(
                selectedValue => selectedValue,
                selectedValue => (int?)selectedValue);

            List<Guid> rosterIds = questionnaire.GetRosterGroupsByRosterSizeQuestion(questionId).ToList();

            Func<Guid, decimal[], bool> isRoster = (groupId, groupOuterRosterVector)
                => rosterIds.Contains(groupId)
                    && AreEqualRosterVectors(groupOuterRosterVector, rosterVector);

            Func<InterviewStateDependentOnAnswers, Guid, decimal[], DistinctDecimalList> getRosterInstanceIds =
                (currentState, groupId, groupOuterRosterVector)
                    => isRoster(groupId, groupOuterRosterVector)
                        ? rosterInstanceIds
                        : GetRosterInstanceIds(this.interviewState, groupId, groupOuterRosterVector);

            string questionKey = ConversionHelper.ConvertIdAndRosterVectorToString(questionId, rosterVector);

            Tuple<decimal, string>[] currentAnswer = this.interviewState.TextListAnswers.ContainsKey(questionKey)
                ? this.interviewState.TextListAnswers[questionKey]
                : new Tuple<decimal, string>[0];

            Tuple<decimal, string>[] changedAnswers =
                answers.Where(tuple => currentAnswer.Any(a => a.Item1 == tuple.Item1 && a.Item2 != tuple.Item2)).ToArray();

            RosterCalculationData rosterCalculationData = this.CalculateRosterDataWithRosterTitlesFromTextListQuestions(
                this.interviewState, questionnaire, rosterVector, rosterIds, rosterInstanceIdsWithSortIndexes, questionnaire, getAnswer,
                getRosterInstanceIds,
                answers, changedAnswers);

            List<RosterIdentity> rosterInstancesWithAffectedTitles = CalculateRosterInstancesWhichTitlesAreAffected(questionId, rosterVector,
                questionnaire);

            string answerFormattedAsRosterTitle = AnswerUtils.AnswerToString(selectedValues,
                answerOptionValue => answers.Single(x => x.Item1 == answerOptionValue).Item2);
            var answerChanges = new List<AnswerChange>()
            {
                new AnswerChange(AnswerChangeType.TextList, userId, questionId, rosterVector, answerTime, answers)
            };

            return new InterviewChanges(answerChanges, null, new ValidityChanges(
                answersDeclaredValid: new List<Identity> { new Identity(questionId, rosterVector) },
                answersDeclaredInvalid: null),
                rosterCalculationData,
                null, rosterInstancesWithAffectedTitles, answerFormattedAsRosterTitle);
        }

        public void AnswerGeoLocationQuestion(Guid userId, Guid questionId, decimal[] rosterVector, DateTime answerTime, double latitude, double longitude,
            double accuracy, double altitude, DateTimeOffset timestamp)
        {
            var answeredQuestion = new Identity(questionId, rosterVector);

            IQuestionnaire questionnaire = this.GetHistoricalQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);
            CheckGpsCoordinatesInvariants(questionId, rosterVector, questionnaire, answeredQuestion, this.interviewState);
            InterviewChanges interviewChanges = CalculateInterviewChangesOnAnswerGeoLocationQuestion(this.interviewState, userId, questionId,
                rosterVector, answerTime, latitude, longitude, accuracy, altitude, timestamp, answeredQuestion, questionnaire);

            this.ApplyInterviewChanges(interviewChanges);
        }

        private void CheckGpsCoordinatesInvariants(Guid questionId, decimal[] rosterVector, IQuestionnaire questionnaire, Identity answeredQuestion,
            InterviewStateDependentOnAnswers currentInterviewState, bool applyStrongChecks = true)
        {
            ThrowIfQuestionDoesNotExist(questionId, questionnaire);
            this.ThrowIfRosterVectorIsIncorrect(currentInterviewState, questionId, rosterVector, questionnaire);
            this.ThrowIfQuestionTypeIsNotOneOfExpected(questionId, questionnaire, QuestionType.GpsCoordinates);
            if (applyStrongChecks)
                ThrowIfQuestionOrParentGroupIsDisabled(currentInterviewState, answeredQuestion, questionnaire);
        }

        private InterviewChanges CalculateInterviewChangesOnAnswerQrBarcodeQuestion(Guid userId,
            Guid questionId, decimal[] rosterVector, DateTime answerTime, string answer,
            IQuestionnaire questionnaire)
        {
            var isQuestionMandatory = questionnaire.IsQuestionMandatory(questionId);
            var questionIdentity = new Identity(questionId, rosterVector);

            List<RosterIdentity> rosterInstancesWithAffectedTitles = CalculateRosterInstancesWhichTitlesAreAffected(questionId, rosterVector,
                questionnaire);

            var answerChanges = new List<AnswerChange>()
            {
                new AnswerChange(AnswerChangeType.QRBarcode, userId, questionId, rosterVector, answerTime, answer)
            };

            var answersDeclaredValid = new List<Identity>();
            var answersDeclaredInvalid = new List<Identity>();

            if (isQuestionMandatory && string.IsNullOrWhiteSpace(answer))
            {
                answersDeclaredInvalid.Add(questionIdentity);
            }
            else
            {
                answersDeclaredValid.Add(questionIdentity);
            }

            var validityChanges = new ValidityChanges(
                answersDeclaredValid: answersDeclaredValid,
                answersDeclaredInvalid: answersDeclaredInvalid);
            return new InterviewChanges(answerChanges, null,
                validityChanges,
                null,
                null,
                rosterInstancesWithAffectedTitles,
                answer);
        }

        private InterviewChanges CalculateInterviewChangesOnAnswerGeoLocationQuestion(InterviewStateDependentOnAnswers state, Guid userId,
            Guid questionId, decimal[] rosterVector, DateTime answerTime, double latitude, double longitude, double accuracy, double altitude, DateTimeOffset timestamp, Identity answeredQuestion,
            IQuestionnaire questionnaire)
        {
            List<RosterIdentity> rosterInstancesWithAffectedTitles = CalculateRosterInstancesWhichTitlesAreAffected(
                questionId, rosterVector, questionnaire);

            var answerChanges = new List<AnswerChange>()
            {
                new AnswerChange( AnswerChangeType.GeoLocation, userId, questionId, rosterVector, answerTime, new GeoLocationPoint(latitude, longitude, accuracy, altitude, timestamp))
            };

            string answerFormattedAsRosterTitle = string.Format(CultureInfo.InvariantCulture, "[{0};{1}]", latitude, longitude);
            return new InterviewChanges(answerChanges, null, new ValidityChanges(
                    answersDeclaredValid: new List<Identity> { new Identity(questionId, rosterVector) },
                    answersDeclaredInvalid: null),
                null,
                null, rosterInstancesWithAffectedTitles, answerFormattedAsRosterTitle);
        }

        public void AnswerSingleOptionLinkedQuestion(Guid userId, Guid questionId, decimal[] rosterVector, DateTime answerTime, decimal[] selectedPropagationVector)
        {
            var answeredQuestion = new Identity(questionId, rosterVector);

            IQuestionnaire questionnaire = this.GetHistoricalQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);
            ThrowIfQuestionDoesNotExist(questionId, questionnaire);
            this.ThrowIfRosterVectorIsIncorrect(this.interviewState, questionId, rosterVector, questionnaire);

            this.ThrowIfQuestionTypeIsNotOneOfExpected(questionId, questionnaire, QuestionType.SingleOption);
            ThrowIfQuestionOrParentGroupIsDisabled(this.interviewState, answeredQuestion, questionnaire);

            Guid linkedQuestionId = this.GetLinkedQuestionIdOrThrow(questionId, questionnaire);
            var answeredLinkedQuestion = new Identity(linkedQuestionId, selectedPropagationVector);

            this.ThrowIfRosterVectorIsIncorrect(this.interviewState, linkedQuestionId, selectedPropagationVector, questionnaire);
            ThrowIfQuestionOrParentGroupIsDisabled(this.interviewState, answeredLinkedQuestion, questionnaire);
            this.ThrowIfLinkedQuestionDoesNotHaveAnswer(this.interviewState, answeredQuestion, answeredLinkedQuestion, questionnaire);


            List<RosterIdentity> rosterInstancesWithAffectedTitles = CalculateRosterInstancesWhichTitlesAreAffected(
                questionId, rosterVector, questionnaire);
            string answerFormattedAsRosterTitle = GetLinkedQuestionAnswerFormattedAsRosterTitle(this.interviewState, answeredLinkedQuestion, questionnaire);


            this.ApplyEvent(new SingleOptionLinkedQuestionAnswered(userId, questionId, rosterVector, answerTime, selectedPropagationVector));

            this.ApplySingleAnswerDeclaredValidEvent(questionId, rosterVector);

            this.ApplyRosterRowsTitleChangedEvents(rosterInstancesWithAffectedTitles, answerFormattedAsRosterTitle);
        }

        #endregion

        public void ReevaluateSynchronizedInterview()
        {
            IQuestionnaire questionnaire = this.GetHistoricalQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);

            List<Identity> questionsToBeEnabled = new List<Identity>();
            List<Identity> questionsToBeDisabled = new List<Identity>();

            List<Identity> groupsToBeEnabled = new List<Identity>();
            List<Identity> groupsToBeDisabled = new List<Identity>();

            List<Identity> questionsDeclaredValid = new List<Identity>();
            List<Identity> questionsDeclaredInvalid = new List<Identity>();

            Func<InterviewStateDependentOnAnswers, Identity, object> getEnabledQuestionAnswerSupportedInExpressions = (state, questionId) =>
                GetEnabledQuestionAnswerSupportedInExpressions(this.interviewState,
                    questionId,
                    (currentState, q) => this.ShouldQuestionBeDisabledByCustomCondition(this.interviewState, q, questionnaire),
                    (currentState, g) => this.ShouldGroupBeDisabledByCustomCondition(this.interviewState, g, questionnaire),
                    questionnaire);

            foreach (var groupWithNotEmptyCustomEnablementCondition in questionnaire.GetAllGroupsWithNotEmptyCustomEnablementConditions())
            {
                var availableRosterLevels = this.AvailableRosterLevelsForGroup(this.interviewState, questionnaire,
                    groupWithNotEmptyCustomEnablementCondition);

                foreach (var availableRosterLevel in availableRosterLevels)
                {
                    Identity groupIdAtInterview = new Identity(groupWithNotEmptyCustomEnablementCondition, availableRosterLevel);

                    PutToCorrespondingListAccordingToEnablementStateChange(groupIdAtInterview, groupsToBeEnabled, groupsToBeDisabled,
                        isNewStateEnabled:
                            this.ShouldGroupBeEnabledByCustomEnablementCondition(this.interviewState, groupIdAtInterview, questionnaire,
                                getEnabledQuestionAnswerSupportedInExpressions),
                        isOldStateEnabled: !IsGroupDisabled(this.interviewState, groupIdAtInterview));
                }
            }

            foreach (var questionWithNotEmptyEnablementCondition in questionnaire.GetAllQuestionsWithNotEmptyCustomEnablementConditions())
            {
                var availableRosterLevels = this.AvailableRosterLevelsForQuestion(this.interviewState, questionnaire,
                    questionWithNotEmptyEnablementCondition);

                foreach (var availableRosterLevel in availableRosterLevels)
                {
                    Identity questionIdAtInterview = new Identity(questionWithNotEmptyEnablementCondition, availableRosterLevel);

                    PutToCorrespondingListAccordingToEnablementStateChange(questionIdAtInterview, questionsToBeEnabled,
                        questionsToBeDisabled,
                        isNewStateEnabled:
                            this.ShouldQuestionBeEnabledByCustomEnablementCondition(this.interviewState, questionIdAtInterview,
                                questionnaire,
                                getEnabledQuestionAnswerSupportedInExpressions),
                        isOldStateEnabled: !IsQuestionDisabled(this.interviewState, questionIdAtInterview));
                }
            }

            Func<Identity, bool> isQuestionDisabled =
                (questionIdAtInterview) => IsQuestionOrParentGroupDisabled(questionIdAtInterview, questionnaire,
                    (group) =>
                        (groupsToBeDisabled.Any(q => AreEqual(q, group)) || IsGroupDisabled(this.interviewState, group)) &&
                            !groupsToBeEnabled.Any(q => AreEqual(q, group)),
                    (question) =>
                        (questionsToBeDisabled.Any(q => AreEqual(q, question)) ||
                            IsQuestionDisabled(this.interviewState, questionIdAtInterview)) &&
                            !questionsToBeEnabled.Any(q => AreEqual(q, question)));

            Func<InterviewStateDependentOnAnswers, Identity, object> getAnswer =
                (state, question) =>
                    GetEnabledQuestionAnswerSupportedInExpressions(state,
                        question,
                        (currstate, currquestion) => !questionsToBeEnabled.Any(q => AreEqual(q, currquestion)) && IsQuestionDisabled(currstate, currquestion),
                        (currstate, currgroup) => !groupsToBeEnabled.Any(q => AreEqual(q, currgroup)) && IsGroupDisabled(currstate, currgroup),
                        questionnaire);

            foreach (var questionWithNotEmptyValidationExpression in questionnaire.GetAllQuestionsWithNotEmptyValidationExpressions())
            {
                var availableRosterLevels = this.AvailableRosterLevelsForQuestion(this.interviewState, questionnaire,
                    questionWithNotEmptyValidationExpression);

                foreach (var availableRosterLevel in availableRosterLevels)
                {
                    Identity questionIdAtInterview = new Identity(questionWithNotEmptyValidationExpression, availableRosterLevel);

                    if (isQuestionDisabled(questionIdAtInterview))
                        continue;

                    string questionKey = ConversionHelper.ConvertIdAndRosterVectorToString(questionIdAtInterview.Id, questionIdAtInterview.RosterVector);

                    if (!this.interviewState.AnsweredQuestions.Contains(questionKey))
                        continue;


                    bool? dependentQuestionValidationResult = this.PerformValidationOfQuestion(this.interviewState,
                        questionIdAtInterview, questionnaire,
                        getAnswer, a => null);

                    switch (dependentQuestionValidationResult)
                    {
                        case true:
                            questionsDeclaredValid.Add(questionIdAtInterview);
                            break;
                        case false:
                            questionsDeclaredInvalid.Add(questionIdAtInterview);
                            break;
                    }
                }
            }

            Func<Identity, bool> wasQuestionValidationPerformed =
                mandatoryQuestion =>
                    questionsDeclaredInvalid.Any(x => AreEqual(x, mandatoryQuestion)) ||
                        questionsDeclaredValid.Any(x => AreEqual(x, mandatoryQuestion));

            IEnumerable<Identity> mandatoryQuestionInstances =
                GetInstancesOfQuestionsWithSameAndDeeperRosterLevelOrThrow(this.interviewState, questionnaire.GetAllMandatoryQuestions(),
                    EmptyRosterVector,
                    questionnaire, GetRosterInstanceIds);


            foreach (Identity mandatoryQuestion in mandatoryQuestionInstances)
            {
                if (isQuestionDisabled(mandatoryQuestion) || wasQuestionValidationPerformed(mandatoryQuestion))
                    continue;

                string questionKey = ConversionHelper.ConvertIdAndRosterVectorToString(mandatoryQuestion.Id, mandatoryQuestion.RosterVector);

                bool hasQuestionAnswer = this.interviewState.AnsweredQuestions.Contains(questionKey);

                if (hasQuestionAnswer)
                {
                    questionsDeclaredValid.Add(mandatoryQuestion);
                }
                else
                {
                    questionsDeclaredInvalid.Add(mandatoryQuestion);
                }
            }
            this.ApplyEnablementChangesEvents(new EnablementChanges(groupsToBeDisabled, groupsToBeEnabled, questionsToBeDisabled,
                questionsToBeEnabled));
            this.ApplyValidityChangesEvents(new ValidityChanges(questionsDeclaredValid, questionsDeclaredInvalid));

            if (!this.HasInvalidAnswers())
            {
                this.ApplyEvent(new InterviewDeclaredValid());
            }
        }

        public void CommentAnswer(Guid userId, Guid questionId, decimal[] rosterVector, DateTime commentTime, string comment)
        {
            IQuestionnaire questionnaire = this.GetHistoricalQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);
            ThrowIfQuestionDoesNotExist(questionId, questionnaire);
            this.ThrowIfRosterVectorIsIncorrect(this.interviewState, questionId, rosterVector, questionnaire);

            this.ApplyEvent(new AnswerCommented(userId, questionId, rosterVector, commentTime, comment));
        }

        public void SetFlagToAnswer(Guid userId, Guid questionId, decimal[] rosterVector)
        {
            IQuestionnaire questionnaire = this.GetHistoricalQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);
            ThrowIfQuestionDoesNotExist(questionId, questionnaire);
            this.ThrowIfRosterVectorIsIncorrect(this.interviewState, questionId, rosterVector, questionnaire);

            this.ApplyEvent(new FlagSetToAnswer(userId, questionId, rosterVector));
        }

        public void RemoveFlagFromAnswer(Guid userId, Guid questionId, decimal[] rosterVector)
        {
            IQuestionnaire questionnaire = this.GetHistoricalQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);
            ThrowIfQuestionDoesNotExist(questionId, questionnaire);
            this.ThrowIfRosterVectorIsIncorrect(this.interviewState, questionId, rosterVector, questionnaire);

            this.ApplyEvent(new FlagRemovedFromAnswer(userId, questionId, rosterVector));
        }

        public void AssignSupervisor(Guid userId, Guid supervisorId)
        {
            this.ThrowIfInterviewStatusIsNotOneOfExpected(InterviewStatus.Created, InterviewStatus.SupervisorAssigned);

            this.ApplyEvent(new SupervisorAssigned(userId, supervisorId));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.SupervisorAssigned, comment: null));
        }

        public void AssignInterviewer(Guid userId, Guid interviewerId)
        {
            this.ThrowIfInterviewStatusIsNotOneOfExpected(
                InterviewStatus.SupervisorAssigned, InterviewStatus.InterviewerAssigned, InterviewStatus.RejectedBySupervisor);

            this.ApplyEvent(new InterviewerAssigned(userId, interviewerId));
            if (!this.wasCompleted && this.status != InterviewStatus.InterviewerAssigned)
            {
                this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.InterviewerAssigned, comment: null));
            }
        }

        public void Delete(Guid userId)
        {
            this.ThrowIfInterviewWasCompleted();
            this.ThrowIfInterviewStatusIsNotOneOfExpected(
                InterviewStatus.Created, InterviewStatus.SupervisorAssigned, InterviewStatus.InterviewerAssigned, InterviewStatus.Restored);

            this.ApplyEvent(new InterviewDeleted(userId));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.Deleted, comment: null));
        }

        public void CancelByHQSynchronization(Guid userId)
        {
            this.ApplyEvent(new InterviewDeleted(userId));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.Deleted, comment: null));
        }

        public void MarkInterviewAsSentToHeadquarters(Guid userId)
        {
            this.ApplyEvent(new InterviewDeleted(userId));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.Deleted, comment: null));
            this.ApplyEvent(new InterviewSentToHeadquarters());
        }

        public void Restore(Guid userId)
        {
            this.ThrowIfInterviewStatusIsNotOneOfExpected(InterviewStatus.Deleted);

            this.ApplyEvent(new InterviewRestored(userId));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.Restored, comment: null));
        }

        public void Complete(Guid userId, string comment, DateTime completeTime)
        {
            this.ThrowIfInterviewStatusIsNotOneOfExpected(
                InterviewStatus.InterviewerAssigned, InterviewStatus.Restarted, InterviewStatus.RejectedBySupervisor);

            /*IQuestionnaire questionnaire = this.GetHistoricalQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);*/
            bool isInterviewInvalid = this.HasInvalidAnswers() /*|| this.HasNotAnsweredMandatoryQuestions(questionnaire)*/;

            this.ApplyEvent(new InterviewCompleted(userId, completeTime));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.Completed, comment));

            this.ApplyEvent(isInterviewInvalid
                ? new InterviewDeclaredInvalid() as object
                : new InterviewDeclaredValid() as object);
        }

        public void Restart(Guid userId, string comment, DateTime restartTime)
        {
            this.ThrowIfInterviewStatusIsNotOneOfExpected(InterviewStatus.Completed);

            this.ApplyEvent(new InterviewRestarted(userId, restartTime));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.Restarted, comment));
        }

        public void Approve(Guid userId, string comment)
        {
            this.ThrowIfInterviewStatusIsNotOneOfExpected(InterviewStatus.Completed,
                InterviewStatus.RejectedBySupervisor,
                InterviewStatus.RejectedByHeadquarters);

            this.ApplyEvent(new InterviewApproved(userId, comment));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.ApprovedBySupervisor, comment));
        }

        public void Reject(Guid userId, string comment)
        {
            this.ThrowIfInterviewStatusIsNotOneOfExpected(InterviewStatus.Completed,
                InterviewStatus.ApprovedBySupervisor,
                InterviewStatus.RejectedByHeadquarters);

            this.ApplyEvent(new InterviewRejected(userId, comment));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.RejectedBySupervisor, comment));
        }

        public void HqApprove(Guid userId, string comment)
        {
            this.ThrowIfInterviewStatusIsNotOneOfExpected(InterviewStatus.ApprovedBySupervisor, InterviewStatus.RejectedByHeadquarters);

            this.ApplyEvent(new InterviewApprovedByHQ(userId, comment));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.ApprovedByHeadquarters, comment));
        }

        public void HqReject(Guid userId, string comment)
        {
            this.ThrowIfInterviewStatusIsNotOneOfExpected(InterviewStatus.ApprovedBySupervisor, InterviewStatus.ApprovedByHeadquarters);

            this.ApplyEvent(new InterviewRejectedByHQ(userId, comment));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.RejectedByHeadquarters, comment));
        }

        public void SynchronizeInterview(Guid userId, InterviewSynchronizationDto synchronizedInterview)
        {
            this.ApplyEvent(new InterviewSynchronized(synchronizedInterview));
        }

        public void SynchronizeInterviewFromHeadquarters(Guid id, Guid userId, Guid supervisorId, InterviewSynchronizationDto interviewDto, DateTime synchronizationTime)
        {
            IQuestionnaire questionnaire = this.GetHistoricalQuestionnaireOrThrow(interviewDto.QuestionnaireId,
                interviewDto.QuestionnaireVersion);

            var rosters = CalculateRostersFromInterviewSynchronizationDto(interviewDto);

            var enablementChanges = new EnablementChanges(
                groupsToBeDisabled: interviewDto.DisabledGroups.Select(ToIdentity).ToList(),
                questionsToBeDisabled: interviewDto.DisabledQuestions.Select(ToIdentity).ToList(),
                groupsToBeEnabled: new List<Identity>(),
                questionsToBeEnabled: new List<Identity>());

            var validityChanges = new ValidityChanges(
                answersDeclaredInvalid: interviewDto.InvalidAnsweredQuestions.Select(ToIdentity).ToList(),
                answersDeclaredValid: new List<Identity>());

            if (interviewDto.CreatedOnClient)
                this.ApplyEvent(new InterviewOnClientCreated(userId, interviewDto.QuestionnaireId, interviewDto.QuestionnaireVersion));
            else
                this.ApplyEvent(new InterviewCreated(userId, interviewDto.QuestionnaireId, interviewDto.QuestionnaireVersion));

            this.ApplyEvent(new SupervisorAssigned(userId, supervisorId));
            this.ApplyEvent(new InterviewStatusChanged(interviewDto.Status, comment: interviewDto.Comments));

            this.ApplyRostersEvents(rosters.ToArray());
            foreach (var answerDto in interviewDto.Answers.Where(x => x.Answer != null))
            {
                Guid questionId = answerDto.Id;
                QuestionType questionType = questionnaire.GetQuestionType(questionId);
                decimal[] rosterVector = answerDto.QuestionPropagationVector;
                object answer = answerDto.Answer;

                switch (questionType)
                {
                    case QuestionType.Text:
                        this.ApplyEvent(new TextQuestionAnswered(userId, questionId, rosterVector, synchronizationTime, (string)answer));
                        break;

                    case QuestionType.DateTime:
                        this.ApplyEvent(new DateTimeQuestionAnswered(userId, questionId, rosterVector, synchronizationTime,
                            (DateTime)answer));
                        break;

                    case QuestionType.TextList:
                        this.ApplyEvent(new TextListQuestionAnswered(userId, questionId, rosterVector, synchronizationTime,
                            (Tuple<decimal, string>[])answer));
                        break;

                    case QuestionType.GpsCoordinates:
                        var geoPosition = (GeoPosition)answer;
                        this.ApplyEvent(new GeoLocationQuestionAnswered(userId, questionId, rosterVector, synchronizationTime,
                            geoPosition.Latitude, geoPosition.Longitude, geoPosition.Accuracy, geoPosition.Altitude,
                            geoPosition.Timestamp));
                        break;

                    case QuestionType.QRBarcode:
                        this.ApplyEvent(new QRBarcodeQuestionAnswered(userId, questionId, rosterVector, synchronizationTime, (string)answer));
                        break;

                    case QuestionType.Numeric:
                        this.ApplyEvent(questionnaire.IsQuestionInteger(questionId)
                            ? new NumericIntegerQuestionAnswered(userId, questionId, rosterVector, synchronizationTime, Convert.ToInt32(answer)) as object
                            : new NumericRealQuestionAnswered(userId, questionId, rosterVector, synchronizationTime, (decimal)answer) as object);
                        break;

                    case QuestionType.SingleOption:
                        this.ApplyEvent(questionnaire.IsQuestionLinked(questionId)
                            ? new SingleOptionLinkedQuestionAnswered(userId, questionId, rosterVector, synchronizationTime,
                                (decimal[])answer) as object
                            : new SingleOptionQuestionAnswered(userId, questionId, rosterVector, synchronizationTime, (decimal)answer) as
                                object);
                        break;

                    case QuestionType.MultyOption:
                        this.ApplyEvent(questionnaire.IsQuestionLinked(questionId)
                            ? new MultipleOptionsLinkedQuestionAnswered(userId, questionId, rosterVector, synchronizationTime,
                                (decimal[][])answer) as object
                            : new MultipleOptionsQuestionAnswered(userId, questionId, rosterVector, synchronizationTime, (decimal[])answer)
                                as object);
                        break;

                    default:
                        throw new InterviewException(string.Format("Question {0} has unknown type {1}.",
                            FormatQuestionForException(questionId, questionnaire), questionType));
                }
            }

            this.ApplyEnablementChangesEvents(enablementChanges);

            this.ApplyValidityChangesEvents(validityChanges);
        }

        public void ApplySynchronizationMetadata(Guid id, Guid userId, Guid questionnaireId, InterviewStatus interviewStatus,
            AnsweredQuestionSynchronizationDto[] featuredQuestionsMeta, string comments, bool valid, bool createdOnClient)
        {
            if (this.status == InterviewStatus.Deleted)
                this.Restore(userId);
            else
                this.ThrowIfStatusNotAllowedToBeChangedWithMetadata(interviewStatus);

            this.ApplyEvent(new SynchronizationMetadataApplied(userId, questionnaireId,
                interviewStatus,
                featuredQuestionsMeta,
                createdOnClient, comments));

            this.ApplyEvent(new InterviewStatusChanged(interviewStatus, comments));

            ApplyValidationEvent(valid);
        }

        private void ApplyValidationEvent(bool isValid)
        {
            if (isValid)
                this.ApplyEvent(new InterviewDeclaredValid());
            else
                this.ApplyEvent(new InterviewDeclaredInvalid());
        }

        #endregion

        #region EventApplying

        private void ApplyInterviewChanges(InterviewChanges interviewChanges)
        {
            if (interviewChanges.InterviewByAnswerChanges != null)
                this.ApplyAnswersEvents(interviewChanges.InterviewByAnswerChanges);

            if (interviewChanges.EnablementChanges != null)
                this.ApplyEnablementChangesEvents(interviewChanges.EnablementChanges);

            if (interviewChanges.ValidityChanges != null)
                this.ApplyValidityChangesEvents(interviewChanges.ValidityChanges);

            if (interviewChanges.RosterCalculationData != null)
                this.ApplyRostersEvents(interviewChanges.RosterCalculationData);

            if (interviewChanges.AnswersForLinkedQuestionsToRemoveByDisabling != null)
                this.ApplyAnswersRemovanceEvents(interviewChanges.AnswersForLinkedQuestionsToRemoveByDisabling);

            if (interviewChanges.RosterInstancesWithAffectedTitles != null)
                this.ApplyRosterRowsTitleChangedEvents(interviewChanges.RosterInstancesWithAffectedTitles,
                    interviewChanges.AnswerAsRosterTitle);
        }

        private void ApplyInterviewChanges(IEnumerable<InterviewChanges> interviewChangesItems)
        {
            var eneblementChanges = new List<EnablementChanges>();
            var validityChanges = new List<ValidityChanges>();

            foreach (var interviewChanges in interviewChangesItems)
            {
                if (interviewChanges.InterviewByAnswerChanges != null)
                    this.ApplyAnswersEvents(interviewChanges.InterviewByAnswerChanges);

                if (interviewChanges.ValidityChanges != null)
                    validityChanges.Add(interviewChanges.ValidityChanges);

                if (interviewChanges.RosterCalculationData != null)
                    this.ApplyRostersEvents(interviewChanges.RosterCalculationData);

                if (interviewChanges.AnswersForLinkedQuestionsToRemoveByDisabling != null)
                    this.ApplyAnswersRemovanceEvents(interviewChanges.AnswersForLinkedQuestionsToRemoveByDisabling);

                if (interviewChanges.RosterInstancesWithAffectedTitles != null)
                    this.ApplyRosterRowsTitleChangedEvents(interviewChanges.RosterInstancesWithAffectedTitles,
                        interviewChanges.AnswerAsRosterTitle);

                if (interviewChanges.EnablementChanges != null)
                    eneblementChanges.Add(interviewChanges.EnablementChanges);
            }

            this.ApplyEnablementChangesEvents(EnablementChanges.UnionAllEnablementChanges(eneblementChanges));

            //merge changes, saving only last state - valid or invalid  
            validityChanges.ForEach(this.ApplyValidityChangesEvents);

        }

        private void ApplyEnablementChangesEvents(EnablementChanges enablementChanges)
        {
            if (enablementChanges.GroupsToBeDisabled.Any())
            {
                this.ApplyEvent(new GroupsDisabled(ToEventIdentities(enablementChanges.GroupsToBeDisabled)));
            }

            if (enablementChanges.GroupsToBeEnabled.Any())
            {
                this.ApplyEvent(new GroupsEnabled(ToEventIdentities(enablementChanges.GroupsToBeEnabled)));
            }

            if (enablementChanges.QuestionsToBeDisabled.Any())
            {
                this.ApplyEvent(new QuestionsDisabled(ToEventIdentities(enablementChanges.QuestionsToBeDisabled)));
            }

            if (enablementChanges.QuestionsToBeEnabled.Any())
            {
                this.ApplyEvent(new QuestionsEnabled(ToEventIdentities(enablementChanges.QuestionsToBeEnabled)));
            }
        }

        private void ApplyValidityChangesEvents(ValidityChanges validityChanges)
        {
            if (validityChanges.AnswersDeclaredValid.Any())
            {
                this.ApplyEvent(new AnswersDeclaredValid(ToEventIdentities(validityChanges.AnswersDeclaredValid)));
            }

            if (validityChanges.AnswersDeclaredInvalid.Any())
            {
                this.ApplyEvent(new AnswersDeclaredInvalid(ToEventIdentities(validityChanges.AnswersDeclaredInvalid)));
            }
        }

        private void ApplyAnswersRemovanceEvents(List<Identity> answersToRemove)
        {
            if (answersToRemove.Any())
            {
                this.ApplyEvent(new AnswersRemoved(ToEventIdentities(answersToRemove)));
            }
        }

        private void ApplySingleAnswerDeclaredValidEvent(Guid questionId, decimal[] rosterVector)
        {
            this.ApplyValidityChangesEvents(new ValidityChanges(
                answersDeclaredValid: new List<Identity> { new Identity(questionId, rosterVector) },
                answersDeclaredInvalid: null));
        }

        private void ApplySingleAnswerDeclaredInvalidEvent(Guid questionId, decimal[] rosterVector)
        {
            this.ApplyValidityChangesEvents(new ValidityChanges(
                answersDeclaredValid: null,
                answersDeclaredInvalid: new List<Identity> { new Identity(questionId, rosterVector) }));
        }

        private void ApplyRosterRowsTitleChangedEvents(List<RosterIdentity> rosterInstances, string rosterTitle)
        {
            if (rosterInstances.Any())
                this.ApplyEvent(new RosterInstancesTitleChanged(
                    rosterInstances.Select(
                        rosterIdentity =>
                            new ChangedRosterInstanceTitleDto(
                                new RosterInstance(rosterIdentity.GroupId, rosterIdentity.OuterRosterVector,
                                    rosterIdentity.RosterInstanceId),
                                rosterTitle)).ToArray()));
        }

        private void ApplyRostersEvents(params RosterCalculationData[] rosterDatas)
        {
            var rosterInstancesToAdd = this.GetUnionOfUniqueRosterDataPropertiesByRosterAndNestedRosters(d => d.RosterInstancesToAdd, new RosterIdentityComparer(), rosterDatas);

            if (rosterInstancesToAdd.Any())
            {
                AddedRosterInstance[] instances = rosterInstancesToAdd
                    .Select(
                        roster =>
                            new AddedRosterInstance(roster.GroupId, roster.OuterRosterVector, roster.RosterInstanceId, roster.SortIndex))
                    .ToArray();

                this.ApplyEvent(new RosterInstancesAdded(instances));
            }

            var rosterInstancesToRemove = this.GetUnionOfUniqueRosterDataPropertiesByRosterAndNestedRosters(
                d => d.RosterInstancesToRemove, new RosterIdentityComparer(), rosterDatas);

            if (rosterInstancesToRemove.Any())
            {
                RosterInstance[] instances = rosterInstancesToRemove
                    .Select(roster => new RosterInstance(roster.GroupId, roster.OuterRosterVector, roster.RosterInstanceId))
                    .ToArray();

                this.ApplyEvent(new RosterInstancesRemoved(instances));
            }

            var changedRosterRowTitleDtoFromRosterData = CreateChangedRosterRowTitleDtoFromRosterData(rosterDatas);
            if (changedRosterRowTitleDtoFromRosterData.Any())
                this.ApplyEvent(new RosterInstancesTitleChanged(CreateChangedRosterRowTitleDtoFromRosterData(rosterDatas)));

            this.ApplyAnswersRemovanceEvents(this.GetUnionOfUniqueRosterDataPropertiesByRosterAndNestedRosters(
                d => d.AnswersToRemoveByDecreasedRosterSize, new IdentityComparer(), rosterDatas));

            this.ApplyEnablementChangesEvents(
                new EnablementChanges(
                    this.GetUnionOfUniqueRosterDataPropertiesByRosterAndNestedRosters(d => d.InitializedGroupsToBeDisabled, new IdentityComparer(), rosterDatas),
                    this.GetUnionOfUniqueRosterDataPropertiesByRosterAndNestedRosters(d => d.InitializedGroupsToBeEnabled, new IdentityComparer(), rosterDatas),
                    this.GetUnionOfUniqueRosterDataPropertiesByRosterAndNestedRosters(d => d.InitializedQuestionsToBeDisabled, new IdentityComparer(), rosterDatas),
                    this.GetUnionOfUniqueRosterDataPropertiesByRosterAndNestedRosters(d => d.InitializedQuestionsToBeEnabled, new IdentityComparer(), rosterDatas)));

            this.ApplyValidityChangesEvents(new ValidityChanges(null,
                this.GetUnionOfUniqueRosterDataPropertiesByRosterAndNestedRosters(d => d.InitializedQuestionsToBeInvalid, new IdentityComparer(), rosterDatas)));
        }

        private ChangedRosterInstanceTitleDto[] CreateChangedRosterRowTitleDtoFromRosterData(params RosterCalculationData[] datas)
        {
            var result = new List<ChangedRosterInstanceTitleDto>();
            foreach (var data in datas)
            {
                if (data.TitlesForRosterInstancesToAdd != null)
                {
                    var rosterRowTitlesChanged = new HashSet<RosterIdentity>(data.RosterInstancesToAdd, new RosterIdentityComparer());
                    if (data.RosterInstancesToChange != null)
                    {
                        foreach (var rosterIdentity in data.RosterInstancesToChange)
                        {
                            rosterRowTitlesChanged.Add(rosterIdentity);
                        }
                    }

                    foreach (var rosterIdentity in rosterRowTitlesChanged)
                    {
                        result.Add(
                            new ChangedRosterInstanceTitleDto(new RosterInstance(rosterIdentity.GroupId, rosterIdentity.OuterRosterVector,
                                rosterIdentity.RosterInstanceId), data.TitlesForRosterInstancesToAdd[rosterIdentity.RosterInstanceId]));
                    }
                }

                foreach (var nestedRosterData in data.RosterInstantiatesFromNestedLevels)
                {
                    result.AddRange(CreateChangedRosterRowTitleDtoFromRosterData(nestedRosterData));
                }
            }
            return result.ToArray();
        }

        private List<T> GetUnionOfUniqueRosterDataPropertiesByRosterAndNestedRosters<T>(Func<RosterCalculationData, IEnumerable<T>> getProperty, IEqualityComparer<T> equalityComparer, params RosterCalculationData[] datas)
        {
            var result = new List<T>();
            foreach (var data in datas)
            {
                var propertyValue = getProperty(data);
                if (propertyValue != null)
                    result.AddRange(propertyValue);

                foreach (var rosterInstantiatesFromNestedLevel in data.RosterInstantiatesFromNestedLevels)
                {
                    result.AddRange(this.GetUnionOfUniqueRosterDataPropertiesByRosterAndNestedRosters(
                        getProperty, equalityComparer, rosterInstantiatesFromNestedLevel));
                }
            }

            return result.Distinct(equalityComparer).ToList();
        }

        private void ApplyAnswersEvents(IEnumerable<AnswerChange> interviewByAnswerChanges)
        {
            foreach (AnswerChange change in interviewByAnswerChanges)
            {
                switch (change.InterviewChangeType)
                {
                    case AnswerChangeType.Text:
                        this.ApplyEvent(new TextQuestionAnswered(change.UserId, change.QuestionId, change.RosterVector, change.AnswerTime, (string)change.Answer));
                        break;
                    case AnswerChangeType.NumericInteger:
                        this.ApplyEvent(new NumericIntegerQuestionAnswered(change.UserId, change.QuestionId, change.RosterVector, change.AnswerTime, (int)change.Answer));
                        break;
                    case AnswerChangeType.NumericReal:
                        this.ApplyEvent(new NumericRealQuestionAnswered(change.UserId, change.QuestionId, change.RosterVector, change.AnswerTime, (decimal)change.Answer));
                        break;
                    case AnswerChangeType.DateTime:
                        this.ApplyEvent(new DateTimeQuestionAnswered(change.UserId, change.QuestionId, change.RosterVector, change.AnswerTime, (DateTime)change.Answer));
                        break;
                    case AnswerChangeType.SingleOption:
                        this.ApplyEvent(new SingleOptionQuestionAnswered(change.UserId, change.QuestionId, change.RosterVector, change.AnswerTime, (decimal)change.Answer));
                        break;
                    case AnswerChangeType.MultipleOptions:
                        this.ApplyEvent(new MultipleOptionsQuestionAnswered(change.UserId, change.QuestionId, change.RosterVector, change.AnswerTime, (decimal[])change.Answer));
                        break;
                    case AnswerChangeType.QRBarcode:
                        this.ApplyEvent(new QRBarcodeQuestionAnswered(change.UserId, change.QuestionId, change.RosterVector, change.AnswerTime, (string)change.Answer));
                        break;
                    case AnswerChangeType.MultipleOptionsLinked:
                        this.ApplyEvent(new MultipleOptionsLinkedQuestionAnswered(change.UserId, change.QuestionId, change.RosterVector, change.AnswerTime, (decimal[][])change.Answer));
                        break;
                    case AnswerChangeType.SingleOptionLinked:
                        this.ApplyEvent(new SingleOptionLinkedQuestionAnswered(change.UserId, change.QuestionId, change.RosterVector, change.AnswerTime, (decimal[])change.Answer));
                        break;
                    case AnswerChangeType.TextList:
                        this.ApplyEvent(new TextListQuestionAnswered(change.UserId, change.QuestionId, change.RosterVector, change.AnswerTime, (Tuple<decimal, string>[])change.Answer));
                        break;
                    case AnswerChangeType.GeoLocation:
                        var geoPosition = (GeoLocationPoint)change.Answer;
                        this.ApplyEvent(new GeoLocationQuestionAnswered(change.UserId, change.QuestionId, change.RosterVector, change.AnswerTime, geoPosition.Latitude, geoPosition.Longitude,
                            geoPosition.Accuracy, geoPosition.Altitude, geoPosition.Timestamp));
                        break;

                    default:
                        throw new InterviewException(string.Format(
                            "Answer on Question {0} has type {1} which is not supported in applying method.",
                            change.QuestionId, change.InterviewChangeType));
                }
            }
        }

        #endregion

        #region CheckInvariants

        private void CheckNumericRealQuestionInvariants(Guid questionId, decimal[] rosterVector, decimal answer,
           IQuestionnaire questionnaire,
           Identity answeredQuestion, InterviewStateDependentOnAnswers currentInterviewState, bool applyStrongChecks = true)
        {
            ThrowIfQuestionDoesNotExist(questionId, questionnaire);
            this.ThrowIfRosterVectorIsIncorrect(currentInterviewState, questionId, rosterVector, questionnaire);
            this.ThrowIfQuestionTypeIsNotOneOfExpected(questionId, questionnaire, QuestionType.Numeric);
            this.ThrowIfNumericQuestionIsNotReal(questionId, questionnaire);
            if (applyStrongChecks)
            {
                ThrowIfNumericAnswerExceedsMaxValue(questionId, answer, questionnaire);
                ThrowIfQuestionOrParentGroupIsDisabled(currentInterviewState, answeredQuestion, questionnaire);
                this.ThrowIfAnswerHasMoreDecimalPlacesThenAccepted(questionnaire, questionId, answer);
            }
        }

        private void CheckDateTimeQuestionInvariants(Guid questionId, decimal[] rosterVector, IQuestionnaire questionnaire,
            Identity answeredQuestion, InterviewStateDependentOnAnswers currentInterviewState, bool applyStrongChecks = true)
        {
            ThrowIfQuestionDoesNotExist(questionId, questionnaire);
            this.ThrowIfRosterVectorIsIncorrect(currentInterviewState, questionId, rosterVector, questionnaire);
            this.ThrowIfQuestionTypeIsNotOneOfExpected(questionId, questionnaire, QuestionType.DateTime);
            if (applyStrongChecks)
                ThrowIfQuestionOrParentGroupIsDisabled(currentInterviewState, answeredQuestion, questionnaire);
        }

        private void CheckSingleOptionQuestionInvariants(Guid questionId, decimal[] rosterVector, decimal selectedValue,
            IQuestionnaire questionnaire, Identity answeredQuestion, InterviewStateDependentOnAnswers currentInterviewState,
            bool applyStrongChecks = true)
        {
            ThrowIfQuestionDoesNotExist(questionId, questionnaire);
            this.ThrowIfRosterVectorIsIncorrect(currentInterviewState, questionId, rosterVector, questionnaire);
            this.ThrowIfQuestionTypeIsNotOneOfExpected(questionId, questionnaire, QuestionType.SingleOption);
            ThrowIfValueIsNotOneOfAvailableOptions(questionId, selectedValue, questionnaire);
            if (applyStrongChecks)
                ThrowIfQuestionOrParentGroupIsDisabled(currentInterviewState, answeredQuestion, questionnaire);
        }

        private void CheckMultipleOptionQuestionInvariants(Guid questionId, decimal[] rosterVector, decimal[] selectedValues,
            IQuestionnaire questionnaire, Identity answeredQuestion, InterviewStateDependentOnAnswers currentInterviewState,
            bool applyStrongChecks = true)
        {
            ThrowIfQuestionDoesNotExist(questionId, questionnaire);
            this.ThrowIfRosterVectorIsIncorrect(currentInterviewState, questionId, rosterVector, questionnaire);
            this.ThrowIfQuestionTypeIsNotOneOfExpected(questionId, questionnaire, QuestionType.MultyOption);
            ThrowIfSomeValuesAreNotFromAvailableOptions(questionId, selectedValues, questionnaire);
            if (applyStrongChecks)
            {
                ThrowIfLengthOfSelectedValuesMoreThanMaxForSelectedAnswerOptions(questionId, selectedValues.Length, questionnaire);
                ThrowIfQuestionOrParentGroupIsDisabled(currentInterviewState, answeredQuestion, questionnaire);
            }
        }

        private void CheckTextQuestionInvariants(Guid questionId, decimal[] rosterVector, IQuestionnaire questionnaire,
            Identity answeredQuestion, InterviewStateDependentOnAnswers currentInterviewState, bool applyStrongChecks = true)
        {
            ThrowIfQuestionDoesNotExist(questionId, questionnaire);
            this.ThrowIfRosterVectorIsIncorrect(currentInterviewState, questionId, rosterVector, questionnaire);
            this.ThrowIfQuestionTypeIsNotOneOfExpected(questionId, questionnaire, QuestionType.Text);
            if (applyStrongChecks)
                ThrowIfQuestionOrParentGroupIsDisabled(currentInterviewState, answeredQuestion, questionnaire);
        }

        private void CheckNumericIntegerQuestionInvariants(Guid questionId, decimal[] rosterVector, int answer, IQuestionnaire questionnaire,
            Identity answeredQuestion, InterviewStateDependentOnAnswers currentInterviewState, bool applyStrongChecks = true)
        {
            ThrowIfQuestionDoesNotExist(questionId, questionnaire);
            this.ThrowIfRosterVectorIsIncorrect(currentInterviewState, questionId, rosterVector, questionnaire);
            this.ThrowIfQuestionTypeIsNotOneOfExpected(questionId, questionnaire, QuestionType.AutoPropagate, QuestionType.Numeric);
            this.ThrowIfNumericQuestionIsNotInteger(questionId, questionnaire);
            if (applyStrongChecks)
            {
                ThrowIfNumericAnswerExceedsMaxValue(questionId, answer, questionnaire);
                ThrowIfQuestionOrParentGroupIsDisabled(currentInterviewState, answeredQuestion, questionnaire);

                if (questionnaire.ShouldQuestionSpecifyRosterSize(questionId))
                {
                    ThrowIfRosterSizeAnswerIsNegative(questionId, answer, questionnaire);
                }
            }
        }

        #endregion

        #region Calculations

        private InterviewChanges CalculateInterviewChangesOnAnswerTextQuestion(InterviewStateDependentOnAnswers state, Guid userId,
            Guid questionId, decimal[] rosterVector, DateTime answerTime, string answer,
            Identity answeredQuestion, Func<InterviewStateDependentOnAnswers, Identity, object> getAnswer, IQuestionnaire questionnaire)
        {
            EnablementChanges enablementChanges = this.CalculateEnablementChanges(state,
                answeredQuestion, answer, questionnaire, GetRosterInstanceIds);

            List<Identity> answersForLinkedQuestionsToRemoveByDisabling =
                this.GetAnswersForLinkedQuestionsToRemoveBecauseOfDisabledGroupsOrQuestions(state,
                    enablementChanges.GroupsToBeDisabled, enablementChanges.QuestionsToBeDisabled, questionnaire, GetRosterInstanceIds);

            Func<Identity, bool?> getNewQuestionState =
                question =>
                {
                    if (enablementChanges.QuestionsToBeDisabled.Any(q => AreEqual(q, question))) return false;
                    if (enablementChanges.QuestionsToBeEnabled.Any(q => AreEqual(q, question))) return true;
                    return null;
                };

            Func<InterviewStateDependentOnAnswers, Identity, object> getAnswerConcerningDisabling =
                (currentState, question) => AreEqual(question, answeredQuestion)
                    ? answer
                    : GetAnswerSupportedInExpressionsForEnabledOrNull(state, question, getNewQuestionState);

            List<Identity> answersDeclaredValid, answersDeclaredInvalid;
            this.PerformValidationOfAnsweredQuestionAndDependentQuestionsAndJustEnabledQuestions(state,
                answeredQuestion, questionnaire, getAnswerConcerningDisabling, getNewQuestionState,
                enablementChanges,
                out answersDeclaredValid, out answersDeclaredInvalid);


            var expressionProcessorState = this.expressionProcessorStatePrototype.Clone();
            expressionProcessorState.UpdateTextAnswer(questionId, rosterVector, answer);

            //Apply other changes on expressionProcessorState
            List<Identity> answersDeclaredValidC, answersDeclaredInvalidC;
            this.expressionSharpProcessor.ProcessValidationExpressions(expressionProcessorState, out answersDeclaredValidC, out answersDeclaredInvalidC);

            answersDeclaredValid.Union(answersDeclaredValidC);
            answersDeclaredInvalid.Union(answersDeclaredInvalidC);

            List<RosterIdentity> rosterInstancesWithAffectedTitles = CalculateRosterInstancesWhichTitlesAreAffected(
                questionId, rosterVector, questionnaire);

            var interviewByAnswerChange = new List<AnswerChange>
            {
                new AnswerChange(AnswerChangeType.Text, userId, questionId, rosterVector, answerTime, answer)
            };

            return new InterviewChanges(interviewByAnswerChange, enablementChanges, new ValidityChanges(answersDeclaredValid, answersDeclaredInvalid),
                null, answersForLinkedQuestionsToRemoveByDisabling, rosterInstancesWithAffectedTitles, answer);
        }

        private InterviewChanges CalculateInterviewChangesOnAnswerNumericIntegerQuestion(InterviewStateDependentOnAnswers state, Guid userId,
            Guid questionId, decimal[] rosterVector, DateTime answerTime, int answer, Identity answeredQuestion,
            Func<InterviewStateDependentOnAnswers, Identity, object> getAnswer, IQuestionnaire questionnaire)
        {
            List<Guid> rosterIds = questionnaire.GetRosterGroupsByRosterSizeQuestion(questionId).ToList();
            int rosterSize = rosterIds.Any() ? ToRosterSize(answer) : 0;

            Func<Guid, decimal[], bool> isRoster = (groupId, groupOuterScopeRosterVector)
                => rosterIds.Contains(groupId)
                    && AreEqualRosterVectors(groupOuterScopeRosterVector, rosterVector);

            var rosterInstanceIds = new DistinctDecimalList(Enumerable.Range(0, rosterSize).Select(index => (decimal)index).ToList());

            Func<InterviewStateDependentOnAnswers, Guid, decimal[], DistinctDecimalList> getRosterInstanceIds =
                (currentstate, groupId, groupOuterRosterVector)
                    => isRoster(groupId, groupOuterRosterVector)
                        ? rosterInstanceIds
                        : GetRosterInstanceIds(state, groupId, groupOuterRosterVector);

            RosterCalculationData rosterCalculationData = CalculateRosterData(state, questionnaire,
                rosterIds, rosterVector, rosterInstanceIds, null, questionnaire, getAnswer, getRosterInstanceIds);

            EnablementChanges enablementChanges = this.CalculateEnablementChanges(state,
                answeredQuestion, answer, questionnaire, getRosterInstanceIds);

            List<Identity> answersForLinkedQuestionsToRemoveByDisabling =
                this.GetAnswersForLinkedQuestionsToRemoveBecauseOfDisabledGroupsOrQuestions(state,
                    Enumerable.Concat(rosterCalculationData.InitializedGroupsToBeDisabled, enablementChanges.GroupsToBeDisabled),
                    Enumerable.Concat(rosterCalculationData.InitializedQuestionsToBeDisabled, enablementChanges.QuestionsToBeDisabled),
                    questionnaire, getRosterInstanceIds);

            Func<Identity, bool?> getNewQuestionState =
                question =>
                {
                    if (rosterCalculationData.InitializedQuestionsToBeDisabled.Any(q => AreEqual(q, question)) ||
                        enablementChanges.QuestionsToBeDisabled.Any(q => AreEqual(q, question))) return false;
                    if (rosterCalculationData.InitializedQuestionsToBeEnabled.Any(q => AreEqual(q, question)) ||
                        enablementChanges.QuestionsToBeEnabled.Any(q => AreEqual(q, question))) return true;
                    return null;
                };

            Func<InterviewStateDependentOnAnswers, Identity, object> getAnswerConcerningDisabling = (currentState, question) =>
                AreEqual(question, answeredQuestion)
                    ? answer
                    : GetAnswerSupportedInExpressionsForEnabledOrNull(state, question, getNewQuestionState);

            List<Identity> answersDeclaredValid, answersDeclaredInvalid;
            this.PerformValidationOfAnsweredQuestionAndDependentQuestionsAndJustEnabledQuestions(state,
                answeredQuestion, questionnaire, getAnswerConcerningDisabling, getNewQuestionState,
                enablementChanges,
                out answersDeclaredValid, out answersDeclaredInvalid);

            var expressionProcessorState = this.expressionProcessorStatePrototype.Clone();
            expressionProcessorState.UpdateIntAnswer(questionId, rosterVector, answer);

            //Apply other changes on expressionProcessorState
            List<Identity> answersDeclaredValidC, answersDeclaredInvalidC;
            this.expressionSharpProcessor.ProcessValidationExpressions(expressionProcessorState, out answersDeclaredValidC, out answersDeclaredInvalidC);

            answersDeclaredValid.Union(answersDeclaredValidC);
            answersDeclaredInvalid.Union(answersDeclaredInvalidC);


            List<RosterIdentity> rosterInstancesWithAffectedTitles = CalculateRosterInstancesWhichTitlesAreAffected(
                questionId, rosterVector, questionnaire);

            var interviewByAnswerChange = new List<AnswerChange>
            {
                new AnswerChange(AnswerChangeType.NumericInteger, userId, questionId, rosterVector, answerTime, answer)
            };

            return new InterviewChanges(interviewByAnswerChange, enablementChanges, new ValidityChanges(answersDeclaredValid, answersDeclaredInvalid),
                rosterCalculationData, answersForLinkedQuestionsToRemoveByDisabling, rosterInstancesWithAffectedTitles, AnswerUtils.AnswerToString(answer));
        }

        private InterviewChanges CalculateInterviewChangesOnAnswerDateTimeQuestion(InterviewStateDependentOnAnswers state, Guid userId,
            Guid questionId, decimal[] rosterVector, DateTime answerTime, DateTime answer,
            Identity answeredQuestion, Func<InterviewStateDependentOnAnswers, Identity, object> getAnswer, IQuestionnaire questionnaire)
        {
            EnablementChanges enablementChanges = this.CalculateEnablementChanges(state,
                answeredQuestion, answer, questionnaire, GetRosterInstanceIds);

            List<Identity> answersForLinkedQuestionsToRemoveByDisabling =
                this.GetAnswersForLinkedQuestionsToRemoveBecauseOfDisabledGroupsOrQuestions(state,
                    enablementChanges.GroupsToBeDisabled, enablementChanges.QuestionsToBeDisabled, questionnaire, GetRosterInstanceIds);

            Func<Identity, bool?> getNewQuestionState =
                question =>
                {
                    if (enablementChanges.QuestionsToBeDisabled.Any(q => AreEqual(q, question))) return false;
                    if (enablementChanges.QuestionsToBeEnabled.Any(q => AreEqual(q, question))) return true;
                    return null;
                };
            Func<InterviewStateDependentOnAnswers, Identity, object> getAnswerConcerningDisabling =
                (currentState, question) =>
                    AreEqual(question, answeredQuestion)
                        ? answer
                        : GetAnswerSupportedInExpressionsForEnabledOrNull(state, question, getNewQuestionState);

            List<RosterIdentity> rosterInstancesWithAffectedTitles = CalculateRosterInstancesWhichTitlesAreAffected(
                questionId, rosterVector, questionnaire);

            List<Identity> answersDeclaredValid, answersDeclaredInvalid;
            this.PerformValidationOfAnsweredQuestionAndDependentQuestionsAndJustEnabledQuestions(state,
                answeredQuestion, questionnaire, getAnswerConcerningDisabling, getNewQuestionState,
                enablementChanges,
                out answersDeclaredValid, out answersDeclaredInvalid);

            var expressionProcessorState = this.expressionProcessorStatePrototype.Clone();
            expressionProcessorState.UpdateDateAnswer(questionId, rosterVector, answer);

            //Apply other changes on expressionProcessorState
            List<Identity> answersDeclaredValidC, answersDeclaredInvalidC;
            this.expressionSharpProcessor.ProcessValidationExpressions(expressionProcessorState, out answersDeclaredValidC, out answersDeclaredInvalidC);

            answersDeclaredValid.Union(answersDeclaredValidC);
            answersDeclaredInvalid.Union(answersDeclaredInvalidC);


            var interviewByAnswerChange = new List<AnswerChange>
            {
                new AnswerChange(AnswerChangeType.DateTime, userId, questionId, rosterVector, answerTime, answer)
            };

            return new InterviewChanges(interviewByAnswerChange, enablementChanges, new ValidityChanges(answersDeclaredValid, answersDeclaredInvalid),
                null, answersForLinkedQuestionsToRemoveByDisabling, rosterInstancesWithAffectedTitles, AnswerUtils.AnswerToString(answer));
        }

        private InterviewChanges CalculateInterviewChangesOnAnswerMultipleOptionsQuestion(InterviewStateDependentOnAnswers state,
            Guid userId, Guid questionId, decimal[] rosterVector, DateTime answerTime,
            decimal[] selectedValues, Identity answeredQuestion, Func<InterviewStateDependentOnAnswers, Identity, object> getAnswer,
            IQuestionnaire questionnaire)
        {
            List<decimal> availableValues = questionnaire.GetAnswerOptionsAsValues(questionId).ToList();

            var rosterInstanceIds = new DistinctDecimalList(selectedValues.ToList());
            Dictionary<decimal, int?> rosterInstanceIdsWithSortIndexes = selectedValues.ToDictionary(
                selectedValue => selectedValue,
                selectedValue => (int?)availableValues.IndexOf(selectedValue));

            List<Guid> rosterIds = questionnaire.GetRosterGroupsByRosterSizeQuestion(questionId).ToList();

            Func<Guid, decimal[], bool> isRoster = (groupId, groupOuterRosterVector)
                => rosterIds.Contains(groupId)
                    && AreEqualRosterVectors(groupOuterRosterVector, rosterVector);

            Func<InterviewStateDependentOnAnswers, Guid, decimal[], DistinctDecimalList> getRosterInstanceIds =
                (currentState, groupId, groupOuterRosterVector)
                    => isRoster(groupId, groupOuterRosterVector)
                        ? rosterInstanceIds
                        : GetRosterInstanceIds(state, groupId, groupOuterRosterVector);

            RosterCalculationData rosterCalculationData = this.CalculateRosterDataWithRosterTitlesFromMultipleOptionsQuestions(state,
                questionnaire,
                questionId, rosterVector, rosterIds, rosterInstanceIdsWithSortIndexes, questionnaire, getAnswer, getRosterInstanceIds);

            EnablementChanges enablementChanges = this.CalculateEnablementChanges(state,
                answeredQuestion, selectedValues, questionnaire, getRosterInstanceIds);

            List<Identity> answersForLinkedQuestionsToRemoveByDisabling =
                this.GetAnswersForLinkedQuestionsToRemoveBecauseOfDisabledGroupsOrQuestions(state,
                    Enumerable.Concat(rosterCalculationData.InitializedGroupsToBeDisabled, enablementChanges.GroupsToBeDisabled),
                    Enumerable.Concat(rosterCalculationData.InitializedQuestionsToBeDisabled, enablementChanges.QuestionsToBeDisabled),
                    questionnaire, getRosterInstanceIds);

            Func<Identity, bool?> getNewQuestionState =
                question =>
                {
                    if (rosterCalculationData.InitializedQuestionsToBeDisabled.Any(q => AreEqual(q, question)) ||
                        enablementChanges.QuestionsToBeDisabled.Any(q => AreEqual(q, question))) return false;
                    if (rosterCalculationData.InitializedQuestionsToBeEnabled.Any(q => AreEqual(q, question)) ||
                        enablementChanges.QuestionsToBeEnabled.Any(q => AreEqual(q, question))) return true;
                    return null;
                };

            Func<InterviewStateDependentOnAnswers, Identity, object> getAnswerConcerningDisabling = (currentState, question) =>
                AreEqual(question, answeredQuestion)
                    ? (selectedValues.Any() ? selectedValues : null)
                    : GetAnswerSupportedInExpressionsForEnabledOrNull(state, question, getNewQuestionState);

            List<Identity> answersDeclaredValid, answersDeclaredInvalid;
            this.PerformValidationOfAnsweredQuestionAndDependentQuestionsAndJustEnabledQuestions(state,
                answeredQuestion, questionnaire, getAnswerConcerningDisabling, getNewQuestionState,
                enablementChanges,
                out answersDeclaredValid, out answersDeclaredInvalid);

            var expressionProcessorState = this.expressionProcessorStatePrototype.Clone();
            expressionProcessorState.UpdateMultiOptionAnswer(questionId, rosterVector, selectedValues);

            //Apply other changes on expressionProcessorState
            List<Identity> answersDeclaredValidC, answersDeclaredInvalidC;
            this.expressionSharpProcessor.ProcessValidationExpressions(expressionProcessorState, out answersDeclaredValidC, out answersDeclaredInvalidC);

            answersDeclaredValid.Union(answersDeclaredValidC);
            answersDeclaredInvalid.Union(answersDeclaredInvalidC);

            List<RosterIdentity> rosterInstancesWithAffectedTitles = CalculateRosterInstancesWhichTitlesAreAffected(
                questionId, rosterVector, questionnaire);
            string answerFormattedAsRosterTitle = AnswerUtils.AnswerToString(selectedValues,
                answerOptionValue => questionnaire.GetAnswerOptionTitle(questionId, answerOptionValue));

            var interviewByAnswerChange = new List<AnswerChange>
            {
                new AnswerChange(AnswerChangeType.MultipleOptions, userId, questionId, rosterVector, answerTime, selectedValues)
            };

            return new InterviewChanges(interviewByAnswerChange, enablementChanges, new ValidityChanges(answersDeclaredValid, answersDeclaredInvalid),
                rosterCalculationData,
                answersForLinkedQuestionsToRemoveByDisabling, rosterInstancesWithAffectedTitles, answerFormattedAsRosterTitle);
        }

        private InterviewChanges CalculateInterviewChangesOnAnswerNumericRealQuestion(InterviewStateDependentOnAnswers state, Guid userId,
            Guid questionId, decimal[] rosterVector,
            DateTime answerTime, decimal answer, Identity answeredQuestion,
            Func<InterviewStateDependentOnAnswers, Identity, object> getAnswer, IQuestionnaire questionnaire)
        {
            EnablementChanges enablementChanges = this.CalculateEnablementChanges(state,
                answeredQuestion, answer, questionnaire, GetRosterInstanceIds);

            List<Identity> answersForLinkedQuestionsToRemoveByDisabling =
                this.GetAnswersForLinkedQuestionsToRemoveBecauseOfDisabledGroupsOrQuestions(state,
                    enablementChanges.GroupsToBeDisabled,
                    enablementChanges.QuestionsToBeDisabled,
                    questionnaire, GetRosterInstanceIds);

            Func<Identity, bool?> getNewQuestionState =
                question =>
                {
                    if (enablementChanges.QuestionsToBeDisabled.Any(q => AreEqual(q, question))) return false;
                    if (enablementChanges.QuestionsToBeEnabled.Any(q => AreEqual(q, question))) return true;
                    return null;
                };

            Func<InterviewStateDependentOnAnswers, Identity, object> getAnswerConcerningDisabling =
                (currentState, question) =>
                    AreEqual(question, answeredQuestion)
                        ? answer
                        : GetAnswerSupportedInExpressionsForEnabledOrNull(state, question, getNewQuestionState);

            List<Identity> answersDeclaredValid, answersDeclaredInvalid;
            this.PerformValidationOfAnsweredQuestionAndDependentQuestionsAndJustEnabledQuestions(state,
                answeredQuestion, questionnaire, getAnswerConcerningDisabling, getNewQuestionState,
                enablementChanges,
                out answersDeclaredValid, out answersDeclaredInvalid);


            var expressionProcessorState = this.expressionProcessorStatePrototype.Clone();
            expressionProcessorState.UpdateDecimalAnswer(questionId, rosterVector, answer);

            //Apply other changes on expressionProcessorState
            List<Identity> answersDeclaredValidC, answersDeclaredInvalidC;
            this.expressionSharpProcessor.ProcessValidationExpressions(expressionProcessorState, out answersDeclaredValidC, out answersDeclaredInvalidC);

            answersDeclaredValid.Union(answersDeclaredValidC);
            answersDeclaredInvalid.Union(answersDeclaredInvalidC);

            List<RosterIdentity> rosterInstancesWithAffectedTitles = CalculateRosterInstancesWhichTitlesAreAffected(
                questionId, rosterVector, questionnaire);

            var interviewByAnswerChange = new List<AnswerChange>
            {
                new AnswerChange(AnswerChangeType.NumericReal, userId, questionId, rosterVector, answerTime, answer)
            };

            return new InterviewChanges(interviewByAnswerChange, enablementChanges, new ValidityChanges(answersDeclaredValid, answersDeclaredInvalid),
                null, answersForLinkedQuestionsToRemoveByDisabling, rosterInstancesWithAffectedTitles, AnswerUtils.AnswerToString(answer));
        }

        private InterviewChanges CalculateInterviewChangesOnAnswerSingleOptionQuestion(InterviewStateDependentOnAnswers state, Guid userId,
            Guid questionId, decimal[] rosterVector, DateTime answerTime,
            decimal selectedValue, Identity answeredQuestion, Func<InterviewStateDependentOnAnswers, Identity, object> getAnswer,
            IQuestionnaire questionnaire)
        {
            EnablementChanges enablementChanges = this.CalculateEnablementChanges(state,
                answeredQuestion, selectedValue, questionnaire, GetRosterInstanceIds);

            List<Identity> answersForLinkedQuestionsToRemoveByDisabling =
                this.GetAnswersForLinkedQuestionsToRemoveBecauseOfDisabledGroupsOrQuestions(state,
                    enablementChanges.GroupsToBeDisabled, enablementChanges.QuestionsToBeDisabled, questionnaire, GetRosterInstanceIds);

            Func<Identity, bool?> getNewQuestionState =
                question =>
                {
                    if (enablementChanges.QuestionsToBeDisabled.Any(q => AreEqual(q, question))) return false;
                    if (enablementChanges.QuestionsToBeEnabled.Any(q => AreEqual(q, question))) return true;
                    return null;
                };

            Func<InterviewStateDependentOnAnswers, Identity, object> getAnswerConcerningDisabling =
                (currentState, question) =>
                    AreEqual(question, answeredQuestion)
                        ? selectedValue
                        : GetAnswerSupportedInExpressionsForEnabledOrNull(state, question, getNewQuestionState);

            List<Identity> answersDeclaredValid, answersDeclaredInvalid;

            this.PerformValidationOfAnsweredQuestionAndDependentQuestionsAndJustEnabledQuestions(state,
                answeredQuestion, questionnaire, getAnswerConcerningDisabling, getNewQuestionState,
                enablementChanges,
                out answersDeclaredValid, out answersDeclaredInvalid);

            List<RosterIdentity> rosterInstancesWithAffectedTitles = CalculateRosterInstancesWhichTitlesAreAffected(
                questionId, rosterVector, questionnaire);
            string answerFormattedAsRosterTitle = AnswerUtils.AnswerToString(selectedValue,
                answerOptionValue => questionnaire.GetAnswerOptionTitle(questionId, answerOptionValue));

            var expressionProcessorState = this.expressionProcessorStatePrototype.Clone();
            expressionProcessorState.UpdateSingleOptionAnswer(questionId, rosterVector, selectedValue);

            //Apply other changes on expressionProcessorState
            List<Identity> answersDeclaredValidC, answersDeclaredInvalidC;
            this.expressionSharpProcessor.ProcessValidationExpressions(expressionProcessorState, out answersDeclaredValidC, out answersDeclaredInvalidC);

            answersDeclaredValid.Union(answersDeclaredValidC);
            answersDeclaredInvalid.Union(answersDeclaredInvalidC);


            var interviewByAnswerChange = new List<AnswerChange>
            {
                new AnswerChange(AnswerChangeType.SingleOption, userId, questionId, rosterVector, answerTime, selectedValue)
            };

            return new InterviewChanges(interviewByAnswerChange, enablementChanges, new ValidityChanges(answersDeclaredValid, answersDeclaredInvalid),
                null, answersForLinkedQuestionsToRemoveByDisabling, rosterInstancesWithAffectedTitles, answerFormattedAsRosterTitle);
        }


        private void CalculateChangesByFeaturedQuestion(InterviewChangeStructures changeStructures, Guid userId,
            IQuestionnaire questionnaire, Dictionary<Guid, object> answersToFeaturedQuestions,
            DateTime answersTime, Dictionary<Identity, object> newAnswers, decimal[] rosterVector = null)
        {
            var currentQuestionRosterVector = rosterVector ?? EmptyRosterVector;
            Func<InterviewStateDependentOnAnswers, Identity, object> getAnswer =
                (currentState, question) => newAnswers.Any(x => AreEqual(question, x.Key))
                    ? newAnswers.SingleOrDefault(x => AreEqual(question, x.Key)).Value
                    : GetEnabledQuestionAnswerSupportedInExpressions(changeStructures.State, question, questionnaire);

            foreach (KeyValuePair<Guid, object> answerToFeaturedQuestion in answersToFeaturedQuestions)
            {
                Guid questionId = answerToFeaturedQuestion.Key;
                object answer = answerToFeaturedQuestion.Value;

                var answeredQuestion = new Identity(questionId, currentQuestionRosterVector);
                QuestionType questionType = questionnaire.GetQuestionType(questionId);

                InterviewChanges interviewChanges;

                switch (questionType)
                {
                    case QuestionType.Text:
                        interviewChanges =
                            this.CalculateInterviewChangesOnAnswerTextQuestion(changeStructures.State, userId, questionId,
                                currentQuestionRosterVector, answersTime, (string)answer, answeredQuestion, getAnswer, questionnaire);
                        break;

                    case QuestionType.AutoPropagate:
                        interviewChanges =
                            this.CalculateInterviewChangesOnAnswerNumericIntegerQuestion(changeStructures.State, userId, questionId,
                                currentQuestionRosterVector, answersTime, (int)answer, answeredQuestion, getAnswer, questionnaire);
                        break;
                    case QuestionType.Numeric:
                        if (questionnaire.IsQuestionInteger(questionId))
                            interviewChanges =
                                this.CalculateInterviewChangesOnAnswerNumericIntegerQuestion(changeStructures.State, userId, questionId,
                                    currentQuestionRosterVector, answersTime, (int)answer, answeredQuestion, getAnswer, questionnaire);
                        else
                            interviewChanges =
                                this.CalculateInterviewChangesOnAnswerNumericRealQuestion(changeStructures.State, userId, questionId,
                                    currentQuestionRosterVector, answersTime, (decimal)answer, answeredQuestion, getAnswer, questionnaire);
                        break;

                    case QuestionType.DateTime:
                        interviewChanges =
                            this.CalculateInterviewChangesOnAnswerDateTimeQuestion(changeStructures.State, userId, questionId,
                                currentQuestionRosterVector, answersTime, (DateTime)answer, answeredQuestion, getAnswer, questionnaire);
                        break;

                    case QuestionType.SingleOption:
                        interviewChanges =
                            this.CalculateInterviewChangesOnAnswerSingleOptionQuestion(changeStructures.State, userId, questionId,
                                currentQuestionRosterVector, answersTime, (decimal)answer, answeredQuestion, getAnswer, questionnaire);
                        break;

                    case QuestionType.MultyOption:
                        interviewChanges =
                            this.CalculateInterviewChangesOnAnswerMultipleOptionsQuestion(changeStructures.State, userId, questionId,
                                currentQuestionRosterVector, answersTime, (decimal[])answer, answeredQuestion, getAnswer, questionnaire);
                        break;
                    case QuestionType.QRBarcode:
                        interviewChanges =
                            this.CalculateInterviewChangesOnAnswerQRBarcodeQuestion(changeStructures.State, userId, questionId, currentQuestionRosterVector, answersTime, (string)answer, answeredQuestion, questionnaire);
                        break;
                    case QuestionType.GpsCoordinates:
                        var geoAnswer = answer as GeoPosition;
                        interviewChanges =
                            this.CalculateInterviewChangesOnAnswerGeoLocationQuestion(changeStructures.State, userId, questionId,
                                currentQuestionRosterVector, answersTime, geoAnswer.Latitude, geoAnswer.Longitude, geoAnswer.Accuracy,
                                geoAnswer.Altitude, geoAnswer.Timestamp, answeredQuestion, questionnaire);
                        break;
                    case QuestionType.TextList:
                        interviewChanges =
                            this.CalculateInterviewChangesOnAnswerTextListQuestion(changeStructures.State, userId, questionId, currentQuestionRosterVector, answersTime, (Tuple<decimal, string>[])answer, answeredQuestion, getAnswer, questionnaire);
                        break;

                    default:
                        throw new InterviewException(string.Format(
                            "Question {0} has type {1} which is not supported as initial pre-filled question.",
                            questionId, questionType));
                }

                changeStructures.State.ApplyInterviewChanges(interviewChanges);
                changeStructures.Changes.Add(interviewChanges);
            }
        }

        private EnablementChanges CalculateEnablementChanges(InterviewStateDependentOnAnswers state, Identity answeredQuestion,
            object answer, IQuestionnaire questionnaire,
            Func<InterviewStateDependentOnAnswers, Guid, decimal[], DistinctDecimalList> getRosterInstanceIds)
        {
            List<Identity> groupsToBeDisabled, groupsToBeEnabled, questionsToBeDisabled, questionsToBeEnabled;

            groupsToBeDisabled = new List<Identity>();
            groupsToBeEnabled = new List<Identity>();

            var collectedQuestionsToBeDisabled = new List<Identity>();
            var collectedQuestionsToBeEnabled = new List<Identity>();

            Func<InterviewStateDependentOnAnswers, Identity, bool> isQuestionDisabled = (currentState, question) =>
            {
                bool isQuestionToBeDisabled =
                    collectedQuestionsToBeDisabled.Any(questionToBeDisabled => AreEqual(questionToBeDisabled, question));
                bool isQuestionToBeEnabled =
                    collectedQuestionsToBeEnabled.Any(questionToBeEnabled => AreEqual(questionToBeEnabled, question));

                return isQuestionToBeDisabled || !isQuestionToBeEnabled && IsQuestionDisabled(state, question);
            };

            Func<InterviewStateDependentOnAnswers, Identity, bool> isGroupDisabled = (currentState, group) =>
            {
                bool isGroupToBeDisabled =
                    groupsToBeDisabled.Any(groupToBeDisabled => AreEqual(groupToBeDisabled, group));
                bool isQuestionToBeEnabled =
                    groupsToBeEnabled.Any(groupToBeEnabled => AreEqual(groupToBeEnabled, group));

                return isGroupToBeDisabled || !isQuestionToBeEnabled && IsQuestionDisabled(state, group);
            };

            Func<InterviewStateDependentOnAnswers, Identity, object> getAnswer = (currentState, question) =>
                GetEnabledQuestionAnswerSupportedInExpressions(state, question, isQuestionDisabled, isGroupDisabled, questionnaire, questionBeingAnswered: answeredQuestion, answerBeingApplied: answer);

            var processedQuestionKeys = new HashSet<string>
            {
                ConversionHelper.ConvertIdAndRosterVectorToString(answeredQuestion.Id, answeredQuestion.RosterVector)
            };
            var processsedGroupKeys = new HashSet<string> { };

            var affectingQuestions = new Queue<Identity>(new[] { answeredQuestion });

            while (affectingQuestions.Count > 0)
            {
                Identity affectingQuestion = affectingQuestions.Dequeue();

                IEnumerable<Guid> dependentGroupIds =
                    questionnaire.GetGroupsWhichCustomEnablementConditionDependsOnSpecifiedQuestion(affectingQuestion.Id);
                IEnumerable<Identity> dependentGroups = GetInstancesOfGroupsWithSameAndDeeperRosterLevelOrThrow(state,
                    dependentGroupIds, affectingQuestion.RosterVector, questionnaire, getRosterInstanceIds);

                foreach (Identity dependentGroup in dependentGroups)
                {
                    if (processsedGroupKeys.Contains(ConversionHelper.ConvertIdentityToString(dependentGroup))) continue;

                    bool isNewStateEnabled = this.ShouldGroupBeEnabledByCustomEnablementCondition(state, dependentGroup, questionnaire, getAnswer);
                    bool isOldStateEnabled = !IsGroupDisabled(state, dependentGroup);
                    PutToCorrespondingListAccordingToEnablementStateChange(dependentGroup, groupsToBeEnabled, groupsToBeDisabled,
                        isNewStateEnabled: isNewStateEnabled,
                        isOldStateEnabled: isOldStateEnabled);

                    processsedGroupKeys.Add(ConversionHelper.ConvertIdentityToString(dependentGroup));

                    if (isNewStateEnabled != isOldStateEnabled)
                    {
                        var underlyingQuestionIds =
                            questionnaire.GetAllUnderlyingQuestions(dependentGroup.Id);

                        IEnumerable<Identity> underlyingQuestions = GetInstancesOfQuestionsWithSameAndDeeperRosterLevelOrThrow(
                            state, underlyingQuestionIds, dependentGroup.RosterVector, questionnaire, getRosterInstanceIds);

                        foreach (var underlyingQuestion in underlyingQuestions)
                        {
                            affectingQuestions.Enqueue(underlyingQuestion);
                        }
                    }
                }

                IEnumerable<Guid> dependentQuestionIds = questionnaire.GetQuestionsWhichCustomEnablementConditionDependsOnSpecifiedQuestion(
                   affectingQuestion.Id);
                IEnumerable<Identity> dependentQuestions = GetInstancesOfQuestionsWithSameAndDeeperRosterLevelOrThrow(state,
                    dependentQuestionIds, affectingQuestion.RosterVector, questionnaire, getRosterInstanceIds);

                foreach (Identity dependentQuestion in dependentQuestions)
                {
                    if (processedQuestionKeys.Contains(ConversionHelper.ConvertIdentityToString(dependentQuestion))) continue;

                    bool isNewStateEnabled = this.ShouldQuestionBeEnabledByCustomEnablementCondition(state, dependentQuestion, questionnaire, getAnswer);
                    bool isOldStateEnabled = !IsQuestionDisabled(state, dependentQuestion);
                    PutToCorrespondingListAccordingToEnablementStateChange(dependentQuestion,
                        collectedQuestionsToBeEnabled, collectedQuestionsToBeDisabled,
                        isNewStateEnabled: isNewStateEnabled,
                        isOldStateEnabled: isOldStateEnabled);

                    processedQuestionKeys.Add(ConversionHelper.ConvertIdAndRosterVectorToString(dependentQuestion.Id, dependentQuestion.RosterVector));

                    if (isNewStateEnabled != isOldStateEnabled)
                    {
                        affectingQuestions.Enqueue(dependentQuestion);
                    }
                }
            }

            questionsToBeDisabled = collectedQuestionsToBeDisabled;
            questionsToBeEnabled = collectedQuestionsToBeEnabled;

            return new EnablementChanges(groupsToBeDisabled, groupsToBeEnabled, questionsToBeDisabled, questionsToBeEnabled);
        }

        private List<RosterCalculationData> CalculateFixedRostersData(InterviewStateDependentOnAnswers state, IQuestionnaire questionnaire,
            decimal[] outerRosterVector = null)
        {
            if (outerRosterVector == null)
                outerRosterVector = EmptyRosterVector;

            Func<InterviewStateDependentOnAnswers, Identity, object> getAnswer = (currentState, question) => null;

            List<Guid> fixedRosterIds = questionnaire.GetFixedRosterGroups().ToList();

            Dictionary<Guid, Dictionary<decimal, string>> rosterTitlesGroupedByRosterId = CalculateFixedRosterData(fixedRosterIds,
                questionnaire);

            Func<Guid, decimal[], bool> isFixedRoster =
                (groupId, groupOuterScopeRosterVector) =>
                    fixedRosterIds.Contains(groupId) && AreEqualRosterVectors(groupOuterScopeRosterVector, outerRosterVector);

            Func<Guid, DistinctDecimalList> getFixedRosterInstanceIds =
                fixedRosterId => new DistinctDecimalList(rosterTitlesGroupedByRosterId[fixedRosterId].Keys.ToList());

            Func<InterviewStateDependentOnAnswers, Guid, decimal[], DistinctDecimalList> getRosterInstanceIds =
                (currentState, groupId, groupOuterRosterVector)
                    => isFixedRoster(groupId, groupOuterRosterVector)
                        ? getFixedRosterInstanceIds(groupId)
                        : GetRosterInstanceIds(state, groupId, groupOuterRosterVector);

            return fixedRosterIds
                .Select(fixedRosterId => CalculateRosterData(state, questionnaire,
                    new List<Guid> { fixedRosterId },
                    outerRosterVector,
                    getFixedRosterInstanceIds(fixedRosterId),
                    rosterTitlesGroupedByRosterId[fixedRosterId],
                    questionnaire, getAnswer, getRosterInstanceIds)
                ).ToList();
        }

        private IEnumerable<RosterCalculationData> CalculateDynamicRostersData(InterviewStateDependentOnAnswers state,
            IQuestionnaire questionnaire, decimal[] outerRosterVector,
            Guid rosterId, Func<InterviewStateDependentOnAnswers, Identity, object> getAnswer)
        {
            var nestedRosterIds = questionnaire.GetNestedRostersOfGroupById(rosterId);

            Func<Guid, decimal[], bool> isRoster = (groupId, groupOuterScopeRosterVector)
                => nestedRosterIds.Contains(groupId)
                    && AreEqualRosterVectors(groupOuterScopeRosterVector, outerRosterVector);

            foreach (var nestedRosterId in nestedRosterIds)
            {
                var rosterInstanceIds = this.GetRosterInstancesById(state, questionnaire, nestedRosterId, outerRosterVector, getAnswer);

                Func<InterviewStateDependentOnAnswers, Guid, decimal[], DistinctDecimalList> getRosterInstanceIds =
                    (currentState, groupId, groupOuterRosterVector)
                        => isRoster(groupId, groupOuterRosterVector)
                            ? new DistinctDecimalList(rosterInstanceIds.Keys)
                            : GetRosterInstanceIds(state, groupId, groupOuterRosterVector);

                yield return
                    CalculateRosterData(state, questionnaire,
                        new List<Guid> { nestedRosterId }, outerRosterVector, rosterInstanceIds.ToDictionary(x => x.Key, x => x.Value.Item2),
                        rosterInstanceIds.Any(x => !string.IsNullOrEmpty(x.Value.Item1))
                            ? rosterInstanceIds.ToDictionary(x => x.Key, x => x.Value.Item1)
                            : null,
                        questionnaire,
                        getAnswer,
                        getRosterInstanceIds);
            }
        }

        private RosterCalculationData CalculateRosterDataWithRosterTitlesFromTextListQuestions(InterviewStateDependentOnAnswers state,
            IQuestionnaire questionnare, decimal[] rosterVector, List<Guid> rosterIds,
            Dictionary<decimal, int?> rosterInstanceIdsWithSortIndexes, IQuestionnaire questionnaire,
            Func<InterviewStateDependentOnAnswers, Identity, object> getAnswer,
            Func<InterviewStateDependentOnAnswers, Guid, decimal[], DistinctDecimalList> getRosterInstanceIds,
            Tuple<decimal, string>[] answers, Tuple<decimal, string>[] changedAnswers)
        {
            RosterCalculationData rosterCalculationData = CalculateRosterData(state, questionnare,
                rosterIds, rosterVector, rosterInstanceIdsWithSortIndexes, null, questionnaire, getAnswer, getRosterInstanceIds);

            rosterCalculationData.TitlesForRosterInstancesToAdd = rosterCalculationData.RosterInstancesToAdd
                .Select(rosterInstance => rosterInstance.RosterInstanceId)
                .Distinct()
                .ToDictionary(
                    rosterInstanceId => rosterInstanceId,
                    rosterInstanceId => answers.Single(x => x.Item1 == rosterInstanceId).Item2);

            foreach (var changedAnswer in changedAnswers)
            {
                rosterCalculationData.TitlesForRosterInstancesToAdd.Add(changedAnswer.Item1, changedAnswer.Item2);
                foreach (var rosterId in rosterIds)
                {
                    rosterCalculationData.RosterInstancesToChange.Add(new RosterIdentity(rosterId, rosterVector, changedAnswer.Item1, null));
                }
            }

            return rosterCalculationData;
        }


        private RosterCalculationData CalculateRosterDataWithRosterTitlesFromMultipleOptionsQuestions(
            InterviewStateDependentOnAnswers state, IQuestionnaire questionnare,
            Guid questionId, decimal[] rosterVector, List<Guid> rosterIds,
            Dictionary<decimal, int?> rosterInstanceIdsWithSortIndexes, IQuestionnaire questionnaire,
            Func<InterviewStateDependentOnAnswers, Identity, object> getAnswer,
            Func<InterviewStateDependentOnAnswers, Guid, decimal[], DistinctDecimalList> getRosterInstanceIds)
        {
            RosterCalculationData rosterCalculationData = CalculateRosterData(state, questionnaire,
                rosterIds, rosterVector, rosterInstanceIdsWithSortIndexes, null, questionnaire, getAnswer, getRosterInstanceIds);

            rosterCalculationData.TitlesForRosterInstancesToAdd =
                rosterCalculationData.RosterInstancesToAdd
                    .Select(rosterInstance => rosterInstance.RosterInstanceId)
                    .Distinct()
                    .ToDictionary(
                        rosterInstanceId => rosterInstanceId,
                        rosterInstanceId => questionnaire.GetAnswerOptionTitle(questionId, rosterInstanceId));

            return rosterCalculationData;
        }

        private static Dictionary<Guid, Dictionary<decimal, string>> CalculateFixedRosterData(IEnumerable<Guid> fixedRosterIds,
            IQuestionnaire questionnaire)
        {
            Dictionary<Guid, Dictionary<decimal, string>> rosterTitlesGroupedByRosterId = fixedRosterIds
                .Select(fixedRosterId =>
                    new
                    {
                        FixedRosterId = fixedRosterId,
                        TitlesWithIds = questionnaire.GetFixedRosterTitles(fixedRosterId)
                            .Select((title, index) => new
                            {
                                Title = title,
                                RosterInstanceId = (decimal)index
                            })
                            .ToDictionary(x => x.RosterInstanceId, x => x.Title)
                    }).ToDictionary(x => x.FixedRosterId, x => x.TitlesWithIds);
            return rosterTitlesGroupedByRosterId;
        }

        private RosterCalculationData CalculateRosterData(InterviewStateDependentOnAnswers state, IQuestionnaire questionnare,
            List<Guid> rosterIds, decimal[] nearestToOuterRosterVector, DistinctDecimalList rosterInstanceIds,
            Dictionary<decimal, string> rosterTitles, IQuestionnaire questionnaire,
            Func<InterviewStateDependentOnAnswers, Identity, object> getAnswer,
            Func<InterviewStateDependentOnAnswers, Guid, decimal[], DistinctDecimalList> getRosterInstanceIds)
        {
            Dictionary<decimal, int?> rosterInstanceIdsWithSortIndexes =
                rosterInstanceIds.ToDictionary(
                    rosterInstanceId => rosterInstanceId,
                    rosterInstanceId => (int?)null);

            return CalculateRosterData(state, questionnaire,
                rosterIds, nearestToOuterRosterVector, rosterInstanceIdsWithSortIndexes, rosterTitles, questionnaire, getAnswer,
                getRosterInstanceIds);
        }

        private RosterCalculationData CalculateRosterData(InterviewStateDependentOnAnswers state, IQuestionnaire questionnare,
            List<Guid> rosterIds, decimal[] nearestToOuterRosterVector, Dictionary<decimal, int?> rosterInstanceIdsWithSortIndexes,
            Dictionary<decimal, string> rosterTitles,
            IQuestionnaire questionnaire,
            Func<InterviewStateDependentOnAnswers, Identity, object> getAnswer,
            Func<InterviewStateDependentOnAnswers, Guid, decimal[], DistinctDecimalList> getRosterInstanceIds)
        {
            List<RosterIdentity> rosterInstancesToAdd, rosterInstancesToRemove, rosterInstancesToChange = new List<RosterIdentity>();
            List<Identity> initializedGroupsToBeDisabled,
                initializedGroupsToBeEnabled,
                initializedQuestionsToBeDisabled,
                initializedQuestionsToBeEnabled,
                initializedQuestionsToBeInvalid;
            List<RosterCalculationData> rosterInstantiatesFromNestedLevels;
            this.CalculateChangesInRosterInstances(state, questionnare, rosterIds, nearestToOuterRosterVector,
                rosterInstanceIdsWithSortIndexes,
                getAnswer,
                out rosterInstancesToAdd, out rosterInstancesToRemove, out rosterInstantiatesFromNestedLevels);

            List<decimal> rosterInstanceIdsBeingAdded = rosterInstancesToAdd.Select(instance => instance.RosterInstanceId).ToList();
            List<decimal> rosterInstanceIdsBeingRemoved = rosterInstancesToRemove.Select(instance => instance.RosterInstanceId).ToList();

            List<Identity> answersToRemoveByDecreasedRosterSize = this.GetAnswersToRemoveIfRosterInstancesAreRemoved(state,
                rosterIds, rosterInstanceIdsBeingRemoved, nearestToOuterRosterVector, questionnaire);

            this.DetermineCustomEnablementStateOfGroupsInitializedByAddedRosterInstances(state,
                rosterIds, rosterInstanceIdsBeingAdded, nearestToOuterRosterVector, questionnaire, getAnswer, getRosterInstanceIds,
                out initializedGroupsToBeDisabled, out initializedGroupsToBeEnabled);
            this.DetermineCustomEnablementStateOfQuestionsInitializedByAddedRosterInstances(state,
                rosterIds, rosterInstanceIdsBeingAdded, nearestToOuterRosterVector, questionnaire, getAnswer, getRosterInstanceIds,
                out initializedQuestionsToBeDisabled, out initializedQuestionsToBeEnabled);
            DetermineValidityStateOfQuestionsInitializedByAddedRosterInstances(state,
                rosterIds, rosterInstanceIdsBeingAdded, nearestToOuterRosterVector, questionnaire, initializedGroupsToBeDisabled,
                initializedQuestionsToBeDisabled, getRosterInstanceIds, out initializedQuestionsToBeInvalid);


            return new RosterCalculationData(rosterInstancesToAdd, rosterInstancesToRemove, rosterInstancesToChange,
                answersToRemoveByDecreasedRosterSize, initializedGroupsToBeDisabled, initializedGroupsToBeEnabled,
                initializedQuestionsToBeDisabled, initializedQuestionsToBeEnabled, initializedQuestionsToBeInvalid,
                rosterTitles, rosterInstantiatesFromNestedLevels);
        }

        private static List<RosterCalculationData> CalculateRostersFromInterviewSynchronizationDto(InterviewSynchronizationDto interviewDto)
        {
            return interviewDto
                .RosterGroupInstances
                .Select(rosterPairDto => CalculateRosterDataFromSingleRosterInstancesSynchronizationDto(rosterPairDto.Value))
                .ToList();
        }

        private static RosterCalculationData CalculateRosterDataFromSingleRosterInstancesSynchronizationDto(
            RosterSynchronizationDto[] rosterInstancesDto)
        {
            List<RosterIdentity> rosterInstancesToAdd = rosterInstancesDto
                .Select(
                    instanceDto =>
                        new RosterIdentity(instanceDto.RosterId, instanceDto.OuterScopePropagationVector, instanceDto.RosterInstanceId,
                            instanceDto.SortIndex))
                .ToList();

            Dictionary<decimal, string> titlesForRosterInstancesToAdd = rosterInstancesDto.ToDictionary(
                dtoInstance => dtoInstance.RosterInstanceId,
                dtoInstance => dtoInstance.RosterTitle);

            return new RosterCalculationData(rosterInstancesToAdd, titlesForRosterInstancesToAdd);
        }

        private void CalculateChangesInRosterInstances(InterviewStateDependentOnAnswers state, IQuestionnaire questionnaire,
            IEnumerable<Guid> rosterIds, decimal[] nearestToOuterRosterVector,
            Dictionary<decimal, int?> rosterInstanceIdsWithSortIndexes, Func<InterviewStateDependentOnAnswers, Identity, object> getAnswer,
            out List<RosterIdentity> rosterInstancesToAdd, out List<RosterIdentity> rosterInstancesToRemove,
            out List<RosterCalculationData> rosterInstantiatesFromNestedLevels)
        {
            rosterInstancesToAdd = new List<RosterIdentity>();
            rosterInstancesToRemove = new List<RosterIdentity>();
            rosterInstantiatesFromNestedLevels = new List<RosterCalculationData>();

            DistinctDecimalList rosterInstanceIds = new DistinctDecimalList(rosterInstanceIdsWithSortIndexes.Keys.ToList());
            foreach (var rosterId in rosterIds)
            {
                Guid[] rosterGroupsStartingFromTop = questionnaire.GetRostersFromTopToSpecifiedGroup(rosterId).ToArray();

                var outerVectorsForExtend =
                    GetOuterVectorForParentRoster(state, rosterGroupsStartingFromTop,
                        nearestToOuterRosterVector);

                foreach (var outerVectorForExtend in outerVectorsForExtend)
                {
                    var rosterInstanceIdsBeingAdded = GetRosterInstanceIdsBeingAdded(
                        existingRosterInstanceIds: GetRosterInstanceIds(state, rosterId, outerVectorForExtend),
                        newRosterInstanceIds: rosterInstanceIds).Select(rosterInstanceId =>
                            new RosterIdentity(rosterId, outerVectorForExtend, rosterInstanceId,
                                sortIndex: rosterInstanceIdsWithSortIndexes[rosterInstanceId])).ToList();

                    rosterInstancesToAdd.AddRange(
                        rosterInstanceIdsBeingAdded);

                    var listOfRosterInstanceIdsForRemove =
                        GetRosterInstanceIdsBeingRemoved(
                            GetRosterInstanceIds(state, rosterId, outerVectorForExtend), rosterInstanceIds).Select(rosterInstanceId =>
                                new RosterIdentity(rosterId, outerVectorForExtend, rosterInstanceId)).ToList();

                    rosterInstancesToRemove.AddRange(
                        listOfRosterInstanceIdsForRemove);

                    foreach (var rosterInstanceIdBeingAdded in rosterInstanceIdsBeingAdded)
                    {
                        var outerRosterVector = ExtendRosterVectorWithOneValue(rosterInstanceIdBeingAdded.OuterRosterVector,
                            rosterInstanceIdBeingAdded.RosterInstanceId);
                        rosterInstantiatesFromNestedLevels.AddRange(
                            this.CalculateDynamicRostersData(state, questionnaire, outerRosterVector, rosterId, getAnswer).ToList());
                    }

                    rosterInstantiatesFromNestedLevels.Add(CalculateNestedRostersDataForDelete(state, questionnaire, rosterId,
                        listOfRosterInstanceIdsForRemove.Select(i => i.RosterInstanceId).ToList(), outerVectorForExtend));
                }
            }
        }

        private RosterCalculationData CalculateNestedRostersDataForDelete(InterviewStateDependentOnAnswers state,
            IQuestionnaire questionnaire, Guid rosterId, List<decimal> rosterInstanceIdsBeingRemoved, decimal[] nearestToOuterRosterVector)
        {
            var nestedRosterIds = questionnaire.GetNestedRostersOfGroupById(rosterId);

            int indexOfRosterInRosterVector = GetIndexOfRosterInRosterVector(rosterId, questionnaire);

            List<Identity> answersToRemoveByDecreasedRosterSize = this.GetAnswersToRemoveIfRosterInstancesAreRemoved(state,
                nestedRosterIds, rosterInstanceIdsBeingRemoved, nearestToOuterRosterVector,
                questionnaire);

            var listOfRosterInstanceIdsForRemove = new List<RosterIdentity>();

            var rosterInstantiatesFromNestedLevels = new List<RosterCalculationData>();
            foreach (var nestedRosterId in nestedRosterIds)
            {
                Guid[] rosterGroupsStartingFromTop = questionnaire.GetRostersFromTopToSpecifiedGroup(nestedRosterId).ToArray();
                var outerVectorsForExtend = ExtendRosterVector(state, nearestToOuterRosterVector, rosterGroupsStartingFromTop.Length, rosterGroupsStartingFromTop, GetRosterInstanceIds);

                foreach (var outerVectorForExtend in outerVectorsForExtend)
                {
                    if (!rosterInstanceIdsBeingRemoved.Contains(outerVectorForExtend[indexOfRosterInRosterVector]))
                        continue;

                    var rosterIdForDelete = new RosterIdentity(nestedRosterId,
                        outerVectorForExtend.Take(outerVectorForExtend.Length - 1).ToArray(), outerVectorForExtend.Last(), null);

                    listOfRosterInstanceIdsForRemove.Add(rosterIdForDelete);

                    rosterInstantiatesFromNestedLevels.Add(CalculateNestedRostersDataForDelete(state, questionnaire, nestedRosterId,
                        new List<decimal> { rosterIdForDelete.RosterInstanceId }, outerVectorForExtend));
                }
            }

            return new RosterCalculationData(new List<RosterIdentity>(), listOfRosterInstanceIdsForRemove, new List<RosterIdentity>(),
                answersToRemoveByDecreasedRosterSize, new List<Identity>(), new List<Identity>(), new List<Identity>(), new List<Identity>(),
                new List<Identity>(), new Dictionary<decimal, string>(),
                rosterInstantiatesFromNestedLevels);
        }

        private static List<RosterIdentity> CalculateRosterInstancesWhichTitlesAreAffected(Guid questionId, decimal[] rosterVector,
            IQuestionnaire questionnaire)
        {
            if (!questionnaire.DoesQuestionSpecifyRosterTitle(questionId))
                return new List<RosterIdentity>();

            Tuple<decimal[], decimal> splittedRosterVector = SplitRosterVectorOntoOuterVectorAndRosterInstanceId(rosterVector);

            return questionnaire
                .GetRostersAffectedByRosterTitleQuestion(questionId)
                .Select(rosterId => new RosterIdentity(rosterId, splittedRosterVector.Item1, splittedRosterVector.Item2))
                .ToList();
        }


        #endregion

        #region ThrowIfs

        private void ThrowIfInterviewWasCompleted()
        {
            if (this.wasCompleted)
                throw new InterviewException(string.Format("Interview was completed by interviewer and cannot be deleted"));
        }

        private static void ThrowIfQuestionDoesNotExist(Guid questionId, IQuestionnaire questionnaire)
        {
            if (!questionnaire.HasQuestion(questionId))
                throw new InterviewException(string.Format("Question with id '{0}' is not found.", questionId));
        }

        private void ThrowIfRosterVectorIsIncorrect(InterviewStateDependentOnAnswers state, Guid questionId, decimal[] rosterVector, IQuestionnaire questionnaire)
        {
            ThrowIfRosterVectorIsNull(questionId, rosterVector, questionnaire);

            Guid[] parentRosterGroupIdsStartingFromTop = questionnaire.GetRostersFromTopToSpecifiedQuestion(questionId).ToArray();

            ThrowIfRosterVectorLengthDoesNotCorrespondToParentRosterGroupsCount(questionId, rosterVector,
                parentRosterGroupIdsStartingFromTop, questionnaire);

            this.ThrowIfSomeOfRosterVectorValuesAreInvalid(state, questionId, rosterVector, parentRosterGroupIdsStartingFromTop,
                questionnaire);
        }

        private static void ThrowIfAnswersExceedsMaxAnswerCountLimit(Tuple<decimal, string>[] answers, int? maxAnswersCountLimit,
            Guid questionId, IQuestionnaire questionnaire)
        {
            if (maxAnswersCountLimit.HasValue && answers.Length > maxAnswersCountLimit.Value)
            {
                throw new InterviewException(string.Format("Answers exceeds MaxAnswerCount limit for question {0}",
                    FormatQuestionForException(questionId, questionnaire)));
            }
        }

        private static void ThrowIfStringValueAreEmptyOrWhitespaces(Tuple<decimal, string>[] answers, Guid questionId, IQuestionnaire questionnaire)
        {
            if (answers.Any(x => string.IsNullOrWhiteSpace(x.Item2)))
            {
                throw new InterviewException(string.Format("String values should be not empty or whitespaces for question {0}",
                    FormatQuestionForException(questionId, questionnaire)));
            }
        }

        private static void ThrowIfDecimalValuesAreNotUnique(Tuple<decimal, string>[] answers, Guid questionId, IQuestionnaire questionnaire)
        {
            var decimals = answers.Select(x => x.Item1).Distinct().ToArray();
            if (answers.Length > decimals.Length)
            {
                throw new InterviewException(string.Format("Decimal values should be unique for question {0}",
                    FormatQuestionForException(questionId, questionnaire)));
            }
        }

        private static void ThrowIfRosterVectorIsNull(Guid questionId, decimal[] rosterVector, IQuestionnaire questionnaire)
        {
            if (rosterVector == null)
                throw new InterviewException(string.Format(
                    "Roster information for question {0} is missing. Roster vector cannot be null.",
                    FormatQuestionForException(questionId, questionnaire)));
        }

        private static void ThrowIfRosterVectorLengthDoesNotCorrespondToParentRosterGroupsCount(
            Guid questionId, decimal[] rosterVector, Guid[] parentRosterGroups, IQuestionnaire questionnaire)
        {
            if (rosterVector.Length != parentRosterGroups.Length)
                throw new InterviewException(string.Format(
                    "Roster information for question {0} is incorrect. " +
                        "Roster vector has {1} elements, but parent roster groups count is {2}.",
                    FormatQuestionForException(questionId, questionnaire), rosterVector.Length, parentRosterGroups.Length));
        }

        private void ThrowIfSomeOfRosterVectorValuesAreInvalid(InterviewStateDependentOnAnswers state,
            Guid questionId, decimal[] rosterVector, Guid[] parentRosterGroupIdsStartingFromTop, IQuestionnaire questionnaire)
        {
            for (int indexOfRosterVectorElement = 0; indexOfRosterVectorElement < rosterVector.Length; indexOfRosterVectorElement++)
            {
                decimal rosterInstanceId = rosterVector[indexOfRosterVectorElement];
                Guid rosterGroupId = parentRosterGroupIdsStartingFromTop[indexOfRosterVectorElement];

                int rosterGroupOuterScopeRosterLevel = indexOfRosterVectorElement;
                decimal[] rosterGroupOuterScopeRosterVector = ShrinkRosterVector(rosterVector, rosterGroupOuterScopeRosterLevel);
                DistinctDecimalList rosterInstanceIds = GetRosterInstanceIds(state,
                    groupId: rosterGroupId,
                    outerRosterVector: rosterGroupOuterScopeRosterVector);

                if (!rosterInstanceIds.Contains(rosterInstanceId))
                    throw new InterviewException(string.Format(
                        "Roster information for question {0} is incorrect. " +
                            "Roster vector element with index [{1}] refers to instance of roster group {2} by instance id [{3}] " +
                            "but roster group has only following roster instances: {4}.",
                        FormatQuestionForException(questionId, questionnaire), indexOfRosterVectorElement,
                        FormatGroupForException(rosterGroupId, questionnaire), rosterInstanceId,
                        string.Join(", ", rosterInstanceIds)));
            }
        }

        private void ThrowIfQuestionTypeIsNotOneOfExpected(Guid questionId, IQuestionnaire questionnaire,
            params QuestionType[] expectedQuestionTypes)
        {
            QuestionType questionType = questionnaire.GetQuestionType(questionId);

            bool typeIsNotExpected = !expectedQuestionTypes.Contains(questionType);
            if (typeIsNotExpected)
                throw new InterviewException(string.Format(
                    "Question {0} has type {1}. But one of the following types was expected: {2}.",
                    FormatQuestionForException(questionId, questionnaire), questionType,
                    string.Join(", ", expectedQuestionTypes.Select(type => type.ToString()))));
        }

        private void ThrowIfNumericQuestionIsNotReal(Guid questionId, IQuestionnaire questionnaire)
        {
            var isNotSupportReal = questionnaire.IsQuestionInteger(questionId);
            if (isNotSupportReal)
                throw new InterviewException(string.Format(
                    "Question {0} doesn't support answer of type real.",
                    FormatQuestionForException(questionId, questionnaire)));
        }

        private void ThrowIfNumericQuestionIsNotInteger(Guid questionId, IQuestionnaire questionnaire)
        {
            var isNotSupportInteger = !questionnaire.IsQuestionInteger(questionId);
            if (isNotSupportInteger)
                throw new InterviewException(string.Format(
                    "Question {0} doesn't support answer of type integer.",
                    FormatQuestionForException(questionId, questionnaire)));
        }

        private void ThrowIfLinkedQuestionDoesNotHaveAnswer(InterviewStateDependentOnAnswers state, Identity answeredQuestion,
            Identity answeredLinkedQuestion, IQuestionnaire questionnaire)
        {
            if (!WasQuestionAnswered(state, answeredLinkedQuestion))
            {
                throw new InterviewException(string.Format(
                    "Could not set answer for question {0} because his dependent linked question {1} does not have answer",
                    FormatQuestionForException(answeredQuestion, questionnaire),
                    FormatQuestionForException(answeredLinkedQuestion, questionnaire)));
            }
        }

        private static void ThrowIfValueIsNotOneOfAvailableOptions(Guid questionId, decimal value, IQuestionnaire questionnaire)
        {
            IEnumerable<decimal> availableValues = questionnaire.GetAnswerOptionsAsValues(questionId);

            bool valueIsNotOneOfAvailable = !availableValues.Contains(value);
            if (valueIsNotOneOfAvailable)
                throw new InterviewException(string.Format(
                    "For question {0} was provided selected value {1} as answer. But only following values are allowed: {2}.",
                    FormatQuestionForException(questionId, questionnaire), value, JoinDecimalsWithComma(availableValues)));
        }

        private static void ThrowIfSomeValuesAreNotFromAvailableOptions(Guid questionId, decimal[] values, IQuestionnaire questionnaire)
        {
            IEnumerable<decimal> availableValues = questionnaire.GetAnswerOptionsAsValues(questionId);

            bool someValueIsNotOneOfAvailable = values.Any(value => !availableValues.Contains(value));
            if (someValueIsNotOneOfAvailable)
                throw new InterviewException(string.Format(
                    "For question {0} were provided selected values {1} as answer. But only following values are allowed: {2}.",
                    FormatQuestionForException(questionId, questionnaire), JoinDecimalsWithComma(values),
                    JoinDecimalsWithComma(availableValues)));
        }

        private static void ThrowIfLengthOfSelectedValuesMoreThanMaxForSelectedAnswerOptions(Guid questionId, int answersCount, IQuestionnaire questionnaire)
        {
            int? maxSelectedOptions = questionnaire.GetMaxSelectedAnswerOptions(questionId);

            if (maxSelectedOptions.HasValue && maxSelectedOptions > 0 && answersCount > maxSelectedOptions)
                throw new InterviewException(string.Format(
                    "For question {0} number of answers is greater than the maximum number of selected answers",
                    FormatQuestionForException(questionId, questionnaire)));
        }

        private static void ThrowIfQuestionOrParentGroupIsDisabled(InterviewStateDependentOnAnswers state, Identity question, IQuestionnaire questionnaire)
        {
            if (IsQuestionDisabled(state, question))
                throw new InterviewException(string.Format(
                    "Question {1} is disabled by it's following enablement condition:{0}{2}",
                    Environment.NewLine,
                    FormatQuestionForException(question, questionnaire),
                    questionnaire.GetCustomEnablementConditionForQuestion(question.Id)));

            IEnumerable<Guid> parentGroupIds = questionnaire.GetAllParentGroupsForQuestion(question.Id);
            IEnumerable<Identity> parentGroups = GetInstancesOfGroupsWithSameAndUpperRosterLevelOrThrow(parentGroupIds,
                question.RosterVector, questionnaire);

            foreach (Identity parentGroup in parentGroups)
            {
                if (IsGroupDisabled(state, parentGroup))
                    throw new InterviewException(string.Format(
                        "Question {1} is disabled because parent group {2} is disabled by it's following enablement condition:{0}{3}",
                        Environment.NewLine,
                        FormatQuestionForException(question, questionnaire),
                        FormatGroupForException(parentGroup, questionnaire),
                        questionnaire.GetCustomEnablementConditionForGroup(parentGroup.Id)));
            }
        }

        private void ThrowIfAnswerHasMoreDecimalPlacesThenAccepted(IQuestionnaire questionnaire, Guid questionId, decimal answer)
        {
            int? countOfDecimalPlacesAllowed = questionnaire.GetCountOfDecimalPlacesAllowedByQuestion(questionId);
            if (!countOfDecimalPlacesAllowed.HasValue)
                return;

            var roundedAnswer = Math.Round(answer, countOfDecimalPlacesAllowed.Value);
            if (roundedAnswer != answer)
                throw new InterviewException(
                    string.Format(
                        "Answer '{0}' for question {1}  is incorrect because has more decimal places then allowed by questionnaire", answer,
                        FormatQuestionForException(questionId, questionnaire)));
        }

        private static void ThrowIfNumericAnswerExceedsMaxValue(Guid questionId, decimal answer, IQuestionnaire questionnaire)
        {
            int? maxValue = questionnaire.GetMaxValueForNumericQuestion(questionId);

            if (!maxValue.HasValue)
                return;

            bool answerExceedsMaxValue = answer > maxValue.Value;

            if (answerExceedsMaxValue)
                throw new InterviewException(string.Format(
                    "Answer '{0}' for question {1} is incorrect because answer is greater than max value '{2}'.",
                    answer, FormatQuestionForException(questionId, questionnaire), maxValue.Value));
        }

        private static void ThrowIfRosterSizeAnswerIsNegative(Guid questionId, int answer, IQuestionnaire questionnaire)
        {
            bool answerIsNegative = answer < 0;

            if (answerIsNegative)
                throw new InterviewException(string.Format(
                    "Answer '{0}' for question {1} is incorrect because question is used as size of roster group and specified answer is negative.",
                    answer, FormatQuestionForException(questionId, questionnaire)));
        }

        private void ThrowIfInterviewStatusIsNotOneOfExpected(params InterviewStatus[] expectedStatuses)
        {
            if (!expectedStatuses.Contains(this.status))
                throw new InterviewException(string.Format(
                    "Interview status is {0}. But one of the following statuses was expected: {1}.",
                    this.status, string.Join(", ", expectedStatuses.Select(expectedStatus => expectedStatus.ToString()))));
        }

        private void ThrowIfStatusNotAllowedToBeChangedWithMetadata(InterviewStatus interviewStatus)
        {
            switch (interviewStatus)
            {
                case InterviewStatus.Completed:
                    this.ThrowIfInterviewStatusIsNotOneOfExpected(InterviewStatus.InterviewerAssigned,
                        InterviewStatus.Restored,
                        InterviewStatus.RejectedBySupervisor,
                        InterviewStatus.Restarted);
                    return;
                case InterviewStatus.RejectedBySupervisor:
                    this.ThrowIfInterviewStatusIsNotOneOfExpected(
                        InterviewStatus.InterviewerAssigned,
                        InterviewStatus.Restored,
                        InterviewStatus.RejectedBySupervisor,
                        InterviewStatus.Restarted,
                        InterviewStatus.Completed,
                        InterviewStatus.ApprovedBySupervisor);
                    return;
                case InterviewStatus.InterviewerAssigned:
                    this.ThrowIfInterviewStatusIsNotOneOfExpected(
                        InterviewStatus.InterviewerAssigned,
                        InterviewStatus.Restored,
                        InterviewStatus.RejectedBySupervisor,
                        InterviewStatus.SupervisorAssigned,
                        InterviewStatus.Restarted);
                    return;
                case InterviewStatus.ApprovedBySupervisor:
                    this.ThrowIfInterviewStatusIsNotOneOfExpected(
                        InterviewStatus.InterviewerAssigned,
                        InterviewStatus.RejectedByHeadquarters,
                        InterviewStatus.SupervisorAssigned);
                    return;
            }
            throw new InterviewException(string.Format(
                "Status {0} not allowed to be changed with ApplySynchronizationMetadata command",
                interviewStatus));
        }

        #endregion

        private bool ShouldQuestionBeDisabledByCustomCondition(InterviewStateDependentOnAnswers state, Identity questionId, IQuestionnaire questionnaire)
        {
            var questionsInvolvedInConditions = questionnaire.GetQuestionsInvolvedInCustomEnablementConditionOfQuestion(questionId.Id);
            if (!questionsInvolvedInConditions.Any() || questionsInvolvedInConditions.Any(q => q == questionId.Id))
                return IsQuestionDisabled(state, questionId);

            return !this.ShouldQuestionBeEnabledByCustomEnablementCondition(state, questionId, questionnaire,
                (currentState, questionInCondition) =>
                    GetEnabledQuestionAnswerSupportedInExpressions(state,
                        questionInCondition,
                        (currState, q) => this.ShouldQuestionBeDisabledByCustomCondition(state, q, questionnaire),
                        (currState, g) => this.ShouldGroupBeDisabledByCustomCondition(state, g, questionnaire),
                        questionnaire));
        }

        private bool ShouldGroupBeDisabledByCustomCondition(InterviewStateDependentOnAnswers state, Identity groupId, IQuestionnaire questionnaire)
        {
            return !ShouldGroupBeEnabledByCustomEnablementCondition(state, groupId, questionnaire,
                (currentState, questionInCondition) =>
                    GetEnabledQuestionAnswerSupportedInExpressions(state,
                        questionInCondition,
                        (currState, q) => this.ShouldQuestionBeDisabledByCustomCondition(state, q, questionnaire),
                        (currState, g) => this.ShouldGroupBeDisabledByCustomCondition(state, g, questionnaire),
                        questionnaire));
        }

        private IQuestionnaire GetHistoricalQuestionnaireOrThrow(Guid id, long version)
        {
            IQuestionnaire questionnaire = this.QuestionnaireRepository.GetHistoricalQuestionnaire(id, version);

            if (questionnaire == null)
                throw new InterviewException(string.Format("Questionnaire with id '{0}' of version {1} is not found.", id, version));

            return questionnaire;
        }

        private IQuestionnaire GetQuestionnaireOrThrow(Guid id)
        {
            IQuestionnaire questionnaire = this.QuestionnaireRepository.GetQuestionnaire(id);

            if (questionnaire == null)
                throw new InterviewException(string.Format("Questionnaire with id '{0}' is not found.", id));

            return questionnaire;
        }

        private Guid GetLinkedQuestionIdOrThrow(Guid questionId, IQuestionnaire questionnaire)
        {
            return questionnaire.GetQuestionReferencedByLinkedQuestion(questionId);
        }

        private Dictionary<decimal, Tuple<string, int?>> GetRosterInstancesById(InterviewStateDependentOnAnswers state,
            IQuestionnaire questionnaire, Guid rosterId,
            decimal[] outerRosterVector, Func<InterviewStateDependentOnAnswers, Identity, object> getAnswer)
        {
            Guid? rosterSizeQuestionId = questionnaire.GetRosterSizeQuestion(rosterId);

            if (!rosterSizeQuestionId.HasValue)
                return questionnaire.GetFixedRosterTitles(rosterId)
                    .Select((title, index) => new
                    {
                        Title = title,
                        RosterInstanceId = (decimal)index
                    })
                    .ToDictionary(x => x.RosterInstanceId, x => new Tuple<string, int?>(x.Title, null));

            var rosterSizeQuestionIdentity = new Identity(rosterSizeQuestionId.Value,
                outerRosterVector.Take(questionnaire.GetRosterLevelForQuestion(rosterSizeQuestionId.Value)).ToArray());

            var answerOnRosterSizeQuestion = getAnswer(state, rosterSizeQuestionIdentity);
            var questionType = questionnaire.GetQuestionType(rosterSizeQuestionId.Value);
            switch (questionType)
            {
                case QuestionType.Numeric:
                    if (questionnaire.IsQuestionInteger(rosterSizeQuestionId.Value))
                    {
                        if (answerOnRosterSizeQuestion == null)
                            return new Dictionary<decimal, Tuple<string, int?>>();
                        try
                        {
                            var intAnswer = Convert.ToInt32(answerOnRosterSizeQuestion);
                            return Enumerable.Range(0, intAnswer)
                                .ToDictionary(x => (decimal)x, x => new Tuple<string, int?>(null, x));
                        }
                        catch (InvalidCastException e)
                        {
                            this.Logger.Error("invalid cast of int answer on trigger question", e);
                        }
                    }

                    break;
                case QuestionType.MultyOption:
                    var multyOptionAnswer = answerOnRosterSizeQuestion as decimal[];
                    if (multyOptionAnswer != null)
                    {
                        return multyOptionAnswer.ToDictionary(x => x,
                            x => new Tuple<string, int?>(questionnaire.GetAnswerOptionTitle(rosterSizeQuestionId.Value, x), (int?)x));
                    }
                    break;
                case QuestionType.TextList:

                    var currentAnswer = answerOnRosterSizeQuestion as Tuple<decimal, string>[];
                    if (currentAnswer != null)
                    {
                        return currentAnswer.ToDictionary(x => x.Item1, x => new Tuple<string, int?>(x.Item2, (int?)x.Item1));
                    }

                    var questionKey = ConversionHelper.ConvertIdAndRosterVectorToString(rosterSizeQuestionIdentity.Id, rosterSizeQuestionIdentity.RosterVector);
                    if (state.TextListAnswers.ContainsKey(questionKey))
                    {
                        return state.TextListAnswers[questionKey].ToDictionary(x => x.Item1,
                            x => new Tuple<string, int?>(x.Item2, (int?)x.Item1));
                    }
                    break;
            }
            return new Dictionary<decimal, Tuple<string, int?>>();
        }


        private void ValidatePrefilledQuestions(IQuestionnaire questionnaire, Dictionary<Guid, object> answersToFeaturedQuestions,
            decimal[] rosterVecor = null, InterviewStateDependentOnAnswers currentInterviewState = null, bool applyStrongChecks = true)
        {
            var currentRosterVector = rosterVecor ?? EmptyRosterVector;
            foreach (KeyValuePair<Guid, object> answerToFeaturedQuestion in answersToFeaturedQuestions)
            {
                Guid questionId = answerToFeaturedQuestion.Key;
                object answer = answerToFeaturedQuestion.Value;

                var answeredQuestion = new Identity(questionId, currentRosterVector);

                QuestionType questionType = questionnaire.GetQuestionType(questionId);

                switch (questionType)
                {
                    case QuestionType.Text:
                        this.CheckTextQuestionInvariants(questionId, currentRosterVector, questionnaire, answeredQuestion,
                            currentInterviewState, applyStrongChecks);
                        break;

                    case QuestionType.AutoPropagate:
                        this.CheckNumericIntegerQuestionInvariants(questionId, currentRosterVector, (int)answer, questionnaire,
                            answeredQuestion, currentInterviewState, applyStrongChecks);
                        break;
                    case QuestionType.Numeric:
                        if (questionnaire.IsQuestionInteger(questionId))
                            this.CheckNumericIntegerQuestionInvariants(questionId, currentRosterVector, (int)answer, questionnaire,
                                answeredQuestion, currentInterviewState, applyStrongChecks);
                        else
                            this.CheckNumericRealQuestionInvariants(questionId, currentRosterVector, (decimal)answer, questionnaire,
                                answeredQuestion, currentInterviewState, applyStrongChecks);
                        break;

                    case QuestionType.DateTime:
                        this.CheckDateTimeQuestionInvariants(questionId, currentRosterVector, questionnaire, answeredQuestion,
                            currentInterviewState, applyStrongChecks);
                        break;

                    case QuestionType.SingleOption:
                        this.CheckSingleOptionQuestionInvariants(questionId, currentRosterVector, (decimal)answer, questionnaire,
                            answeredQuestion, currentInterviewState, applyStrongChecks);
                        break;

                    case QuestionType.MultyOption:
                        this.CheckMultipleOptionQuestionInvariants(questionId, currentRosterVector, (decimal[])answer, questionnaire,
                            answeredQuestion, currentInterviewState, applyStrongChecks);
                        break;
                    case QuestionType.QRBarcode:
                        this.CheckQRBarcodeInvariants(questionId, currentRosterVector, questionnaire, answeredQuestion, currentInterviewState, applyStrongChecks);
                        break;
                    case QuestionType.GpsCoordinates:
                        this.CheckGpsCoordinatesInvariants(questionId, currentRosterVector, questionnaire, answeredQuestion, currentInterviewState, applyStrongChecks);
                        break;
                    case QuestionType.TextList:
                        this.CheckTextListInvariants(questionId, currentRosterVector, questionnaire, answeredQuestion, currentInterviewState, (Tuple<decimal, string>[])answer, applyStrongChecks);
                        break;

                    default:
                        throw new InterviewException(string.Format(
                            "Question {0} has type {1} which is not supported as initial pre-filled question.",
                            questionId, questionType));
                }
            }
        }

        private void PerformValidationOfAnsweredQuestionAndDependentQuestionsAndJustEnabledQuestions(
            InterviewStateDependentOnAnswers state,
            Identity answeredQuestion, IQuestionnaire questionnaire, Func<InterviewStateDependentOnAnswers, Identity, object> getAnswer,
            Func<Identity, bool?> getNewQuestionStatus,
            EnablementChanges enablementChanges,
            out List<Identity> questionsToBeDeclaredValid, out List<Identity> questionsToBeDeclaredInvalid)
        {
            questionsToBeDeclaredValid = new List<Identity>();
            questionsToBeDeclaredInvalid = new List<Identity>();

            bool? answeredQuestionValidationResult = this.PerformValidationOfQuestion(state, answeredQuestion, questionnaire, getAnswer,
                getNewQuestionStatus);
            switch (answeredQuestionValidationResult)
            {
                case true:
                    questionsToBeDeclaredValid.Add(answeredQuestion);
                    break;
                case false:
                    questionsToBeDeclaredInvalid.Add(answeredQuestion);
                    break;
            }

            List<Identity> affectedQuestionsDeclaredValid;
            List<Identity> affectedQuestionsDeclaredInvalid;
            this.PerformValidationOfAffectedQuestions(state, answeredQuestion, questionnaire, getAnswer,
                GetRosterInstanceIds, getNewQuestionStatus,
                enablementChanges,
                out affectedQuestionsDeclaredValid, out affectedQuestionsDeclaredInvalid);

            questionsToBeDeclaredValid.AddRange(affectedQuestionsDeclaredValid);
            questionsToBeDeclaredInvalid.AddRange(affectedQuestionsDeclaredInvalid);

            questionsToBeDeclaredValid = RemoveQuestionsAlreadyDeclaredValid(state, questionsToBeDeclaredValid);
            questionsToBeDeclaredInvalid = RemoveQuestionsAlreadyDeclaredInvalid(state, questionsToBeDeclaredInvalid);
        }

        private static List<Identity> RemoveQuestionsAlreadyDeclaredValid(InterviewStateDependentOnAnswers state,
            IEnumerable<Identity> questionsToBeDeclaredValid)
        {
            return questionsToBeDeclaredValid.Where(question => !IsQuestionAnsweredValid(state, question)).ToList();
        }

        private static List<Identity> RemoveQuestionsAlreadyDeclaredInvalid(InterviewStateDependentOnAnswers state,
            IEnumerable<Identity> questionsToBeDeclaredInvalid)
        {
            return questionsToBeDeclaredInvalid.Where(question => !IsQuestionAnsweredInvalid(state, question)).ToList();
        }

        private void PerformValidationOfAffectedQuestions(InterviewStateDependentOnAnswers state,
            Identity answeredQuestion, IQuestionnaire questionnaire,
            Func<InterviewStateDependentOnAnswers, Identity, object> getAnswer,
            Func<InterviewStateDependentOnAnswers, Guid, decimal[], DistinctDecimalList> getRosterInstanceIds,
            Func<Identity, bool?> getNewQuestionStatus,
            EnablementChanges enablementChanges,
            out List<Identity> questionsDeclaredValid, out List<Identity> questionsDeclaredInvalid)
        {
            questionsDeclaredValid = new List<Identity>();
            questionsDeclaredInvalid = new List<Identity>();

            IEnumerable<Guid> dependentQuestionIds = questionnaire.GetQuestionsWhichCustomValidationDependsOnSpecifiedQuestion(answeredQuestion.Id);
            IEnumerable<Identity> dependentQuestions = GetInstancesOfQuestionsWithSameAndDeeperRosterLevelOrThrow(state,
                dependentQuestionIds, answeredQuestion.RosterVector, questionnaire, getRosterInstanceIds);

            IEnumerable<Identity> mandatoryQuestionsAndQuestionsWithCustomValidationFromJustEnabledGroupsAndQuestions =
                GetMandatoryQuestionsAndQuestionsWithCustomValidationFromJustEnabledGroupsAndQuestions(state,
                    questionnaire, enablementChanges.GroupsToBeEnabled, enablementChanges.QuestionsToBeEnabled, getRosterInstanceIds);

            IEnumerable<Identity> questionsWithCustomValidationDependentOnEnablementChanges =
                GetQuestionsWithCustomValidationDependentOnEnablementChanges(state, questionnaire, enablementChanges, getRosterInstanceIds);

            var questionsToBeValidated =
                dependentQuestions
                    .Concat(mandatoryQuestionsAndQuestionsWithCustomValidationFromJustEnabledGroupsAndQuestions)
                    .Concat(questionsWithCustomValidationDependentOnEnablementChanges)
                    .Distinct(new IdentityComparer())
                    .ToList();

            foreach (Identity questionToValidate in questionsToBeValidated)
            {
                bool? dependentQuestionValidationResult = this.PerformValidationOfQuestion(state, questionToValidate, questionnaire,
                    getAnswer, getNewQuestionStatus);
                switch (dependentQuestionValidationResult)
                {
                    case true:
                        questionsDeclaredValid.Add(questionToValidate);
                        break;
                    case false:
                        questionsDeclaredInvalid.Add(questionToValidate);
                        break;
                }
            }
        }

        private static IEnumerable<Identity> GetMandatoryQuestionsAndQuestionsWithCustomValidationFromJustEnabledGroupsAndQuestions(
            InterviewStateDependentOnAnswers state, IQuestionnaire questionnaire,
            List<Identity> groupsToBeEnabled, List<Identity> questionsToBeEnabled,
            Func<InterviewStateDependentOnAnswers, Guid, decimal[], DistinctDecimalList> getRosterInstanceIds)
        {
            foreach (var question in questionsToBeEnabled)
            {
                if (questionnaire.IsQuestionMandatory(question.Id) || questionnaire.IsCustomValidationDefined(question.Id))
                    yield return question;
            }

            foreach (var group in groupsToBeEnabled)
            {
                IEnumerable<Guid> affectedUnderlyingQuestionIds =
                    questionnaire.GetUnderlyingMandatoryQuestions(group.Id)
                        .Union(questionnaire.GetUnderlyingQuestionsWithNotEmptyCustomValidationExpressions(group.Id));

                IEnumerable<Identity> affectedUnderlyingQuestionInstances = GetInstancesOfQuestionsWithSameAndDeeperRosterLevelOrThrow(
                    state,
                    affectedUnderlyingQuestionIds, group.RosterVector, questionnaire, getRosterInstanceIds);

                foreach (var underlyingQuestionInstance in affectedUnderlyingQuestionInstances)
                {
                    yield return underlyingQuestionInstance;
                }
            }
        }

        private static IEnumerable<Identity> GetQuestionsWithCustomValidationDependentOnEnablementChanges(
            InterviewStateDependentOnAnswers state, IQuestionnaire questionnaire, EnablementChanges enablementChanges,
            Func<InterviewStateDependentOnAnswers, Guid, decimal[], DistinctDecimalList> getRosterInstanceIds)
        {
            IEnumerable<Identity> changedQuestions = Enumerable.Concat(enablementChanges.QuestionsToBeDisabled, enablementChanges.QuestionsToBeEnabled);
            IEnumerable<Identity> changedGroups = Enumerable.Concat(enablementChanges.GroupsToBeDisabled, enablementChanges.GroupsToBeEnabled);

            IEnumerable<Identity> underlyingQuestionsFromChangedGroups =
                from changedGroup in changedGroups
                let underlyingQuestionIds = questionnaire.GetAllUnderlyingQuestions(changedGroup.Id)
                let underlyingQuestions = GetInstancesOfQuestionsWithSameAndDeeperRosterLevelOrThrow(
                    state, underlyingQuestionIds, changedGroup.RosterVector, questionnaire, getRosterInstanceIds)
                from underlyingQuestion in underlyingQuestions
                select underlyingQuestion;

            IEnumerable<Identity> allChangedQuestions = Enumerable.Concat(changedQuestions, underlyingQuestionsFromChangedGroups);

            IEnumerable<Identity> allChangedQuestionsWhichAreAnswered =
                from changedQuestion in allChangedQuestions
                let changedQuestionKey = ConversionHelper.ConvertIdAndRosterVectorToString(changedQuestion.Id, changedQuestion.RosterVector)
                where state.AnsweredQuestions.Contains(changedQuestionKey)
                select changedQuestion;

            foreach (Identity changedQuestion in allChangedQuestionsWhichAreAnswered)
            {
                IEnumerable<Guid> dependentQuestionIds = questionnaire.GetQuestionsWhichCustomValidationDependsOnSpecifiedQuestion(changedQuestion.Id);

                IEnumerable<Identity> dependentQuestions = GetInstancesOfQuestionsWithSameAndDeeperRosterLevelOrThrow(
                    state, dependentQuestionIds, changedQuestion.RosterVector, questionnaire, getRosterInstanceIds);

                foreach (Identity dependentQuestion in dependentQuestions)
                {
                    yield return dependentQuestion;
                }
            }
        }

        private bool? PerformValidationOfQuestion(InterviewStateDependentOnAnswers state, Identity question, IQuestionnaire questionnaire,
            Func<InterviewStateDependentOnAnswers, Identity, object> getAnswer, Func<Identity, bool?> getNewQuestionState)
        {
            if (questionnaire.IsQuestionMandatory(question.Id))
            {
                if (getAnswer(state, question) == null)
                    return false;
            }

            if (!questionnaire.IsCustomValidationDefined(question.Id))
                return true;

            if (getAnswer(state, question) == null)
                return true;

            bool? questionChangedState = getNewQuestionState(question);

            //we treat newly disabled questions with validations as valid
            if (questionChangedState == false)
                return true;

            if (questionChangedState == null && IsQuestionDisabled(state, question))
                return true;

            string validationExpression = questionnaire.GetCustomValidationExpression(question.Id);

            IEnumerable<Guid> involvedQuestionIds = questionnaire.GetQuestionsInvolvedInCustomValidation(question.Id);
            IEnumerable<KeyValuePair<string, Identity>> involvedQuestions =
                GetInstancesOfQuestionsWithSameAndUpperRosterLevelOrThrow(involvedQuestionIds, question.RosterVector, questionnaire);

            return this.EvaluateBooleanExpressionOrReturnNullIfExecutionFailsWhenNotEnoughAnswers(state,
                validationExpression, involvedQuestions, getAnswer, resultIfExecutionFailsWhenAnswersAreEnough: false,
                thisIdentifierQuestionId: question.Id);
        }

        private void DetermineCustomEnablementStateOfGroupsInitializedByAddedRosterInstances(
            InterviewStateDependentOnAnswers state,
            IEnumerable<Guid> rosterIds, List<decimal> rosterInstanceIdsBeingAdded, decimal[] nearestToOuterRosterVector,
            IQuestionnaire questionnaire,
            Func<InterviewStateDependentOnAnswers, Identity, object> getAnswer,
            Func<InterviewStateDependentOnAnswers, Guid, decimal[], DistinctDecimalList> getRosterInstanceIds,
            out List<Identity> groupsToBeDisabled, out List<Identity> groupsToBeEnabled)
        {
            groupsToBeDisabled = new List<Identity>();
            groupsToBeEnabled = new List<Identity>();

            if (rosterInstanceIdsBeingAdded.Count == 0)
                return;

            foreach (Guid rosterId in rosterIds)
            {
                int indexOfRosterInRosterVector = GetIndexOfRosterInRosterVector(rosterId, questionnaire);

                IEnumerable<Guid> affectedGroupIds =
                    questionnaire.GetGroupAndUnderlyingGroupsWithNotEmptyCustomEnablementConditions(rosterId);

                IEnumerable<Identity> affectedGroups = GetInstancesOfGroupsWithSameAndDeeperRosterLevelOrThrow(state,
                    affectedGroupIds, nearestToOuterRosterVector, questionnaire, getRosterInstanceIds)
                    .Where(group =>
                        group.RosterVector.Length > indexOfRosterInRosterVector &&
                            rosterInstanceIdsBeingAdded.Contains(group.RosterVector[indexOfRosterInRosterVector]));

                foreach (Identity group in affectedGroups)
                {
                    PutToCorrespondingListAccordingToEnablementStateChange(group, groupsToBeEnabled, groupsToBeDisabled,
                        isNewStateEnabled: this.ShouldGroupBeEnabledByCustomEnablementCondition(state, group, questionnaire, getAnswer),
                        isOldStateEnabled: true);
                }
            }
        }

        private void DetermineCustomEnablementStateOfQuestionsInitializedByAddedRosterInstances(
            InterviewStateDependentOnAnswers state,
            IEnumerable<Guid> rosterIds, List<decimal> rosterInstanceIdsBeingAdded, decimal[] nearestToOuterRosterVector,
            IQuestionnaire questionnaire,
            Func<InterviewStateDependentOnAnswers, Identity, object> getAnswer,
            Func<InterviewStateDependentOnAnswers, Guid, decimal[], DistinctDecimalList> getRosterInstanceIds,
            out List<Identity> questionsToBeDisabled, out List<Identity> questionsToBeEnabled)
        {
            questionsToBeDisabled = new List<Identity>();
            questionsToBeEnabled = new List<Identity>();

            if (rosterInstanceIdsBeingAdded.Count == 0)
                return;

            foreach (Guid rosterId in rosterIds)
            {
                int indexOfRosterInRosterVector = GetIndexOfRosterInRosterVector(rosterId, questionnaire);

                IEnumerable<Guid> affectedQuestionIds = questionnaire.GetUnderlyingQuestionsWithNotEmptyCustomEnablementConditions(rosterId);

                IEnumerable<Identity> affectedQuestions =
                    GetInstancesOfQuestionsWithSameAndDeeperRosterLevelOrThrow(state,
                        affectedQuestionIds, nearestToOuterRosterVector, questionnaire, getRosterInstanceIds)
                        .Where(question => rosterInstanceIdsBeingAdded.Contains(question.RosterVector[indexOfRosterInRosterVector]));

                foreach (Identity question in affectedQuestions)
                {
                    PutToCorrespondingListAccordingToEnablementStateChange(question, questionsToBeEnabled, questionsToBeDisabled,
                        isNewStateEnabled:
                            this.ShouldQuestionBeEnabledByCustomEnablementCondition(state, question, questionnaire, getAnswer),
                        isOldStateEnabled: true);
                }
            }
        }

        private static void DetermineValidityStateOfQuestionsInitializedByAddedRosterInstances(
            InterviewStateDependentOnAnswers state,
            List<Guid> rosterIds, List<decimal> rosterInstanceIdsBeingAdded, decimal[] nearestToOuterRosterVector,
            IQuestionnaire questionnaire,
            List<Identity> groupsToBeDisabled, List<Identity> questionsToBeDisabled,
            Func<InterviewStateDependentOnAnswers, Guid, decimal[], DistinctDecimalList> getRosterInstanceIds,
            out List<Identity> questionsToBeInvalid)
        {
            questionsToBeInvalid = new List<Identity>();

            if (rosterInstanceIdsBeingAdded.Count == 0)
                return;

            foreach (Guid rosterId in rosterIds)
            {
                int indexOfRosterInRosterVector = GetIndexOfRosterInRosterVector(rosterId, questionnaire);

                IEnumerable<Guid> affectedQuestionIds = questionnaire.GetUnderlyingMandatoryQuestions(rosterId);

                IEnumerable<Identity> affectedQuestions = GetInstancesOfQuestionsWithSameAndDeeperRosterLevelOrThrow(state,
                    affectedQuestionIds, nearestToOuterRosterVector, questionnaire, getRosterInstanceIds)
                    .Where(question
                        => rosterInstanceIdsBeingAdded.Contains(question.RosterVector[indexOfRosterInRosterVector])
                            &&
                            !IsQuestionOrParentGroupDisabled(question, questionnaire,
                                (questionId) => groupsToBeDisabled.Any(q => AreEqual(q, questionId)),
                                (questionId) => questionsToBeDisabled.Any(q => AreEqual(q, questionId))));

                questionsToBeInvalid.AddRange(affectedQuestions);
            }
        }

        private bool ShouldGroupBeEnabledByCustomEnablementCondition(InterviewStateDependentOnAnswers state, Identity group,
            IQuestionnaire questionnaire,
            Func<InterviewStateDependentOnAnswers, Identity, object> getAnswer)
        {
            return this.ShouldBeEnabledByCustomEnablementCondition(state,
                questionnaire.GetCustomEnablementConditionForGroup(group.Id),
                group.RosterVector,
                questionnaire.GetQuestionsInvolvedInCustomEnablementConditionOfGroup(group.Id),
                questionnaire,
                getAnswer);
        }

        private bool ShouldQuestionBeEnabledByCustomEnablementCondition(InterviewStateDependentOnAnswers state, Identity question,
            IQuestionnaire questionnaire,
            Func<InterviewStateDependentOnAnswers, Identity, object> getAnswer)
        {
            return this.ShouldBeEnabledByCustomEnablementCondition(state,
                questionnaire.GetCustomEnablementConditionForQuestion(question.Id),
                question.RosterVector,
                questionnaire.GetQuestionsInvolvedInCustomEnablementConditionOfQuestion(question.Id),
                questionnaire,
                getAnswer);
        }

        private bool ShouldBeEnabledByCustomEnablementCondition(InterviewStateDependentOnAnswers state, string enablementCondition,
            decimal[] rosterVector,
            IEnumerable<Guid> involvedQuestionIds, IQuestionnaire questionnaire,
            Func<InterviewStateDependentOnAnswers, Identity, object> getAnswer)
        {
            const bool ShouldBeEnabledIfSomeInvolvedQuestionsAreNotAnswered = false;

            IEnumerable<KeyValuePair<string, Identity>> involvedQuestions =
                GetInstancesOfQuestionsWithSameAndUpperRosterLevelOrThrow(involvedQuestionIds, rosterVector, questionnaire);

            return this.EvaluateBooleanExpressionOrReturnNullIfExecutionFailsWhenNotEnoughAnswers(state,
                enablementCondition, involvedQuestions, getAnswer, resultIfExecutionFailsWhenAnswersAreEnough: true)
                ?? ShouldBeEnabledIfSomeInvolvedQuestionsAreNotAnswered;
        }

        private List<Identity> GetAnswersToRemoveIfRosterInstancesAreRemoved(InterviewStateDependentOnAnswers state,
            IEnumerable<Guid> rosterIds, List<decimal> rosterInstanceIdsBeingRemoved, decimal[] nearestToOuterRosterVector,
            IQuestionnaire questionnaire)
        {
            if (rosterInstanceIdsBeingRemoved.Count == 0)
                return new List<Identity>();

            return rosterIds
                .SelectMany(rosterId =>
                    this.GetAnswersToRemoveIfRosterInstancesAreRemoved(state, rosterId, rosterInstanceIdsBeingRemoved,
                        nearestToOuterRosterVector, questionnaire))
                .ToList();
        }

        private IEnumerable<Identity> GetAnswersToRemoveIfRosterInstancesAreRemoved(InterviewStateDependentOnAnswers state,
            Guid rosterId, List<decimal> rosterInstanceIdsBeingRemoved, decimal[] nearestToOuterRosterVector,
            IQuestionnaire questionnaire)
        {
            int indexOfRosterInRosterVector = GetIndexOfRosterInRosterVector(rosterId, questionnaire);

            IEnumerable<Guid> underlyingQuestionIds = questionnaire.GetAllUnderlyingQuestions(rosterId);

            IEnumerable<Identity> underlyingQuestionInstances = GetInstancesOfQuestionsWithSameAndDeeperRosterLevelOrThrow(state,
                underlyingQuestionIds, nearestToOuterRosterVector, questionnaire, GetRosterInstanceIds);

            IEnumerable<Identity> underlyingQuestionsBeingRemovedByRemovedRosterInstances = (
                from question in underlyingQuestionInstances
                where WasQuestionAnswered(state, question)
                where rosterInstanceIdsBeingRemoved.Contains(question.RosterVector[indexOfRosterInRosterVector])
                select question
                ).ToList();

            IEnumerable<Identity> linkedQuestionsWithNoLongerValidAnswersBecauseOfSelectedOptionBeingRemoved =
                this.GetAnswersForLinkedQuestionsToRemoveBecauseOfRemovedQuestionAnswers(state,
                    underlyingQuestionsBeingRemovedByRemovedRosterInstances, questionnaire, GetRosterInstanceIds);

            return Enumerable.Concat(
                underlyingQuestionsBeingRemovedByRemovedRosterInstances,
                linkedQuestionsWithNoLongerValidAnswersBecauseOfSelectedOptionBeingRemoved);
        }

        private static DistinctDecimalList GetRosterInstanceIdsBeingAdded(DistinctDecimalList existingRosterInstanceIds,
            DistinctDecimalList newRosterInstanceIds)
        {
            return
                new DistinctDecimalList(
                    newRosterInstanceIds.Where(newRosterInstanceId => !existingRosterInstanceIds.Contains(newRosterInstanceId)).ToList());
        }

        private static DistinctDecimalList GetRosterInstanceIdsBeingRemoved(DistinctDecimalList existingRosterInstanceIds,
            DistinctDecimalList newRosterInstanceIds)
        {
            return
                new DistinctDecimalList(
                    existingRosterInstanceIds.Where(existingRosterInstanceId => !newRosterInstanceIds.Contains(existingRosterInstanceId))
                        .ToList());
        }

        private IEnumerable<Identity> GetAnswersForLinkedQuestionsToRemoveBecauseOfRemovedQuestionAnswers(
            InterviewStateDependentOnAnswers state,
            IEnumerable<Identity> questionsToRemove, IQuestionnaire questionnaire,
            Func<InterviewStateDependentOnAnswers, Guid, decimal[], DistinctDecimalList> getRosterInstanceIds)
        {
            bool nothingGoingToBeRemoved = !questionsToRemove.Any();
            if (nothingGoingToBeRemoved)
                return Enumerable.Empty<Identity>();

            return this.GetAnswersForLinkedQuestionsToRemoveBecauseOfReferencedAnswersGoingToDisappear(state, questionnaire,
                getRosterInstanceIds,
                isQuestionAnswerGoingToDisappear:
                    question => questionsToRemove.Any(questionToRemove => AreEqual(question, questionToRemove)));
        }

        private List<Identity> GetAnswersForLinkedQuestionsToRemoveBecauseOfDisabledGroupsOrQuestions(
            InterviewStateDependentOnAnswers state,
            IEnumerable<Identity> groupsToBeDisabled, IEnumerable<Identity> questionsToBeDisabled, IQuestionnaire questionnaire,
            Func<InterviewStateDependentOnAnswers, Guid, decimal[], DistinctDecimalList> getRosterInstanceIds)
        {
            bool nothingGoingToBeDisabled = !groupsToBeDisabled.Any() && !questionsToBeDisabled.Any();
            if (nothingGoingToBeDisabled)
                return new List<Identity>();

            return this.GetAnswersForLinkedQuestionsToRemoveBecauseOfReferencedAnswersGoingToDisappear(state, questionnaire,
                getRosterInstanceIds,
                isQuestionAnswerGoingToDisappear:
                    question => IsQuestionGoingToBeDisabled(question, groupsToBeDisabled, questionsToBeDisabled, questionnaire));
        }

        private List<Identity> GetAnswersForLinkedQuestionsToRemoveBecauseOfReferencedAnswersGoingToDisappear(
            InterviewStateDependentOnAnswers state,
            IQuestionnaire questionnaire,
            Func<InterviewStateDependentOnAnswers, Guid, decimal[], DistinctDecimalList> getRosterInstanceIds,
            Func<Identity, bool> isQuestionAnswerGoingToDisappear)
        {
            var answersToRemove = new List<Identity>();

            // we currently have a bug that after restore linked single option answers list is filled with not linked answers after sync
            var realLinkedSingleOptionAnswers =
                state.LinkedSingleOptionAnswersBuggy.Values.Where(answer => questionnaire.IsQuestionLinked(answer.Item1));

            foreach (Tuple<Guid, decimal[], decimal[]> linkedSingleOptionAnswer in realLinkedSingleOptionAnswers)
            {
                var linkedQuestion = new Identity(linkedSingleOptionAnswer.Item1, linkedSingleOptionAnswer.Item2);
                decimal[] linkedQuestionSelectedOption = linkedSingleOptionAnswer.Item3;

                IEnumerable<Identity> questionsReferencedByLinkedQuestion =
                    this.GetQuestionsReferencedByLinkedQuestion(state, linkedQuestion, questionnaire, getRosterInstanceIds);

                Identity questionSelectedAsAnswer =
                    questionsReferencedByLinkedQuestion
                        .SingleOrDefault(
                            question => AreEqualRosterVectors(linkedQuestionSelectedOption, question.RosterVector));

                bool isSelectedOptionGoingToDisappear = questionSelectedAsAnswer != null &&
                    isQuestionAnswerGoingToDisappear(questionSelectedAsAnswer);
                if (isSelectedOptionGoingToDisappear)
                {
                    answersToRemove.Add(linkedQuestion);
                }
            }

            foreach (Tuple<Guid, decimal[], decimal[][]> linkedMultipleOptionsAnswer in state.LinkedMultipleOptionsAnswers.Values)
            {
                var linkedQuestion = new Identity(linkedMultipleOptionsAnswer.Item1, linkedMultipleOptionsAnswer.Item2);
                decimal[][] linkedQuestionSelectedOptions = linkedMultipleOptionsAnswer.Item3;

                IEnumerable<Identity> questionsReferencedByLinkedQuestion =
                    this.GetQuestionsReferencedByLinkedQuestion(state, linkedQuestion, questionnaire, getRosterInstanceIds);

                IEnumerable<Identity> questionsSelectedAsAnswers =
                    questionsReferencedByLinkedQuestion
                        .Where(
                            question => linkedQuestionSelectedOptions.Any(
                                selectedOption => AreEqualRosterVectors(selectedOption, question.RosterVector)));

                bool isSomeOfSelectedOptionsGoingToDisappear = questionsSelectedAsAnswers.Any(isQuestionAnswerGoingToDisappear);
                if (isSomeOfSelectedOptionsGoingToDisappear)
                {
                    answersToRemove.Add(linkedQuestion);
                }
            }

            return answersToRemove;
        }

        private IEnumerable<Identity> GetQuestionsReferencedByLinkedQuestion(InterviewStateDependentOnAnswers state,
            Identity linkedQuestion, IQuestionnaire questionnaire,
            Func<InterviewStateDependentOnAnswers, Guid, decimal[], DistinctDecimalList> getRosterInstanceIds)
        {
            Guid referencedQuestionId = questionnaire.GetQuestionReferencedByLinkedQuestion(linkedQuestion.Id);

            return GetInstancesOfQuestionsInAllRosterLevels(state,
                referencedQuestionId, linkedQuestion.RosterVector, questionnaire, getRosterInstanceIds);
        }

        private static bool IsQuestionGoingToBeDisabled(Identity question,
            IEnumerable<Identity> groupsToBeDisabled, IEnumerable<Identity> questionsToBeDisabled, IQuestionnaire questionnaire)
        {
            bool questionIsListedToBeDisabled =
                questionsToBeDisabled.Any(questionToBeDisabled => AreEqual(question, questionToBeDisabled));

            IEnumerable<Guid> parentGroupIds = questionnaire.GetAllParentGroupsForQuestion(question.Id);
            IEnumerable<Identity> parentGroups = GetInstancesOfGroupsWithSameAndUpperRosterLevelOrThrow(parentGroupIds,
                question.RosterVector, questionnaire);

            bool someOfQuestionParentGroupsAreListedToBeDisabled = parentGroups.Any(parentGroup =>
                groupsToBeDisabled.Any(groupToBeDisabled => AreEqual(parentGroup, groupToBeDisabled)));

            return questionIsListedToBeDisabled || someOfQuestionParentGroupsAreListedToBeDisabled;
        }

        private static int GetIndexOfRosterInRosterVector(Guid rosterId, IQuestionnaire questionnaire)
        {
            return questionnaire
                .GetRostersFromTopToSpecifiedGroup(rosterId)
                .ToList()
                .IndexOf(rosterId);
        }

        private bool? EvaluateBooleanExpressionOrReturnNullIfExecutionFailsWhenNotEnoughAnswers(InterviewStateDependentOnAnswers state,
            string expression,
            IEnumerable<KeyValuePair<string, Identity>> involvedQuestions,
            Func<InterviewStateDependentOnAnswers, Identity, object> getAnswer, bool? resultIfExecutionFailsWhenAnswersAreEnough,
            Guid? thisIdentifierQuestionId = null)
        {
            Dictionary<Guid, object> involvedAnswers = involvedQuestions.ToDictionary(
                involvedQuestion => involvedQuestion.Value.Id,
                involvedQuestion => getAnswer(state, involvedQuestion.Value));

            Dictionary<string, Guid> questionMappedOnVariableNames = involvedQuestions.ToDictionary(
                involvedQuestion => involvedQuestion.Key,
                involvedQuestion => involvedQuestion.Value.Id);

            bool isSpecialThisIdentifierSupportedByExpression = thisIdentifierQuestionId.HasValue;

            var mapIdentifierToQuestionId = isSpecialThisIdentifierSupportedByExpression
                ? (Func<string, Guid>)
                    (identifier =>
                        GetQuestionIdByExpressionIdentifierIncludingThis(identifier, questionMappedOnVariableNames,
                            thisIdentifierQuestionId.Value))
                : (Func<string, Guid>)
                    (identifier => GetQuestionIdByExpressionIdentifierExcludingThis(identifier, questionMappedOnVariableNames));

            try
            {
                return this.ExpressionProcessor.EvaluateBooleanExpression(expression,
                    getValueForIdentifier: identifier => involvedAnswers[mapIdentifierToQuestionId(identifier)]);
            }
            catch (Exception exception)
            {
                bool areAllInvolvedQuestionsAnswered = involvedAnswers.Values.All(answer => answer != null);

                if (areAllInvolvedQuestionsAnswered)
                {
                    this.Logger.Warn(
                        string.Format("Failed to evaluate boolean expression '{0}' which has all involved answers given.", expression),
                        exception);
                }
                else
                {
                    this.Logger.Info(
                        string.Format("Failed to evaluate boolean expression '{0}' which has some involved answers missing.", expression),
                        exception);
                }

                return areAllInvolvedQuestionsAnswered
                    ? resultIfExecutionFailsWhenAnswersAreEnough
                    : null;
            }
        }

        private static Guid GetQuestionIdByExpressionIdentifierIncludingThis(string identifier,
            Dictionary<string, Guid> questionMappedOnVariableNames, Guid contextQuestionId)
        {
            if (identifier.ToLower() == "this")
                return contextQuestionId;

            return GetQuestionIdByExpressionIdentifierExcludingThis(identifier, questionMappedOnVariableNames);
        }

        private static Guid GetQuestionIdByExpressionIdentifierExcludingThis(string identifier,
            Dictionary<string, Guid> questionMappedOnVariableNames)
        {
            Guid questionId;
            if (Guid.TryParse(identifier, out questionId))
                return questionId;
            return questionMappedOnVariableNames[identifier];
        }

        private static DistinctDecimalList GetRosterInstanceIds(InterviewStateDependentOnAnswers state, Guid groupId,
            decimal[] outerRosterVector)
        {
            string groupKey = ConversionHelper.ConvertIdAndRosterVectorToString(groupId, outerRosterVector);

            return state.RosterGroupInstanceIds.ContainsKey(groupKey)
                ? state.RosterGroupInstanceIds[groupKey]
                : new DistinctDecimalList();
        }

        private IEnumerable<decimal[]> AvailableRosterLevelsForGroup(InterviewStateDependentOnAnswers state, IQuestionnaire questionnaire,
            Guid groupdId)
        {
            int rosterGroupLevel = questionnaire.GetRosterLevelForGroup(groupdId);

            Guid[] parentRosterGroupsStartingFromTop =
                questionnaire.GetRostersFromTopToSpecifiedGroup(groupdId)
                    .ToArray();

            var availableRosterLevels = ExtendRosterVector(state, EmptyRosterVector, rosterGroupLevel,
                parentRosterGroupsStartingFromTop, GetRosterInstanceIds);
            return availableRosterLevels;
        }

        private IEnumerable<decimal[]> AvailableRosterLevelsForQuestion(InterviewStateDependentOnAnswers state, IQuestionnaire questionnaire,
            Guid questionId)
        {
            int questionRosterLevel = questionnaire.GetRosterLevelForQuestion(questionId);

            Guid[] parentRosterGroupsStartingFromTop =
                questionnaire.GetRostersFromTopToSpecifiedQuestion(questionId)
                    .ToArray();

            var availableRosterLevels = ExtendRosterVector(state, EmptyRosterVector, questionRosterLevel,
                parentRosterGroupsStartingFromTop, GetRosterInstanceIds);

            return availableRosterLevels;
        }

        private static bool IsQuestionAnsweredValid(InterviewStateDependentOnAnswers state, Identity question)
        {
            string questionKey = ConversionHelper.ConvertIdAndRosterVectorToString(question.Id, question.RosterVector);

            return state.ValidAnsweredQuestions.Contains(questionKey);
        }

        private static bool IsQuestionAnsweredInvalid(InterviewStateDependentOnAnswers state, Identity question)
        {
            string questionKey = ConversionHelper.ConvertIdAndRosterVectorToString(question.Id, question.RosterVector);

            return state.InvalidAnsweredQuestions.Contains(questionKey);
        }

        private bool HasInvalidAnswers()
        {
            return this.interviewState.InvalidAnsweredQuestions.Any();
        }

    }
}