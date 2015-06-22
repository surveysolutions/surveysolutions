using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Microsoft.Practices.ServiceLocation;
using Ncqrs.Domain;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Sourcing;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Preloading;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Snapshots;
using WB.Core.SharedKernels.DataCollection.Implementation.Providers;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.DataCollection.V2;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    internal class Interview : AggregateRootMappedByConvention, ISnapshotable<InterviewState>
    {
        private static readonly decimal[] EmptyRosterVector = { };

        private Guid questionnaireId;
        private Guid interviewerId;
        private long questionnaireVersion;
        private bool wasCompleted;
        private bool wasHardDeleted;
        private InterviewStatus status;

        private IInterviewExpressionStateV2 expressionProcessorStatePrototype = null;
        private IInterviewExpressionStateV2 ExpressionProcessorStatePrototype
        {
            get
            {
                if (this.expressionProcessorStatePrototype == null)
                {
                    var stateProvider = this.ExpressionProcessorStatePrototypeProvider;
                    this.expressionProcessorStatePrototype = stateProvider.GetExpressionState(this.questionnaireId, this.questionnaireVersion);
                }

                return this.expressionProcessorStatePrototype;
            }

            set
            {
                expressionProcessorStatePrototype = value;
            }
        }

        private InterviewStateDependentOnAnswers interviewState = new InterviewStateDependentOnAnswers();

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
            this.interviewState = new InterviewStateDependentOnAnswers();
            this.questionnaireId = @event.InterviewData.QuestionnaireId;
            this.questionnaireVersion = @event.InterviewData.QuestionnaireVersion;
            this.status = @event.InterviewData.Status;
            this.wasCompleted = @event.InterviewData.WasCompleted;
            this.ExpressionProcessorStatePrototype =
                ExpressionProcessorStatePrototypeProvider.GetExpressionState(@event.InterviewData.QuestionnaireId, @event.InterviewData.QuestionnaireVersion);

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

            var orderedRosterInstances = @event.InterviewData.RosterGroupInstances.SelectMany(x => x.Value).OrderBy(x=>x.OuterScopePropagationVector.Length).ToList();
            foreach (RosterSynchronizationDto roster in orderedRosterInstances)
            {
                this.ExpressionProcessorStatePrototype.AddRoster(roster.RosterId, roster.OuterScopePropagationVector, roster.RosterInstanceId, roster.SortIndex);
                this.ExpressionProcessorStatePrototype.UpdateRosterTitle(roster.RosterId, roster.OuterScopePropagationVector, roster.RosterInstanceId, roster.RosterTitle);
            }
           
            if (@event.InterviewData.Answers != null)
            {
                foreach (var question in @event.InterviewData.Answers)
                {
                    decimal[] questionPropagationVector = question.QuestionPropagationVector;
                    if (question.Answer is long)
                    {
                        this.ExpressionProcessorStatePrototype.UpdateNumericIntegerAnswer(question.Id, questionPropagationVector, (long)question.Answer);
                    }
                    if (question.Answer is decimal)
                    {
                        this.ExpressionProcessorStatePrototype.UpdateNumericRealAnswer(question.Id, questionPropagationVector, Convert.ToDouble(question.Answer));
                        this.ExpressionProcessorStatePrototype.UpdateSingleOptionAnswer(question.Id, questionPropagationVector, Convert.ToDecimal(question.Answer));
                    }
                    var answer = question.Answer as string;
                    if (answer != null)
                    {
                        this.ExpressionProcessorStatePrototype.UpdateTextAnswer(question.Id, questionPropagationVector, answer);
                        this.ExpressionProcessorStatePrototype.UpdateQrBarcodeAnswer(question.Id, questionPropagationVector, answer);
                    }

                    if (question.Answer is decimal[])
                    {
                        this.ExpressionProcessorStatePrototype.UpdateMultiOptionAnswer(question.Id, questionPropagationVector, (decimal[])(question.Answer));
                        this.ExpressionProcessorStatePrototype.UpdateLinkedSingleOptionAnswer(question.Id, questionPropagationVector, (decimal[])(question.Answer));
                    }
                    var geoAnswer = question.Answer as GeoPosition;
                    if (geoAnswer != null)
                    {
                        this.ExpressionProcessorStatePrototype.UpdateGeoLocationAnswer(question.Id, questionPropagationVector, geoAnswer.Latitude, geoAnswer.Longitude, geoAnswer.Accuracy, geoAnswer.Altitude);
                    }
                    if (question.Answer is DateTime)
                    {
                        this.ExpressionProcessorStatePrototype.UpdateDateAnswer(question.Id, questionPropagationVector, (DateTime)question.Answer);
                    }
                    if (question.Answer is decimal[][])
                    {
                        this.ExpressionProcessorStatePrototype.UpdateLinkedMultiOptionAnswer(question.Id, questionPropagationVector, (decimal[][])(question.Answer));
                    }
                    if (question.Answer is Tuple<decimal, string>[])
                    {
                        this.ExpressionProcessorStatePrototype.UpdateTextListAnswer(question.Id, questionPropagationVector, (Tuple<decimal, string>[])(question.Answer));
                    }
                }
            }

            this.ExpressionProcessorStatePrototype.DeclareAnswersValid(@event.InterviewData.ValidAnsweredQuestions.Select(validAnsweredQuestion => new Identity(validAnsweredQuestion.Id, validAnsweredQuestion.InterviewItemPropagationVector)));
            this.ExpressionProcessorStatePrototype.DeclareAnswersInvalid(@event.InterviewData.InvalidAnsweredQuestions.Select(validAnsweredQuestion => new Identity(validAnsweredQuestion.Id, validAnsweredQuestion.InterviewItemPropagationVector)));
            this.ExpressionProcessorStatePrototype.DisableQuestions(@event.InterviewData.DisabledQuestions.Select(validAnsweredQuestion => new Identity(validAnsweredQuestion.Id, validAnsweredQuestion.InterviewItemPropagationVector)));
            this.ExpressionProcessorStatePrototype.DisableGroups(@event.InterviewData.DisabledGroups.Select(validAnsweredQuestion => new Identity(validAnsweredQuestion.Id, validAnsweredQuestion.InterviewItemPropagationVector)));


            this.interviewState.DisabledGroups = ToHashSetOfIdAndRosterVectorStrings(@event.InterviewData.DisabledGroups);
            this.interviewState.DisabledQuestions = ToHashSetOfIdAndRosterVectorStrings(@event.InterviewData.DisabledQuestions);
            this.interviewState.RosterGroupInstanceIds = BuildRosterInstanceIdsFromSynchronizationDto(@event.InterviewData);
            this.interviewState.ValidAnsweredQuestions = ToHashSetOfIdAndRosterVectorStrings(@event.InterviewData.ValidAnsweredQuestions);
            this.interviewState.InvalidAnsweredQuestions = ToHashSetOfIdAndRosterVectorStrings(@event.InterviewData.InvalidAnsweredQuestions);
        }

        private void Apply(SynchronizationMetadataApplied @event)
        {
            this.questionnaireId = @event.QuestionnaireId;
            this.questionnaireVersion = @event.QuestionnaireVersion;
            this.status = @event.Status;
        }

        internal void Apply(TextQuestionAnswered @event)
        {
            string questionKey = ConversionHelper.ConvertIdAndRosterVectorToString(@event.QuestionId, @event.PropagationVector);

            this.interviewState.AnswersSupportedInExpressions[questionKey] = @event.Answer;
            this.interviewState.AnsweredQuestions.Add(questionKey);

            this.ExpressionProcessorStatePrototype.UpdateTextAnswer(@event.QuestionId, @event.PropagationVector, @event.Answer);
        }

        internal void Apply(QRBarcodeQuestionAnswered @event)
        {
            string questionKey = ConversionHelper.ConvertIdAndRosterVectorToString(@event.QuestionId, @event.PropagationVector);

            this.interviewState.AnswersSupportedInExpressions[questionKey] = @event.Answer;
            this.interviewState.AnsweredQuestions.Add(questionKey);

            this.ExpressionProcessorStatePrototype.UpdateQrBarcodeAnswer(@event.QuestionId, @event.PropagationVector, @event.Answer);
        }

        internal void Apply(PictureQuestionAnswered @event)
        {
            string questionKey = ConversionHelper.ConvertIdAndRosterVectorToString(@event.QuestionId, @event.PropagationVector);

            this.interviewState.AnswersSupportedInExpressions[questionKey] = @event.PictureFileName;
            this.interviewState.AnsweredQuestions.Add(questionKey);

            this.ExpressionProcessorStatePrototype.UpdateMediaAnswer(@event.QuestionId, @event.PropagationVector, @event.PictureFileName);
        }

        internal void Apply(NumericRealQuestionAnswered @event)
        {
            string questionKey = ConversionHelper.ConvertIdAndRosterVectorToString(@event.QuestionId, @event.PropagationVector);

            this.interviewState.AnswersSupportedInExpressions[questionKey] = @event.Answer;
            this.interviewState.AnsweredQuestions.Add(questionKey);

            this.ExpressionProcessorStatePrototype.UpdateNumericRealAnswer(@event.QuestionId, @event.PropagationVector, (double)@event.Answer);
        }

        internal void Apply(NumericIntegerQuestionAnswered @event)
        {
            string questionKey = ConversionHelper.ConvertIdAndRosterVectorToString(@event.QuestionId, @event.PropagationVector);

            this.interviewState.AnswersSupportedInExpressions[questionKey] = @event.Answer;
            this.interviewState.AnsweredQuestions.Add(questionKey);

            this.ExpressionProcessorStatePrototype.UpdateNumericIntegerAnswer(@event.QuestionId, @event.PropagationVector, @event.Answer);
        }

        internal void Apply(DateTimeQuestionAnswered @event)
        {
            string questionKey = ConversionHelper.ConvertIdAndRosterVectorToString(@event.QuestionId, @event.PropagationVector);

            this.interviewState.AnswersSupportedInExpressions[questionKey] = @event.Answer;
            this.interviewState.AnsweredQuestions.Add(questionKey);

            this.ExpressionProcessorStatePrototype.UpdateDateAnswer(@event.QuestionId, @event.PropagationVector, @event.Answer);
        }

        internal void Apply(SingleOptionQuestionAnswered @event)
        {
            string questionKey = ConversionHelper.ConvertIdAndRosterVectorToString(@event.QuestionId, @event.PropagationVector);

            this.interviewState.AnswersSupportedInExpressions[questionKey] = @event.SelectedValue;
            this.interviewState.AnsweredQuestions.Add(questionKey);

            this.ExpressionProcessorStatePrototype.UpdateSingleOptionAnswer(@event.QuestionId, @event.PropagationVector, @event.SelectedValue);
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
            this.ExpressionProcessorStatePrototype.UpdateMultiOptionAnswer(@event.QuestionId, @event.PropagationVector, @event.SelectedValues);
        }

        internal void Apply(GeoLocationQuestionAnswered @event)
        {
            string questionKey = ConversionHelper.ConvertIdAndRosterVectorToString(@event.QuestionId, @event.PropagationVector);

            this.interviewState.AnsweredQuestions.Add(questionKey);

            this.ExpressionProcessorStatePrototype.UpdateGeoLocationAnswer(@event.QuestionId, @event.PropagationVector, @event.Latitude,
                @event.Longitude, @event.Accuracy, @event.Altitude);
        }

        internal void Apply(TextListQuestionAnswered @event)
        {
            string questionKey = ConversionHelper.ConvertIdAndRosterVectorToString(@event.QuestionId, @event.PropagationVector);
            this.interviewState.TextListAnswers[questionKey] = @event.Answers;
            this.interviewState.AnsweredQuestions.Add(questionKey);

            this.ExpressionProcessorStatePrototype.UpdateTextListAnswer(@event.QuestionId, @event.PropagationVector, @event.Answers);
        }

        private void Apply(SingleOptionLinkedQuestionAnswered @event)
        {
            string questionKey = ConversionHelper.ConvertIdAndRosterVectorToString(@event.QuestionId, @event.PropagationVector);

            this.interviewState.LinkedSingleOptionAnswersBuggy[questionKey] = Tuple.Create(@event.QuestionId, @event.PropagationVector,
                @event.SelectedPropagationVector);
            this.interviewState.AnsweredQuestions.Add(questionKey);

            this.ExpressionProcessorStatePrototype.UpdateLinkedSingleOptionAnswer(@event.QuestionId, @event.PropagationVector, @event.SelectedPropagationVector);
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

            this.ExpressionProcessorStatePrototype.UpdateLinkedMultiOptionAnswer(@event.QuestionId, @event.PropagationVector, @event.SelectedPropagationVectors);
        }

        internal void Apply(AnswersDeclaredValid @event)
        {
            this.interviewState.DeclareAnswersValid(@event.Questions);
            this.ExpressionProcessorStatePrototype.DeclareAnswersValid(@event.Questions.ToIdentities());
        }

        internal void Apply(AnswersDeclaredInvalid @event)
        {
            this.interviewState.DeclareAnswersInvalid(@event.Questions);
            this.ExpressionProcessorStatePrototype.DeclareAnswersInvalid(@event.Questions.ToIdentities());
        }

        internal void Apply(GroupsDisabled @event)
        {
            this.interviewState.DisableGroups(@event.Groups);

            this.ExpressionProcessorStatePrototype.DisableGroups(@event.Groups.ToIdentities());
        }

        internal void Apply(GroupsEnabled @event)
        {
            this.interviewState.EnableGroups(@event.Groups);

            this.ExpressionProcessorStatePrototype.EnableGroups(@event.Groups.ToIdentities());
        }

        internal void Apply(QuestionsDisabled @event)
        {
            this.interviewState.DisableQuestions(@event.Questions);

            this.ExpressionProcessorStatePrototype.DisableQuestions(@event.Questions.ToIdentities());
        }

        internal void Apply(QuestionsEnabled @event)
        {
            this.interviewState.EnableQuestions(@event.Questions);

            this.ExpressionProcessorStatePrototype.EnableQuestions(@event.Questions.ToIdentities());

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

            //expressionProcessorStatePrototype could also be changed but it's an old code.
        }

        private void Apply(RosterInstancesTitleChanged @event)
        {
            foreach (var instance in @event.ChangedInstances)
            {
                this.ExpressionProcessorStatePrototype.UpdateRosterTitle(instance.RosterInstance.GroupId, instance.RosterInstance.OuterRosterVector, instance.RosterInstance.RosterInstanceId, instance.Title);
            }
        }

        internal void Apply(RosterInstancesAdded @event)
        {
            this.interviewState.AddRosterInstances(@event.Instances);

            foreach (var instance in @event.Instances)
            {
                this.ExpressionProcessorStatePrototype.AddRoster(instance.GroupId, instance.OuterRosterVector, instance.RosterInstanceId, instance.SortIndex);
            }
        }

        private void Apply(RosterInstancesRemoved @event)
        {
            this.interviewState.RemoveRosterInstances(@event.Instances);
            foreach (var instance in @event.Instances)
            {
                this.ExpressionProcessorStatePrototype.RemoveRoster(instance.GroupId, instance.OuterRosterVector, instance.RosterInstanceId);
            }
        }

        internal void Apply(InterviewStatusChanged @event)
        {
            this.status = @event.Status;
        }

        private void Apply(SupervisorAssigned @event) { }

        internal void Apply(InterviewerAssigned @event)
        {
            this.interviewerId = @event.InterviewerId;
        }

        private void Apply(InterviewDeleted @event) { }

        internal void Apply(InterviewHardDeleted @event)
        {
            wasHardDeleted = true;
        }

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

        private void Apply(AnswersRemoved @event)
        {
            this.interviewState.RemoveAnswers(@event.Questions);
        }

        public InterviewState CreateSnapshot()
        {
            if (wasHardDeleted)
                return new InterviewState(wasHardDeleted);

            return new InterviewState(
                this.questionnaireId,
                this.questionnaireVersion,
                this.status,
                this.interviewState.AnswersSupportedInExpressions,
                this.interviewState.LinkedSingleOptionAnswersBuggy,
                this.interviewState.LinkedMultipleOptionsAnswers,
                this.interviewState.TextListAnswers,
                this.interviewState.AnsweredQuestions,
                this.interviewState.AnswerComments,
                this.interviewState.DisabledGroups,
                this.interviewState.DisabledQuestions,
                this.interviewState.RosterGroupInstanceIds,
                this.interviewState.ValidAnsweredQuestions,
                this.interviewState.InvalidAnsweredQuestions,
                this.wasCompleted,
                this.ExpressionProcessorStatePrototype, this.interviewerId);
        }

        public void RestoreFromSnapshot(InterviewState snapshot)
        {
            this.ExpressionProcessorStatePrototype = snapshot.ExpressionProcessorState;
            this.questionnaireId = snapshot.QuestionnaireId;
            this.questionnaireVersion = snapshot.QuestionnaireVersion;
            this.status = snapshot.Status;
            this.interviewState.AnswersSupportedInExpressions = snapshot.AnswersSupportedInExpressions;
            this.interviewState.LinkedSingleOptionAnswersBuggy = snapshot.LinkedSingleOptionAnswers;
            this.interviewState.LinkedMultipleOptionsAnswers = snapshot.LinkedMultipleOptionsAnswers;
            this.interviewState.TextListAnswers = snapshot.TextListAnswers;
            this.interviewState.AnsweredQuestions = snapshot.AnsweredQuestions;
            this.interviewState.AnswerComments = snapshot.AnswerComments;
            this.interviewState.DisabledGroups = snapshot.DisabledGroups;
            this.interviewState.DisabledQuestions = snapshot.DisabledQuestions;
            this.interviewState.RosterGroupInstanceIds = snapshot.RosterGroupInstanceIds;
            this.interviewState.ValidAnsweredQuestions = snapshot.ValidAnsweredQuestions;
            this.interviewState.InvalidAnsweredQuestions = snapshot.InvalidAnsweredQuestions;
            this.wasCompleted = snapshot.WasCompleted;
            this.wasHardDeleted = snapshot.WasHardDeleted;
            this.interviewerId = snapshot.InterviewewerId;
        }

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
        private static IQuestionnaireRepository QuestionnaireRepository
        {
            get { return ServiceLocator.Current.GetInstance<IQuestionnaireRepository>(); }
        }

        private IInterviewExpressionStatePrototypeProvider ExpressionProcessorStatePrototypeProvider
        {
            get { return ServiceLocator.Current.GetInstance<IInterviewExpressionStatePrototypeProvider>(); }
        }

        private IInterviewPreconditionsService InterviewPreconditionsService
        {
            get { return ServiceLocator.Current.GetInstance<IInterviewPreconditionsService>(); }
        }

        #endregion

        #region .ctors

        /// <remarks>Is used to restore aggregate from event stream.</remarks>
        public Interview()
        {
        }

        public Interview(Func<Guid> getQuestionnaireId, Func<long> getVersion, Guid id)
            : base(id)
        {
            this.SetQuestionnaireProperties(getQuestionnaireId(), getVersion());

            if (ExpressionProcessorStatePrototype == null)
            {
                throw new InterviewException(string.Format("Interview activation error. Code EC0002. QuestionnaireId: {0}, Questionnaire version: {1}, InterviewId: {2}", getQuestionnaireId(), getVersion(), id));
            }

        }

        private void SetQuestionnaireProperties(Guid questionnaireId, long questionnaireVersion)
        {
            this.questionnaireId = questionnaireId;
            this.questionnaireVersion = questionnaireVersion;
        }

        public Interview(Guid id, Guid userId, Guid questionnaireId, long version, PreloadedDataDto preloadedData, DateTime answersTime,
            Guid supervisorId)
            : this(() => questionnaireId, () => version, id)
        {
            this.CreateInterviewWithPreloadedData(questionnaireId, version, preloadedData, supervisorId, answersTime, userId);
        }

        public void CreateInterviewWithPreloadedData(Guid questionnaireId, long version, PreloadedDataDto preloadedData, Guid supervisorId, DateTime answersTime, Guid userId)
        {
            this.ThrowIfInterviewCountLimitReached();

            this.SetQuestionnaireProperties(questionnaireId, version);

            IQuestionnaire questionnaire = GetHistoricalQuestionnaireOrThrow(questionnaireId, version);

            var interviewChangeStructures = new InterviewChangeStructures();

            var fixedRosterCalculationDatas = this.CalculateFixedRostersData(interviewChangeStructures.State, questionnaire);

            foreach (var fixedRosterCalculationData in fixedRosterCalculationDatas)
            {
                var fixedRosterChanges = new InterviewChanges(null, null, null, fixedRosterCalculationData, null, null, null);
                interviewChangeStructures.State.ApplyInterviewChanges(fixedRosterChanges);
                interviewChangeStructures.Changes.Add(fixedRosterChanges);
            }

            var orderedData = preloadedData.Data.OrderBy(x => x.RosterVector.Length).ToArray();
            var expressionProcessorStatePrototypeLocal = this.ExpressionProcessorStatePrototype.Clone();
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

                this.CalculateChangesByFeaturedQuestion(expressionProcessorStatePrototypeLocal, interviewChangeStructures, userId, questionnaire, answersToFeaturedQuestions,
                    answersTime,
                    newAnswers, preloadedLevel.RosterVector);
            }

            var enablementAndValidityChanges = this.UpdateExpressionStateWithAnswersAndGetChanges(
                interviewChangeStructures,
                fixedRosterCalculationDatas,
                questionnaire);

            //apply events
            this.ApplyEvent(new InterviewFromPreloadedDataCreated(userId, questionnaireId, questionnaire.Version));
            this.ApplyInterviewChanges(interviewChangeStructures.Changes);
            this.ApplyInterviewChanges(enablementAndValidityChanges);
            this.ApplyEvent(new SupervisorAssigned(userId, supervisorId));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.SupervisorAssigned, comment: null));
        }

        public Interview(Guid id, Guid userId, Guid questionnaireId, long questionnaireVersion, Dictionary<Guid, object> answersToFeaturedQuestions,
            DateTime answersTime, Guid supervisorId)
            : this(() => questionnaireId, () => questionnaireVersion, id)
        {
            this.CreateInterview(questionnaireId, questionnaireVersion, supervisorId, answersToFeaturedQuestions, answersTime, userId);
        }

        public void CreateInterview(Guid questionnaireId, long questionnaireVersion, Guid supervisorId,
            Dictionary<Guid, object> answersToFeaturedQuestions, DateTime answersTime, Guid userId)
        {
            this.ThrowIfInterviewCountLimitReached();

            this.SetQuestionnaireProperties(questionnaireId, questionnaireVersion);

            IQuestionnaire questionnaire = GetHistoricalQuestionnaireOrThrow(questionnaireId, questionnaireVersion);

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

            var expressionProcessorStatePrototypeLocal = this.ExpressionProcessorStatePrototype.Clone();
            this.CalculateChangesByFeaturedQuestion(expressionProcessorStatePrototypeLocal, interviewChangeStructures, userId, questionnaire, answersToFeaturedQuestions,
                answersTime, newAnswers);

            var fixedRosterCalculationDatas = this.CalculateFixedRostersData(interviewChangeStructures.State, questionnaire);

            var enablementAndValidityChanges = this.UpdateExpressionStateWithAnswersAndGetChanges(
                interviewChangeStructures,
                fixedRosterCalculationDatas,
                questionnaire);

            //apply events
            this.ApplyEvent(new InterviewCreated(userId, questionnaireId, questionnaire.Version));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.Created, comment: null));
            this.ApplyInterviewChanges(interviewChangeStructures.Changes);
            this.ApplyRostersEvents(fixedRosterCalculationDatas.ToArray());
            this.ApplyInterviewChanges(enablementAndValidityChanges);
            this.ApplyEvent(new SupervisorAssigned(userId, supervisorId));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.SupervisorAssigned, comment: null));
        }

        public Interview(Guid id, Guid userId, Guid questionnaireId, Dictionary<Guid, object> answersToFeaturedQuestions,
            DateTime answersTime)
            : this(() => questionnaireId, () => GetQuestionnaireOrThrow(questionnaireId).Version, id)
        {
            this.CreateInterviewForTesting(questionnaireId, answersToFeaturedQuestions, answersTime, userId);
        }

        public void CreateInterviewForTesting(Guid questionnaireId, Dictionary<Guid, object> answersToFeaturedQuestions, DateTime answersTime, Guid userId)
        {
            this.ThrowIfInterviewCountLimitReached();

            this.SetQuestionnaireProperties(questionnaireId, GetQuestionnaireOrThrow(questionnaireId).Version);

            IQuestionnaire questionnaire = GetQuestionnaireOrThrow(questionnaireId);

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

            var expressionProcessorStatePrototypeLocal = this.ExpressionProcessorStatePrototype.Clone();
            this.CalculateChangesByFeaturedQuestion(expressionProcessorStatePrototypeLocal, interviewChangeStructures, userId, questionnaire, answersToFeaturedQuestions,
                answersTime, newAnswers);
            var fixedRosterCalculationDatas = this.CalculateFixedRostersData(interviewChangeStructures.State, questionnaire);

            var enablementAndValidityChanges = this.UpdateExpressionStateWithAnswersAndGetChanges(
                interviewChangeStructures,
                fixedRosterCalculationDatas,
                questionnaire);

            //apply events
            this.ApplyEvent(new InterviewForTestingCreated(userId, questionnaireId, questionnaire.Version));

            this.ApplyInterviewChanges(interviewChangeStructures.Changes);
            this.ApplyRostersEvents(fixedRosterCalculationDatas.ToArray());
            this.ApplyInterviewChanges(enablementAndValidityChanges);
        }

        public Interview(Guid id, Guid userId, Guid questionnaireId, long? questionnaireVersion, DateTime answersTime, Guid supervisorId)
            : this(() => questionnaireId, () =>
            {
                IQuestionnaire questionnaire = questionnaireVersion.HasValue
                    ? GetHistoricalQuestionnaireOrThrow(questionnaireId, questionnaireVersion.Value)
                    : GetQuestionnaireOrThrow(questionnaireId);

                return questionnaire.Version;
            }, id)
        {
            this.CreateInterviewOnClient(questionnaireId, questionnaireVersion, supervisorId, answersTime, userId);
        }

        public void CreateInterviewOnClient(Guid questionnaireId, long? questionnaireVersion, Guid supervisorId, DateTime answersTime, Guid userId)
        {
            this.ThrowIfInterviewCountLimitReached();

            this.SetQuestionnaireProperties(questionnaireId, (questionnaireVersion.HasValue
                    ? GetHistoricalQuestionnaireOrThrow(questionnaireId, questionnaireVersion.Value)
                    : GetQuestionnaireOrThrow(questionnaireId)).Version);

            IQuestionnaire questionnaire = questionnaireVersion.HasValue
                ? GetHistoricalQuestionnaireOrThrow(questionnaireId, questionnaireVersion.Value)
                : GetQuestionnaireOrThrow(questionnaireId);

            InterviewChangeStructures interviewChangeStructures = new InterviewChangeStructures();

            var fixedRosterCalculationDatas = this.CalculateFixedRostersData(interviewChangeStructures.State, questionnaire);

            var enablementAndValidityChanges = this.UpdateExpressionStateWithAnswersAndGetChanges(
                interviewChangeStructures,
                fixedRosterCalculationDatas,
                questionnaire);

            //apply events
            this.ApplyEvent(new InterviewOnClientCreated(userId, questionnaireId, questionnaire.Version));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.Created, comment: null));

            this.ApplyInterviewChanges(interviewChangeStructures.Changes);
            this.ApplyRostersEvents(fixedRosterCalculationDatas.ToArray());
            this.ApplyInterviewChanges(enablementAndValidityChanges);
            this.ApplyEvent(new SupervisorAssigned(userId, supervisorId));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.SupervisorAssigned, comment: null));

            this.ApplyEvent(new InterviewerAssigned(userId, userId, answersTime));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.InterviewerAssigned, comment: null));
        }

        public Interview(Guid id, Guid userId, Guid questionnaireId, long questionnaireVersion,
            InterviewStatus interviewStatus, AnsweredQuestionSynchronizationDto[] featuredQuestionsMeta, bool isValid)
            : this(() => questionnaireId, () => questionnaireVersion, id)
        {
            this.CreateInterviewCreatedOnClient(questionnaireId, questionnaireVersion, interviewStatus, featuredQuestionsMeta, isValid, userId);
        }

        public void CreateInterviewCreatedOnClient(Guid questionnaireId, long questionnaireVersion,
            InterviewStatus interviewStatus,
            AnsweredQuestionSynchronizationDto[] featuredQuestionsMeta, bool isValid, Guid userId)
        {
            this.ThrowIfInterviewCountLimitReached();

            this.SetQuestionnaireProperties(questionnaireId, questionnaireVersion);

            GetHistoricalQuestionnaireOrThrow(questionnaireId, questionnaireVersion);
            this.ApplyEvent(new InterviewOnClientCreated(userId, questionnaireId, questionnaireVersion));
            this.ApplyEvent(new SynchronizationMetadataApplied(userId, questionnaireId, questionnaireVersion,
                interviewStatus, featuredQuestionsMeta, true, null));
            this.ApplyEvent(new InterviewStatusChanged(interviewStatus, string.Empty));
            this.ApplyValidationEvent(isValid);
        }

        public Interview(Guid id, Guid userId, Guid questionnaireId, long questionnaireVersion, InterviewStatus interviewStatus,
            AnsweredQuestionSynchronizationDto[] featuredQuestionsMeta, string comments, bool valid, bool createdOnClient)
            : this(() => questionnaireId, () => questionnaireVersion, id)
        {
            this.ThrowIfInterviewCountLimitReached();

            this.ApplySynchronizationMetadata(id, userId, questionnaireId, questionnaireVersion, interviewStatus, featuredQuestionsMeta, comments, valid, createdOnClient);
        }

        public Interview(Guid id, Guid userId, Guid supervisorId, InterviewSynchronizationDto interviewDto, DateTime synchronizationTime)
            : this(() => interviewDto.QuestionnaireId, () => interviewDto.QuestionnaireVersion, id)
        {
            this.ThrowIfInterviewCountLimitReached();

            this.SynchronizeInterviewFromHeadquarters(id, userId, supervisorId, interviewDto, synchronizationTime);
        }

        #endregion

        #region StaticMethods

        private static Dictionary<string, DistinctDecimalList> BuildRosterInstanceIdsFromSynchronizationDto(InterviewSynchronizationDto synchronizationDto)
        {
            return synchronizationDto.RosterGroupInstances.ToDictionary(
                pair => ConversionHelper.ConvertIdAndRosterVectorToString(pair.Key.Id, pair.Key.InterviewItemPropagationVector),
                pair => new DistinctDecimalList(pair.Value.Select(rosterInstance => rosterInstance.RosterInstanceId).ToList()));
        }

        private string GetLinkedQuestionAnswerFormattedAsRosterTitle(InterviewStateDependentOnAnswers state, Identity linkedQuestion, IQuestionnaire questionnaire)
        {
            // set of answers that support expressions includes set of answers that may be linked to, so following line is correct
            object answer = GetEnabledQuestionAnswerSupportedInExpressions(state, linkedQuestion, questionnaire);

            return AnswerUtils.AnswerToString(answer);
        }

        private static Events.Interview.Dtos.Identity[] ToEventIdentities(IEnumerable<Identity> answersDeclaredValid)
        {
            return answersDeclaredValid.Select(Events.Interview.Dtos.Identity.ToEventIdentity).ToArray();
        }

        private bool IsQuestionOrParentGroupDisabled(Identity question, IQuestionnaire questionnaire, Func<Identity, bool> isGroupDisabled, Func<Identity, bool> isQuestionDisabled)
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

        private object GetEnabledQuestionAnswerSupportedInExpressions(InterviewStateDependentOnAnswers state, Identity question, IQuestionnaire questionnaire)
        {
            return GetEnabledQuestionAnswerSupportedInExpressions(state, question, IsQuestionDisabled, IsGroupDisabled, questionnaire);
        }

        private object GetEnabledQuestionAnswerSupportedInExpressions(InterviewStateDependentOnAnswers state,
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

        private decimal[] ShrinkRosterVector(decimal[] rosterVector, int length)
        {
            if (length == 0)
                return EmptyRosterVector;

            if (length == rosterVector.Length)
                return rosterVector;

            if (length > rosterVector.Length)
                throw new ArgumentException(string.Format("Cannot shrink vector with length {0} to bigger length {1}. InterviewId: {2}.", rosterVector.Length,
                    length, EventSourceId));

            return rosterVector.Take(length).ToArray();
        }

        /// <remarks>
        /// If roster vector should be extended, result will be a set of vectors depending on roster count of corresponding groups.
        /// </remarks>
        private IEnumerable<decimal[]> ExtendRosterVector(InterviewStateDependentOnAnswers state, decimal[] rosterVector, int length,
            Guid[] rosterGroupsStartingFromTop,
            Func<InterviewStateDependentOnAnswers, Guid, decimal[], DistinctDecimalList> getRosterInstanceIds)
        {
            if (length < rosterVector.Length)
                throw new ArgumentException(string.Format(
                    "Cannot extend vector with length {0} to smaller length {1}. InterviewId: {2}", rosterVector.Length, length, EventSourceId));

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

        private Identity GetInstanceOfQuestionWithSameAndUpperRosterLevelOrThrow(Guid questionId,
            decimal[] rosterVector, IQuestionnaire questionnare)
        {
            int vectorRosterLevel = rosterVector.Length;
            int questionRosterLevel = questionnare.GetRosterLevelForQuestion(questionId);

            if (questionRosterLevel > vectorRosterLevel)
                throw new InterviewException(string.Format(
                    "Question {0} expected to have roster level not deeper than {1} but it is {2}. InterviewId: {3}",
                    FormatQuestionForException(questionId, questionnare), vectorRosterLevel, questionRosterLevel, EventSourceId));

            decimal[] questionRosterVector = ShrinkRosterVector(rosterVector, questionRosterLevel);

            return new Identity(questionId, questionRosterVector);
        }

        private IEnumerable<Identity> GetInstancesOfQuestionsWithSameAndDeeperRosterLevelOrThrow(
            InterviewStateDependentOnAnswers state,
            IEnumerable<Guid> questionIds, decimal[] rosterVector, IQuestionnaire questionnare,
            Func<InterviewStateDependentOnAnswers, Guid, decimal[], DistinctDecimalList> getRosterInstanceIds)
        {
            return questionIds.SelectMany(questionId =>
                GetInstancesOfQuestionsWithSameAndDeeperRosterLevelOrThrow(state, questionId, rosterVector, questionnare,
                    getRosterInstanceIds));
        }

        private IEnumerable<Identity> GetInstancesOfQuestionsWithSameAndDeeperRosterLevelOrThrow(
            InterviewStateDependentOnAnswers state,
            Guid questionId, decimal[] rosterVector, IQuestionnaire questionnare,
            Func<InterviewStateDependentOnAnswers, Guid, decimal[], DistinctDecimalList> getRosterInstanceIds)
        {
            int vectorRosterLevel = rosterVector.Length;
            int questionRosterLevel = questionnare.GetRosterLevelForQuestion(questionId);

            if (questionRosterLevel < vectorRosterLevel)
                throw new InterviewException(string.Format(
                    "Question {0} expected to have roster level not upper than {1} but it is {2}. InterviewId: {3}",
                    FormatQuestionForException(questionId, questionnare), vectorRosterLevel, questionRosterLevel, EventSourceId));

            Guid[] parentRosterGroupsStartingFromTop =
                questionnare.GetRostersFromTopToSpecifiedQuestion(questionId).ToArray();

            IEnumerable<decimal[]> questionRosterVectors = ExtendRosterVector(state,
                rosterVector, questionRosterLevel, parentRosterGroupsStartingFromTop, getRosterInstanceIds);

            foreach (decimal[] questionRosterVector in questionRosterVectors)
            {
                yield return new Identity(questionId, questionRosterVector);
            }
        }

        private IEnumerable<Identity> GetInstancesOfGroupsWithSameAndDeeperRosterLevelOrThrow(InterviewStateDependentOnAnswers state,
            IEnumerable<Guid> groupIds, 
            decimal[] rosterVector, 
            IQuestionnaire questionnaire, 
            Func<InterviewStateDependentOnAnswers, Guid, decimal[], DistinctDecimalList> getRosterInstanceIds)
        {
            return groupIds.SelectMany(groupId =>
                GetInstancesOfGroupsByGroupIdWithSameAndDeeperRosterLevelOrThrow(state, groupId, rosterVector, questionnaire, getRosterInstanceIds));
        }

        private IEnumerable<Identity> GetInstancesOfGroupsByGroupIdWithSameAndDeeperRosterLevelOrThrow(InterviewStateDependentOnAnswers state,
            Guid groupId, decimal[] rosterVector, IQuestionnaire questionnaire,
            Func<InterviewStateDependentOnAnswers, Guid, decimal[], DistinctDecimalList> getRosterInstanceIds)
        {
            int vectorRosterLevel = rosterVector.Length;
            int groupRosterLevel = questionnaire.GetRosterLevelForGroup(groupId);

            if (groupRosterLevel < vectorRosterLevel)
                throw new InterviewException(string.Format(
                    "Question {0} expected to have roster level not upper than {1} but it is {2}. InterviewId: {3}",
                    FormatQuestionForException(groupId, questionnaire), vectorRosterLevel, groupRosterLevel, EventSourceId));

            Guid[] parentRosterGroupsStartingFromTop = questionnaire.GetRostersFromTopToSpecifiedGroup(groupId).ToArray();

            IEnumerable<decimal[]> groupRosterVectors = ExtendRosterVector(state,
                rosterVector, groupRosterLevel, parentRosterGroupsStartingFromTop, getRosterInstanceIds);

            return groupRosterVectors.Select(groupRosterVector => new Identity(groupId, groupRosterVector));
        }

        private IEnumerable<Identity> GetInstancesOfQuestionsInAllRosterLevels(
            InterviewStateDependentOnAnswers state,
            Guid questionId, decimal[] rosterVector, IQuestionnaire questionnaire,
            Func<InterviewStateDependentOnAnswers, Guid, decimal[], DistinctDecimalList> getRosterInstanceIds)
        {
            int vectorRosterLevel = rosterVector.Length;
            int questionRosterLevel = questionnaire.GetRosterLevelForQuestion(questionId);

            if (questionRosterLevel > vectorRosterLevel)
            {
                Guid[] parentRosterGroupsStartingFromTop =
                    questionnaire.GetRostersFromTopToSpecifiedQuestion(questionId).ToArray();

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

        private IEnumerable<Identity> GetInstancesOfGroupsWithSameAndUpperRosterLevelOrThrow(
            IEnumerable<Guid> groupIds, decimal[] rosterVector, IQuestionnaire questionnaire)
        {
            int vectorRosterLevel = rosterVector.Length;

            foreach (Guid groupId in groupIds)
            {
                int groupRosterLevel = questionnaire.GetRosterLevelForGroup(groupId);

                if (groupRosterLevel > vectorRosterLevel)
                    throw new InterviewException(string.Format(
                        "Group {0} expected to have roster level not deeper than {1} but it is {2}. InterviewId: {3}",
                        FormatGroupForException(groupId, questionnaire), vectorRosterLevel, groupRosterLevel, EventSourceId));

                decimal[] groupRosterVector = ShrinkRosterVector(rosterVector, groupRosterLevel);

                yield return new Identity(groupId, groupRosterVector);
            }
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
            ThrowIfInterviewHardDeleted();
            var answeredQuestion = new Identity(questionId, rosterVector);
            IQuestionnaire questionnaire = GetHistoricalQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);

            this.CheckTextQuestionInvariants(questionId, rosterVector, questionnaire, answeredQuestion, this.interviewState);

            var expressionProcessorState = this.PrepareExpressionProcessorStateForCalculations();

            InterviewChanges interviewChanges = this.CalculateInterviewChangesOnAnswerTextQuestion(expressionProcessorState, this.interviewState, userId, questionId,
                rosterVector, answerTime, answer, questionnaire);

            this.ApplyInterviewChanges(interviewChanges);
        }

        public void AnswerNumericRealQuestion(Guid userId, Guid questionId, decimal[] rosterVector, DateTime answerTime, decimal answer)
        {
            ThrowIfInterviewHardDeleted();

            var answeredQuestion = new Identity(questionId, rosterVector);

            IQuestionnaire questionnaire = GetHistoricalQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);

            this.CheckNumericRealQuestionInvariants(questionId, rosterVector, answer, questionnaire, answeredQuestion, this.interviewState);

            var expressionProcessorState = this.PrepareExpressionProcessorStateForCalculations();

            InterviewChanges interviewChanges = this.CalculateInterviewChangesOnAnswerNumericRealQuestion(expressionProcessorState, this.interviewState, userId,
                questionId, rosterVector, answerTime, answer, questionnaire);

            this.ApplyInterviewChanges(interviewChanges);
        }

        public void AnswerQRBarcodeQuestion(Guid userId, Guid questionId, decimal[] rosterVector, DateTime answerTime, string answer)
        {
            ThrowIfInterviewHardDeleted();

            var answeredQuestion = new Identity(questionId, rosterVector);

            IQuestionnaire questionnaire = GetHistoricalQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);
            ThrowIfQuestionDoesNotExist(questionId, questionnaire);
            this.ThrowIfRosterVectorIsIncorrect(this.interviewState, questionId, rosterVector, questionnaire);
            this.ThrowIfQuestionTypeIsNotOneOfExpected(questionId, questionnaire, QuestionType.QRBarcode);
            ThrowIfQuestionOrParentGroupIsDisabled(this.interviewState, answeredQuestion, questionnaire);

            var expressionProcessorState = this.PrepareExpressionProcessorStateForCalculations();

            InterviewChanges interviewChanges = this.CalculateInterviewChangesOnAnswerQRBarcodeQuestion(expressionProcessorState, this.interviewState, userId,
                questionId, rosterVector, answerTime, answer, questionnaire);

            this.ApplyInterviewChanges(interviewChanges);
        }

        public void AnswerPictureQuestion(Guid userId, Guid questionId, decimal[] rosterVector, DateTime answerTime, string pictureFileName)
        {
            ThrowIfInterviewHardDeleted();

            var answeredQuestion = new Identity(questionId, rosterVector);

            IQuestionnaire questionnaire = GetHistoricalQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);
            ThrowIfQuestionDoesNotExist(questionId, questionnaire);
            this.ThrowIfRosterVectorIsIncorrect(this.interviewState, questionId, rosterVector, questionnaire);
            this.ThrowIfQuestionTypeIsNotOneOfExpected(questionId, questionnaire, QuestionType.Multimedia);
            ThrowIfQuestionOrParentGroupIsDisabled(this.interviewState, answeredQuestion, questionnaire);

            var expressionProcessorState = this.ExpressionProcessorStatePrototype.Clone();

            this.CalculateInterviewChangesOnAnswerPictureQuestion(expressionProcessorState, userId, questionId, rosterVector, answerTime, pictureFileName, questionnaire);
        }

        public void AnswerNumericIntegerQuestion(Guid userId, Guid questionId, decimal[] rosterVector, DateTime answerTime, int answer)
        {
            ThrowIfInterviewHardDeleted();

            var answeredQuestion = new Identity(questionId, rosterVector);
            IQuestionnaire questionnaire = GetHistoricalQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);
            this.CheckNumericIntegerQuestionInvariants(questionId, rosterVector, answer, questionnaire, answeredQuestion,
                this.interviewState);

            Func<InterviewStateDependentOnAnswers, Identity, object> getAnswer =
                (currentState, question) => AreEqual(question, answeredQuestion)
                    ? answer
                    : GetEnabledQuestionAnswerSupportedInExpressions(this.interviewState, question, questionnaire);

            var expressionProcessorState = this.ExpressionProcessorStatePrototype.Clone();

            InterviewChanges interviewChanges = this.CalculateInterviewChangesOnAnswerNumericIntegerQuestion(expressionProcessorState, this.interviewState, userId,
                questionId, rosterVector, answerTime, answer, getAnswer, questionnaire);

            this.ApplyInterviewChanges(interviewChanges);
        }

        public void AnswerMultipleOptionsQuestion(Guid userId, Guid questionId, decimal[] rosterVector, DateTime answerTime, decimal[] selectedValues)
        {
            ThrowIfInterviewHardDeleted();

            var answeredQuestion = new Identity(questionId, rosterVector);

            IQuestionnaire questionnaire = GetHistoricalQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);
            this.CheckMultipleOptionQuestionInvariants(questionId, rosterVector, selectedValues, questionnaire, answeredQuestion,
                this.interviewState);

            Func<InterviewStateDependentOnAnswers, Identity, object> getAnswer =
                (currentState, question) => AreEqual(question, answeredQuestion)
                    ? selectedValues
                    : GetEnabledQuestionAnswerSupportedInExpressions(this.interviewState, question, questionnaire);

            var expressionProcessorState = this.ExpressionProcessorStatePrototype.Clone();

            InterviewChanges interviewChanges = this.CalculateInterviewChangesOnAnswerMultipleOptionsQuestion(expressionProcessorState, this.interviewState, userId,
                questionId, rosterVector, answerTime, selectedValues, getAnswer, questionnaire);

            this.ApplyInterviewChanges(interviewChanges);
        }

        public void AnswerMultipleOptionsLinkedQuestion(Guid userId, Guid questionId, decimal[] rosterVector, DateTime answerTime, decimal[][] selectedPropagationVectors)
        {
            ThrowIfInterviewHardDeleted();

            var answeredQuestion = new Identity(questionId, rosterVector);

            IQuestionnaire questionnaire = GetHistoricalQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);

            this.CheckLinkedMultiOptionQuestionInvariants(questionId, rosterVector, selectedPropagationVectors, questionnaire, answeredQuestion);

            Guid linkedQuestionId = this.GetLinkedQuestionIdOrThrow(questionId, questionnaire);
            var answeredLinkedQuestions = selectedPropagationVectors.Select(selectedRosterVector => new Identity(linkedQuestionId, selectedRosterVector));

            string answerFormattedAsRosterTitle = string.Join(", ", answeredLinkedQuestions.Select(q => GetLinkedQuestionAnswerFormattedAsRosterTitle(this.interviewState, q, questionnaire)));

            IInterviewExpressionStateV2 expressionProcessorState = this.ExpressionProcessorStatePrototype.Clone();

            expressionProcessorState.UpdateLinkedMultiOptionAnswer(questionId, rosterVector, selectedPropagationVectors);

            InterviewChanges interviewChanges = this.CalculateInterviewChangesOnAnswerQuestion(this.interviewState, userId, questionId, rosterVector, selectedPropagationVectors, answerFormattedAsRosterTitle, AnswerChangeType.MultipleOptionsLinked, answerTime, questionnaire, expressionProcessorState);


            this.ApplyInterviewChanges(interviewChanges);
        }

        public void AnswerDateTimeQuestion(Guid userId, Guid questionId, decimal[] rosterVector, DateTime answerTime, DateTime answer)
        {
            ThrowIfInterviewHardDeleted();
            var answeredQuestion = new Identity(questionId, rosterVector);

            IQuestionnaire questionnaire = GetHistoricalQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);
            this.CheckDateTimeQuestionInvariants(questionId, rosterVector, questionnaire, answeredQuestion, this.interviewState);

            var expressionProcessorState = this.PrepareExpressionProcessorStateForCalculations();

            InterviewChanges interviewChanges = this.CalculateInterviewChangesOnAnswerDateTimeQuestion(expressionProcessorState, this.interviewState, userId,
                questionId, rosterVector, answerTime, answer, questionnaire);

            this.ApplyInterviewChanges(interviewChanges);
        }

        public void AnswerSingleOptionQuestion(Guid userId, Guid questionId, decimal[] rosterVector, DateTime answerTime, decimal selectedValue)
        {
            ThrowIfInterviewHardDeleted();
            var answeredQuestion = new Identity(questionId, rosterVector);

            IQuestionnaire questionnaire = GetHistoricalQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);
            CheckSingleOptionQuestionInvariants(questionId, rosterVector, selectedValue, questionnaire, answeredQuestion,
                this.interviewState);

            var expressionProcessorState = this.PrepareExpressionProcessorStateForCalculations();

            InterviewChanges interviewChanges = this.CalculateInterviewChangesOnAnswerSingleOptionQuestion(expressionProcessorState, this.interviewState, userId,
                questionId, rosterVector, answerTime, selectedValue, questionnaire);

            this.ApplyInterviewChanges(interviewChanges);
        }

        public void AnswerTextListQuestion(Guid userId, Guid questionId, decimal[] rosterVector, DateTime answerTime,
            Tuple<decimal, string>[] answers)
        {
            ThrowIfInterviewHardDeleted();
            var answeredQuestion = new Identity(questionId, rosterVector);

            IQuestionnaire questionnaire = GetHistoricalQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);

            CheckTextListInvariants(questionId, rosterVector, questionnaire, answeredQuestion, this.interviewState, answers);

            Func<InterviewStateDependentOnAnswers, Identity, object> getAnswer = (currentState, question) => AreEqual(question, answeredQuestion) ?
                answers :
                GetEnabledQuestionAnswerSupportedInExpressions(this.interviewState, question, questionnaire);

            var expressionProcessorState = this.ExpressionProcessorStatePrototype.Clone();

            InterviewChanges interviewChanges = this.CalculateInterviewChangesOnAnswerTextListQuestion(expressionProcessorState, userId, questionId,
                rosterVector, answerTime, answers, getAnswer, questionnaire);

            this.ApplyInterviewChanges(interviewChanges);
        }

        public void AnswerGeoLocationQuestion(Guid userId, Guid questionId, decimal[] rosterVector, DateTime answerTime, double latitude, double longitude,
            double accuracy, double altitude, DateTimeOffset timestamp)
        {
            ThrowIfInterviewHardDeleted();
            var answeredQuestion = new Identity(questionId, rosterVector);

            IQuestionnaire questionnaire = GetHistoricalQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);
            CheckGpsCoordinatesInvariants(questionId, rosterVector, questionnaire, answeredQuestion, this.interviewState);

            var expressionProcessorState = this.ExpressionProcessorStatePrototype.Clone();

            InterviewChanges interviewChanges = CalculateInterviewChangesOnAnswerGeoLocationQuestion(expressionProcessorState, this.interviewState, userId, questionId,
                rosterVector, answerTime, latitude, longitude, accuracy, altitude, timestamp, answeredQuestion, questionnaire);

            this.ApplyInterviewChanges(interviewChanges);
        }

        private void CalculateInterviewChangesOnAnswerPictureQuestion(IInterviewExpressionStateV2 expressionProcessorState, Guid userId, Guid questionId, decimal[] rosterVector,
            DateTime answerTime, string pictureFileName, IQuestionnaire questionnaire)
        {
            expressionProcessorState.UpdateMediaAnswer(questionId, rosterVector, pictureFileName);

            InterviewChanges interviewChanges = this.CalculateInterviewChangesOnAnswerQuestion(this.interviewState,
                userId, questionId, rosterVector, pictureFileName, pictureFileName, AnswerChangeType.Picture, answerTime,
                questionnaire, expressionProcessorState);
            
            this.ApplyInterviewChanges(interviewChanges);
        }

        public void AnswerSingleOptionLinkedQuestion(Guid userId, Guid questionId, decimal[] rosterVector, DateTime answerTime, decimal[] selectedPropagationVector)
        {
            ThrowIfInterviewHardDeleted();
            var answeredQuestion = new Identity(questionId, rosterVector);

            IQuestionnaire questionnaire = GetHistoricalQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);

            this.CheckLinkedSingleOptionQuestionInvariants(questionId, rosterVector, selectedPropagationVector, questionnaire, answeredQuestion);

            Guid linkedQuestionId = this.GetLinkedQuestionIdOrThrow(questionId, questionnaire);
            var answeredLinkedQuestion = new Identity(linkedQuestionId, selectedPropagationVector);

            string answerFormattedAsRosterTitle = GetLinkedQuestionAnswerFormattedAsRosterTitle(this.interviewState, answeredLinkedQuestion, questionnaire);

            IInterviewExpressionStateV2 expressionProcessorState = this.ExpressionProcessorStatePrototype.Clone();

            expressionProcessorState.UpdateLinkedSingleOptionAnswer(questionId, rosterVector, selectedPropagationVector);

            InterviewChanges interviewChanges = this.CalculateInterviewChangesOnAnswerQuestion(this.interviewState,
                userId, questionId, rosterVector, selectedPropagationVector, answerFormattedAsRosterTitle,
                AnswerChangeType.SingleOptionLinked, answerTime, questionnaire, expressionProcessorState);

            this.ApplyInterviewChanges(interviewChanges);
        }

        #endregion

        public void ReevaluateSynchronizedInterview()
        {
            ThrowIfInterviewHardDeleted();

            var expressionProcessorState = this.ExpressionProcessorStatePrototype.Clone();

            expressionProcessorState.SaveAllCurrentStatesAsPrevious();
            EnablementChanges enablementChanges = expressionProcessorState.ProcessEnablementConditions();
            ValidityChanges validationChanges = expressionProcessorState.ProcessValidationExpressions();

            this.ApplyEnablementChangesEvents(enablementChanges);

            this.ApplyValidityChangesEvents(validationChanges);

            if (!this.HasInvalidAnswers())
            {
                this.ApplyEvent(new InterviewDeclaredValid());
            }
        }

        public void RepeatLastInterviewStatus(RepeatLastInterviewStatus command)
        {
            this.ApplyEvent(new InterviewStatusChanged(this.status, command.Comment));
        }

        public void CommentAnswer(Guid userId, Guid questionId, decimal[] rosterVector, DateTime commentTime, string comment)
        {
            ThrowIfInterviewHardDeleted();
            IQuestionnaire questionnaire = GetHistoricalQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);
            ThrowIfQuestionDoesNotExist(questionId, questionnaire);
            this.ThrowIfRosterVectorIsIncorrect(this.interviewState, questionId, rosterVector, questionnaire);

            this.ApplyEvent(new AnswerCommented(userId, questionId, rosterVector, commentTime, comment));
        }

        public void SetFlagToAnswer(Guid userId, Guid questionId, decimal[] rosterVector)
        {
            ThrowIfInterviewHardDeleted();
            IQuestionnaire questionnaire = GetHistoricalQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);
            ThrowIfQuestionDoesNotExist(questionId, questionnaire);
            this.ThrowIfRosterVectorIsIncorrect(this.interviewState, questionId, rosterVector, questionnaire);

            this.ApplyEvent(new FlagSetToAnswer(userId, questionId, rosterVector));
        }

        public void RemoveFlagFromAnswer(Guid userId, Guid questionId, decimal[] rosterVector)
        {
            ThrowIfInterviewHardDeleted();
            IQuestionnaire questionnaire = GetHistoricalQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);
            ThrowIfQuestionDoesNotExist(questionId, questionnaire);
            this.ThrowIfRosterVectorIsIncorrect(this.interviewState, questionId, rosterVector, questionnaire);

            this.ApplyEvent(new FlagRemovedFromAnswer(userId, questionId, rosterVector));
        }

        public void AssignSupervisor(Guid userId, Guid supervisorId)
        {
            ThrowIfInterviewHardDeleted();
            this.ThrowIfInterviewStatusIsNotOneOfExpected(InterviewStatus.Created, InterviewStatus.SupervisorAssigned);

            this.ApplyEvent(new SupervisorAssigned(userId, supervisorId));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.SupervisorAssigned, comment: null));
        }

        public void AssignInterviewer(Guid userId, Guid interviewerId, DateTime assignTime)
        {
            ThrowIfInterviewHardDeleted();
            this.ThrowIfInterviewStatusIsNotOneOfExpected(
                InterviewStatus.SupervisorAssigned, InterviewStatus.InterviewerAssigned, InterviewStatus.RejectedBySupervisor);

            this.ApplyEvent(new InterviewerAssigned(userId, interviewerId, assignTime));
            if (!this.wasCompleted && this.status != InterviewStatus.InterviewerAssigned)
            {
                this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.InterviewerAssigned, comment: null));
            }
        }

        public void Delete(Guid userId)
        {
            ThrowIfInterviewHardDeleted();
            this.ThrowIfInterviewWasCompleted();
            this.ThrowIfInterviewStatusIsNotOneOfExpected(
                InterviewStatus.Created, InterviewStatus.SupervisorAssigned, InterviewStatus.InterviewerAssigned, InterviewStatus.Restored);

            this.ApplyEvent(new InterviewDeleted(userId));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.Deleted, comment: null));
        }

        public void HardDelete(Guid userId)
        {
            if (this.wasHardDeleted)
                return;

            this.ApplyEvent(new InterviewHardDeleted(userId));
        }

        public void CancelByHQSynchronization(Guid userId)
        {
            ThrowIfInterviewHardDeleted();

            if (status != InterviewStatus.Completed)
            {
                this.ApplyEvent(new InterviewDeleted(userId));
                this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.Deleted, comment: null));
            }
        }

        public void MarkInterviewAsSentToHeadquarters(Guid userId)
        {
            if (!this.wasHardDeleted)
            {
                this.ApplyEvent(new InterviewDeleted(userId));
                this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.Deleted, comment: null));
            }
            this.ApplyEvent(new InterviewSentToHeadquarters());
        }

        public void Restore(Guid userId)
        {
            ThrowIfInterviewHardDeleted();
            this.ThrowIfInterviewStatusIsNotOneOfExpected(InterviewStatus.Deleted);

            this.ApplyEvent(new InterviewRestored(userId));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.Restored, comment: null));
        }

        public void Complete(Guid userId, string comment, DateTime completeTime)
        {
            ThrowIfInterviewHardDeleted();
            this.ThrowIfInterviewStatusIsNotOneOfExpected(
                InterviewStatus.InterviewerAssigned, InterviewStatus.Restarted, InterviewStatus.RejectedBySupervisor);

            /*IQuestionnaire questionnaire = this.GetHistoricalQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);*/
            bool isInterviewInvalid = this.HasInvalidAnswers() /*|| this.HasNotAnsweredMandatoryQuestions(questionnaire)*/;

            this.ApplyEvent(new InterviewCompleted(userId, completeTime, comment));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.Completed, comment));

            this.ApplyEvent(isInterviewInvalid
                ? new InterviewDeclaredInvalid() as object
                : new InterviewDeclaredValid() as object);
        }

        public void Restart(Guid userId, string comment, DateTime restartTime)
        {
            ThrowIfInterviewHardDeleted();
            this.ThrowIfInterviewStatusIsNotOneOfExpected(InterviewStatus.Completed);

            this.ApplyEvent(new InterviewRestarted(userId, restartTime, comment));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.Restarted, comment));
        }

        public void Approve(Guid userId, string comment, DateTime approveTime)
        {
            ThrowIfInterviewHardDeleted();
            this.ThrowIfInterviewStatusIsNotOneOfExpected(InterviewStatus.Completed,
                InterviewStatus.RejectedBySupervisor,
                InterviewStatus.RejectedByHeadquarters);

            this.ApplyEvent(new InterviewApproved(userId, comment, approveTime));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.ApprovedBySupervisor, comment));
        }

        public void Reject(Guid userId, string comment, DateTime rejectTime)
        {
            ThrowIfInterviewHardDeleted();
            this.ThrowIfInterviewStatusIsNotOneOfExpected(InterviewStatus.Completed,
                InterviewStatus.ApprovedBySupervisor,
                InterviewStatus.RejectedByHeadquarters);

            this.ApplyEvent(new InterviewRejected(userId, comment, rejectTime));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.RejectedBySupervisor, comment));
        }

        public void HqApprove(Guid userId, string comment)
        {
            ThrowIfInterviewHardDeleted();
            this.ThrowIfInterviewStatusIsNotOneOfExpected(InterviewStatus.ApprovedBySupervisor, InterviewStatus.RejectedByHeadquarters);

            this.ApplyEvent(new InterviewApprovedByHQ(userId, comment));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.ApprovedByHeadquarters, comment));
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
                this.ApplyEvent(new InterviewerAssigned(userId, interviewerId.Value, synchronizationTime));
            }

            foreach (var commentedAnswer in commentedAnswers)
            {
                this.ApplyEvent(new AnswerCommented(commentedAnswer.UserId, commentedAnswer.QuestionId, commentedAnswer.RosterVector, commentedAnswer.Date, commentedAnswer.Text));
            }
        }

        public void HqReject(Guid userId, string comment)
        {
            ThrowIfInterviewHardDeleted();
            this.ThrowIfInterviewStatusIsNotOneOfExpected(InterviewStatus.ApprovedBySupervisor, InterviewStatus.Deleted);

            this.ApplyEvent(new InterviewRejectedByHQ(userId, comment));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.RejectedByHeadquarters, comment));
        }

        public void SynchronizeInterview(Guid userId, InterviewSynchronizationDto synchronizedInterview)
        {
            ThrowIfInterviewHardDeleted();
            this.ApplyEvent(new InterviewSynchronized(synchronizedInterview));
        }

        public void SynchronizeInterviewFromHeadquarters(Guid id, Guid userId, Guid supervisorId, InterviewSynchronizationDto interviewDto, DateTime synchronizationTime)
        {
            this.SetQuestionnaireProperties(interviewDto.QuestionnaireId, interviewDto.QuestionnaireVersion);

            ThrowIfInterviewHardDeleted();
            IQuestionnaire questionnaire = GetHistoricalQuestionnaireOrThrow(interviewDto.QuestionnaireId,
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
                    case QuestionType.Multimedia:
                        this.ApplyEvent(new PictureQuestionAnswered(userId, questionId, rosterVector, synchronizationTime, (string)answer));
                        break;
                    default:
                        throw new InterviewException(string.Format("Question {0} has unknown type {1}. InterviewId: {2}",
                            FormatQuestionForException(questionId, questionnaire), questionType, EventSourceId));
                }
            }

            this.ApplyEnablementChangesEvents(enablementChanges);

            this.ApplyValidityChangesEvents(validityChanges);
        }

        public void SynchronizeInterviewEvents(Guid userId, Guid questionnaireId, long questionnaireVersion,
            InterviewStatus interviewStatus, object[] synchronizedEvents, bool createdOnClient)
        {
            ThrowIfOtherUserIsResponsible(userId);

            SetQuestionnaireProperties(questionnaireId, questionnaireVersion);

            GetHistoricalQuestionnaireOrThrow(questionnaireId, questionnaireVersion);

            var isInterviewNeedToBeCreated = createdOnClient && this.Version == 0;

            if (isInterviewNeedToBeCreated)
            {
                ThrowIfInterviewCountLimitReached();
                this.ApplyEvent(new InterviewOnClientCreated(userId, questionnaireId, questionnaireVersion));
            }
            else
            {
                if (this.status == InterviewStatus.Deleted)
                    this.Restore(userId);
                else
                    this.ThrowIfStatusNotAllowedToBeChangedWithMetadata(interviewStatus);
            }

            foreach (var synchronizedEvent in synchronizedEvents)
            {
                this.ApplyEvent(synchronizedEvent);
            }
        }

        private void ThrowIfOtherUserIsResponsible(Guid userId)
        {
            if (this.interviewerId != Guid.Empty && userId != this.interviewerId)
                throw new InterviewException(
                    string.Format(
                        "interviewer with id {0} is not responsible for the interview anymore, interviewer with id {1} is.",
                        userId, interviewerId), InterviewDomainExceptionType.OtherUserIsResponsible);
        }

        public void ApplySynchronizationMetadata(Guid id, Guid userId, Guid questionnaireId, long questionnaireVersion,
            InterviewStatus interviewStatus,
            AnsweredQuestionSynchronizationDto[] featuredQuestionsMeta, string comments, bool valid,
            bool createdOnClient)
        {
            this.SetQuestionnaireProperties(questionnaireId, questionnaireVersion);

            if (this.status == InterviewStatus.Deleted)
                this.Restore(userId);
            else
                this.ThrowIfStatusNotAllowedToBeChangedWithMetadata(interviewStatus);

            this.ApplyEvent(new SynchronizationMetadataApplied(userId, 
                questionnaireId, 
                questionnaireVersion,
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

            if (interviewChanges.RosterCalculationData != null)
                this.ApplyRostersEvents(interviewChanges.RosterCalculationData);

            if (interviewChanges.AnswersForLinkedQuestionsToRemove != null)
                this.ApplyAnswersRemovanceEvents(interviewChanges.AnswersForLinkedQuestionsToRemove);

            if (interviewChanges.RosterInstancesWithAffectedTitles != null)
                this.ApplyRosterRowsTitleChangedEvents(interviewChanges.RosterInstancesWithAffectedTitles,
                    interviewChanges.AnswerAsRosterTitle);

            if (interviewChanges.EnablementChanges != null)
                this.ApplyEnablementChangesEvents(interviewChanges.EnablementChanges);

            if (interviewChanges.ValidityChanges != null)
                this.ApplyValidityChangesEvents(interviewChanges.ValidityChanges);
        }

        private void ApplyInterviewChanges(IEnumerable<InterviewChanges> interviewChangesItems)
        {
            var eneblementChanges = new List<EnablementChanges>();
            var validityChanges = new List<ValidityChanges>();

            foreach (var interviewChanges in interviewChangesItems)
            {
                if (interviewChanges.InterviewByAnswerChanges != null)
                    this.ApplyAnswersEvents(interviewChanges.InterviewByAnswerChanges);

                if (interviewChanges.RosterCalculationData != null)
                    this.ApplyRostersEvents(interviewChanges.RosterCalculationData);

                if (interviewChanges.AnswersForLinkedQuestionsToRemove != null)
                    this.ApplyAnswersRemovanceEvents(interviewChanges.AnswersForLinkedQuestionsToRemove);

                if (interviewChanges.RosterInstancesWithAffectedTitles != null)
                    this.ApplyRosterRowsTitleChangedEvents(interviewChanges.RosterInstancesWithAffectedTitles,
                        interviewChanges.AnswerAsRosterTitle);

                if (interviewChanges.ValidityChanges != null)
                    validityChanges.Add(interviewChanges.ValidityChanges);

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
            var rosterInstancesToAdd = this.GetUnionOfUniqueRosterDataPropertiesByRosterAndNestedRosters(
                d => d.RosterInstancesToAdd, new RosterIdentityComparer(), rosterDatas);

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

        private Dictionary<RosterIdentity, string>
            GetUnionOfUniqueRosterInstancesToAddWithRosterTitlesByRosterAndNestedRosters(
            params RosterCalculationData[] datas)
        {
            var result = new Dictionary<RosterIdentity, string>(new RosterIdentityComparer());
            foreach (var data in datas)
            {
                var rosterInstancesToAdd = data.RosterInstancesToAdd;
                if (rosterInstancesToAdd != null)
                {
                    foreach (var rosterIdentity in rosterInstancesToAdd)
                    {
                        result[rosterIdentity] = data.TitlesForRosterInstancesToAdd == null || !data.TitlesForRosterInstancesToAdd.ContainsKey(rosterIdentity.RosterInstanceId)
                            ? null
                            : data.TitlesForRosterInstancesToAdd[rosterIdentity.RosterInstanceId];
                    }
                }

                foreach (var rosterInstantiatesFromNestedLevel in data.RosterInstantiatesFromNestedLevels)
                {
                    var nestedRosterInstancesToAdd =
                        GetUnionOfUniqueRosterInstancesToAddWithRosterTitlesByRosterAndNestedRosters(
                            rosterInstantiatesFromNestedLevel);
                    foreach (var nestedRosterInstanceToAdd in nestedRosterInstancesToAdd)
                    {
                        result[nestedRosterInstanceToAdd.Key] = nestedRosterInstanceToAdd.Value;
                    }
                }
            }

            return result;
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
                    case AnswerChangeType.Picture:
                        this.ApplyEvent(new PictureQuestionAnswered(change.UserId, change.QuestionId, change.RosterVector, change.AnswerTime, (string)change.Answer));
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
                            "Answer on Question {0} has type {1} which is not supported in applying method. InterviewId {2}",
                            change.QuestionId, change.InterviewChangeType, EventSourceId));
                }
            }
        }

        #endregion

        #region CheckInvariants

        private void CheckLinkedMultiOptionQuestionInvariants(Guid questionId, decimal[] rosterVector, decimal[][] selectedPropagationVectors, IQuestionnaire questionnaire,
    Identity answeredQuestion)
        {
            ThrowIfQuestionDoesNotExist(questionId, questionnaire);
            ThrowIfRosterVectorIsIncorrect(this.interviewState, questionId, rosterVector, questionnaire);
            ThrowIfQuestionTypeIsNotOneOfExpected(questionId, questionnaire, QuestionType.MultyOption);
            ThrowIfQuestionOrParentGroupIsDisabled(this.interviewState, answeredQuestion, questionnaire);

            Guid linkedQuestionId = this.GetLinkedQuestionIdOrThrow(questionId, questionnaire);
            var answeredLinkedQuestions =
                selectedPropagationVectors.Select(selectedRosterVector => new Identity(linkedQuestionId, selectedRosterVector));

            foreach (var answeredLinkedQuestion in answeredLinkedQuestions)
            {
                ThrowIfRosterVectorIsIncorrect(this.interviewState, linkedQuestionId, answeredLinkedQuestion.RosterVector, questionnaire);
                ThrowIfQuestionOrParentGroupIsDisabled(this.interviewState, answeredLinkedQuestion, questionnaire);
                ThrowIfLinkedQuestionDoesNotHaveAnswer(this.interviewState, answeredQuestion, answeredLinkedQuestion, questionnaire);
            }
            ThrowIfLengthOfSelectedValuesMoreThanMaxForSelectedAnswerOptions(questionId, selectedPropagationVectors.Length, questionnaire);
        }

        private void CheckLinkedSingleOptionQuestionInvariants(Guid questionId, decimal[] rosterVector, decimal[] selectedPropagationVector, IQuestionnaire questionnaire,
    Identity answeredQuestion)
        {
            ThrowIfQuestionDoesNotExist(questionId, questionnaire);
            ThrowIfRosterVectorIsIncorrect(this.interviewState, questionId, rosterVector, questionnaire);

            ThrowIfQuestionTypeIsNotOneOfExpected(questionId, questionnaire, QuestionType.SingleOption);
            ThrowIfQuestionOrParentGroupIsDisabled(this.interviewState, answeredQuestion, questionnaire);

            Guid linkedQuestionId = this.GetLinkedQuestionIdOrThrow(questionId, questionnaire);
            var answeredLinkedQuestion = new Identity(linkedQuestionId, selectedPropagationVector);

            ThrowIfRosterVectorIsIncorrect(this.interviewState, linkedQuestionId, selectedPropagationVector, questionnaire);
            ThrowIfQuestionOrParentGroupIsDisabled(this.interviewState, answeredLinkedQuestion, questionnaire);
            ThrowIfLinkedQuestionDoesNotHaveAnswer(this.interviewState, answeredQuestion, answeredLinkedQuestion, questionnaire);
        }

        private void CheckNumericRealQuestionInvariants(Guid questionId, decimal[] rosterVector, decimal answer,
           IQuestionnaire questionnaire,
           Identity answeredQuestion, InterviewStateDependentOnAnswers currentInterviewState, bool applyStrongChecks = true)
        {
            ThrowIfQuestionDoesNotExist(questionId, questionnaire);
            ThrowIfRosterVectorIsIncorrect(currentInterviewState, questionId, rosterVector, questionnaire);
            ThrowIfQuestionTypeIsNotOneOfExpected(questionId, questionnaire, QuestionType.Numeric);
            ThrowIfNumericQuestionIsNotReal(questionId, questionnaire);
            if (applyStrongChecks)
            {
                ThrowIfNumericAnswerExceedsMaxValue(questionId, answer, questionnaire);
                ThrowIfQuestionOrParentGroupIsDisabled(currentInterviewState, answeredQuestion, questionnaire);
                ThrowIfAnswerHasMoreDecimalPlacesThenAccepted(questionnaire, questionId, answer);
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
            ThrowIfRosterVectorIsIncorrect(currentInterviewState, questionId, rosterVector, questionnaire);
            ThrowIfQuestionTypeIsNotOneOfExpected(questionId, questionnaire, QuestionType.SingleOption);
            ThrowIfValueIsNotOneOfAvailableOptions(questionId, selectedValue, questionnaire);
            ThrowIfCascadingQuestionValueIsNotOneOfParentAvailableOptions(this.interviewState, answeredQuestion, rosterVector, selectedValue, questionnaire);
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

        private void CheckGpsCoordinatesInvariants(Guid questionId, decimal[] rosterVector, IQuestionnaire questionnaire, Identity answeredQuestion,
            InterviewStateDependentOnAnswers currentInterviewState, bool applyStrongChecks = true)
        {
            ThrowIfQuestionDoesNotExist(questionId, questionnaire);
            this.ThrowIfRosterVectorIsIncorrect(currentInterviewState, questionId, rosterVector, questionnaire);
            this.ThrowIfQuestionTypeIsNotOneOfExpected(questionId, questionnaire, QuestionType.GpsCoordinates);
            if (applyStrongChecks)
                ThrowIfQuestionOrParentGroupIsDisabled(currentInterviewState, answeredQuestion, questionnaire);
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

        #endregion

        #region Calculations

        // triggers roster
        private InterviewChanges CalculateInterviewChangesOnAnswerNumericIntegerQuestion(IInterviewExpressionStateV2 expressionProcessorState, InterviewStateDependentOnAnswers state, Guid userId,
            Guid questionId, decimal[] rosterVector, DateTime answerTime, int answer,
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

            RosterCalculationData rosterCalculationData = this.CalculateRosterData(state, rosterIds, rosterVector, rosterInstanceIds, null, questionnaire, getAnswer);

            //Update State
            expressionProcessorState.UpdateNumericIntegerAnswer(questionId, rosterVector, answer);

            var rosterInstancesToAdd = this.GetUnionOfUniqueRosterInstancesToAddWithRosterTitlesByRosterAndNestedRosters(rosterCalculationData);

            var rosterInstancesToRemove = this.GetUnionOfUniqueRosterDataPropertiesByRosterAndNestedRosters(
                d => d.RosterInstancesToRemove, new RosterIdentityComparer(), rosterCalculationData);

            List<RosterIdentity> rosterInstancesWithAffectedTitles = CalculateRosterInstancesWhichTitlesAreAffected(questionId, rosterVector, questionnaire);

            foreach (var rosterInstancesWithAffectedTitle in rosterInstancesWithAffectedTitles)
            {
                expressionProcessorState.UpdateRosterTitle(rosterInstancesWithAffectedTitle.GroupId,
                    rosterInstancesWithAffectedTitle.OuterRosterVector,
                    rosterInstancesWithAffectedTitle.RosterInstanceId, AnswerUtils.AnswerToString(answer));
            }

            foreach (var rosterIdentityToAdd in rosterInstancesToAdd)
            {
                expressionProcessorState.AddRoster(rosterIdentityToAdd.Key.GroupId, rosterIdentityToAdd.Key.OuterRosterVector,
                    rosterIdentityToAdd.Key.RosterInstanceId, rosterIdentityToAdd.Key.SortIndex);
                expressionProcessorState.UpdateRosterTitle(rosterIdentityToAdd.Key.GroupId,
                    rosterIdentityToAdd.Key.OuterRosterVector, rosterIdentityToAdd.Key.RosterInstanceId,
                    rosterIdentityToAdd.Value);
            }

            rosterInstancesToRemove.ForEach(r => expressionProcessorState.RemoveRoster(r.GroupId, r.OuterRosterVector, r.RosterInstanceId));

            expressionProcessorState.SaveAllCurrentStatesAsPrevious();
            EnablementChanges enablementChanges = expressionProcessorState.ProcessEnablementConditions();
            ValidityChanges validationChanges = expressionProcessorState.ProcessValidationExpressions();

            enablementChanges.QuestionsToBeEnabled.AddRange(rosterCalculationData.DisabledAnswersToEnableByDecreasedRosterSize);
            enablementChanges.GroupsToBeEnabled.AddRange(rosterCalculationData.DisabledGroupsToEnableByDecreasedRosterSize);

            List<Identity> answersForLinkedQuestionsToRemoveByDisabling =
                this.GetAnswersForLinkedQuestionsToRemoveBecauseOfDisabledGroupsOrQuestions(
                    state,
                    enablementChanges.GroupsToBeDisabled,
                    enablementChanges.QuestionsToBeDisabled,
                    questionnaire, getRosterInstanceIds);

            var interviewByAnswerChange = new List<AnswerChange>
            {
                new AnswerChange(AnswerChangeType.NumericInteger, userId, questionId, rosterVector, answerTime, answer)
            };

            return new InterviewChanges(interviewByAnswerChange, enablementChanges, validationChanges,
                rosterCalculationData, answersForLinkedQuestionsToRemoveByDisabling, rosterInstancesWithAffectedTitles, AnswerUtils.AnswerToString(answer));
        }

        private InterviewChanges CalculateInterviewChangesOnAnswerMultipleOptionsQuestion(IInterviewExpressionStateV2 expressionProcessorState, InterviewStateDependentOnAnswers state,
            Guid userId, Guid questionId, decimal[] rosterVector, DateTime answerTime,
            decimal[] selectedValues, Func<InterviewStateDependentOnAnswers, Identity, object> getAnswer,
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
                questionId, rosterVector, rosterIds, rosterInstanceIdsWithSortIndexes, questionnaire, getAnswer);

            expressionProcessorState.UpdateMultiOptionAnswer(questionId, rosterVector, selectedValues);

            var rosterInstancesToAdd = this.GetUnionOfUniqueRosterInstancesToAddWithRosterTitlesByRosterAndNestedRosters(rosterCalculationData);

            var rosterInstancesToRemove = this.GetUnionOfUniqueRosterDataPropertiesByRosterAndNestedRosters(
                d => d.RosterInstancesToRemove, new RosterIdentityComparer(), rosterCalculationData);

            foreach (var rosterInstanceToAdd in rosterInstancesToAdd)
            {
                expressionProcessorState.AddRoster(rosterInstanceToAdd.Key.GroupId,
                    rosterInstanceToAdd.Key.OuterRosterVector, rosterInstanceToAdd.Key.RosterInstanceId,
                    rosterInstanceToAdd.Key.SortIndex);
                expressionProcessorState.UpdateRosterTitle(rosterInstanceToAdd.Key.GroupId,
                    rosterInstanceToAdd.Key.OuterRosterVector, rosterInstanceToAdd.Key.RosterInstanceId,
                    rosterInstanceToAdd.Value);
            }
            rosterInstancesToRemove.ForEach(r => expressionProcessorState.RemoveRoster(r.GroupId, r.OuterRosterVector, r.RosterInstanceId));

            //Apply other changes on expressionProcessorState
            string answerFormattedAsRosterTitle = AnswerUtils.AnswerToString(selectedValues,
                answerOptionValue => questionnaire.GetAnswerOptionTitle(questionId, answerOptionValue));

            List<RosterIdentity> rosterInstancesWithAffectedTitles = CalculateRosterInstancesWhichTitlesAreAffected(
                questionId, rosterVector, questionnaire);

            foreach (var rosterInstancesWithAffectedTitle in rosterInstancesWithAffectedTitles)
            {
                expressionProcessorState.UpdateRosterTitle(rosterInstancesWithAffectedTitle.GroupId,
                    rosterInstancesWithAffectedTitle.OuterRosterVector,
                    rosterInstancesWithAffectedTitle.RosterInstanceId, answerFormattedAsRosterTitle);
            }

            expressionProcessorState.SaveAllCurrentStatesAsPrevious();
            EnablementChanges enablementChanges = expressionProcessorState.ProcessEnablementConditions();
            ValidityChanges validationChanges = expressionProcessorState.ProcessValidationExpressions();

            enablementChanges.QuestionsToBeEnabled.AddRange(rosterCalculationData.DisabledAnswersToEnableByDecreasedRosterSize);
            enablementChanges.GroupsToBeEnabled.AddRange(rosterCalculationData.DisabledGroupsToEnableByDecreasedRosterSize);

            List<Identity> answersForLinkedQuestionsToRemoveByDisabling =
                this.GetAnswersForLinkedQuestionsToRemoveBecauseOfDisabledGroupsOrQuestions(state,
                    enablementChanges.GroupsToBeDisabled,
                    enablementChanges.QuestionsToBeDisabled,
                    questionnaire, getRosterInstanceIds);


            var interviewByAnswerChange = new List<AnswerChange>
            {
                new AnswerChange(AnswerChangeType.MultipleOptions, userId, questionId, rosterVector, answerTime, selectedValues)
            };

            return new InterviewChanges(interviewByAnswerChange, enablementChanges, validationChanges,
                rosterCalculationData,
                answersForLinkedQuestionsToRemoveByDisabling, rosterInstancesWithAffectedTitles, answerFormattedAsRosterTitle);
        }

        private InterviewChanges CalculateInterviewChangesOnAnswerTextListQuestion(IInterviewExpressionStateV2 expressionProcessorState, Guid userId,
            Guid questionId, decimal[] rosterVector, DateTime answerTime, Tuple<decimal, string>[] answers,
            Func<InterviewStateDependentOnAnswers, Identity, object> getAnswer, IQuestionnaire questionnaire)
        {
            var selectedValues = answers.Select(x => x.Item1).ToArray();
            var rosterInstanceIds = new DistinctDecimalList(selectedValues.ToList());
            var rosterInstanceIdsWithSortIndexes = selectedValues.ToDictionary(
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
                answers, changedAnswers);

            expressionProcessorState.UpdateTextListAnswer(questionId, rosterVector, answers);
            var rosterInstancesToAdd = this.GetUnionOfUniqueRosterInstancesToAddWithRosterTitlesByRosterAndNestedRosters(rosterCalculationData);
            var rosterInstancesToRemove = this.GetUnionOfUniqueRosterDataPropertiesByRosterAndNestedRosters(
                d => d.RosterInstancesToRemove, new RosterIdentityComparer(), rosterCalculationData);

            foreach (var rosterInstanceToAdd in rosterInstancesToAdd)
            {
                expressionProcessorState.AddRoster(rosterInstanceToAdd.Key.GroupId,
                    rosterInstanceToAdd.Key.OuterRosterVector, rosterInstanceToAdd.Key.RosterInstanceId,
                    rosterInstanceToAdd.Key.SortIndex);
                expressionProcessorState.UpdateRosterTitle(rosterInstanceToAdd.Key.GroupId,
                    rosterInstanceToAdd.Key.OuterRosterVector, rosterInstanceToAdd.Key.RosterInstanceId,
                    rosterInstanceToAdd.Value);
            }
            rosterInstancesToRemove.ForEach(r => expressionProcessorState.RemoveRoster(r.GroupId, r.OuterRosterVector, r.RosterInstanceId));

            List<RosterIdentity> rosterInstancesWithAffectedTitles = CalculateRosterInstancesWhichTitlesAreAffected(questionId, rosterVector,
              questionnaire);
            string answerFormattedAsRosterTitle = AnswerUtils.AnswerToString(selectedValues,
              answerOptionValue => answers.Single(x => x.Item1 == answerOptionValue).Item2);
            foreach (var rosterInstancesWithAffectedTitle in rosterInstancesWithAffectedTitles)
            {
                expressionProcessorState.UpdateRosterTitle(rosterInstancesWithAffectedTitle.GroupId,
                    rosterInstancesWithAffectedTitle.OuterRosterVector,
                    rosterInstancesWithAffectedTitle.RosterInstanceId, answerFormattedAsRosterTitle);
            }
            expressionProcessorState.SaveAllCurrentStatesAsPrevious();
            EnablementChanges enablementChanges = expressionProcessorState.ProcessEnablementConditions();
            ValidityChanges validationChanges = expressionProcessorState.ProcessValidationExpressions();

            enablementChanges.QuestionsToBeEnabled.AddRange(rosterCalculationData.DisabledAnswersToEnableByDecreasedRosterSize);
            enablementChanges.GroupsToBeEnabled.AddRange(rosterCalculationData.DisabledGroupsToEnableByDecreasedRosterSize);
          
            var answerChanges = new List<AnswerChange>()
            {
                new AnswerChange(AnswerChangeType.TextList, userId, questionId, rosterVector, answerTime, answers)
            };

            return new InterviewChanges(answerChanges, enablementChanges,
                validationChanges,
                rosterCalculationData,
                null, rosterInstancesWithAffectedTitles, answerFormattedAsRosterTitle);
        }

        // do not triggers roster
        private InterviewChanges CalculateInterviewChangesOnAnswerTextQuestion(IInterviewExpressionStateV2 expressionProcessorState, InterviewStateDependentOnAnswers state, Guid userId,
            Guid questionId, decimal[] rosterVector, DateTime answerTime, string answer, IQuestionnaire questionnaire)
        {
            expressionProcessorState.UpdateTextAnswer(questionId, rosterVector, answer);

            return this.CalculateInterviewChangesOnAnswerQuestion(state, userId, questionId, rosterVector, answer, answer, AnswerChangeType.Text, answerTime, questionnaire, expressionProcessorState);
        }

        private InterviewChanges CalculateInterviewChangesOnAnswerDateTimeQuestion(IInterviewExpressionStateV2 expressionProcessorState, InterviewStateDependentOnAnswers state, Guid userId,
            Guid questionId, decimal[] rosterVector, DateTime answerTime, DateTime answer, IQuestionnaire questionnaire)
        {
            string answerFormattedAsRosterTitle = AnswerUtils.AnswerToString(answer);

            expressionProcessorState.UpdateDateAnswer(questionId, rosterVector, answer);

            return this.CalculateInterviewChangesOnAnswerQuestion(state, userId, questionId, rosterVector, answer, answerFormattedAsRosterTitle, AnswerChangeType.DateTime, answerTime, questionnaire, expressionProcessorState);
        }

        private InterviewChanges CalculateInterviewChangesOnAnswerNumericRealQuestion(IInterviewExpressionStateV2 expressionProcessorState, InterviewStateDependentOnAnswers state, Guid userId,
            Guid questionId, decimal[] rosterVector,
            DateTime answerTime, decimal answer, IQuestionnaire questionnaire)
        {
            string answerFormattedAsRosterTitle = AnswerUtils.AnswerToString(answer);

            expressionProcessorState.UpdateNumericRealAnswer(questionId, rosterVector, (double)answer);

            return this.CalculateInterviewChangesOnAnswerQuestion(state, userId, questionId, rosterVector, answer, answerFormattedAsRosterTitle, AnswerChangeType.NumericReal, answerTime, questionnaire, expressionProcessorState);
        }

        private InterviewChanges CalculateInterviewChangesOnAnswerSingleOptionQuestion(IInterviewExpressionStateV2 expressionProcessorState, InterviewStateDependentOnAnswers state,
            Guid userId,
            Guid questionId,
            decimal[] rosterVector,
            DateTime answerTime,
            decimal selectedValue,
            IQuestionnaire questionnaire)
        {
            string answerFormattedAsRosterTitle = AnswerUtils.AnswerToString(selectedValue, answerOptionValue => questionnaire.GetAnswerOptionTitle(questionId, answerOptionValue));

            var questionIdentity = new Identity(questionId, rosterVector);
            var previsousAnswer = GetEnabledQuestionAnswerSupportedInExpressions(state, questionIdentity, questionnaire);
            bool answerChanged = WasQuestionAnswered(state, questionIdentity) && (decimal?)previsousAnswer != (decimal?)selectedValue;

            var answersToRemoveByCascading = answerChanged ? this.GetQuestionsToRemoveAnswersFromDependingOnCascading(questionId, rosterVector, questionnaire, state) : Enumerable.Empty<Identity>();

            var cascadingQuestionsToDisable = questionnaire.GetCascadingQuestionsThatDirectlyDependUponQuestion(questionId)
                .Where(question => !questionnaire.DoesCascadingQuestionHaveOptionsForParentValue(question, selectedValue)).ToList();

            var cascadingQuestionsToDisableIdentities = GetInstancesOfQuestionsWithSameAndDeeperRosterLevelOrThrow(state,
                cascadingQuestionsToDisable, rosterVector, questionnaire, GetRosterInstanceIds);

            expressionProcessorState.UpdateSingleOptionAnswer(questionId, rosterVector, selectedValue);
            answersToRemoveByCascading.ToList().ForEach(x => expressionProcessorState.UpdateSingleOptionAnswer(x.Id, x.RosterVector, (decimal?)null));
            expressionProcessorState.DisableQuestions(cascadingQuestionsToDisableIdentities);

            InterviewChanges interviewChanges = this.CalculateInterviewChangesOnAnswerQuestion(state, userId, questionId, rosterVector, selectedValue, answerFormattedAsRosterTitle, AnswerChangeType.SingleOption, answerTime, questionnaire, expressionProcessorState);

            interviewChanges.AnswersForLinkedQuestionsToRemove.AddRange(answersToRemoveByCascading);

            return interviewChanges;
        }

        private InterviewChanges CalculateInterviewChangesOnAnswerQRBarcodeQuestion(IInterviewExpressionStateV2 expressionProcessorState, InterviewStateDependentOnAnswers state,
            Guid userId, Guid questionId, decimal[] rosterVector, DateTime answerTime, string answer, IQuestionnaire questionnaire)
        {
            expressionProcessorState.UpdateQrBarcodeAnswer(questionId, rosterVector, answer);

            return this.CalculateInterviewChangesOnAnswerQuestion(state, userId, questionId, rosterVector, answer, answer, AnswerChangeType.QRBarcode, answerTime, questionnaire, expressionProcessorState);
        }

        private InterviewChanges CalculateInterviewChangesOnAnswerGeoLocationQuestion(IInterviewExpressionStateV2 expressionProcessorState, InterviewStateDependentOnAnswers state, Guid userId,
            Guid questionId, decimal[] rosterVector, DateTime answerTime, double latitude, double longitude, double accuracy, double altitude, DateTimeOffset timestamp, Identity answeredQuestion,
            IQuestionnaire questionnaire)
        {
            string answerFormattedAsRosterTitle = string.Format(CultureInfo.InvariantCulture, "[{0};{1}]", latitude, longitude);

            expressionProcessorState.UpdateGeoLocationAnswer(questionId, rosterVector, latitude, longitude, accuracy, altitude);

            return this.CalculateInterviewChangesOnAnswerQuestion(state, userId, questionId, rosterVector,
                new GeoLocationPoint(latitude, longitude, accuracy, altitude, timestamp), answerFormattedAsRosterTitle,
                AnswerChangeType.GeoLocation, answerTime, questionnaire, expressionProcessorState);
        }

        private IInterviewExpressionStateV2 PrepareExpressionProcessorStateForCalculations()
        {
            IInterviewExpressionStateV2 expressionProcessorState = this.ExpressionProcessorStatePrototype.Clone();

            expressionProcessorState.SaveAllCurrentStatesAsPrevious();

            return expressionProcessorState;
        }

        private InterviewChanges CalculateInterviewChangesOnAnswerQuestion(InterviewStateDependentOnAnswers state, Guid userId, Guid questionId, decimal[] rosterVector,
            object answer, string answerFormattedAsRosterTitle, AnswerChangeType answerChangeType, DateTime answerTime, IQuestionnaire questionnaire,
            IInterviewExpressionStateV2 expressionProcessorState)
        {
            List<RosterIdentity> rosterInstancesWithAffectedTitles = CalculateRosterInstancesWhichTitlesAreAffected(questionId, rosterVector, questionnaire);

            foreach (var rosterInstancesWithAffectedTitle in rosterInstancesWithAffectedTitles)
            {
                expressionProcessorState.UpdateRosterTitle(rosterInstancesWithAffectedTitle.GroupId,
                    rosterInstancesWithAffectedTitle.OuterRosterVector,
                    rosterInstancesWithAffectedTitle.RosterInstanceId, answerFormattedAsRosterTitle);
            }

            EnablementChanges enablementChanges = expressionProcessorState.ProcessEnablementConditions();
            ValidityChanges validationChanges = expressionProcessorState.ProcessValidationExpressions();

            List<Identity> answersForLinkedQuestionsToRemoveByDisabling = this
                .GetAnswersForLinkedQuestionsToRemoveBecauseOfDisabledGroupsOrQuestions(
                    state,
                    enablementChanges.GroupsToBeDisabled,
                    enablementChanges.QuestionsToBeDisabled,
                    questionnaire,
                    GetRosterInstanceIds);

            var interviewByAnswerChange = new List<AnswerChange>
            {
                new AnswerChange(answerChangeType, userId, questionId, rosterVector, answerTime, answer)
            };

            return new InterviewChanges(
                interviewByAnswerChange,
                enablementChanges,
                validationChanges,
                null,
                answersForLinkedQuestionsToRemoveByDisabling,
                rosterInstancesWithAffectedTitles,
                answerFormattedAsRosterTitle);
        }

        private IEnumerable<Identity> GetQuestionsToRemoveAnswersFromDependingOnCascading(Guid questionId, decimal[] rosterVector, IQuestionnaire questionnaire, InterviewStateDependentOnAnswers state)
        {
            IEnumerable<Guid> dependentQuesions = questionnaire.GetCascadingQuestionsThatDependUponQuestion(questionId);
            var result = GetInstancesOfQuestionsWithSameAndDeeperRosterLevelOrThrow(state, dependentQuesions, rosterVector, questionnaire, GetRosterInstanceIds);

            foreach (var identity in result)
            {
                string questionKey = ConversionHelper.ConvertIdAndRosterVectorToString(identity.Id, identity.RosterVector);
                if (state.AnsweredQuestions.Contains(questionKey))
                {
                    yield return identity;
                }
            }
        }

        private void CalculateChangesByFeaturedQuestion(IInterviewExpressionStateV2 expressionProcessorState, InterviewChangeStructures changeStructures, Guid userId,
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
                            this.CalculateInterviewChangesOnAnswerTextQuestion(expressionProcessorState, changeStructures.State, userId, questionId,
                                currentQuestionRosterVector, answersTime, (string)answer, questionnaire);
                        break;

                    case QuestionType.AutoPropagate:
                        interviewChanges =
                            this.CalculateInterviewChangesOnAnswerNumericIntegerQuestion(expressionProcessorState, changeStructures.State, userId, questionId,
                                currentQuestionRosterVector, answersTime, (int)answer, getAnswer, questionnaire);
                        break;
                    case QuestionType.Numeric:
                        if (questionnaire.IsQuestionInteger(questionId))
                            interviewChanges =
                                this.CalculateInterviewChangesOnAnswerNumericIntegerQuestion(expressionProcessorState, changeStructures.State, userId, questionId,
                                    currentQuestionRosterVector, answersTime, (int)answer, getAnswer, questionnaire);
                        else
                            interviewChanges =
                                this.CalculateInterviewChangesOnAnswerNumericRealQuestion(expressionProcessorState, changeStructures.State, userId, questionId,
                                    currentQuestionRosterVector, answersTime, (decimal)answer, questionnaire);
                        break;

                    case QuestionType.DateTime:
                        interviewChanges =
                            this.CalculateInterviewChangesOnAnswerDateTimeQuestion(expressionProcessorState, changeStructures.State, userId, questionId,
                                currentQuestionRosterVector, answersTime, (DateTime)answer, questionnaire);
                        break;

                    case QuestionType.SingleOption:
                        interviewChanges =
                            this.CalculateInterviewChangesOnAnswerSingleOptionQuestion(expressionProcessorState, changeStructures.State, userId, questionId,
                                currentQuestionRosterVector, answersTime, (decimal)answer, questionnaire);
                        break;

                    case QuestionType.MultyOption:
                        interviewChanges =
                            this.CalculateInterviewChangesOnAnswerMultipleOptionsQuestion(expressionProcessorState, changeStructures.State, userId, questionId,
                                currentQuestionRosterVector, answersTime, (decimal[])answer, getAnswer, questionnaire);
                        break;
                    case QuestionType.QRBarcode:
                        interviewChanges =
                            this.CalculateInterviewChangesOnAnswerQRBarcodeQuestion(expressionProcessorState, changeStructures.State, userId, questionId, currentQuestionRosterVector, answersTime, (string)answer, questionnaire);
                        break;
                    case QuestionType.GpsCoordinates:
                        var geoAnswer = answer as GeoPosition;
                        interviewChanges =
                            this.CalculateInterviewChangesOnAnswerGeoLocationQuestion(expressionProcessorState, changeStructures.State, userId, questionId,
                                currentQuestionRosterVector, answersTime, geoAnswer.Latitude, geoAnswer.Longitude, geoAnswer.Accuracy,
                                geoAnswer.Altitude, geoAnswer.Timestamp, answeredQuestion, questionnaire);
                        break;
                    case QuestionType.TextList:
                        interviewChanges =
                            this.CalculateInterviewChangesOnAnswerTextListQuestion(expressionProcessorState, userId, questionId, currentQuestionRosterVector, answersTime, (Tuple<decimal, string>[])answer, getAnswer, questionnaire);
                        break;

                    default:
                        throw new InterviewException(string.Format(
                            "Question {0} has type {1} which is not supported as initial pre-filled question. InterviewId: {2}",
                            questionId, questionType, EventSourceId));
                }

                changeStructures.State.ApplyInterviewChanges(interviewChanges);
                changeStructures.Changes.Add(interviewChanges);
            }
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
                .Select(fixedRosterId => this.CalculateRosterData(state,
                    new List<Guid> { fixedRosterId },
                    outerRosterVector,
                    getFixedRosterInstanceIds(fixedRosterId),
                    rosterTitlesGroupedByRosterId[fixedRosterId],
                    questionnaire, getAnswer)
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
                    this.CalculateRosterData(state, questionnaire,
                        new List<Guid> { nestedRosterId }, outerRosterVector, rosterInstanceIds.ToDictionary(x => x.Key, x => x.Value.Item2),
                        rosterInstanceIds.Any(x => !string.IsNullOrEmpty(x.Value.Item1))
                            ? rosterInstanceIds.ToDictionary(x => x.Key, x => x.Value.Item1)
                            : null,
                        questionnaire,
                        getAnswer);
            }
        }


        private RosterCalculationData CalculateRosterDataWithRosterTitlesFromTextListQuestions(InterviewStateDependentOnAnswers state,
            IQuestionnaire questionnare, decimal[] rosterVector, List<Guid> rosterIds,
            Dictionary<decimal, int?> rosterInstanceIdsWithSortIndexes, IQuestionnaire questionnaire,
            Func<InterviewStateDependentOnAnswers, Identity, object> getAnswer,
            Tuple<decimal, string>[] answers, Tuple<decimal, string>[] changedAnswers)
        {
            RosterCalculationData rosterCalculationData = this.CalculateRosterData(state, questionnare,
                rosterIds, rosterVector, rosterInstanceIdsWithSortIndexes, null, questionnaire, getAnswer);

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
            InterviewStateDependentOnAnswers state,
            Guid questionId, decimal[] rosterVector, List<Guid> rosterIds,
            Dictionary<decimal, int?> rosterInstanceIdsWithSortIndexes, IQuestionnaire questionnaire,
            Func<InterviewStateDependentOnAnswers, Identity, object> getAnswer)
        {
            RosterCalculationData rosterCalculationData = this.CalculateRosterData(state, questionnaire,
                rosterIds, rosterVector, rosterInstanceIdsWithSortIndexes, null, questionnaire, getAnswer);

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
                        .Select(title => new
                            {
                                Title = title.Title,
                                RosterInstanceId = title.Value
                            }).ToDictionary(x => x.RosterInstanceId, x => x.Title)
                    }).ToDictionary(x => x.FixedRosterId, x => x.TitlesWithIds);
            return rosterTitlesGroupedByRosterId;
        }

        private RosterCalculationData CalculateRosterData(InterviewStateDependentOnAnswers state,
            List<Guid> rosterIds, decimal[] nearestToOuterRosterVector, DistinctDecimalList rosterInstanceIds,
            Dictionary<decimal, string> rosterTitles, IQuestionnaire questionnaire,
            Func<InterviewStateDependentOnAnswers, Identity, object> getAnswer)
        {
            Dictionary<decimal, int?> rosterInstanceIdsWithSortIndexes =
                rosterInstanceIds.ToDictionary(
                    rosterInstanceId => rosterInstanceId,
                    rosterInstanceId => (int?)null);

            return this.CalculateRosterData(state, questionnaire,
                rosterIds, nearestToOuterRosterVector, rosterInstanceIdsWithSortIndexes, rosterTitles, questionnaire, getAnswer);
        }

        private RosterCalculationData CalculateRosterData(InterviewStateDependentOnAnswers state, IQuestionnaire questionnare,
            List<Guid> rosterIds, decimal[] nearestToOuterRosterVector, Dictionary<decimal, int?> rosterInstanceIdsWithSortIndexes,
            Dictionary<decimal, string> rosterTitles,
            IQuestionnaire questionnaire,
            Func<InterviewStateDependentOnAnswers, Identity, object> getAnswer)
        {
            List<RosterIdentity> rosterInstancesToAdd, rosterInstancesToRemove, rosterInstancesToChange = new List<RosterIdentity>();

            List<RosterCalculationData> rosterInstantiatesFromNestedLevels;
            this.CalculateChangesInRosterInstances(state, questionnare, rosterIds, nearestToOuterRosterVector,
                rosterInstanceIdsWithSortIndexes,
                getAnswer,
                out rosterInstancesToAdd, out rosterInstancesToRemove, out rosterInstantiatesFromNestedLevels);

            List<decimal> rosterInstanceIdsBeingRemoved = rosterInstancesToRemove.Select(instance => instance.RosterInstanceId).ToList();

            List<Identity> answersToRemoveByDecreasedRosterSize = this.GetAnswersToRemoveIfRosterInstancesAreRemoved(state,
                rosterIds, rosterInstanceIdsBeingRemoved, nearestToOuterRosterVector, questionnaire);

            List<Identity> disabledAnswersToEnableByDecreasedRosterSize = GetDisabledAnswersToEnableByDecreasedRosterSize(state,
                rosterIds, rosterInstanceIdsBeingRemoved, nearestToOuterRosterVector, questionnaire);

            List<Identity> disabledGroupsToEnableByDecreasedRosterSize = GetDisabledGroupsToEnableByDecreasedRosterSize(state,
                rosterInstancesToRemove, questionnaire);

            return new RosterCalculationData(rosterInstancesToAdd, rosterInstancesToRemove, rosterInstancesToChange,
                answersToRemoveByDecreasedRosterSize, disabledAnswersToEnableByDecreasedRosterSize, disabledGroupsToEnableByDecreasedRosterSize,
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
            var nestedRosterIds = questionnaire.GetNestedRostersOfGroupById(rosterId).ToList();

            int indexOfRosterInRosterVector = GetIndexOfRosterInRosterVector(rosterId, questionnaire);

            List<Identity> answersToRemoveByDecreasedRosterSize = this.GetAnswersToRemoveIfRosterInstancesAreRemoved(state,
                nestedRosterIds, rosterInstanceIdsBeingRemoved, nearestToOuterRosterVector,
                questionnaire);

            List<Identity> disabledAnswersToEnableByDecreasedRosterSize = GetDisabledAnswersToEnableByDecreasedRosterSize(state,
                nestedRosterIds, rosterInstanceIdsBeingRemoved, nearestToOuterRosterVector, questionnaire);

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

            List<Identity> disabledGroupsToEnableByDecreasedRosterSize = GetDisabledGroupsToEnableByDecreasedRosterSize(state,
               listOfRosterInstanceIdsForRemove, questionnaire);

            return new RosterCalculationData(new List<RosterIdentity>(), listOfRosterInstanceIdsForRemove, new List<RosterIdentity>(),
                answersToRemoveByDecreasedRosterSize, disabledAnswersToEnableByDecreasedRosterSize, disabledGroupsToEnableByDecreasedRosterSize,
                new Dictionary<decimal, string>(), rosterInstantiatesFromNestedLevels);
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

        private void ThrowIfInterviewCountLimitReached()
        {
            if (this.InterviewPreconditionsService.GetInterviewsCountAllowedToCreateUntilLimitReached() <= 0)
                throw new InterviewException(string.Format("Max number of interviews '{0}' is reached.",
                    this.InterviewPreconditionsService.GetMaxAllowedInterviewsCount()),
                    InterviewDomainExceptionType.InterviewLimitReached);
        }

        private void ThrowIfInterviewWasCompleted()
        {
            if (this.wasCompleted)
                throw new InterviewException(string.Format("Interview was completed by interviewer and cannot be deleted. InterviewId: {0}", EventSourceId));
        }

        private void ThrowIfQuestionDoesNotExist(Guid questionId, IQuestionnaire questionnaire)
        {
            if (!questionnaire.HasQuestion(questionId))
                throw new InterviewException(string.Format("Question with id '{0}' is not found. InterviewId: {1}", questionId, EventSourceId));
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

        private void ThrowIfAnswersExceedsMaxAnswerCountLimit(Tuple<decimal, string>[] answers, int? maxAnswersCountLimit,
            Guid questionId, IQuestionnaire questionnaire)
        {
            if (maxAnswersCountLimit.HasValue && answers.Length > maxAnswersCountLimit.Value)
            {
                throw new InterviewException(string.Format("Answers exceeds MaxAnswerCount limit for question {0}. InterviewId: {1}",
                    FormatQuestionForException(questionId, questionnaire), EventSourceId));
            }
        }

        private void ThrowIfStringValueAreEmptyOrWhitespaces(Tuple<decimal, string>[] answers, Guid questionId, IQuestionnaire questionnaire)
        {
            if (answers.Any(x => string.IsNullOrWhiteSpace(x.Item2)))
            {
                throw new InterviewException(string.Format("String values should be not empty or whitespaces for question {0}. InterviewId: {1}",
                    FormatQuestionForException(questionId, questionnaire), EventSourceId));
            }
        }

        private void ThrowIfDecimalValuesAreNotUnique(Tuple<decimal, string>[] answers, Guid questionId, IQuestionnaire questionnaire)
        {
            var decimals = answers.Select(x => x.Item1).Distinct().ToArray();
            if (answers.Length > decimals.Length)
            {
                throw new InterviewException(string.Format("Decimal values should be unique for question {0}. InterviewId: {1}",
                    FormatQuestionForException(questionId, questionnaire), EventSourceId));
            }
        }

        private void ThrowIfRosterVectorIsNull(Guid questionId, decimal[] rosterVector, IQuestionnaire questionnaire)
        {
            if (rosterVector == null)
                throw new InterviewException(string.Format(
                    "Roster information for question {0} is missing. Roster vector cannot be null. InterviewId: {1}",
                    FormatQuestionForException(questionId, questionnaire), EventSourceId));
        }

        private void ThrowIfRosterVectorLengthDoesNotCorrespondToParentRosterGroupsCount(
            Guid questionId, decimal[] rosterVector, Guid[] parentRosterGroups, IQuestionnaire questionnaire)
        {
            if (rosterVector.Length != parentRosterGroups.Length)
                throw new InterviewException(string.Format(
                    "Roster information for question {0} is incorrect. " +
                        "Roster vector has {1} elements, but parent roster groups count is {2}. InterviewId: {3}",
                    FormatQuestionForException(questionId, questionnaire), rosterVector.Length, parentRosterGroups.Length, EventSourceId));
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
                            "but roster group has only following roster instances: {4}. InterviewId: {5}",
                        FormatQuestionForException(questionId, questionnaire), indexOfRosterVectorElement,
                        FormatGroupForException(rosterGroupId, questionnaire), rosterInstanceId,
                        string.Join(", ", rosterInstanceIds), EventSourceId));
            }
        }

        private void ThrowIfQuestionTypeIsNotOneOfExpected(Guid questionId, IQuestionnaire questionnaire,
            params QuestionType[] expectedQuestionTypes)
        {
            QuestionType questionType = questionnaire.GetQuestionType(questionId);

            bool typeIsNotExpected = !expectedQuestionTypes.Contains(questionType);
            if (typeIsNotExpected)
                throw new InterviewException(string.Format(
                    "Question {0} has type {1}. But one of the following types was expected: {2}. InterviewId: {3}",
                    FormatQuestionForException(questionId, questionnaire), questionType,
                    string.Join(", ", expectedQuestionTypes.Select(type => type.ToString())),
                    EventSourceId));
        }

        private void ThrowIfNumericQuestionIsNotReal(Guid questionId, IQuestionnaire questionnaire)
        {
            var isNotSupportReal = questionnaire.IsQuestionInteger(questionId);
            if (isNotSupportReal)
                throw new InterviewException(string.Format(
                    "Question {0} doesn't support answer of type real. InterviewId: {1}",
                    FormatQuestionForException(questionId, questionnaire), EventSourceId));
        }

        private void ThrowIfNumericQuestionIsNotInteger(Guid questionId, IQuestionnaire questionnaire)
        {
            var isNotSupportInteger = !questionnaire.IsQuestionInteger(questionId);
            if (isNotSupportInteger)
                throw new InterviewException(string.Format(
                    "Question {0} doesn't support answer of type integer. InterviewId: {1}",
                    FormatQuestionForException(questionId, questionnaire), EventSourceId));
        }

        private void ThrowIfLinkedQuestionDoesNotHaveAnswer(InterviewStateDependentOnAnswers state, Identity answeredQuestion,
            Identity answeredLinkedQuestion, IQuestionnaire questionnaire)
        {
            if (!WasQuestionAnswered(state, answeredLinkedQuestion))
            {
                throw new InterviewException(string.Format(
                    "Could not set answer for question {0} because his dependent linked question {1} does not have answer. InterviewId: {2}",
                    FormatQuestionForException(answeredQuestion, questionnaire),
                    FormatQuestionForException(answeredLinkedQuestion, questionnaire),
                    EventSourceId));
            }
        }

        private void ThrowIfValueIsNotOneOfAvailableOptions(Guid questionId, decimal value, IQuestionnaire questionnaire)
        {
            IEnumerable<decimal> availableValues = questionnaire.GetAnswerOptionsAsValues(questionId);

            bool valueIsNotOneOfAvailable = !availableValues.Contains(value);
            if (valueIsNotOneOfAvailable)
                throw new InterviewException(string.Format(
                    "For question {0} was provided selected value {1} as answer. But only following values are allowed: {2}. InterviewId: {3}",
                    FormatQuestionForException(questionId, questionnaire), value, JoinDecimalsWithComma(availableValues), EventSourceId));
        }

        private void ThrowIfCascadingQuestionValueIsNotOneOfParentAvailableOptions(InterviewStateDependentOnAnswers interviewState, Identity answeredQuestion, decimal[] rosterVector, decimal value, IQuestionnaire questionnaire)
        {
            var questionId = answeredQuestion.Id;
            Guid? cascadingId = questionnaire.GetCascadingQuestionParentId(questionId);

            if (!cascadingId.HasValue) return;

            decimal childParentValue = questionnaire.GetCascadingParentValue(questionId, value);

            var questionIdentity = GetInstanceOfQuestionWithSameAndUpperRosterLevelOrThrow(cascadingId.Value, rosterVector, questionnaire);
            string questionKey = ConversionHelper.ConvertIdentityToString(questionIdentity);

            if (!interviewState.AnswersSupportedInExpressions.ContainsKey(questionKey))
                return;

            object answer = interviewState.AnswersSupportedInExpressions[questionKey];
            string parentAnswer = AnswerUtils.AnswerToString(answer);

            var answerNotExistsInParent = Convert.ToDecimal(parentAnswer, CultureInfo.InvariantCulture) != childParentValue;
            if (answerNotExistsInParent)
                throw new InterviewException(string.Format(
                    "For question {0} was provided selected value {1} as answer with parent value {2}, but this do not correspond to the parent answer selected value {3}. InterviewId: {4}",
                    FormatQuestionForException(questionId, questionnaire), value, childParentValue, parentAnswer, EventSourceId));
        }

        private void ThrowIfSomeValuesAreNotFromAvailableOptions(Guid questionId, decimal[] values, IQuestionnaire questionnaire)
        {
            IEnumerable<decimal> availableValues = questionnaire.GetAnswerOptionsAsValues(questionId);

            bool someValueIsNotOneOfAvailable = values.Any(value => !availableValues.Contains(value));
            if (someValueIsNotOneOfAvailable)
                throw new InterviewException(string.Format(
                    "For question {0} were provided selected values {1} as answer. But only following values are allowed: {2}. InterviewId: {3}",
                    FormatQuestionForException(questionId, questionnaire), JoinDecimalsWithComma(values),
                    JoinDecimalsWithComma(availableValues),
                    EventSourceId));
        }

        private void ThrowIfLengthOfSelectedValuesMoreThanMaxForSelectedAnswerOptions(Guid questionId, int answersCount, IQuestionnaire questionnaire)
        {
            int? maxSelectedOptions = questionnaire.GetMaxSelectedAnswerOptions(questionId);

            if (maxSelectedOptions.HasValue && maxSelectedOptions > 0 && answersCount > maxSelectedOptions)
                throw new InterviewException(string.Format(
                    "For question {0} number of answers is greater than the maximum number of selected answers. InterviewId: {1}",
                    FormatQuestionForException(questionId, questionnaire), EventSourceId));
        }

        private void ThrowIfQuestionOrParentGroupIsDisabled(InterviewStateDependentOnAnswers state, Identity question, IQuestionnaire questionnaire)
        {
            if (IsQuestionDisabled(state, question))
                throw new InterviewException(string.Format(
                    "Question {1} is disabled by it's following enablement condition:{0}{2}{0}InterviewId: {3}",
                    Environment.NewLine,
                    FormatQuestionForException(question, questionnaire),
                    questionnaire.GetCustomEnablementConditionForQuestion(question.Id),
                    EventSourceId));

            IEnumerable<Guid> parentGroupIds = questionnaire.GetAllParentGroupsForQuestion(question.Id);
            IEnumerable<Identity> parentGroups = GetInstancesOfGroupsWithSameAndUpperRosterLevelOrThrow(parentGroupIds,
                question.RosterVector, questionnaire);

            foreach (Identity parentGroup in parentGroups)
            {
                if (IsGroupDisabled(state, parentGroup))
                    throw new InterviewException(string.Format(
                        "Question {1} is disabled because parent group {2} is disabled by it's following enablement condition:{0}{3}{0}InterviewId: {4}",
                        Environment.NewLine,
                        FormatQuestionForException(question, questionnaire),
                        FormatGroupForException(parentGroup, questionnaire),
                        questionnaire.GetCustomEnablementConditionForGroup(parentGroup.Id),
                        EventSourceId));
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
                        "Answer '{0}' for question {1}  is incorrect because has more decimal places then allowed by questionnaire. InterviewId: {2}", answer,
                        FormatQuestionForException(questionId, questionnaire), EventSourceId));
        }

        private void ThrowIfNumericAnswerExceedsMaxValue(Guid questionId, decimal answer, IQuestionnaire questionnaire)
        {
            int? maxValue = questionnaire.GetMaxValueForNumericQuestion(questionId);

            if (!maxValue.HasValue)
                return;

            bool answerExceedsMaxValue = answer > maxValue.Value;

            if (answerExceedsMaxValue)
                throw new InterviewException(string.Format(
                    "Answer '{0}' for question {1} is incorrect because answer is greater than Roster upper bound '{2}'. InterviewId: {3}",
                    answer, FormatQuestionForException(questionId, questionnaire), maxValue.Value, EventSourceId));
        }

        private void ThrowIfRosterSizeAnswerIsNegative(Guid questionId, int answer, IQuestionnaire questionnaire)
        {
            bool answerIsNegative = answer < 0;

            if (answerIsNegative)
                throw new InterviewException(string.Format(
                    "Answer '{0}' for question {1} is incorrect because question is used as size of roster group and specified answer is negative. InterviewId: {2}",
                    answer, FormatQuestionForException(questionId, questionnaire), EventSourceId));
        }

        private void ThrowIfInterviewStatusIsNotOneOfExpected(params InterviewStatus[] expectedStatuses)
        {
            if (!expectedStatuses.Contains(this.status))
                throw new InterviewException(string.Format(
                    "Interview status is {0}. But one of the following statuses was expected: {1}. InterviewId: {2}",
                    this.status, 
                    string.Join(", ", expectedStatuses.Select(expectedStatus => expectedStatus.ToString())),
                    EventSourceId), InterviewDomainExceptionType.StatusIsNotOneOfExpected);
        }

        private void ThrowIfInterviewHardDeleted()
        {
            if (this.wasHardDeleted)
                throw new InterviewException(string.Format("Interview {0} status is hard deleted.", EventSourceId), InterviewDomainExceptionType.InterviewHardDeleted);
        }

        private void ThrowIfStatusNotAllowedToBeChangedWithMetadata(InterviewStatus interviewStatus)
        {
            ThrowIfInterviewHardDeleted();
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
                        InterviewStatus.Restarted,
                        InterviewStatus.Completed);
                    return;
                case InterviewStatus.ApprovedBySupervisor:
                    this.ThrowIfInterviewStatusIsNotOneOfExpected(
                        InterviewStatus.InterviewerAssigned,
                        InterviewStatus.RejectedByHeadquarters,
                        InterviewStatus.SupervisorAssigned);
                    return;
            }
            throw new InterviewException(string.Format(
                "Status {0} not allowed to be changed with ApplySynchronizationMetadata command. InterviewId: {1}",
                interviewStatus, EventSourceId), InterviewDomainExceptionType.StatusIsNotOneOfExpected);
        }

        #endregion

        private static IQuestionnaire GetHistoricalQuestionnaireOrThrow(Guid id, long version)
        {
            IQuestionnaire questionnaire = QuestionnaireRepository.GetHistoricalQuestionnaire(id, version);

            if (questionnaire == null)
                throw new InterviewException(string.Format("Questionnaire with id '{0}' of version {1} is not found.", id, version), InterviewDomainExceptionType.QuestionnaireIsMissing);

            return questionnaire;
        }

        private static IQuestionnaire GetQuestionnaireOrThrow(Guid id)
        {
            IQuestionnaire questionnaire = QuestionnaireRepository.GetQuestionnaire(id);

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
                    .ToDictionary(x => x.Value, x => new Tuple<string, int?>(x.Title, null));

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
            decimal[] rosterVector = null, InterviewStateDependentOnAnswers currentInterviewState = null, bool applyStrongChecks = true)
        {
            var currentRosterVector = rosterVector ?? EmptyRosterVector;
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
                            "Question {0} has type {1} which is not supported as initial pre-filled question. InterviewId: {2}",
                            questionId, questionType, this.EventSourceId));
                }
            }
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

        private List<Identity> GetDisabledGroupsToEnableByDecreasedRosterSize(
            InterviewStateDependentOnAnswers state,
            List<RosterIdentity> rosterIdentities, 
            IQuestionnaire questionnaire)
        {
            if (rosterIdentities.Count == 0)
                return new List<Identity>();

            return rosterIdentities.SelectMany(rosterIdentity =>
                {
                    var rosterVector = rosterIdentity.OuterRosterVector.Concat(new[] { rosterIdentity.RosterInstanceId }).ToArray();

                    var rosterAsGroupIdentity = new Identity(rosterIdentity.GroupId, rosterVector);

                    var underlyingChildGroupIds = questionnaire.GetAllUnderlyingChildGroups(rosterIdentity.GroupId).ToList();

                    var underlyingGroupInstances = GetInstancesOfGroupsWithSameAndDeeperRosterLevelOrThrow(state,
                        underlyingChildGroupIds, rosterAsGroupIdentity.RosterVector, questionnaire, GetRosterInstanceIds)
                        .ToList();

                    var underlyingDisabledQuestionsByRemovedRosterInstances = (
                        from @group in underlyingGroupInstances
                        where state.DisabledGroups.Contains(ConversionHelper.ConvertIdentityToString(@group))
                        select @group
                        ).ToList();

                    if (state.DisabledGroups.Contains(ConversionHelper.ConvertIdentityToString(rosterAsGroupIdentity)))
                    {
                        underlyingDisabledQuestionsByRemovedRosterInstances.Add(rosterAsGroupIdentity);
                    }

                    return underlyingDisabledQuestionsByRemovedRosterInstances;
                }).ToList();
        }

        private List<Identity> GetDisabledAnswersToEnableByDecreasedRosterSize(
            InterviewStateDependentOnAnswers state,
            IEnumerable<Guid> rosterIds, List<decimal> rosterInstanceIdsBeingRemoved, 
            decimal[] nearestToOuterRosterVector,
            IQuestionnaire questionnaire)
        {
             if (rosterInstanceIdsBeingRemoved.Count == 0)
                return new List<Identity>();

             return rosterIds.SelectMany(rosterId =>
                 {
                     int indexOfRosterInRosterVector = GetIndexOfRosterInRosterVector(rosterId, questionnaire);

                     IEnumerable<Guid> underlyingQuestionIds = questionnaire.GetAllUnderlyingQuestions(rosterId);

                     IEnumerable<Identity> underlyingQuestionInstances = GetInstancesOfQuestionsWithSameAndDeeperRosterLevelOrThrow(state,
                         underlyingQuestionIds, nearestToOuterRosterVector, questionnaire, GetRosterInstanceIds);

                     var underlyingDisabledQuestionsByRemovedRosterInstances = (
                         from question in underlyingQuestionInstances
                         where rosterInstanceIdsBeingRemoved.Contains(question.RosterVector[indexOfRosterInRosterVector])
                         where state.DisabledQuestions.Contains(ConversionHelper.ConvertIdAndRosterVectorToString(question.Id, question.RosterVector))
                         select question
                         ).ToList();

                     return underlyingDisabledQuestionsByRemovedRosterInstances;
                 }).ToList();
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

        private bool IsQuestionGoingToBeDisabled(Identity question,
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

        private static DistinctDecimalList GetRosterInstanceIds(InterviewStateDependentOnAnswers state, Guid groupId,
            decimal[] outerRosterVector)
        {
            string groupKey = ConversionHelper.ConvertIdAndRosterVectorToString(groupId, outerRosterVector);

            return state.RosterGroupInstanceIds.ContainsKey(groupKey)
                ? state.RosterGroupInstanceIds[groupKey]
                : new DistinctDecimalList();
        }


        private InterviewChanges UpdateExpressionStateWithAnswersAndGetChanges(InterviewChangeStructures interviewChanges,
            IEnumerable<RosterCalculationData> rosterDatas,
            IQuestionnaire questionnaire)
        {
            var expressionProcessorState = this.ExpressionProcessorStatePrototype.Clone();

            foreach (var changes in interviewChanges.Changes)
            {
                if (changes.ValidityChanges != null)
                {
                    changes.ValidityChanges.Clear();
                }

                if (changes.EnablementChanges != null)
                {
                    changes.EnablementChanges.Clear();
                }

                if (changes.InterviewByAnswerChanges != null)
                {
                    foreach (var answerChange in changes.InterviewByAnswerChanges)
                    {
                        UpdateExpressionProcessorStateWithAnswerChange(answerChange, expressionProcessorState);
                    }
                }

                if (changes.RosterCalculationData != null)
                {
                    foreach (var r in changes.RosterCalculationData.RosterInstancesToAdd)
                    {
                        expressionProcessorState.AddRoster(r.GroupId, r.OuterRosterVector, r.RosterInstanceId,
                            r.SortIndex);
                        if (changes.RosterCalculationData.TitlesForRosterInstancesToAdd != null)
                        {
                            if (
                                changes.RosterCalculationData.TitlesForRosterInstancesToAdd.ContainsKey(
                                    r.RosterInstanceId))
                            {
                                expressionProcessorState.UpdateRosterTitle(r.GroupId, r.OuterRosterVector,
                                    r.RosterInstanceId,
                                    changes.RosterCalculationData.TitlesForRosterInstancesToAdd[r.RosterInstanceId]);
                            }
                        }
                    }
                    changes.RosterCalculationData.RosterInstancesToRemove.ForEach(r => expressionProcessorState.RemoveRoster(r.GroupId, r.OuterRosterVector, r.RosterInstanceId));
                }
            }

            foreach (var rosterData in rosterDatas)
            {
                foreach (var r in rosterData.RosterInstancesToAdd)
                {
                    expressionProcessorState.AddRoster(r.GroupId, r.OuterRosterVector, r.RosterInstanceId,
                        r.SortIndex);
                    if (rosterData.TitlesForRosterInstancesToAdd != null)
                    {
                        if (rosterData.TitlesForRosterInstancesToAdd.ContainsKey(r.RosterInstanceId))
                        {
                            expressionProcessorState.UpdateRosterTitle(r.GroupId, r.OuterRosterVector,
                                r.RosterInstanceId,
                                rosterData.TitlesForRosterInstancesToAdd[r.RosterInstanceId]);
                        }
                    }
                }
                rosterData.RosterInstancesToRemove.ForEach(r => expressionProcessorState.RemoveRoster(r.GroupId, r.OuterRosterVector, r.RosterInstanceId));
            }

            expressionProcessorState.SaveAllCurrentStatesAsPrevious();
            EnablementChanges enablementChanges = expressionProcessorState.ProcessEnablementConditions();
            ValidityChanges validationChanges = expressionProcessorState.ProcessValidationExpressions();

            var enablementAndValidityChanges = new InterviewChanges(
                null,
                enablementChanges,
                validationChanges,
                null,
                null,
                null,
                null);
            return enablementAndValidityChanges;
        }

        private static void UpdateExpressionProcessorStateWithAnswerChange(AnswerChange answerChange,
            IInterviewExpressionState expressionProcessorState)
        {
            switch (answerChange.InterviewChangeType)
            {
                case AnswerChangeType.Text:
                    expressionProcessorState.UpdateTextAnswer(answerChange.QuestionId, answerChange.RosterVector, (string)answerChange.Answer);
                    break;
                case AnswerChangeType.DateTime:
                    expressionProcessorState.UpdateDateAnswer(answerChange.QuestionId, answerChange.RosterVector, (DateTime)answerChange.Answer);
                    break;
                case AnswerChangeType.TextList:
                    expressionProcessorState.UpdateTextListAnswer(answerChange.QuestionId, answerChange.RosterVector,
                        (Tuple<decimal, string>[])answerChange.Answer);
                    break;
                case AnswerChangeType.GeoLocation:
                    {
                        var answer = answerChange.Answer as GeoLocationPoint;
                        expressionProcessorState.UpdateGeoLocationAnswer(answerChange.QuestionId, answerChange.RosterVector,
                            answer.Latitude,
                            answer.Longitude,
                            answer.Accuracy,
                            answer.Altitude);
                    }
                    break;
                case AnswerChangeType.QRBarcode:
                    expressionProcessorState.UpdateQrBarcodeAnswer(answerChange.QuestionId, answerChange.RosterVector, (string)answerChange.Answer);
                    break;
                case AnswerChangeType.NumericInteger:
                    expressionProcessorState.UpdateNumericIntegerAnswer(answerChange.QuestionId, answerChange.RosterVector, (int)answerChange.Answer);
                    break;
                case AnswerChangeType.NumericReal:
                    expressionProcessorState.UpdateNumericRealAnswer(answerChange.QuestionId, answerChange.RosterVector, Convert.ToDouble(answerChange.Answer));
                    break;
                case AnswerChangeType.SingleOptionLinked:
                    expressionProcessorState.UpdateLinkedSingleOptionAnswer(answerChange.QuestionId, answerChange.RosterVector,
                        (decimal[])answerChange.Answer);
                    break;
                case AnswerChangeType.SingleOption:
                    expressionProcessorState.UpdateSingleOptionAnswer(answerChange.QuestionId, answerChange.RosterVector,
                        (decimal)answerChange.Answer);
                    break;
                case AnswerChangeType.MultipleOptionsLinked:
                    expressionProcessorState.UpdateLinkedMultiOptionAnswer(answerChange.QuestionId, answerChange.RosterVector,
                        (decimal[][])answerChange.Answer);
                    break;
                case AnswerChangeType.MultipleOptions:
                    expressionProcessorState.UpdateMultiOptionAnswer(answerChange.QuestionId, answerChange.RosterVector,
                        (decimal[])answerChange.Answer);
                    break;
            }
        }

        private bool HasInvalidAnswers()
        {
            return this.interviewState.InvalidAnsweredQuestions.Any();
        }
    }
}