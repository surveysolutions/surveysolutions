using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Ncqrs.Domain;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.EventBus;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Preloading;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.DataCollection.V6;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    public class Interview : AggregateRootMappedByConvention
    {
        private static readonly decimal[] EmptyRosterVector = { };

        protected Guid questionnaireId;
        protected Guid interviewerId;

        protected long questionnaireVersion;
        private bool wasCompleted;
        private bool wasHardDeleted;
        private bool receivedByInterviewer;
        protected InterviewStatus status;

        private IInterviewExpressionStateV6 expressionProcessorStatePrototype = null;
        private IInterviewExpressionStateV6 ExpressionProcessorStatePrototype
        {
            get
            {
                if (this.expressionProcessorStatePrototype == null)
                {
                    var stateProvider = this.expressionProcessorStatePrototypeProvider;
                    this.expressionProcessorStatePrototype = stateProvider.GetExpressionState(this.questionnaireId, this.questionnaireVersion);
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

        public virtual void Apply(InterviewReceivedByInterviewer @event)
        {
            receivedByInterviewer = true;
        }

        public virtual void Apply(InterviewReceivedBySupervisor @event)
        {
            receivedByInterviewer = false;
        }

        public virtual void Apply(InterviewCreated @event)
        {
            this.questionnaireId = @event.QuestionnaireId;
            this.questionnaireVersion = @event.QuestionnaireVersion;
        }

        public virtual void Apply(InterviewFromPreloadedDataCreated @event)
        {
            this.questionnaireId = @event.QuestionnaireId;
            this.questionnaireVersion = @event.QuestionnaireVersion;
        }

        public virtual void Apply(InterviewForTestingCreated @event)
        {
            this.questionnaireId = @event.QuestionnaireId;
            this.questionnaireVersion = @event.QuestionnaireVersion;
        }

        public virtual void Apply(InterviewOnClientCreated @event)
        {
            this.questionnaireId = @event.QuestionnaireId;
            this.questionnaireVersion = @event.QuestionnaireVersion;
        }

        public virtual void Apply(InterviewSynchronized @event)
        {
            this.interviewState = new InterviewStateDependentOnAnswers();
            this.questionnaireId = @event.InterviewData.QuestionnaireId;
            this.questionnaireVersion = @event.InterviewData.QuestionnaireVersion;
            this.status = @event.InterviewData.Status;
            this.wasCompleted = @event.InterviewData.WasCompleted;
            this.ExpressionProcessorStatePrototype =
                this.expressionProcessorStatePrototypeProvider.GetExpressionState(@event.InterviewData.QuestionnaireId, @event.InterviewData.QuestionnaireVersion);

            this.ExpressionProcessorStatePrototype.SetInterviewProperties(new InterviewProperties(EventSourceId));

            this.interviewState.AnswersSupportedInExpressions = @event.InterviewData.Answers == null
                ? new ConcurrentDictionary<string, object>()
                : @event.InterviewData.Answers
                    .Where(
                        question =>
                            !(question.Answer is decimal[] || question.Answer is decimal[][] ||
                                question.Answer is Tuple<decimal, string>[]))
                    .ToConcurrentDictionary(
                        question => ConversionHelper.ConvertIdAndRosterVectorToString(question.Id, question.QuestionRosterVector),
                        question => question.Answer);

            this.interviewState.LinkedSingleOptionAnswersBuggy = @event.InterviewData.Answers == null
                ? new ConcurrentDictionary<string, Tuple<Identity, RosterVector>>()
                : @event.InterviewData.Answers
                    .Where(question => question.Answer is decimal[]) // bug: here we get multioption questions as well
                    .ToConcurrentDictionary(
                        question => ConversionHelper.ConvertIdAndRosterVectorToString(question.Id, question.QuestionRosterVector),
                        question => Tuple.Create(new Identity(question.Id, question.QuestionRosterVector), (RosterVector)(decimal[])question.Answer));

            this.interviewState.LinkedMultipleOptionsAnswers = @event.InterviewData.Answers == null
                ? new ConcurrentDictionary<string, Tuple<Identity, RosterVector[]>>()
                : @event.InterviewData.Answers
                    .Where(question => question.Answer is decimal[][])
                    .ToConcurrentDictionary(
                        question => ConversionHelper.ConvertIdAndRosterVectorToString(question.Id, question.QuestionRosterVector),
                        question => Tuple.Create(
                            new Identity(question.Id, question.QuestionRosterVector),
                            ((decimal[][])question.Answer).Select(rosterVector => (RosterVector) rosterVector).ToArray()));

            this.interviewState.TextListAnswers = @event.InterviewData.Answers == null
                ? new ConcurrentDictionary<string, Tuple<decimal, string>[]>()
                : @event.InterviewData.Answers
                    .Where(question => question.Answer is Tuple<decimal, string>[])
                    .ToConcurrentDictionary(
                        question => ConversionHelper.ConvertIdAndRosterVectorToString(question.Id, question.QuestionRosterVector),
                        question => (Tuple<decimal, string>[])question.Answer
                    );

            this.interviewState.AnsweredQuestions = new ConcurrentHashSet<string>(
                @event.InterviewData.Answers.Select(
                    question => ConversionHelper.ConvertIdAndRosterVectorToString(question.Id, question.QuestionRosterVector)));

            var orderedRosterInstances = @event.InterviewData.RosterGroupInstances.SelectMany(x => x.Value).OrderBy(x=>x.OuterScopeRosterVector.Length).ToList();
            foreach (RosterSynchronizationDto roster in orderedRosterInstances)
            {
                this.ExpressionProcessorStatePrototype.AddRoster(roster.RosterId, roster.OuterScopeRosterVector, roster.RosterInstanceId, roster.SortIndex);
                this.ExpressionProcessorStatePrototype.UpdateRosterTitle(roster.RosterId, roster.OuterScopeRosterVector, roster.RosterInstanceId, roster.RosterTitle);
            }
           
            if (@event.InterviewData.Answers != null)
            {
                foreach (var question in @event.InterviewData.Answers)
                {
                    decimal[] questionRosterVector = question.QuestionRosterVector;
                    if (question.Answer is long)
                    {
                        this.ExpressionProcessorStatePrototype.UpdateNumericIntegerAnswer(question.Id, questionRosterVector, (long)question.Answer);
                    }
                    if (question.Answer is decimal || question.Answer is double)
                    {
                        this.ExpressionProcessorStatePrototype.UpdateNumericRealAnswer(question.Id, questionRosterVector, Convert.ToDouble(question.Answer));
                        this.ExpressionProcessorStatePrototype.UpdateSingleOptionAnswer(question.Id, questionRosterVector, Convert.ToDecimal(question.Answer));
                    }
                    var answer = question.Answer as string;
                    if (answer != null)
                    {
                        this.ExpressionProcessorStatePrototype.UpdateTextAnswer(question.Id, questionRosterVector, answer);
                        this.ExpressionProcessorStatePrototype.UpdateQrBarcodeAnswer(question.Id, questionRosterVector, answer);
                    }

                    if (question.Answer is decimal[])
                    {
                        this.ExpressionProcessorStatePrototype.UpdateMultiOptionAnswer(question.Id, questionRosterVector, (decimal[])(question.Answer));
                        this.ExpressionProcessorStatePrototype.UpdateLinkedSingleOptionAnswer(question.Id, questionRosterVector, (decimal[])(question.Answer));
                    }
                    var geoAnswer = question.Answer as GeoPosition;
                    if (geoAnswer != null)
                    {
                        this.ExpressionProcessorStatePrototype.UpdateGeoLocationAnswer(question.Id, questionRosterVector, geoAnswer.Latitude, geoAnswer.Longitude, geoAnswer.Accuracy, geoAnswer.Altitude);
                    }
                    if (question.Answer is DateTime)
                    {
                        this.ExpressionProcessorStatePrototype.UpdateDateAnswer(question.Id, questionRosterVector, (DateTime)question.Answer);
                    }
                    if (question.Answer is decimal[][])
                    {
                        this.ExpressionProcessorStatePrototype.UpdateLinkedMultiOptionAnswer(question.Id, questionRosterVector, (decimal[][])(question.Answer));
                    }
                    if (question.Answer is AnsweredYesNoOption[])
                    {
                        this.ExpressionProcessorStatePrototype.UpdateYesNoAnswer(question.Id, questionRosterVector, ConvertToYesNoAnswersOnly((AnsweredYesNoOption[])question.Answer));
                    }
                    if (question.Answer is Tuple<decimal, string>[])
                    {
                        this.ExpressionProcessorStatePrototype.UpdateTextListAnswer(question.Id, questionRosterVector, (Tuple<decimal, string>[])(question.Answer));
                    }
                }
            }

            this.ExpressionProcessorStatePrototype.DeclareAnswersValid(@event.InterviewData.ValidAnsweredQuestions.Select(validAnsweredQuestion => new Identity(validAnsweredQuestion.Id, validAnsweredQuestion.InterviewItemRosterVector)));
            //should call this.ExpressionProcessorStatePrototype.ApplyFailedValidations(...) when sync is ready
            this.ExpressionProcessorStatePrototype.DeclareAnswersInvalid(@event.InterviewData.InvalidAnsweredQuestions.Select(validAnsweredQuestion => new Identity(validAnsweredQuestion.Id, validAnsweredQuestion.InterviewItemRosterVector)));

            this.ExpressionProcessorStatePrototype.DisableQuestions(@event.InterviewData.DisabledQuestions.Select(validAnsweredQuestion => new Identity(validAnsweredQuestion.Id, validAnsweredQuestion.InterviewItemRosterVector)));
            this.ExpressionProcessorStatePrototype.DisableGroups(@event.InterviewData.DisabledGroups.Select(validAnsweredQuestion => new Identity(validAnsweredQuestion.Id, validAnsweredQuestion.InterviewItemRosterVector)));


            this.interviewState.DisabledGroups = ToHashSetOfIdAndRosterVectorStrings(@event.InterviewData.DisabledGroups);
            this.interviewState.DisabledQuestions = ToHashSetOfIdAndRosterVectorStrings(@event.InterviewData.DisabledQuestions);
            this.interviewState.RosterGroupInstanceIds = BuildRosterInstanceIdsFromSynchronizationDto(@event.InterviewData);
            this.interviewState.ValidAnsweredQuestions = ToHashSetOfIdAndRosterVectorStrings(@event.InterviewData.ValidAnsweredQuestions);
            this.interviewState.InvalidAnsweredQuestions = ToHashSetOfIdAndRosterVectorStrings(@event.InterviewData.InvalidAnsweredQuestions);
        }

        public virtual void Apply(SynchronizationMetadataApplied @event)
        {
            this.questionnaireId = @event.QuestionnaireId;
            this.questionnaireVersion = @event.QuestionnaireVersion;
            this.status = @event.Status;
        }

        public virtual void Apply(TextQuestionAnswered @event)
        {
            string questionKey = ConversionHelper.ConvertIdAndRosterVectorToString(@event.QuestionId, @event.RosterVector);

            this.interviewState.AnswersSupportedInExpressions[questionKey] = @event.Answer;
            this.interviewState.AnsweredQuestions.Add(questionKey);

            this.ExpressionProcessorStatePrototype.UpdateTextAnswer(@event.QuestionId, @event.RosterVector, @event.Answer);
        }

        public virtual void Apply(QRBarcodeQuestionAnswered @event)
        {
            string questionKey = ConversionHelper.ConvertIdAndRosterVectorToString(@event.QuestionId, @event.RosterVector);

            this.interviewState.AnswersSupportedInExpressions[questionKey] = @event.Answer;
            this.interviewState.AnsweredQuestions.Add(questionKey);

            this.ExpressionProcessorStatePrototype.UpdateQrBarcodeAnswer(@event.QuestionId, @event.RosterVector, @event.Answer);
        }

        public virtual void Apply(PictureQuestionAnswered @event)
        {
            string questionKey = ConversionHelper.ConvertIdAndRosterVectorToString(@event.QuestionId, @event.RosterVector);

            this.interviewState.AnswersSupportedInExpressions[questionKey] = @event.PictureFileName;
            this.interviewState.AnsweredQuestions.Add(questionKey);

            this.ExpressionProcessorStatePrototype.UpdateMediaAnswer(@event.QuestionId, @event.RosterVector, @event.PictureFileName);
        }

        public virtual void Apply(NumericRealQuestionAnswered @event)
        {
            string questionKey = ConversionHelper.ConvertIdAndRosterVectorToString(@event.QuestionId, @event.RosterVector);

            this.interviewState.AnswersSupportedInExpressions[questionKey] = @event.Answer;
            this.interviewState.AnsweredQuestions.Add(questionKey);

            this.ExpressionProcessorStatePrototype.UpdateNumericRealAnswer(@event.QuestionId, @event.RosterVector, (double)@event.Answer);
        }

        public virtual void Apply(NumericIntegerQuestionAnswered @event)
        {
            string questionKey = ConversionHelper.ConvertIdAndRosterVectorToString(@event.QuestionId, @event.RosterVector);

            this.interviewState.AnswersSupportedInExpressions[questionKey] = @event.Answer;
            this.interviewState.AnsweredQuestions.Add(questionKey);

            this.ExpressionProcessorStatePrototype.UpdateNumericIntegerAnswer(@event.QuestionId, @event.RosterVector, @event.Answer);
        }

        public virtual void Apply(DateTimeQuestionAnswered @event)
        {
            string questionKey = ConversionHelper.ConvertIdAndRosterVectorToString(@event.QuestionId, @event.RosterVector);

            this.interviewState.AnswersSupportedInExpressions[questionKey] = @event.Answer;
            this.interviewState.AnsweredQuestions.Add(questionKey);

            this.ExpressionProcessorStatePrototype.UpdateDateAnswer(@event.QuestionId, @event.RosterVector, @event.Answer);
        }

        public virtual void Apply(SingleOptionQuestionAnswered @event)
        {
            string questionKey = ConversionHelper.ConvertIdAndRosterVectorToString(@event.QuestionId, @event.RosterVector);

            this.interviewState.AnswersSupportedInExpressions[questionKey] = @event.SelectedValue;
            this.interviewState.AnsweredQuestions.Add(questionKey);

            this.ExpressionProcessorStatePrototype.UpdateSingleOptionAnswer(@event.QuestionId, @event.RosterVector, @event.SelectedValue);
        }

        public virtual void Apply(MultipleOptionsQuestionAnswered @event)
        {
            string questionKey = ConversionHelper.ConvertIdAndRosterVectorToString(@event.QuestionId, @event.RosterVector);

            this.interviewState.AnswersSupportedInExpressions[questionKey] = @event.SelectedValues;

            if (@event.SelectedValues.Length != 0)
            {
                this.interviewState.AnsweredQuestions.Add(questionKey);
            }
            else
            {
                this.interviewState.AnsweredQuestions.Remove(questionKey);
            }
            this.ExpressionProcessorStatePrototype.UpdateMultiOptionAnswer(@event.QuestionId, @event.RosterVector, @event.SelectedValues);
        }

        public virtual void Apply(YesNoQuestionAnswered @event)
        {
            string questionKey = ConversionHelper.ConvertIdAndRosterVectorToString(@event.QuestionId, @event.RosterVector);

            this.interviewState.AnswersSupportedInExpressions[questionKey] = @event.AnsweredOptions;

            if (@event.AnsweredOptions.Length != 0)
            {
                this.interviewState.AnsweredQuestions.Add(questionKey);
            }
            else
            {
                this.interviewState.AnsweredQuestions.Remove(questionKey);
            }

            var yesNoAnswers = ConvertToYesNoAnswersOnly(@event.AnsweredOptions);
            this.ExpressionProcessorStatePrototype.UpdateYesNoAnswer(@event.QuestionId, @event.RosterVector, yesNoAnswers);
        }

        public virtual void Apply(GeoLocationQuestionAnswered @event)
        {
            string questionKey = ConversionHelper.ConvertIdAndRosterVectorToString(@event.QuestionId, @event.RosterVector);

            this.interviewState.AnswersSupportedInExpressions[questionKey] = new GeoPosition(
                @event.Latitude, @event.Longitude, @event.Accuracy, @event.Altitude, @event.Timestamp);
                
            this.interviewState.AnsweredQuestions.Add(questionKey);

            this.ExpressionProcessorStatePrototype.UpdateGeoLocationAnswer(@event.QuestionId, @event.RosterVector, @event.Latitude,
                @event.Longitude, @event.Accuracy, @event.Altitude);
        }

        public virtual void Apply(TextListQuestionAnswered @event)
        {
            string questionKey = ConversionHelper.ConvertIdAndRosterVectorToString(@event.QuestionId, @event.RosterVector);
            this.interviewState.TextListAnswers[questionKey] = @event.Answers;
            this.interviewState.AnsweredQuestions.Add(questionKey);

            this.ExpressionProcessorStatePrototype.UpdateTextListAnswer(@event.QuestionId, @event.RosterVector, @event.Answers);
        }

        public virtual void Apply(SingleOptionLinkedQuestionAnswered @event)
        {
            string questionKey = ConversionHelper.ConvertIdAndRosterVectorToString(@event.QuestionId, @event.RosterVector);

            this.interviewState.LinkedSingleOptionAnswersBuggy[questionKey] = Tuple.Create(
                new Identity(@event.QuestionId, @event.RosterVector),
                (RosterVector) @event.SelectedRosterVector);

            this.interviewState.AnsweredQuestions.Add(questionKey);

            this.ExpressionProcessorStatePrototype.UpdateLinkedSingleOptionAnswer(@event.QuestionId, @event.RosterVector, @event.SelectedRosterVector);
        }

        public virtual void Apply(MultipleOptionsLinkedQuestionAnswered @event)
        {
            string questionKey = ConversionHelper.ConvertIdAndRosterVectorToString(@event.QuestionId, @event.RosterVector);

            this.interviewState.LinkedMultipleOptionsAnswers[questionKey] = Tuple.Create(
                new Identity(@event.QuestionId, @event.RosterVector),
                @event.SelectedRosterVectors.Select(rosterVector => (RosterVector)rosterVector).ToArray());

            if (@event.SelectedRosterVectors.Length != 0)
            {
                this.interviewState.AnsweredQuestions.Add(questionKey);
            }
            else
            {
                this.interviewState.AnsweredQuestions.Remove(questionKey);
            }

            this.ExpressionProcessorStatePrototype.UpdateLinkedMultiOptionAnswer(@event.QuestionId, @event.RosterVector, @event.SelectedRosterVectors);
        }

        public virtual void Apply(AnswersDeclaredValid @event)
        {
            this.interviewState.DeclareAnswersValid(@event.Questions);
            this.ExpressionProcessorStatePrototype.DeclareAnswersValid(@event.Questions);
        }

        public virtual void Apply(AnswersDeclaredInvalid @event)
        {
            this.interviewState.DeclareAnswersInvalid(@event.FailedValidationConditions.Keys);

            if (@event.FailedValidationConditions.Count > 0)
            {
                this.ExpressionProcessorStatePrototype.ApplyFailedValidations(@event.FailedValidationConditions);
            }
            else //handling of old events
            {
                this.ExpressionProcessorStatePrototype.DeclareAnswersInvalid(@event.FailedValidationConditions.Keys);
            }
        }

        public virtual void Apply(GroupsDisabled @event)
        {
            this.interviewState.DisableGroups(@event.Groups);

            this.ExpressionProcessorStatePrototype.DisableGroups(@event.Groups);
        }

        public virtual void Apply(GroupsEnabled @event)
        {
            this.interviewState.EnableGroups(@event.Groups);

            this.ExpressionProcessorStatePrototype.EnableGroups(@event.Groups);
        }

        public virtual void Apply(QuestionsDisabled @event)
        {
            this.interviewState.DisableQuestions(@event.Questions);

            this.ExpressionProcessorStatePrototype.DisableQuestions(@event.Questions);
        }

        public virtual void Apply(QuestionsEnabled @event)
        {
            this.interviewState.EnableQuestions(@event.Questions);

            this.ExpressionProcessorStatePrototype.EnableQuestions(@event.Questions);

        }

        public virtual void Apply(AnswerCommented @event)
        {
            this.interviewState.AnswerComments.Add(new AnswerComment(@event.UserId, @event.CommentTime, @event.Comment, @event.QuestionId, @event.RosterVector));
        }

        public virtual void Apply(FlagSetToAnswer @event) { }

        public virtual void Apply(FlagRemovedFromAnswer @event) { }

        public virtual void Apply(SubstitutionTitlesChanged @event) { }

        public virtual void Apply(GroupPropagated @event)
        {
            string rosterGroupKey = ConversionHelper.ConvertIdAndRosterVectorToString(@event.GroupId, @event.OuterScopeRosterVector);
            var rosterRowInstances = new ConcurrentDistinctList<decimal>();

            for (int i = 0; i < @event.Count; i++)
            {
                rosterRowInstances.Add(i);
            }

            this.interviewState.RosterGroupInstanceIds[rosterGroupKey] = rosterRowInstances;

            //expressionProcessorStatePrototype could also be changed but it's an old code.
        }

        public virtual void Apply(RosterInstancesTitleChanged @event)
        {
            this.interviewState.ChangeRosterTitles(@event.ChangedInstances);
            foreach (var instance in @event.ChangedInstances)
            {
                this.ExpressionProcessorStatePrototype.UpdateRosterTitle(instance.RosterInstance.GroupId, instance.RosterInstance.OuterRosterVector, instance.RosterInstance.RosterInstanceId, instance.Title);
            }
        }

        public virtual void Apply(RosterInstancesAdded @event)
        {
            this.interviewState.AddRosterInstances(@event.Instances);

            foreach (var instance in @event.Instances)
            {
                this.ExpressionProcessorStatePrototype.AddRoster(instance.GroupId, instance.OuterRosterVector, instance.RosterInstanceId, instance.SortIndex);
            }
        }

        public virtual void Apply(RosterInstancesRemoved @event)
        {
            this.interviewState.RemoveRosterInstances(@event.Instances);
            foreach (var instance in @event.Instances)
            {
                this.ExpressionProcessorStatePrototype.RemoveRoster(instance.GroupId, instance.OuterRosterVector, instance.RosterInstanceId);
            }
        }

        public virtual void Apply(InterviewStatusChanged @event)
        {
            this.status = @event.Status;
        }

        public virtual void Apply(SupervisorAssigned @event) { }

        public virtual void Apply(InterviewerAssigned @event)
        {
            this.interviewerId = @event.InterviewerId;
            this.receivedByInterviewer = false;
        }

        public virtual void Apply(InterviewDeleted @event) { }

        public virtual void Apply(InterviewHardDeleted @event)
        {
            wasHardDeleted = true;
        }

        public virtual void Apply(InterviewSentToHeadquarters @event) { }

        public virtual void Apply(InterviewRestored @event) { }

        public virtual void Apply(InterviewCompleted @event)
        {
            this.wasCompleted = true;
        }

        public virtual void Apply(InterviewRestarted @event) { }

        public virtual void Apply(InterviewApproved @event) { }

        public virtual void Apply(InterviewApprovedByHQ @event) { }

        public virtual void Apply(UnapprovedByHeadquarters @event) { }

        public virtual void Apply(InterviewRejected @event)
        {
            this.wasCompleted = false;
        }

        public virtual void Apply(InterviewRejectedByHQ @event) { }

        public virtual void Apply(InterviewDeclaredValid @event) { }

        public virtual void Apply(InterviewDeclaredInvalid @event) { }

        public virtual void Apply(AnswersRemoved @event)
        {
            this.interviewState.RemoveAnswers(@event.Questions);

            foreach (var identity in @event.Questions)
            {
                RemoveAnswerFromExpressionProcessorState(this.ExpressionProcessorStatePrototype, identity.Id,
                    identity.RosterVector);
            }
        }

        public virtual void Apply(AnswerRemoved @event)
        {
            this.interviewState.RemoveAnswers(new[] { new Identity(@event.QuestionId, @event.RosterVector) });
            RemoveAnswerFromExpressionProcessorState(this.ExpressionProcessorStatePrototype, @event.QuestionId, @event.RosterVector);
        }

        private void RemoveAnswerFromExpressionProcessorState(IInterviewExpressionStateV6 state, Guid questionId, RosterVector rosterVector)
        {
            state.UpdateNumericIntegerAnswer(questionId, rosterVector, null);
            state.UpdateNumericRealAnswer(questionId, rosterVector, null);
            state.UpdateDateAnswer(questionId, rosterVector, null);
            state.UpdateMediaAnswer(questionId, rosterVector, null);
            state.UpdateTextAnswer(questionId, rosterVector, null);
            state.UpdateQrBarcodeAnswer(questionId, rosterVector, null);
            state.UpdateSingleOptionAnswer(questionId, rosterVector, null);
            state.UpdateMultiOptionAnswer(questionId, rosterVector, null);
            state.UpdateGeoLocationAnswer(questionId, rosterVector, 0, 0, 0, 0);
            state.UpdateTextListAnswer(questionId, rosterVector, null);
            state.UpdateLinkedSingleOptionAnswer(questionId, rosterVector, null);
            state.UpdateLinkedMultiOptionAnswer(questionId, rosterVector, null);
            state.UpdateYesNoAnswer(questionId, rosterVector, null);
        }

        #region Dependencies

        private readonly ILogger logger;

        /// <remarks>
        /// Repository operations are time-consuming.
        /// So this repository may be used only in command handlers.
        /// And should never be used in event handlers!!
        /// </remarks>
        private readonly IPlainQuestionnaireRepository questionnaireRepository;

        private readonly IInterviewExpressionStatePrototypeProvider expressionProcessorStatePrototypeProvider;

        #endregion

        public Interview(ILogger logger, IPlainQuestionnaireRepository questionnaireRepository, IInterviewExpressionStatePrototypeProvider expressionProcessorStatePrototypeProvider)
        {
            this.logger = logger;
            this.questionnaireRepository = questionnaireRepository;
            this.expressionProcessorStatePrototypeProvider = expressionProcessorStatePrototypeProvider;
        }

        private void SetQuestionnaireProperties(Guid questionnaireId, long questionnaireVersion)
        {
            this.questionnaireId = questionnaireId;
            this.questionnaireVersion = questionnaireVersion;
        }

        public void CreateInterviewByPrefilledQuestions(QuestionnaireIdentity questionnaireIdentity, Guid headquartersId,
            Guid supervisorId, Guid? interviewerId, DateTime answersTime,
            Dictionary<Guid, object> answersOnPrefilledQuestions)
        {
            this.SetQuestionnaireProperties(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version);

            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow(
                questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version);

            var interviewChangeStructures = new InterviewChangeStructures();

            var fixedRosterCalculationDatas = this.CalculateFixedRostersData(interviewChangeStructures.State,
                questionnaire);

            foreach (var fixedRosterCalculationData in fixedRosterCalculationDatas)
            {
                var fixedRosterChanges = new InterviewChanges(null, null, null, fixedRosterCalculationData, null, null,
                    null, null);
                interviewChangeStructures.State.ApplyInterviewChanges(fixedRosterChanges);
                interviewChangeStructures.Changes.Add(fixedRosterChanges);
            }

            var interviewExpressionState = new InterviewExpressionStateForPreloading();
            
            this.ValidatePrefilledQuestions(questionnaire, answersOnPrefilledQuestions, RosterVector.Empty,
                interviewChangeStructures.State, false);

            foreach (var newAnswer in answersOnPrefilledQuestions)
            {
                string key = ConversionHelper.ConvertIdAndRosterVectorToString(newAnswer.Key, RosterVector.Empty);

                interviewChangeStructures.State.AnswersSupportedInExpressions[key] = newAnswer.Value;
                interviewChangeStructures.State.AnsweredQuestions.Add(key);
            }

            this.CalculateChangesByFeaturedQuestion(interviewExpressionState, interviewChangeStructures, headquartersId,
                questionnaire, answersOnPrefilledQuestions, answersTime,
                answersOnPrefilledQuestions.ToDictionary(x => new Identity(x.Key, RosterVector.Empty), x => x.Value),
                RosterVector.Empty);


            var enablementAndValidityChanges = this.UpdateExpressionStateWithAnswersAndGetChanges(
                interviewChangeStructures,
                fixedRosterCalculationDatas);

            //apply events
            this.ApplyEvent(new InterviewFromPreloadedDataCreated(headquartersId, questionnaireId, questionnaire.Version));
            this.ApplyInterviewChanges(interviewChangeStructures.Changes);
            this.ApplyInterviewChanges(enablementAndValidityChanges);
            this.ApplyEvent(new SupervisorAssigned(headquartersId, supervisorId));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.SupervisorAssigned, comment: null));

            if (interviewerId.HasValue)
            {
                this.ApplyEvent(new InterviewerAssigned(headquartersId, interviewerId.Value, answersTime));
                this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.InterviewerAssigned, comment: null));
            }
        }

        public void CreateInterviewWithPreloadedData(Guid questionnaireId, long version, PreloadedDataDto preloadedData, Guid supervisorId, DateTime answersTime, Guid userId)
        {
            this.SetQuestionnaireProperties(questionnaireId, version);

            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow(questionnaireId, version);

            var interviewChangeStructures = new InterviewChangeStructures();

            var fixedRosterCalculationDatas = this.CalculateFixedRostersData(interviewChangeStructures.State, questionnaire);

            foreach (var fixedRosterCalculationData in fixedRosterCalculationDatas)
            {
                var fixedRosterChanges = new InterviewChanges(null, null, null, fixedRosterCalculationData, null, null, null, null);
                interviewChangeStructures.State.ApplyInterviewChanges(fixedRosterChanges);
                interviewChangeStructures.Changes.Add(fixedRosterChanges);
            }

            var orderedData = preloadedData.Data.OrderBy(x => x.RosterVector.Length).ToArray();

            var interviewExpressionStateForPreloading = new InterviewExpressionStateForPreloading();

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

                this.CalculateChangesByFeaturedQuestion(interviewExpressionStateForPreloading, interviewChangeStructures, userId, questionnaire, answersToFeaturedQuestions,
                    answersTime,
                    newAnswers, preloadedLevel.RosterVector);
            }

            var enablementAndValidityChanges = this.UpdateExpressionStateWithAnswersAndGetChanges(
                interviewChangeStructures,
                fixedRosterCalculationDatas);

            //apply events
            this.ApplyEvent(new InterviewFromPreloadedDataCreated(userId, questionnaireId, questionnaire.Version));
            this.ApplyInterviewChanges(interviewChangeStructures.Changes);
            this.ApplyInterviewChanges(enablementAndValidityChanges);
            this.ApplyEvent(new SupervisorAssigned(userId, supervisorId));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.SupervisorAssigned, comment: null));
        }

        public void CreateInterview(Guid questionnaireId, long questionnaireVersion, Guid supervisorId,
            Dictionary<Guid, object> answersToFeaturedQuestions, DateTime answersTime, Guid userId)
        {
            this.SetQuestionnaireProperties(questionnaireId, questionnaireVersion);

            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow(questionnaireId, questionnaireVersion);

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
                fixedRosterCalculationDatas);

            //apply events
            this.ApplyEvent(new InterviewCreated(userId, questionnaireId, questionnaire.Version));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.Created, comment: null));
            this.ApplyInterviewChanges(interviewChangeStructures.Changes);
            this.ApplyRostersEvents(fixedRosterCalculationDatas.ToArray());
            this.ApplyInterviewChanges(enablementAndValidityChanges);
            this.ApplyEvent(new SupervisorAssigned(userId, supervisorId));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.SupervisorAssigned, comment: null));
        }

        public void CreateInterviewOnClient(QuestionnaireIdentity questionnaireIdentity, Guid supervisorId, DateTime answersTime, Guid userId)
        {
            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version);
            this.SetQuestionnaireProperties(questionnaireIdentity.QuestionnaireId, questionnaire.Version);

            InterviewChangeStructures interviewChangeStructures = new InterviewChangeStructures();

            var fixedRosterCalculationDatas = this.CalculateFixedRostersData(interviewChangeStructures.State, questionnaire);

            var enablementAndValidityChanges = this.UpdateExpressionStateWithAnswersAndGetChanges(
                interviewChangeStructures,
                fixedRosterCalculationDatas);

            //apply events
            this.ApplyEvent(new InterviewOnClientCreated(userId, questionnaireIdentity.QuestionnaireId, questionnaire.Version));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.Created, comment: null));

            this.ApplyInterviewChanges(interviewChangeStructures.Changes);
            this.ApplyRostersEvents(fixedRosterCalculationDatas.ToArray());
            this.ApplyInterviewChanges(enablementAndValidityChanges);
            this.ApplyEvent(new SupervisorAssigned(userId, supervisorId));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.SupervisorAssigned, comment: null));

            this.ApplyEvent(new InterviewerAssigned(userId, userId, answersTime));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.InterviewerAssigned, comment: null));
        }

        public void CreateInterviewCreatedOnClient(Guid questionnaireId, long questionnaireVersion,
            InterviewStatus interviewStatus,
            AnsweredQuestionSynchronizationDto[] featuredQuestionsMeta, bool isValid, Guid userId)
        {
            this.SetQuestionnaireProperties(questionnaireId, questionnaireVersion);

            this.GetQuestionnaireOrThrow(questionnaireId, questionnaireVersion);
            this.ApplyEvent(new InterviewOnClientCreated(userId, questionnaireId, questionnaireVersion));
            this.ApplyEvent(new SynchronizationMetadataApplied(userId, questionnaireId, questionnaireVersion,
                interviewStatus, featuredQuestionsMeta, true, null, null, null));
            this.ApplyEvent(new InterviewStatusChanged(interviewStatus, string.Empty));
            this.ApplyValidationEvent(isValid);
        }

        #region StaticMethods

        private static ConcurrentDictionary<string, ConcurrentDistinctList<decimal>> BuildRosterInstanceIdsFromSynchronizationDto(InterviewSynchronizationDto synchronizationDto)
        {
            return synchronizationDto.RosterGroupInstances.ToConcurrentDictionary(
                pair => ConversionHelper.ConvertIdAndRosterVectorToString(pair.Key.Id, pair.Key.InterviewItemRosterVector),
                pair => new ConcurrentDistinctList<decimal>(pair.Value.Select(rosterInstance => rosterInstance.RosterInstanceId).ToList()));
        }

        private string GetLinkedQuestionAnswerFormattedAsRosterTitle(IReadOnlyInterviewStateDependentOnAnswers state, Identity linkedQuestion, IQuestionnaire questionnaire)
        {
            // set of answers that support expressions includes set of answers that may be linked to, so following line is correct
            object answer = GetEnabledQuestionAnswerSupportedInExpressions(state, linkedQuestion, questionnaire);

            return AnswerUtils.AnswerToString(answer);
        }

        private bool IsQuestionOrParentGroupDisabled(Identity question, IQuestionnaire questionnaire, IReadOnlyInterviewStateDependentOnAnswers state)
        {
            if (state.IsQuestionDisabled(question))
                return true;

            IEnumerable<Guid> parentGroupIds = questionnaire.GetAllParentGroupsForQuestion(question.Id);
            IEnumerable<Identity> parentGroups = GetInstancesOfGroupsWithSameAndUpperRosterLevelOrThrow(parentGroupIds,
                question.RosterVector, questionnaire).ToList();

            var result = parentGroups.Any(state.IsGroupDisabled);
            return result;
        }

        private object GetEnabledQuestionAnswerSupportedInExpressions(IReadOnlyInterviewStateDependentOnAnswers state, Identity question, IQuestionnaire questionnaire)
        {
            return this.IsQuestionOrParentGroupDisabled(question, questionnaire, state)
                ? null
                : state.GetAnswerSupportedInExpressions(question);
        }

        /// <remarks>
        /// If roster vector should be extended, result will be a set of vectors depending on roster count of corresponding groups.
        /// </remarks>
        protected IEnumerable<RosterVector> ExtendRosterVector(IReadOnlyInterviewStateDependentOnAnswers state, RosterVector rosterVector, int length, Guid[] rosterGroupsStartingFromTop)
        {
            if (length < rosterVector.Length)
                throw new ArgumentException(string.Format(
                    "Cannot extend vector with length {0} to smaller length {1}. InterviewId: {2}", rosterVector.Length, length, EventSourceId));

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
                    yield return ((RosterVector) outerVectorForExtend).ExtendWithOneCoordinate(rosterInstanceId);
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

        private static bool AreEqualRosterVectors(RosterVector rosterVectorA, RosterVector rosterVectorB)
        {
            return rosterVectorA.SequenceEqual(rosterVectorB);
        }

        private static Tuple<decimal[], decimal> SplitRosterVectorOntoOuterVectorAndRosterInstanceId(RosterVector rosterVector)
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

        private static ConcurrentHashSet<string> ToHashSetOfIdAndRosterVectorStrings(IEnumerable<InterviewItemId> synchronizationIdentities)
        {
            return new ConcurrentHashSet<string>(
                synchronizationIdentities.Select(
                    question => ConversionHelper.ConvertIdAndRosterVectorToString(question.Id, question.InterviewItemRosterVector)));
        }

        private static Identity ToIdentity(InterviewItemId synchronizationIdentity)
        {
            return new Identity(synchronizationIdentity.Id, synchronizationIdentity.InterviewItemRosterVector);
        }

        private Identity GetInstanceOfQuestionWithSameAndUpperRosterLevelOrThrow(Guid questionId,
            RosterVector rosterVector, IQuestionnaire questionnare)
        {
            int vectorRosterLevel = rosterVector.Length;
            int questionRosterLevel = questionnare.GetRosterLevelForQuestion(questionId);

            if (questionRosterLevel > vectorRosterLevel)
                throw new InterviewException(string.Format(
                    "Question {0} expected to have roster level not deeper than {1} but it is {2}. InterviewId: {3}",
                    FormatQuestionForException(questionId, questionnare), vectorRosterLevel, questionRosterLevel, EventSourceId));

            decimal[] questionRosterVector = rosterVector.Shrink(questionRosterLevel);

            return new Identity(questionId, questionRosterVector);
        }

        protected IEnumerable<Identity> GetInstancesOfQuestionsWithSameAndDeeperRosterLevelOrThrow(
            IReadOnlyInterviewStateDependentOnAnswers state,
            IEnumerable<Guid> questionIds, RosterVector rosterVector, IQuestionnaire questionnare)
        {
            return questionIds.SelectMany(questionId =>
                GetInstancesOfQuestionsWithSameAndDeeperRosterLevelOrThrow(state, questionId, rosterVector, questionnare));
        }

        protected IEnumerable<Identity> GetInstancesOfEntitiesWithSameAndDeeperRosterLevelOrThrow(
            IReadOnlyInterviewStateDependentOnAnswers state,
            IEnumerable<Guid> entityIds, RosterVector rosterVector, IQuestionnaire questionnare)
        {
            return entityIds.SelectMany(entityId =>
                GetInstancesOfEntitiesWithSameAndDeeperRosterLevelOrThrow(state, entityId, rosterVector, questionnare));
        }

        protected IEnumerable<Identity> GetInstancesOfQuestionsWithSameAndDeeperRosterLevelOrThrow(
            IReadOnlyInterviewStateDependentOnAnswers state,
            Guid questionId, RosterVector rosterVector, IQuestionnaire questionnare)
        {
            int vectorRosterLevel = rosterVector.Length;
            int questionRosterLevel = questionnare.GetRosterLevelForQuestion(questionId);

            if (questionRosterLevel < vectorRosterLevel)
                throw new InterviewException(string.Format(
                    "Question {0} expected to have roster level not upper than {1} but it is {2}. InterviewId: {3}",
                    FormatQuestionForException(questionId, questionnare), vectorRosterLevel, questionRosterLevel, EventSourceId));

            Guid[] parentRosterGroupsStartingFromTop =
                questionnare.GetRostersFromTopToSpecifiedQuestion(questionId).ToArray();

            IEnumerable<RosterVector> questionRosterVectors = ExtendRosterVector(state,
                rosterVector, questionRosterLevel, parentRosterGroupsStartingFromTop);

            foreach (decimal[] questionRosterVector in questionRosterVectors)
            {
                yield return new Identity(questionId, questionRosterVector);
            }
        }

        protected IEnumerable<Identity> GetInstancesOfEntitiesWithSameAndDeeperRosterLevelOrThrow(
            IReadOnlyInterviewStateDependentOnAnswers state,
            Guid entityId, RosterVector rosterVector, IQuestionnaire questionnare)
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

      
        private IEnumerable<Identity> GetInstancesOfGroupsWithSameAndUpperRosterLevelOrThrow(
            IEnumerable<Guid> groupIds, RosterVector rosterVector, IQuestionnaire questionnaire)
        {
            return groupIds.Select(groupId => this.GetInstanceOfGroupWithSameAndUpperRosterLevelOrThrow(groupId, rosterVector, questionnaire));
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

        private static bool IsQuestionUnderRosterGroup(IQuestionnaire questionnaire, Guid questionId)
        {
            return questionnaire.GetRostersFromTopToSpecifiedQuestion(questionId).Any();
        }

        #endregion

        #region Handlers

        #region AnsweringQuestions

        public void AnswerTextQuestion(Guid userId, Guid questionId, RosterVector rosterVector, DateTime answerTime, string answer)
        {
            ThrowIfInterviewHardDeleted();

            var answeredQuestion = new Identity(questionId, rosterVector);
            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);

            this.CheckTextQuestionInvariants(questionId, rosterVector, questionnaire, answeredQuestion, this.interviewState);
            this.ThrowIfInterviewReceivedByInterviewer();

            var expressionProcessorState = this.PrepareExpressionProcessorStateForCalculations();

            InterviewChanges interviewChanges = this.CalculateInterviewChangesOnAnswerTextQuestion(expressionProcessorState, this.interviewState, userId, questionId,
                rosterVector, answerTime, answer, questionnaire);

            this.ApplyInterviewChanges(interviewChanges);
        }

        public void AnswerNumericRealQuestion(Guid userId, Guid questionId, RosterVector rosterVector, DateTime answerTime, decimal answer)
        {
            ThrowIfInterviewHardDeleted();

            var answeredQuestion = new Identity(questionId, rosterVector);

            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);

            this.CheckNumericRealQuestionInvariants(questionId, rosterVector, answer, questionnaire, answeredQuestion, this.interviewState);
            this.ThrowIfInterviewReceivedByInterviewer();

            var expressionProcessorState = this.PrepareExpressionProcessorStateForCalculations();

            InterviewChanges interviewChanges = this.CalculateInterviewChangesOnAnswerNumericRealQuestion(expressionProcessorState, this.interviewState, userId,
                questionId, rosterVector, answerTime, answer, questionnaire);

            this.ApplyInterviewChanges(interviewChanges);
        }

        public void AnswerQRBarcodeQuestion(Guid userId, Guid questionId, RosterVector rosterVector, DateTime answerTime, string answer)
        {
            ThrowIfInterviewHardDeleted();

            var answeredQuestion = new Identity(questionId, rosterVector);

            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);
            ThrowIfQuestionDoesNotExist(questionId, questionnaire);
            this.ThrowIfRosterVectorIsIncorrect(this.interviewState, questionId, rosterVector, questionnaire);
            this.ThrowIfQuestionTypeIsNotOneOfExpected(questionId, questionnaire, QuestionType.QRBarcode);
            ThrowIfQuestionOrParentGroupIsDisabled(this.interviewState, answeredQuestion, questionnaire);
            this.ThrowIfInterviewReceivedByInterviewer();

            var expressionProcessorState = this.PrepareExpressionProcessorStateForCalculations();

            InterviewChanges interviewChanges = this.CalculateInterviewChangesOnAnswerQRBarcodeQuestion(expressionProcessorState, this.interviewState, userId,
                questionId, rosterVector, answerTime, answer, questionnaire);

            this.ApplyInterviewChanges(interviewChanges);
        }

        public void AnswerPictureQuestion(Guid userId, Guid questionId, RosterVector rosterVector, DateTime answerTime, string pictureFileName)
        {
            ThrowIfInterviewHardDeleted();

            var answeredQuestion = new Identity(questionId, rosterVector);

            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);
            ThrowIfQuestionDoesNotExist(questionId, questionnaire);
            this.ThrowIfRosterVectorIsIncorrect(this.interviewState, questionId, rosterVector, questionnaire);
            this.ThrowIfQuestionTypeIsNotOneOfExpected(questionId, questionnaire, QuestionType.Multimedia);
            ThrowIfQuestionOrParentGroupIsDisabled(this.interviewState, answeredQuestion, questionnaire);
            this.ThrowIfInterviewReceivedByInterviewer();

            var expressionProcessorState = this.ExpressionProcessorStatePrototype.Clone();

            this.CalculateInterviewChangesOnAnswerPictureQuestion(expressionProcessorState, userId, questionId, rosterVector, answerTime, pictureFileName, questionnaire);
        }

        public void AnswerNumericIntegerQuestion(Guid userId, Guid questionId, RosterVector rosterVector, DateTime answerTime, int answer)
        {
            ThrowIfInterviewHardDeleted();

            var answeredQuestion = new Identity(questionId, rosterVector);
            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);
            this.CheckNumericIntegerQuestionInvariants(questionId, rosterVector, answer, questionnaire, answeredQuestion,
                this.interviewState);
            this.ThrowIfInterviewReceivedByInterviewer();

            Func<IReadOnlyInterviewStateDependentOnAnswers, Identity, object> getAnswer =
                (currentState, question) => question == answeredQuestion
                    ? answer
                    : this.GetEnabledQuestionAnswerSupportedInExpressions(this.interviewState, question, questionnaire);

            var expressionProcessorState = this.ExpressionProcessorStatePrototype.Clone();

            InterviewChanges interviewChanges = this.CalculateInterviewChangesOnAnswerNumericIntegerQuestion(expressionProcessorState, this.interviewState, userId,
                questionId, rosterVector, answerTime, answer, getAnswer, questionnaire);
            
            this.ApplyInterviewChanges(interviewChanges);
        }

        public void AnswerMultipleOptionsQuestion(Guid userId, Guid questionId, RosterVector rosterVector, DateTime answerTime, decimal[] selectedValues)
        {
            ThrowIfInterviewHardDeleted();

            var answeredQuestion = new Identity(questionId, rosterVector);

            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);
            this.CheckMultipleOptionQuestionInvariants(questionId, rosterVector, selectedValues, questionnaire, answeredQuestion,
                this.interviewState);
            this.ThrowIfInterviewReceivedByInterviewer();

            Func<IReadOnlyInterviewStateDependentOnAnswers, Identity, object> getAnswer =
                (currentState, question) => question == answeredQuestion
                    ? selectedValues
                    : this.GetEnabledQuestionAnswerSupportedInExpressions(this.interviewState, question, questionnaire);

            var expressionProcessorState = this.ExpressionProcessorStatePrototype.Clone();

            InterviewChanges interviewChanges = this.CalculateInterviewChangesOnAnswerMultipleOptionsQuestion(expressionProcessorState, this.interviewState, userId,
                questionId, rosterVector, answerTime, selectedValues, getAnswer, questionnaire);

            this.ApplyInterviewChanges(interviewChanges);
        }

        public void AnswerYesNoQuestion(AnswerYesNoQuestion command)
        {
            ThrowIfInterviewHardDeleted();

            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);
            this.CheckYesNoQuestionInvariants(command.Question, command.AnsweredOptions, questionnaire, this.interviewState);
            this.ThrowIfInterviewReceivedByInterviewer();

            AnsweredYesNoOption[] answer = command.AnsweredOptions.ToArray();

            Func<IReadOnlyInterviewStateDependentOnAnswers, Identity, object> getAnswer =
                (currentState, question) => question == command.Question
                    ? answer
                    : this.GetEnabledQuestionAnswerSupportedInExpressions(this.interviewState, question, questionnaire);

            var expressionProcessorState = this.ExpressionProcessorStatePrototype.Clone();

            InterviewChanges interviewChanges = this.CalculateInterviewChangesOnYesNoQuestionAnswer(
                command.Question, answer, command.AnswerTime, command.UserId, questionnaire, expressionProcessorState, this.interviewState, getAnswer);

            this.ApplyInterviewChanges(interviewChanges);
        }

        public void AnswerMultipleOptionsLinkedQuestion(Guid userId, Guid questionId, RosterVector rosterVector, DateTime answerTime, decimal[][] selectedRosterVectors)
        {
            ThrowIfInterviewHardDeleted();

            var answeredQuestion = new Identity(questionId, rosterVector);

            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);

            this.CheckLinkedMultiOptionQuestionInvariants(questionId, rosterVector, selectedRosterVectors, questionnaire, answeredQuestion);
            this.ThrowIfInterviewReceivedByInterviewer();

            string answerFormattedAsRosterTitle;
            if (questionnaire.IsQuestionLinkedToRoster(questionId))
            {
                Guid linkedRosterId = questionnaire.GetRosterReferencedByLinkedQuestion(questionId);
                answerFormattedAsRosterTitle = string.Join(", ", selectedRosterVectors.Select(selectedRosterVector => this.interviewState.GetRosterTitle(linkedRosterId,
                    new RosterVector(selectedRosterVector))));
            }
            else
            {
                Guid linkedQuestionId = this.GetLinkedQuestionIdOrThrow(questionId, questionnaire);
                var answeredLinkedQuestions = selectedRosterVectors.Select(selectedRosterVector => new Identity(linkedQuestionId, selectedRosterVector));
                answerFormattedAsRosterTitle = string.Join(", ", answeredLinkedQuestions.Select(q => GetLinkedQuestionAnswerFormattedAsRosterTitle(this.interviewState, q, questionnaire)));
            }

            IInterviewExpressionStateV6 expressionProcessorState = this.ExpressionProcessorStatePrototype.Clone();

            expressionProcessorState.UpdateLinkedMultiOptionAnswer(questionId, rosterVector, selectedRosterVectors);

            InterviewChanges interviewChanges = this.CalculateInterviewChangesOnAnswerQuestion(this.interviewState, userId, questionId, rosterVector, selectedRosterVectors, answerFormattedAsRosterTitle, AnswerChangeType.MultipleOptionsLinked, answerTime, questionnaire, expressionProcessorState);


            this.ApplyInterviewChanges(interviewChanges);
        }

        public void AnswerDateTimeQuestion(Guid userId, Guid questionId, RosterVector rosterVector, DateTime answerTime, DateTime answer)
        {
            ThrowIfInterviewHardDeleted();
            var answeredQuestion = new Identity(questionId, rosterVector);

            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);
            this.CheckDateTimeQuestionInvariants(questionId, rosterVector, questionnaire, answeredQuestion, this.interviewState);
            this.ThrowIfInterviewReceivedByInterviewer();

            var expressionProcessorState = this.PrepareExpressionProcessorStateForCalculations();

            InterviewChanges interviewChanges = this.CalculateInterviewChangesOnAnswerDateTimeQuestion(expressionProcessorState, this.interviewState, userId,
                questionId, rosterVector, answerTime, answer, questionnaire);

            this.ApplyInterviewChanges(interviewChanges);
        }

        public void AnswerSingleOptionQuestion(Guid userId, Guid questionId, RosterVector rosterVector, DateTime answerTime, decimal selectedValue)
        {
            ThrowIfInterviewHardDeleted();
            var answeredQuestion = new Identity(questionId, rosterVector);

            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);
            CheckSingleOptionQuestionInvariants(questionId, rosterVector, selectedValue, questionnaire, answeredQuestion,
                this.interviewState);
            this.ThrowIfInterviewReceivedByInterviewer();

            var expressionProcessorState = this.PrepareExpressionProcessorStateForCalculations();

            InterviewChanges interviewChanges = this.CalculateInterviewChangesOnAnswerSingleOptionQuestion(expressionProcessorState, this.interviewState, userId,
                questionId, rosterVector, answerTime, selectedValue, questionnaire);

            this.ApplyInterviewChanges(interviewChanges);
        }

        public void AnswerTextListQuestion(Guid userId, Guid questionId, RosterVector rosterVector, DateTime answerTime,
            Tuple<decimal, string>[] answers)
        {
            ThrowIfInterviewHardDeleted();
            var answeredQuestion = new Identity(questionId, rosterVector);

            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);

            CheckTextListInvariants(questionId, rosterVector, questionnaire, answeredQuestion, this.interviewState, answers);
            this.ThrowIfInterviewReceivedByInterviewer();

            Func<IReadOnlyInterviewStateDependentOnAnswers, Identity, object> getAnswer = (currentState, question) => question == answeredQuestion ?
                answers :
                this.GetEnabledQuestionAnswerSupportedInExpressions(this.interviewState, question, questionnaire);

            var expressionProcessorState = this.ExpressionProcessorStatePrototype.Clone();

            InterviewChanges interviewChanges = this.CalculateInterviewChangesOnAnswerTextListQuestion(expressionProcessorState, this.interviewState, userId, questionId,
                rosterVector, answerTime, answers, getAnswer, questionnaire);

            this.ApplyInterviewChanges(interviewChanges);
        }

        public void AnswerGeoLocationQuestion(Guid userId, Guid questionId, RosterVector rosterVector, DateTime answerTime, double latitude, double longitude,
            double accuracy, double altitude, DateTimeOffset timestamp)
        {
            ThrowIfInterviewHardDeleted();
            var answeredQuestion = new Identity(questionId, rosterVector);

            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);
            CheckGpsCoordinatesInvariants(questionId, rosterVector, questionnaire, answeredQuestion, this.interviewState);
            this.ThrowIfInterviewReceivedByInterviewer();

            var expressionProcessorState = this.PrepareExpressionProcessorStateForCalculations();

            InterviewChanges interviewChanges = CalculateInterviewChangesOnAnswerGeoLocationQuestion(expressionProcessorState, this.interviewState, userId, questionId,
                rosterVector, answerTime, latitude, longitude, accuracy, altitude, timestamp, answeredQuestion, questionnaire);

            this.ApplyInterviewChanges(interviewChanges);
        }

        private void CalculateInterviewChangesOnAnswerPictureQuestion(IInterviewExpressionStateV6 expressionProcessorState, Guid userId, Guid questionId, RosterVector rosterVector,
            DateTime answerTime, string pictureFileName, IQuestionnaire questionnaire)
        {
            expressionProcessorState.UpdateMediaAnswer(questionId, rosterVector, pictureFileName);

            InterviewChanges interviewChanges = this.CalculateInterviewChangesOnAnswerQuestion(this.interviewState,
                userId, questionId, rosterVector, pictureFileName, pictureFileName, AnswerChangeType.Picture, answerTime,
                questionnaire, expressionProcessorState);
            
            this.ApplyInterviewChanges(interviewChanges);
        }

        public void AnswerSingleOptionLinkedQuestion(Guid userId, Guid questionId, RosterVector rosterVector, DateTime answerTime, decimal[] selectedRosterVector)
        {
            ThrowIfInterviewHardDeleted();
            var answeredQuestion = new Identity(questionId, rosterVector);

            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);

            this.CheckLinkedSingleOptionQuestionInvariants(questionId, rosterVector, selectedRosterVector, questionnaire, answeredQuestion);
            this.ThrowIfInterviewReceivedByInterviewer();

            string answerFormattedAsRosterTitle;
            if (questionnaire.IsQuestionLinkedToRoster(questionId))
            {
                Guid linkedRosterId = questionnaire.GetRosterReferencedByLinkedQuestion(questionId);
                answerFormattedAsRosterTitle = this.interviewState.GetRosterTitle(linkedRosterId,
                    new RosterVector(selectedRosterVector));
            }
            else
            {
                Guid linkedQuestionId = this.GetLinkedQuestionIdOrThrow(questionId, questionnaire);
                var answeredLinkedQuestion = new Identity(linkedQuestionId, selectedRosterVector);
                answerFormattedAsRosterTitle = GetLinkedQuestionAnswerFormattedAsRosterTitle(this.interviewState, answeredLinkedQuestion, questionnaire);
            }


            IInterviewExpressionStateV6 expressionProcessorState = this.ExpressionProcessorStatePrototype.Clone();

            expressionProcessorState.UpdateLinkedSingleOptionAnswer(questionId, rosterVector, selectedRosterVector);

            InterviewChanges interviewChanges = this.CalculateInterviewChangesOnAnswerQuestion(this.interviewState,
                userId, questionId, rosterVector, selectedRosterVector, answerFormattedAsRosterTitle,
                AnswerChangeType.SingleOptionLinked, answerTime, questionnaire, expressionProcessorState);

            this.ApplyInterviewChanges(interviewChanges);
        }

        #endregion

        public void RemoveAnswer(Guid questionId, RosterVector rosterVector, Guid userId, DateTime removeTime)
        {
            ThrowIfInterviewHardDeleted();

            var answeredQuestion = new Identity(questionId, rosterVector);

            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);

            ThrowIfQuestionDoesNotExist(questionId, questionnaire);
            ThrowIfRosterVectorIsIncorrect(this.interviewState, questionId, rosterVector, questionnaire);
            ThrowIfQuestionOrParentGroupIsDisabled(this.interviewState, answeredQuestion, questionnaire);
            this.ThrowIfInterviewReceivedByInterviewer();

            IInterviewExpressionStateV6 expressionProcessorState = this.ExpressionProcessorStatePrototype.Clone();

            InterviewChanges interviewChanges = this.CalculateInterviewChangesOnAnswerRemove(this.interviewState,
                userId, questionId, rosterVector, removeTime, questionnaire, expressionProcessorState);

            this.ApplyInterviewChanges(interviewChanges);

        }

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

        public void CommentAnswer(Guid userId, Guid questionId, RosterVector rosterVector, DateTime commentTime, string comment)
        {
            ThrowIfInterviewHardDeleted();
            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);
            ThrowIfQuestionDoesNotExist(questionId, questionnaire);
            this.ThrowIfRosterVectorIsIncorrect(this.interviewState, questionId, rosterVector, questionnaire);
            this.ThrowIfInterviewReceivedByInterviewer();

            this.ApplyEvent(new AnswerCommented(userId, questionId, rosterVector, commentTime, comment));
        }

        public void SetFlagToAnswer(Guid userId, Guid questionId, RosterVector rosterVector)
        {
            ThrowIfInterviewHardDeleted();
            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);
            ThrowIfQuestionDoesNotExist(questionId, questionnaire);
            this.ThrowIfRosterVectorIsIncorrect(this.interviewState, questionId, rosterVector, questionnaire);
            this.ThrowIfInterviewReceivedByInterviewer();

            this.ApplyEvent(new FlagSetToAnswer(userId, questionId, rosterVector));
        }

        public void RemoveFlagFromAnswer(Guid userId, Guid questionId, RosterVector rosterVector)
        {
            ThrowIfInterviewHardDeleted();
            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion);
            ThrowIfQuestionDoesNotExist(questionId, questionnaire);
            this.ThrowIfRosterVectorIsIncorrect(this.interviewState, questionId, rosterVector, questionnaire);
            this.ThrowIfInterviewReceivedByInterviewer();

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
            this.ThrowIfInterviewStatusIsNotOneOfExpected(InterviewStatus.SupervisorAssigned, InterviewStatus.InterviewerAssigned, InterviewStatus.RejectedBySupervisor);
            this.ThrowIfTryAssignToSameInterviewer(interviewerId);

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

        public void MarkInterviewAsReceivedByInterviwer(Guid userId)
        {
            this.ThrowIfInterviewHardDeleted();

            this.ApplyEvent(new InterviewReceivedByInterviewer());
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

            bool isInterviewInvalid = this.HasInvalidAnswers() ;

            this.ApplyEvent(new InterviewCompleted(userId, completeTime, comment));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.Completed, comment));

            this.ApplyEvent(isInterviewInvalid
                ? new InterviewDeclaredInvalid() as IEvent
                : new InterviewDeclaredValid());
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

        public void UnapproveByHeadquarters(Guid userId, string comment)
        {
            ThrowIfInterviewHardDeleted();
            this.ThrowIfInterviewStatusIsNotOneOfExpected(InterviewStatus.ApprovedByHeadquarters);

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
            if (this.Version > 0)
            {
                throw  new InterviewException(string.Format("Interview with id {0} already created", EventSourceId));
            }

            this.SetQuestionnaireProperties(interviewDto.QuestionnaireId, interviewDto.QuestionnaireVersion);

            ThrowIfInterviewHardDeleted();
            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow(interviewDto.QuestionnaireId,
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

            this.ApplyEvent(new SupervisorAssigned(supervisorId, supervisorId));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.SupervisorAssigned, comment: interviewDto.Comments));

            if (interviewDto.Status == InterviewStatus.InterviewerAssigned)
            {
                this.ApplyEvent(new InterviewerAssigned(supervisorId, userId, synchronizationTime));
                this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.InterviewerAssigned, comment: null));
            }

            this.ApplyRostersEvents(rosters.ToArray());
            foreach (var answerDto in interviewDto.Answers.Where(x => x.Answer != null))
            {
                Guid questionId = answerDto.Id;
                QuestionType questionType = questionnaire.GetQuestionType(questionId);
                RosterVector rosterVector = answerDto.QuestionRosterVector;
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
                            ? new NumericIntegerQuestionAnswered(userId, questionId, rosterVector, synchronizationTime, Convert.ToInt32(answer)) as IEvent
                            : new NumericRealQuestionAnswered(userId, questionId, rosterVector, synchronizationTime, (decimal)answer) as IEvent);
                        break;

                    case QuestionType.SingleOption:
                        this.ApplyEvent(questionnaire.IsQuestionLinked(questionId)
                            ? new SingleOptionLinkedQuestionAnswered(userId, questionId, rosterVector, synchronizationTime, (decimal[])answer) as IEvent
                            : new SingleOptionQuestionAnswered(userId, questionId, rosterVector, synchronizationTime, (decimal)answer) as IEvent);
                        break;

                    case QuestionType.MultyOption:
                        this.ApplyEvent(questionnaire.IsQuestionLinked(questionId)
                            ? new MultipleOptionsLinkedQuestionAnswered(userId, questionId, rosterVector, synchronizationTime, (decimal[][])answer) as IEvent
                            : questionnaire.IsQuestionYesNo(questionId)
                                ? new YesNoQuestionAnswered(userId, questionId, rosterVector, synchronizationTime, (AnsweredYesNoOption[])answer) as IEvent
                                : new MultipleOptionsQuestionAnswered(userId, questionId, rosterVector, synchronizationTime, (decimal[])answer) as IEvent);
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
            InterviewStatus interviewStatus, IEvent[] synchronizedEvents, bool createdOnClient)
        {
            ThrowIfOtherUserIsResponsible(userId);

            SetQuestionnaireProperties(questionnaireId, questionnaireVersion);

            this.GetQuestionnaireOrThrow(questionnaireId, questionnaireVersion);

            var isInterviewNeedToBeCreated = createdOnClient && this.Version == 0;

            if (isInterviewNeedToBeCreated)
            {
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

            if (this.status == InterviewStatus.Deleted)
                this.Restore(userId);
            else
                this.ThrowIfStatusNotAllowedToBeChangedWithMetadata(interviewStatus);

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

            ApplyValidationEvent(valid);
        }

        private void ThrowIfOtherUserIsResponsible(Guid userId)
        {
            if (this.interviewerId != Guid.Empty && userId != this.interviewerId)
                throw new InterviewException(
                    string.Format(
                        "interviewer with id {0} is not responsible for the interview anymore, interviewer with id {1} is.",
                        userId, this.interviewerId), InterviewDomainExceptionType.OtherUserIsResponsible);
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

            if (interviewChanges.ChangedQuestionTitles != null)
            {
                this.ApplySubstitutionChangesEvents(interviewChanges.ChangedQuestionTitles);
            }
        }

        private void ApplySubstitutionChangesEvents(List<Identity> changedQuestionIds)
        {
            if (changedQuestionIds.Count > 0)
            {
                this.ApplyEvent(new SubstitutionTitlesChanged(changedQuestionIds.ToArray()));
            }
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
                this.ApplyEvent(new GroupsDisabled(enablementChanges.GroupsToBeDisabled.ToArray()));
            }

            if (enablementChanges.GroupsToBeEnabled.Any())
            {
                this.ApplyEvent(new GroupsEnabled(enablementChanges.GroupsToBeEnabled.ToArray()));
            }

            if (enablementChanges.QuestionsToBeDisabled.Any())
            {
                this.ApplyEvent(new QuestionsDisabled(enablementChanges.QuestionsToBeDisabled.ToArray()));
            }

            if (enablementChanges.QuestionsToBeEnabled.Any())
            {
                this.ApplyEvent(new QuestionsEnabled(enablementChanges.QuestionsToBeEnabled.ToArray()));
            }
        }

        private void ApplyValidityChangesEvents(ValidityChanges validityChanges)
        {
            if (validityChanges.AnswersDeclaredValid.Any())
            {
                this.ApplyEvent(new AnswersDeclaredValid(validityChanges.AnswersDeclaredValid.ToArray()));
            }

            if (validityChanges.AnswersDeclaredInvalid.Any())
            {
                this.ApplyEvent(new AnswersDeclaredInvalid(validityChanges.FailedValidationConditions));
            }
        }

        private void ApplyAnswersRemovanceEvents(List<Identity> answersToRemove)
        {
            if (answersToRemove.Any())
            {
                this.ApplyEvent(new AnswersRemoved(answersToRemove.ToArray()));
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
                    .Select(roster => new AddedRosterInstance(roster.GroupId, roster.OuterRosterVector, roster.RosterInstanceId, roster.SortIndex))
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
                if (data.AreTitlesForRosterInstancesSpecified())
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
                                rosterIdentity.RosterInstanceId), data.GetRosterInstanceTitle(rosterIdentity.GroupId, rosterIdentity.RosterInstanceId)));
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
                        result[rosterIdentity] = data.GetRosterInstanceTitle(rosterIdentity.GroupId, rosterIdentity.RosterInstanceId);
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
                if (change.Answer == null)
                {
                    this.ApplyEvent(new AnswerRemoved(change.UserId, change.QuestionId, change.RosterVector, change.AnswerTime));
                    continue;
                }

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
                    case AnswerChangeType.YesNo:
                        this.ApplyEvent(new YesNoQuestionAnswered(change.UserId, change.QuestionId, change.RosterVector, change.AnswerTime, (AnsweredYesNoOption[])change.Answer));
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

        private void CheckLinkedMultiOptionQuestionInvariants(Guid questionId, RosterVector rosterVector, decimal[][] selectedRosterVectors, IQuestionnaire questionnaire, Identity answeredQuestion)
        {
            ThrowIfQuestionDoesNotExist(questionId, questionnaire);
            ThrowIfRosterVectorIsIncorrect(this.interviewState, questionId, rosterVector, questionnaire);
            ThrowIfQuestionTypeIsNotOneOfExpected(questionId, questionnaire, QuestionType.MultyOption);
            ThrowIfQuestionOrParentGroupIsDisabled(this.interviewState, answeredQuestion, questionnaire);

            if (questionnaire.IsQuestionLinkedToRoster(questionId))
            {
                foreach (var selectedRosterVector in selectedRosterVectors)
                {
                    this.ThrowIfSelectedRosterRowIsAbsent(questionId, selectedRosterVector, questionnaire);
                }
            }
            else
            {
                Guid linkedQuestionId = this.GetLinkedQuestionIdOrThrow(questionId, questionnaire);
                var answeredLinkedQuestions =
                    selectedRosterVectors.Select(
                        selectedRosterVector => new Identity(linkedQuestionId, selectedRosterVector));

                foreach (var answeredLinkedQuestion in answeredLinkedQuestions)
                {
                    ThrowIfRosterVectorIsIncorrect(this.interviewState, linkedQuestionId,
                        answeredLinkedQuestion.RosterVector, questionnaire);
                    ThrowIfQuestionOrParentGroupIsDisabled(this.interviewState, answeredLinkedQuestion, questionnaire);
                    ThrowIfLinkedQuestionDoesNotHaveAnswer(this.interviewState, answeredQuestion, answeredLinkedQuestion,
                        questionnaire);
                }
            }
            ThrowIfLengthOfSelectedValuesMoreThanMaxForSelectedAnswerOptions(questionId, selectedRosterVectors.Length, questionnaire);
        }

        private void CheckLinkedSingleOptionQuestionInvariants(Guid questionId, RosterVector rosterVector, decimal[] selectedRosterVector, IQuestionnaire questionnaire, Identity answeredQuestion)
        {
            ThrowIfQuestionDoesNotExist(questionId, questionnaire);
            ThrowIfRosterVectorIsIncorrect(this.interviewState, questionId, rosterVector, questionnaire);

            ThrowIfQuestionTypeIsNotOneOfExpected(questionId, questionnaire, QuestionType.SingleOption);
            ThrowIfQuestionOrParentGroupIsDisabled(this.interviewState, answeredQuestion, questionnaire);

            if (questionnaire.IsQuestionLinkedToRoster(questionId))
            {
                this.ThrowIfSelectedRosterRowIsAbsent(questionId, selectedRosterVector, questionnaire);
            }
            else
            {
                Guid linkedQuestionId = this.GetLinkedQuestionIdOrThrow(questionId, questionnaire);
                var answeredLinkedQuestion = new Identity(linkedQuestionId, selectedRosterVector);

                ThrowIfRosterVectorIsIncorrect(this.interviewState, linkedQuestionId, selectedRosterVector, questionnaire);
                ThrowIfQuestionOrParentGroupIsDisabled(this.interviewState, answeredLinkedQuestion, questionnaire);
                ThrowIfLinkedQuestionDoesNotHaveAnswer(this.interviewState, answeredQuestion, answeredLinkedQuestion,
                    questionnaire);
            }
        }

        private void ThrowIfSelectedRosterRowIsAbsent(Guid questionId, decimal[] selectedRosterVector,
            IQuestionnaire questionnaire)
        {
            Guid linkedRosterId = questionnaire.GetRosterReferencedByLinkedQuestion(questionId);
            var availableRosterInstanceIds = this.interviewState.GetRosterInstanceIds(linkedRosterId,
                new RosterVector(selectedRosterVector.WithoutLast()));
            var rosterInstanceId = selectedRosterVector.Last();
            if (!availableRosterInstanceIds.Contains(rosterInstanceId))
            {
                throw new InterviewException(string.Format(
                    "Answer on linked to roster question {0} is incorrect. " +
                    "Answer refers to instance of roster group {1} by instance id [{2}] " +
                    "but roster group has only following roster instances: {3}. InterviewId: {4}",
                    FormatQuestionForException(questionId, questionnaire),
                    FormatGroupForException(linkedRosterId, questionnaire), rosterInstanceId,
                    string.Join(", ", availableRosterInstanceIds), this.EventSourceId));
            }
        }

        private void CheckNumericRealQuestionInvariants(Guid questionId, RosterVector rosterVector, decimal answer,
           IQuestionnaire questionnaire,
           Identity answeredQuestion, IReadOnlyInterviewStateDependentOnAnswers currentInterviewState, bool applyStrongChecks = true)
        {
            ThrowIfQuestionDoesNotExist(questionId, questionnaire);
            ThrowIfRosterVectorIsIncorrect(currentInterviewState, questionId, rosterVector, questionnaire);
            ThrowIfQuestionTypeIsNotOneOfExpected(questionId, questionnaire, QuestionType.Numeric);
            ThrowIfNumericQuestionIsNotReal(questionId, questionnaire);
            if (applyStrongChecks)
            {
                ThrowIfQuestionOrParentGroupIsDisabled(currentInterviewState, answeredQuestion, questionnaire);
                ThrowIfAnswerHasMoreDecimalPlacesThenAccepted(questionnaire, questionId, answer);
            }
        }

        private void CheckDateTimeQuestionInvariants(Guid questionId, RosterVector rosterVector, IQuestionnaire questionnaire,
            Identity answeredQuestion, IReadOnlyInterviewStateDependentOnAnswers currentInterviewState, bool applyStrongChecks = true)
        {
            ThrowIfQuestionDoesNotExist(questionId, questionnaire);
            this.ThrowIfRosterVectorIsIncorrect(currentInterviewState, questionId, rosterVector, questionnaire);
            this.ThrowIfQuestionTypeIsNotOneOfExpected(questionId, questionnaire, QuestionType.DateTime);
            if (applyStrongChecks)
                ThrowIfQuestionOrParentGroupIsDisabled(currentInterviewState, answeredQuestion, questionnaire);
        }

        private void CheckSingleOptionQuestionInvariants(Guid questionId, RosterVector rosterVector, decimal selectedValue,
            IQuestionnaire questionnaire, Identity answeredQuestion, IReadOnlyInterviewStateDependentOnAnswers currentInterviewState,
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

        private void CheckMultipleOptionQuestionInvariants(Guid questionId, RosterVector rosterVector, decimal[] selectedValues,
            IQuestionnaire questionnaire, Identity answeredQuestion, IReadOnlyInterviewStateDependentOnAnswers currentInterviewState,
            bool applyStrongChecks = true)
        {
            this.ThrowIfQuestionDoesNotExist(questionId, questionnaire);
            this.ThrowIfRosterVectorIsIncorrect(currentInterviewState, questionId, rosterVector, questionnaire);
            this.ThrowIfQuestionTypeIsNotOneOfExpected(questionId, questionnaire, QuestionType.MultyOption);
            this.ThrowIfSomeValuesAreNotFromAvailableOptions(questionId, selectedValues, questionnaire);

            if (questionnaire.ShouldQuestionSpecifyRosterSize(questionId))
            {
                this.ThrowIfRosterSizeAnswerIsNegativeOrGreaterThenMaxRosterRowCount(questionId, selectedValues.Length, questionnaire);
            }

            if (applyStrongChecks)
            {
                this.ThrowIfLengthOfSelectedValuesMoreThanMaxForSelectedAnswerOptions(questionId, selectedValues.Length, questionnaire);
                this.ThrowIfQuestionOrParentGroupIsDisabled(currentInterviewState, answeredQuestion, questionnaire);
            }
        }

        private void CheckYesNoQuestionInvariants(Identity question, AnsweredYesNoOption[] answeredOptions, IQuestionnaire questionnaire,
            IReadOnlyInterviewStateDependentOnAnswers state)
        {
            decimal[] selectedValues = answeredOptions.Select(answeredOption => answeredOption.OptionValue).ToArray();
            var yesAnswersCount = answeredOptions.Count(answeredOption => answeredOption.Yes);

            this.ThrowIfQuestionDoesNotExist(question.Id, questionnaire);
            this.ThrowIfRosterVectorIsIncorrect(state, question.Id, question.RosterVector, questionnaire);
            this.ThrowIfQuestionTypeIsNotOneOfExpected(question.Id, questionnaire, QuestionType.MultyOption);
            this.ThrowIfSomeValuesAreNotFromAvailableOptions(question.Id, selectedValues, questionnaire);

            if (questionnaire.ShouldQuestionSpecifyRosterSize(question.Id))
            {
                this.ThrowIfRosterSizeAnswerIsNegativeOrGreaterThenMaxRosterRowCount(question.Id, yesAnswersCount, questionnaire);
            }

            this.ThrowIfLengthOfSelectedValuesMoreThanMaxForSelectedAnswerOptions(question.Id, yesAnswersCount, questionnaire);
            this.ThrowIfQuestionOrParentGroupIsDisabled(state, question, questionnaire);
        }

        private void CheckTextQuestionInvariants(Guid questionId, RosterVector rosterVector, IQuestionnaire questionnaire,
            Identity answeredQuestion, IReadOnlyInterviewStateDependentOnAnswers currentInterviewState, bool applyStrongChecks = true)
        {
            ThrowIfQuestionDoesNotExist(questionId, questionnaire);
            this.ThrowIfRosterVectorIsIncorrect(currentInterviewState, questionId, rosterVector, questionnaire);
            this.ThrowIfQuestionTypeIsNotOneOfExpected(questionId, questionnaire, QuestionType.Text);
            if (applyStrongChecks)
                ThrowIfQuestionOrParentGroupIsDisabled(currentInterviewState, answeredQuestion, questionnaire);
        }

        private void CheckNumericIntegerQuestionInvariants(Guid questionId, RosterVector rosterVector, int answer, IQuestionnaire questionnaire,
            Identity answeredQuestion, IReadOnlyInterviewStateDependentOnAnswers currentInterviewState, bool applyStrongChecks = true)
        {
            this.ThrowIfQuestionDoesNotExist(questionId, questionnaire);
            this.ThrowIfRosterVectorIsIncorrect(currentInterviewState, questionId, rosterVector, questionnaire);
            this.ThrowIfQuestionTypeIsNotOneOfExpected(questionId, questionnaire, QuestionType.AutoPropagate, QuestionType.Numeric);
            this.ThrowIfNumericQuestionIsNotInteger(questionId, questionnaire);

            if (questionnaire.ShouldQuestionSpecifyRosterSize(questionId))
            {
                this.ThrowIfRosterSizeAnswerIsNegativeOrGreaterThenMaxRosterRowCount(questionId, answer, questionnaire);
            }

            if (applyStrongChecks)
            {
                this.ThrowIfQuestionOrParentGroupIsDisabled(currentInterviewState, answeredQuestion, questionnaire);
            }
        }

        private void CheckTextListInvariants(Guid questionId, RosterVector rosterVector, IQuestionnaire questionnaire, Identity answeredQuestion,
            IReadOnlyInterviewStateDependentOnAnswers currentInterviewState, Tuple<decimal, string>[] answers, bool applyStrongChecks = true)
        {
            this.ThrowIfQuestionDoesNotExist(questionId, questionnaire);
            this.ThrowIfRosterVectorIsIncorrect(currentInterviewState, questionId, rosterVector, questionnaire);
            this.ThrowIfQuestionTypeIsNotOneOfExpected(questionId, questionnaire, QuestionType.TextList);

            if (questionnaire.ShouldQuestionSpecifyRosterSize(questionId))
            {
                this.ThrowIfRosterSizeAnswerIsNegativeOrGreaterThenMaxRosterRowCount(questionId, answers.Length, questionnaire);
            }

            if (applyStrongChecks)
            {
                this.ThrowIfQuestionOrParentGroupIsDisabled(currentInterviewState, answeredQuestion, questionnaire);
                this.ThrowIfDecimalValuesAreNotUnique(answers, questionId, questionnaire);
                this.ThrowIfStringValueAreEmptyOrWhitespaces(answers, questionId, questionnaire);
                var maxAnswersCountLimit = questionnaire.GetListSizeForListQuestion(questionId);
                this.ThrowIfAnswersExceedsMaxAnswerCountLimit(answers, maxAnswersCountLimit, questionId, questionnaire);
            }
        }

        private void CheckGpsCoordinatesInvariants(Guid questionId, RosterVector rosterVector, IQuestionnaire questionnaire, Identity answeredQuestion,
            IReadOnlyInterviewStateDependentOnAnswers currentInterviewState, bool applyStrongChecks = true)
        {
            ThrowIfQuestionDoesNotExist(questionId, questionnaire);
            this.ThrowIfRosterVectorIsIncorrect(currentInterviewState, questionId, rosterVector, questionnaire);
            this.ThrowIfQuestionTypeIsNotOneOfExpected(questionId, questionnaire, QuestionType.GpsCoordinates);
            if (applyStrongChecks)
                ThrowIfQuestionOrParentGroupIsDisabled(currentInterviewState, answeredQuestion, questionnaire);
        }

        private void CheckQRBarcodeInvariants(Guid questionId, RosterVector rosterVector, IQuestionnaire questionnaire,
         Identity answeredQuestion, IReadOnlyInterviewStateDependentOnAnswers currentInterviewState, bool applyStrongChecks = true)
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
        private InterviewChanges CalculateInterviewChangesOnAnswerNumericIntegerQuestion(IInterviewExpressionStateV6 expressionProcessorState, IReadOnlyInterviewStateDependentOnAnswers state, Guid userId,
            Guid questionId, RosterVector rosterVector, DateTime answerTime, int answer,
            Func<IReadOnlyInterviewStateDependentOnAnswers, Identity, object> getAnswer, IQuestionnaire questionnaire)
        {
            List<Guid> rosterIds = questionnaire.GetRosterGroupsByRosterSizeQuestion(questionId).ToList();
            int rosterSize = rosterIds.Any() ? ToRosterSize(answer) : 0;

            Func<Guid, decimal[], bool> isRoster = (groupId, groupOuterScopeRosterVector)
                => rosterIds.Contains(groupId)
                    && AreEqualRosterVectors(groupOuterScopeRosterVector, rosterVector);

            IEnumerable<decimal> rosterInstanceIds = Enumerable.Range(0, rosterSize).Select(index => (decimal)index).ToList();

            RosterCalculationData rosterCalculationData = this.CalculateRosterData(state, rosterIds, rosterVector, rosterInstanceIds, null, questionnaire, getAnswer);

            var rostersWithoutRosterTitleQuestions = rosterIds
                .Where(rosterId => !questionnaire.IsRosterTitleQuestionAvailable(rosterId));

            if (rostersWithoutRosterTitleQuestions.Any())
            {
                Dictionary<decimal, string> numericRosterTitles = Enumerable.Range(0, rosterSize).ToDictionary(
                    index => (decimal) index,
                    index => (index + 1).ToString(CultureInfo.InvariantCulture));

                var rosterTitlesForRostersWithoutRosterTitleQuestions =
                    rostersWithoutRosterTitleQuestions
                        .ToDictionary(
                            rosterId => rosterId,
                            rosterId => numericRosterTitles);

                rosterCalculationData.SetTitlesForRosterInstances(rosterTitlesForRostersWithoutRosterTitleQuestions);
            }

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

            IReadOnlyInterviewStateDependentOnAnswers alteredState = state.Amend(getRosterInstanceIds: (groupId, groupOuterRosterVector)
                => isRoster(groupId, groupOuterRosterVector)
                    ? rosterInstanceIds
                    : state.GetRosterInstanceIds(groupId, groupOuterRosterVector));
            List<Identity> answersForLinkedQuestionsToRemoveByDisabling =
                this.GetAnswersForLinkedQuestionsToRemoveBecauseOfDisabledGroupsOrQuestions(
                    alteredState,
                    enablementChanges.GroupsToBeDisabled,
                    enablementChanges.QuestionsToBeDisabled,
                    questionnaire);

            var interviewByAnswerChange = new List<AnswerChange>
            {
                new AnswerChange(AnswerChangeType.NumericInteger, userId, questionId, rosterVector, answerTime, answer)
            };

            var substitutionChanges = new List<Identity>(this.CalculateChangesInSubstitutedQuestions(questionId, rosterVector, questionnaire, alteredState));

            return new InterviewChanges(interviewByAnswerChange,
                enablementChanges,
                validationChanges,
                rosterCalculationData,
                answersForLinkedQuestionsToRemoveByDisabling,
                rosterInstancesWithAffectedTitles,
                AnswerUtils.AnswerToString(answer),
                substitutionChanges);
        }

        private InterviewChanges CalculateInterviewChangesOnAnswerMultipleOptionsQuestion(IInterviewExpressionStateV6 expressionProcessorState, IReadOnlyInterviewStateDependentOnAnswers state,
            Guid userId, Guid questionId, RosterVector rosterVector, DateTime answerTime,
            decimal[] selectedValues, Func<IReadOnlyInterviewStateDependentOnAnswers, Identity, object> getAnswer,
            IQuestionnaire questionnaire)
        {
            List<decimal> availableValues = questionnaire.GetAnswerOptionsAsValues(questionId).ToList();

            IEnumerable<decimal> rosterInstanceIds = selectedValues.ToList();
            Dictionary<decimal, int?> rosterInstanceIdsWithSortIndexes = selectedValues.ToDictionary(
                selectedValue => selectedValue,
                selectedValue => (int?)availableValues.IndexOf(selectedValue));

            List<Guid> rosterIds = questionnaire.GetRosterGroupsByRosterSizeQuestion(questionId).ToList();

            Func<Guid, decimal[], bool> isRoster = (groupId, groupOuterRosterVector)
                => rosterIds.Contains(groupId)
                    && AreEqualRosterVectors(groupOuterRosterVector, rosterVector);

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

            IReadOnlyInterviewStateDependentOnAnswers alteredState = state.Amend(getRosterInstanceIds: (groupId, groupOuterRosterVector)
                => isRoster(groupId, groupOuterRosterVector)
                    ? rosterInstanceIds
                    : state.GetRosterInstanceIds(groupId, groupOuterRosterVector));
            List<Identity> answersForLinkedQuestionsToRemoveByDisabling =
                this.GetAnswersForLinkedQuestionsToRemoveBecauseOfDisabledGroupsOrQuestions(
                    alteredState,
                    enablementChanges.GroupsToBeDisabled,
                    enablementChanges.QuestionsToBeDisabled,
                    questionnaire);


            var interviewByAnswerChange = new List<AnswerChange>
            {
                new AnswerChange(AnswerChangeType.MultipleOptions, userId, questionId, rosterVector, answerTime, selectedValues)
            };

            var substitutionChanges = new List<Identity>(this.CalculateChangesInSubstitutedQuestions(questionId, rosterVector, questionnaire, alteredState));

            return new InterviewChanges(interviewByAnswerChange, 
                enablementChanges, 
                validationChanges,
                rosterCalculationData,
                answersForLinkedQuestionsToRemoveByDisabling, 
                rosterInstancesWithAffectedTitles, 
                answerFormattedAsRosterTitle,
                substitutionChanges);
        }

        private InterviewChanges CalculateInterviewChangesOnYesNoQuestionAnswer(Identity question, AnsweredYesNoOption[] answer, DateTime answerTime, Guid userId, IQuestionnaire questionnaire,
            IInterviewExpressionStateV6 expressionProcessorState, IReadOnlyInterviewStateDependentOnAnswers state, Func<IReadOnlyInterviewStateDependentOnAnswers, Identity, object> getAnswer)
        {
            List<decimal> availableValues = questionnaire.GetAnswerOptionsAsValues(question.Id).ToList();

            IEnumerable<decimal> rosterInstanceIds = answer.Where(answeredOption => answeredOption.Yes).Select(answeredOption => answeredOption.OptionValue).ToList();
            Dictionary<decimal, int?> rosterInstanceIdsWithSortIndexes = rosterInstanceIds.ToDictionary(
                selectedValue => selectedValue,
                selectedValue => (int?)availableValues.IndexOf(selectedValue));

            List<Guid> rosterIds = questionnaire.GetRosterGroupsByRosterSizeQuestion(question.Id).ToList();

            Func<Guid, decimal[], bool> isRoster = (groupId, groupOuterRosterVector)
                => rosterIds.Contains(groupId)
                && AreEqualRosterVectors(groupOuterRosterVector, question.RosterVector);

            RosterCalculationData rosterCalculationData = this.CalculateRosterDataWithRosterTitlesFromYesNoQuestions(
                question, rosterIds, rosterInstanceIdsWithSortIndexes, questionnaire, state, getAnswer);

            expressionProcessorState.UpdateYesNoAnswer(question.Id, question.RosterVector, ConvertToYesNoAnswersOnly(answer));

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
            string answerFormattedAsRosterTitle = AnswerUtils.AnswerToString(answer,
                answerOptionValue => questionnaire.GetAnswerOptionTitle(question.Id, answerOptionValue));

            List<RosterIdentity> rosterInstancesWithAffectedTitles = CalculateRosterInstancesWhichTitlesAreAffected(question.Id, question.RosterVector, questionnaire);

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

            IReadOnlyInterviewStateDependentOnAnswers alteredState = state.Amend(getRosterInstanceIds: (groupId, groupOuterRosterVector)
                => isRoster(groupId, groupOuterRosterVector)
                    ? rosterInstanceIds
                    : state.GetRosterInstanceIds(groupId, groupOuterRosterVector));
            List<Identity> answersForLinkedQuestionsToRemoveByDisabling =
                this.GetAnswersForLinkedQuestionsToRemoveBecauseOfDisabledGroupsOrQuestions(
                    alteredState,
                    enablementChanges.GroupsToBeDisabled,
                    enablementChanges.QuestionsToBeDisabled,
                    questionnaire);


            var interviewByAnswerChange = new List<AnswerChange>
            {
                new AnswerChange(AnswerChangeType.YesNo, userId, question.Id, question.RosterVector, answerTime, answer)
            };

            var substitutionChanges = new List<Identity>(this.CalculateChangesInSubstitutedQuestions(question.Id, question.RosterVector, questionnaire, alteredState));

            return new InterviewChanges(
                interviewByAnswerChange,
                enablementChanges,
                validationChanges,
                rosterCalculationData,
                answersForLinkedQuestionsToRemoveByDisabling,
                rosterInstancesWithAffectedTitles,
                answerFormattedAsRosterTitle,
                substitutionChanges);
        }

        private InterviewChanges CalculateInterviewChangesOnAnswerTextListQuestion(IInterviewExpressionStateV6 expressionProcessorState, IReadOnlyInterviewStateDependentOnAnswers state, 
            Guid userId,Guid questionId, RosterVector rosterVector, DateTime answerTime, Tuple<decimal, string>[] answers,
            Func<IReadOnlyInterviewStateDependentOnAnswers, Identity, object> getAnswer, IQuestionnaire questionnaire)
        {
            var selectedValues = answers.Select(x => x.Item1).ToArray();
            IEnumerable<decimal> rosterInstanceIds = selectedValues.ToList();
            var rosterInstanceIdsWithSortIndexes = selectedValues.ToDictionary(
                selectedValue => selectedValue,
                selectedValue => (int?)selectedValue);

            List<Guid> rosterIds = questionnaire.GetRosterGroupsByRosterSizeQuestion(questionId).ToList();

            Func<Guid, decimal[], bool> isRoster = (groupId, groupOuterRosterVector)
                => rosterIds.Contains(groupId)
                    && AreEqualRosterVectors(groupOuterRosterVector, rosterVector);

            string questionKey = ConversionHelper.ConvertIdAndRosterVectorToString(questionId, rosterVector);

            Tuple<decimal, string>[] currentAnswer = this.interviewState.TextListAnswers.ContainsKey(questionKey)
                ? this.interviewState.TextListAnswers[questionKey]
                : new Tuple<decimal, string>[0];

            Tuple<decimal, string>[] changedAnswers =
                answers.Where(tuple => currentAnswer.Any(a => a.Item1 == tuple.Item1 && a.Item2 != tuple.Item2)).ToArray();

            RosterCalculationData rosterCalculationData = this.CalculateRosterDataWithRosterTitlesFromTextListQuestions(
                state, questionnaire, rosterVector, rosterIds, rosterInstanceIdsWithSortIndexes, questionnaire, getAnswer,
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

            var substitutionChanges = new List<Identity>(this.CalculateChangesInSubstitutedQuestions(questionId, rosterVector, questionnaire, this.interviewState));

            var answerChanges = new List<AnswerChange>()
            {
                new AnswerChange(AnswerChangeType.TextList, userId, questionId, rosterVector, answerTime, answers)
            };

            return new InterviewChanges(answerChanges, 
                enablementChanges,
                validationChanges,
                rosterCalculationData,
                null, 
                rosterInstancesWithAffectedTitles,
                answerFormattedAsRosterTitle,
                substitutionChanges);
        }

        // do not triggers roster
        private InterviewChanges CalculateInterviewChangesOnAnswerTextQuestion(IInterviewExpressionStateV6 expressionProcessorState, IReadOnlyInterviewStateDependentOnAnswers state, Guid userId,
            Guid questionId, RosterVector rosterVector, DateTime answerTime, string answer, IQuestionnaire questionnaire)
        {
            expressionProcessorState.UpdateTextAnswer(questionId, rosterVector, answer);

            return this.CalculateInterviewChangesOnAnswerQuestion(state, userId, questionId, rosterVector, answer, answer, string.IsNullOrWhiteSpace(answer), AnswerChangeType.Text, answerTime, questionnaire, expressionProcessorState);
        }

        private InterviewChanges CalculateInterviewChangesOnAnswerDateTimeQuestion(IInterviewExpressionStateV6 expressionProcessorState, IReadOnlyInterviewStateDependentOnAnswers state, Guid userId,
            Guid questionId, RosterVector rosterVector, DateTime answerTime, DateTime answer, IQuestionnaire questionnaire)
        {
            string answerFormattedAsRosterTitle = AnswerUtils.AnswerToString(answer);

            expressionProcessorState.UpdateDateAnswer(questionId, rosterVector, answer);

            return this.CalculateInterviewChangesOnAnswerQuestion(state, userId, questionId, rosterVector, answer, answerFormattedAsRosterTitle, AnswerChangeType.DateTime, answerTime, questionnaire, expressionProcessorState);
        }

        private InterviewChanges CalculateInterviewChangesOnAnswerNumericRealQuestion(IInterviewExpressionStateV6 expressionProcessorState, IReadOnlyInterviewStateDependentOnAnswers state, Guid userId,
            Guid questionId, RosterVector rosterVector,
            DateTime answerTime, decimal answer, IQuestionnaire questionnaire)
        {
            string answerFormattedAsRosterTitle = AnswerUtils.AnswerToString(answer);

            expressionProcessorState.UpdateNumericRealAnswer(questionId, rosterVector, (double)answer);

            return this.CalculateInterviewChangesOnAnswerQuestion(state, userId, questionId, rosterVector, answer, answerFormattedAsRosterTitle, AnswerChangeType.NumericReal, answerTime, questionnaire, expressionProcessorState);
        }

        private InterviewChanges CalculateInterviewChangesOnAnswerSingleOptionQuestion(IInterviewExpressionStateV6 expressionProcessorState, IReadOnlyInterviewStateDependentOnAnswers state,
            Guid userId,
            Guid questionId,
            RosterVector rosterVector,
            DateTime answerTime,
            decimal selectedValue,
            IQuestionnaire questionnaire)
        {
            string answerFormattedAsRosterTitle = AnswerUtils.AnswerToString(selectedValue, answerOptionValue => questionnaire.GetAnswerOptionTitle(questionId, answerOptionValue));

            var questionIdentity = new Identity(questionId, rosterVector);
            var previsousAnswer = GetEnabledQuestionAnswerSupportedInExpressions(state, questionIdentity, questionnaire);
            bool answerChanged = state.WasQuestionAnswered(questionIdentity) && (decimal?)previsousAnswer != (decimal?)selectedValue;

            var answersToRemoveByCascading = answerChanged ? this.GetQuestionsToRemoveAnswersFromDependingOnCascading(questionId, rosterVector, questionnaire, state) : Enumerable.Empty<Identity>();

            var cascadingQuestionsToDisable = questionnaire.GetCascadingQuestionsThatDirectlyDependUponQuestion(questionId)
                .Where(question => !questionnaire.DoesCascadingQuestionHaveOptionsForParentValue(question, selectedValue)).ToList();

            var cascadingQuestionsToDisableIdentities = this.GetInstancesOfQuestionsWithSameAndDeeperRosterLevelOrThrow(state,
                cascadingQuestionsToDisable, rosterVector, questionnaire);

            expressionProcessorState.UpdateSingleOptionAnswer(questionId, rosterVector, selectedValue);
            answersToRemoveByCascading.ToList().ForEach(x => expressionProcessorState.UpdateSingleOptionAnswer(x.Id, x.RosterVector, (decimal?)null));
            expressionProcessorState.DisableQuestions(cascadingQuestionsToDisableIdentities);

            InterviewChanges interviewChanges = this.CalculateInterviewChangesOnAnswerQuestion(state, userId, questionId, rosterVector, selectedValue, answerFormattedAsRosterTitle, AnswerChangeType.SingleOption, answerTime, questionnaire, expressionProcessorState);

            interviewChanges.AnswersForLinkedQuestionsToRemove.AddRange(answersToRemoveByCascading);

            return interviewChanges;
        }

        private InterviewChanges CalculateInterviewChangesOnAnswerQRBarcodeQuestion(IInterviewExpressionStateV6 expressionProcessorState, IReadOnlyInterviewStateDependentOnAnswers state,
            Guid userId, Guid questionId, RosterVector rosterVector, DateTime answerTime, string answer, IQuestionnaire questionnaire)
        {
            expressionProcessorState.UpdateQrBarcodeAnswer(questionId, rosterVector, answer);

            return this.CalculateInterviewChangesOnAnswerQuestion(state, userId, questionId, rosterVector, answer, answer, AnswerChangeType.QRBarcode, answerTime, questionnaire, expressionProcessorState);
        }

        private InterviewChanges CalculateInterviewChangesOnAnswerGeoLocationQuestion(IInterviewExpressionStateV6 expressionProcessorState, IReadOnlyInterviewStateDependentOnAnswers state, Guid userId,
            Guid questionId, RosterVector rosterVector, DateTime answerTime, double latitude, double longitude, double accuracy, double altitude, DateTimeOffset timestamp, Identity answeredQuestion,
            IQuestionnaire questionnaire)
        {
            string answerFormattedAsRosterTitle = string.Format(CultureInfo.InvariantCulture, "[{0};{1}]", latitude, longitude);

            expressionProcessorState.UpdateGeoLocationAnswer(questionId, rosterVector, latitude, longitude, accuracy, altitude);

            return this.CalculateInterviewChangesOnAnswerQuestion(state, userId, questionId, rosterVector,
                new GeoLocationPoint(latitude, longitude, accuracy, altitude, timestamp), answerFormattedAsRosterTitle,
                AnswerChangeType.GeoLocation, answerTime, questionnaire, expressionProcessorState);
        }



        private InterviewChanges CalculateInterviewChangesOnAnswerRemove(
            IReadOnlyInterviewStateDependentOnAnswers state, Guid userId, Guid questionId,
            RosterVector rosterVector, DateTime removeTime, IQuestionnaire questionnaire,
            IInterviewExpressionStateV6 expressionProcessorState)
        {
            List<Guid> rosterIds = questionnaire.GetRosterGroupsByRosterSizeQuestion(questionId).ToList();
            Func<Guid, decimal[], bool> isRoster = (groupId, groupOuterScopeRosterVector)
                => rosterIds.Contains(groupId)
                    && AreEqualRosterVectors(groupOuterScopeRosterVector, rosterVector);

            var rosterInstanceIds = Enumerable.Empty<decimal>();

            RosterCalculationData rosterCalculationData = this.CalculateRosterData(state, rosterIds, rosterVector,
                rosterInstanceIds, null, questionnaire, (s, i) => null);

            expressionProcessorState.SaveAllCurrentStatesAsPrevious();

            //Update State
            RemoveAnswerFromExpressionProcessorState(expressionProcessorState, questionId, rosterVector);

            var answersToRemoveByCascading = this.GetQuestionsToRemoveAnswersFromDependingOnCascading(questionId, rosterVector, questionnaire, state).ToArray();
            
            expressionProcessorState.DisableQuestions(answersToRemoveByCascading);

            var rosterInstancesToRemove = this.GetUnionOfUniqueRosterDataPropertiesByRosterAndNestedRosters(
                d => d.RosterInstancesToRemove, new RosterIdentityComparer(), rosterCalculationData);

            List<RosterIdentity> rosterInstancesWithAffectedTitles = CalculateRosterInstancesWhichTitlesAreAffected(questionId, rosterVector, questionnaire);

            foreach (var rosterInstancesWithAffectedTitle in rosterInstancesWithAffectedTitles)
            {
                expressionProcessorState.UpdateRosterTitle(rosterInstancesWithAffectedTitle.GroupId,
                    rosterInstancesWithAffectedTitle.OuterRosterVector,
                    rosterInstancesWithAffectedTitle.RosterInstanceId, null);
            }

            rosterInstancesToRemove.ForEach(r => expressionProcessorState.RemoveRoster(r.GroupId, r.OuterRosterVector, r.RosterInstanceId));
            EnablementChanges enablementChanges = expressionProcessorState.ProcessEnablementConditions();
            ValidityChanges validationChanges = expressionProcessorState.ProcessValidationExpressions();

            enablementChanges.QuestionsToBeEnabled.AddRange(rosterCalculationData.DisabledAnswersToEnableByDecreasedRosterSize);
            enablementChanges.GroupsToBeEnabled.AddRange(rosterCalculationData.DisabledGroupsToEnableByDecreasedRosterSize);

            IReadOnlyInterviewStateDependentOnAnswers alteredState = state.Amend(getRosterInstanceIds: (groupId, groupOuterRosterVector)
                => isRoster(groupId, groupOuterRosterVector)
                    ? rosterInstanceIds
                    : state.GetRosterInstanceIds(groupId, groupOuterRosterVector));
            List<Identity> answersForLinkedQuestionsToRemoveByDisabling =
                this.GetAnswersForLinkedQuestionsToRemoveBecauseOfDisabledGroupsOrQuestions(
                    alteredState,
                    enablementChanges.GroupsToBeDisabled,
                    enablementChanges.QuestionsToBeDisabled,
                    questionnaire);

            IEnumerable<Identity> answersForLinkedQuestionsToRemoveByEmptyAnswer = this
                .GetAnswersForLinkedQuestionsToRemoveBecauseOfRemovedQuestionAnswers(
                    state,
                    new Identity(questionId, rosterVector).ToEnumerable(),
                    questionnaire);


            List<Identity> answersForLinkedQuestionsToRemove = Enumerable
                .Union(answersForLinkedQuestionsToRemoveByEmptyAnswer, answersForLinkedQuestionsToRemoveByDisabling)
                .ToList();

            var interviewByAnswerChange = new List<AnswerChange>
            {
                new AnswerChange(AnswerChangeType.RemoveAnswer, userId, questionId, rosterVector, removeTime, null)
            };

            var substitutionChanges = new List<Identity>(this.CalculateChangesInSubstitutedQuestions(questionId, rosterVector, questionnaire, alteredState));

            var interviewChanges= new InterviewChanges(interviewByAnswerChange,
                enablementChanges,
                validationChanges,
                rosterCalculationData,
                answersForLinkedQuestionsToRemove,
                rosterInstancesWithAffectedTitles,
                null,
                substitutionChanges);

            interviewChanges.AnswersForLinkedQuestionsToRemove.AddRange(answersToRemoveByCascading);
            
            return interviewChanges;
        }

        private IInterviewExpressionStateV6 PrepareExpressionProcessorStateForCalculations()
        {
            IInterviewExpressionStateV6 expressionProcessorState = this.ExpressionProcessorStatePrototype.Clone();

            expressionProcessorState.SaveAllCurrentStatesAsPrevious();

            return expressionProcessorState;
        }

        private InterviewChanges CalculateInterviewChangesOnAnswerQuestion(IReadOnlyInterviewStateDependentOnAnswers state, Guid userId, Guid questionId, RosterVector rosterVector,
            object answer, string answerFormattedAsRosterTitle, AnswerChangeType answerChangeType, DateTime answerTime, IQuestionnaire questionnaire,
            IInterviewExpressionStateV6 expressionProcessorState)
        {
            return this.CalculateInterviewChangesOnAnswerQuestion(
                state, userId, questionId, rosterVector, answer, answerFormattedAsRosterTitle, answer == null, answerChangeType, answerTime, questionnaire, expressionProcessorState);
        }

        private InterviewChanges CalculateInterviewChangesOnAnswerQuestion(IReadOnlyInterviewStateDependentOnAnswers state, Guid userId, Guid questionId, RosterVector rosterVector,
            object answer, string answerFormattedAsRosterTitle, bool isNewAnswerEmpty, AnswerChangeType answerChangeType, DateTime answerTime, IQuestionnaire questionnaire,
            IInterviewExpressionStateV6 expressionProcessorState)
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

            IEnumerable<Identity> answersForLinkedQuestionsToRemoveByEmptyAnswer =
                isNewAnswerEmpty
                    ? this.GetAnswersForLinkedQuestionsToRemoveBecauseOfRemovedQuestionAnswers(
                        state,
                        new Identity(questionId, rosterVector).ToEnumerable(), 
                        questionnaire)
                    : Enumerable.Empty<Identity>();

            List<Identity> answersForLinkedQuestionsToRemoveByDisabling = this
                .GetAnswersForLinkedQuestionsToRemoveBecauseOfDisabledGroupsOrQuestions(
                    state,
                    enablementChanges.GroupsToBeDisabled,
                    enablementChanges.QuestionsToBeDisabled,
                    questionnaire);

            List<Identity> answersForLinkedQuestionsToRemove = Enumerable
                .Union(answersForLinkedQuestionsToRemoveByEmptyAnswer, answersForLinkedQuestionsToRemoveByDisabling)
                .ToList();

            var interviewByAnswerChange = new List<AnswerChange>
            {
                new AnswerChange(answerChangeType, userId, questionId, rosterVector, answerTime, answer)
            };

            var substitutedQuestions = new List<Identity>(this.CalculateChangesInSubstitutedQuestions(questionId, rosterVector, questionnaire, this.interviewState));

            return new InterviewChanges(
                interviewByAnswerChange,
                enablementChanges,
                validationChanges,
                null,
                answersForLinkedQuestionsToRemove,
                rosterInstancesWithAffectedTitles,
                answerFormattedAsRosterTitle,
                substitutedQuestions);
        }

        private IEnumerable<Identity> CalculateChangesInSubstitutedQuestions(Guid questionId, RosterVector rosterVector, IQuestionnaire questionnaire,
            IReadOnlyInterviewStateDependentOnAnswers state)
        {
            var substitutedQuestionIds = questionnaire.GetSubstitutedQuestions(questionId);

            var instances = this.GetInstancesOfQuestionsWithSameAndDeeperRosterLevelOrThrow(state, 
                substitutedQuestionIds, 
                rosterVector, 
                questionnaire);

            return instances;
        }

        private IEnumerable<Identity> GetQuestionsToRemoveAnswersFromDependingOnCascading(Guid questionId, RosterVector rosterVector, IQuestionnaire questionnaire, IReadOnlyInterviewStateDependentOnAnswers state)
        {
            IEnumerable<Guid> dependentQuestionIds = questionnaire.GetCascadingQuestionsThatDependUponQuestion(questionId);
            IEnumerable<Identity> questions = this.GetInstancesOfQuestionsWithSameAndDeeperRosterLevelOrThrow(state, dependentQuestionIds, rosterVector, questionnaire);

            return questions.Where(state.WasQuestionAnswered);
        }

        private void CalculateChangesByFeaturedQuestion(IInterviewExpressionStateV6 expressionProcessorState, InterviewChangeStructures changeStructures, Guid userId,
            IQuestionnaire questionnaire, Dictionary<Guid, object> answersToFeaturedQuestions,
            DateTime answersTime, Dictionary<Identity, object> newAnswers, RosterVector rosterVector = null)
        {
            var currentQuestionRosterVector = rosterVector ?? EmptyRosterVector;
            Func<IReadOnlyInterviewStateDependentOnAnswers, Identity, object> getAnswer =
                (currentState, question) => newAnswers.Any(x => question == x.Key)
                    ? newAnswers.SingleOrDefault(x => question == x.Key).Value
                    : this.GetEnabledQuestionAnswerSupportedInExpressions(changeStructures.State, question, questionnaire);

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
                        interviewChanges = questionnaire.IsQuestionYesNo(questionId)
                            ? this.CalculateInterviewChangesOnYesNoQuestionAnswer(
                                new Identity(questionId, currentQuestionRosterVector), (AnsweredYesNoOption[]) answer,
                                answersTime, userId, questionnaire, expressionProcessorState, changeStructures.State, getAnswer)
                            : this.CalculateInterviewChangesOnAnswerMultipleOptionsQuestion(expressionProcessorState, changeStructures.State, userId, questionId,
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
                            this.CalculateInterviewChangesOnAnswerTextListQuestion(expressionProcessorState, changeStructures.State, userId, questionId, currentQuestionRosterVector, answersTime, (Tuple<decimal, string>[])answer, getAnswer, questionnaire);
                        break;

                    default:
                        throw new InterviewException(string.Format(
                            "Question {0} has type {1} which is not supported as initial pre-filled question. InterviewId: {2}",
                            questionId, questionType, this.EventSourceId));
                }

                changeStructures.State.ApplyInterviewChanges(interviewChanges);
                changeStructures.Changes.Add(interviewChanges);
            }
        }

        private List<RosterCalculationData> CalculateFixedRostersData(IReadOnlyInterviewStateDependentOnAnswers state, IQuestionnaire questionnaire,
            decimal[] outerRosterVector = null)
        {
            if (outerRosterVector == null)
                outerRosterVector = EmptyRosterVector;

            Func<IReadOnlyInterviewStateDependentOnAnswers, Identity, object> getAnswer = (currentState, question) => null;

            List<Guid> fixedRosterIds = questionnaire.GetFixedRosterGroups().ToList();

            Dictionary<Guid, Dictionary<decimal, string>> rosterTitlesGroupedByRosterId = CalculateFixedRosterData(fixedRosterIds,                 questionnaire);

            Func<Guid, IEnumerable<decimal>> getFixedRosterInstanceIds =
                fixedRosterId => rosterTitlesGroupedByRosterId[fixedRosterId].Keys.ToList();

            return fixedRosterIds
                .Select(fixedRosterId => this.CalculateRosterData(state,
                    new List<Guid> { fixedRosterId },
                    outerRosterVector,
                    getFixedRosterInstanceIds(fixedRosterId),
                    rosterTitlesGroupedByRosterId[fixedRosterId],
                    questionnaire, getAnswer)
                ).ToList();
        }

        private IEnumerable<RosterCalculationData> CalculateDynamicRostersData(IReadOnlyInterviewStateDependentOnAnswers state,
            IQuestionnaire questionnaire, decimal[] outerRosterVector,
            Guid rosterId, Func<IReadOnlyInterviewStateDependentOnAnswers, Identity, object> getAnswer)
        {
            var nestedRosterIds = questionnaire.GetNestedRostersOfGroupById(rosterId);

            Func<Guid, decimal[], bool> isRoster = (groupId, groupOuterScopeRosterVector)
                => nestedRosterIds.Contains(groupId)
                    && AreEqualRosterVectors(groupOuterScopeRosterVector, outerRosterVector);

            foreach (var nestedRosterId in nestedRosterIds)
            {
                var rosterInstanceIds = this.GetRosterInstancesById(state, questionnaire, nestedRosterId, outerRosterVector, getAnswer);

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


        private RosterCalculationData CalculateRosterDataWithRosterTitlesFromTextListQuestions(IReadOnlyInterviewStateDependentOnAnswers state,
            IQuestionnaire questionnare, 
            RosterVector rosterVector, 
            List<Guid> rosterIds, Dictionary<decimal, int?> rosterInstanceIdsWithSortIndexes, 
            IQuestionnaire questionnaire, 
            Func<IReadOnlyInterviewStateDependentOnAnswers, Identity, object> getAnswer, 
            Tuple<decimal, string>[] answers, Tuple<decimal, string>[] changedAnswers)
        {
            RosterCalculationData rosterCalculationData = this.CalculateRosterData(state, questionnare,
                rosterIds, rosterVector, rosterInstanceIdsWithSortIndexes, null, questionnaire, getAnswer);

            var titlesForRosterInstances = rosterCalculationData
                .RosterInstancesToAdd
                .Select(rosterInstance => rosterInstance.RosterInstanceId)
                .Distinct()
                .ToDictionary(
                    rosterInstanceId => rosterInstanceId,
                    rosterInstanceId => answers.Single(x => x.Item1 == rosterInstanceId).Item2);

            foreach (var changedAnswer in changedAnswers)
            {
                titlesForRosterInstances.Add(changedAnswer.Item1, changedAnswer.Item2);
                foreach (var rosterId in rosterIds)
                {
                    int rosterRosterLevel = questionnaire.GetRosterLevelForGroup(rosterId);
                    int rosterOuterRosterLevel = rosterRosterLevel - 1;
                    IEnumerable<Guid> rosterGroupsStartingFromTop = questionnaire.GetRostersFromTopToSpecifiedGroup(rosterId);
                    IEnumerable<RosterVector> rosterOuterRosterVectors = ExtendRosterVector(state, 
                        rosterVector, 
                        rosterOuterRosterLevel, 
                        rosterGroupsStartingFromTop.WithoutLast().ToArray());

                    foreach (var rosterOuterRosterVector in rosterOuterRosterVectors)
                    {
                        rosterCalculationData.RosterInstancesToChange.Add(new RosterIdentity(rosterId, rosterOuterRosterVector, changedAnswer.Item1, null));
                    }
                }
            }

            rosterCalculationData.SetTitlesForRosterInstances(titlesForRosterInstances);

            return rosterCalculationData;
        }

        private RosterCalculationData CalculateRosterDataWithRosterTitlesFromMultipleOptionsQuestions(
            IReadOnlyInterviewStateDependentOnAnswers state,
            Guid questionId, RosterVector rosterVector, List<Guid> rosterIds,
            Dictionary<decimal, int?> rosterInstanceIdsWithSortIndexes, IQuestionnaire questionnaire,
            Func<IReadOnlyInterviewStateDependentOnAnswers, Identity, object> getAnswer)
        {
            RosterCalculationData rosterCalculationData = this.CalculateRosterData(state, questionnaire,
                rosterIds, rosterVector, rosterInstanceIdsWithSortIndexes, null, questionnaire, getAnswer);

            var titlesForRosterInstances = rosterCalculationData
                .RosterInstancesToAdd
                .Select(rosterInstance => rosterInstance.RosterInstanceId)
                .Distinct()
                .ToDictionary(
                    rosterInstanceId => rosterInstanceId,
                    rosterInstanceId => questionnaire.GetAnswerOptionTitle(questionId, rosterInstanceId));

            rosterCalculationData.SetTitlesForRosterInstances(titlesForRosterInstances);

            return rosterCalculationData;
        }

        private RosterCalculationData CalculateRosterDataWithRosterTitlesFromYesNoQuestions(Identity question, List<Guid> rosterIds,
             Dictionary<decimal, int?> rosterInstanceIdsWithSortIndexes, IQuestionnaire questionnaire,
             IReadOnlyInterviewStateDependentOnAnswers state, Func<IReadOnlyInterviewStateDependentOnAnswers, Identity, object> getAnswer)
        {
            RosterCalculationData rosterCalculationData = this.CalculateRosterData(state, questionnaire,
                rosterIds, question.RosterVector, rosterInstanceIdsWithSortIndexes, null, questionnaire, getAnswer);

            var titlesForRosterInstances = rosterCalculationData
                .RosterInstancesToAdd
                .Select(rosterInstance => rosterInstance.RosterInstanceId)
                .Distinct()
                .ToDictionary(
                    rosterInstanceId => rosterInstanceId,
                    rosterInstanceId => questionnaire.GetAnswerOptionTitle(question.Id, rosterInstanceId));

            rosterCalculationData.SetTitlesForRosterInstances(titlesForRosterInstances);

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

        private RosterCalculationData CalculateRosterData(IReadOnlyInterviewStateDependentOnAnswers state,
            List<Guid> rosterIds, decimal[] nearestToOuterRosterVector, IEnumerable<decimal> rosterInstanceIds,
            Dictionary<decimal, string> rosterTitles, IQuestionnaire questionnaire,
            Func<IReadOnlyInterviewStateDependentOnAnswers, Identity, object> getAnswer)
        {
            Dictionary<decimal, int?> rosterInstanceIdsWithSortIndexes =
                rosterInstanceIds.ToDictionary(
                    rosterInstanceId => rosterInstanceId,
                    rosterInstanceId => (int?)null);

            return this.CalculateRosterData(state, questionnaire,
                rosterIds, nearestToOuterRosterVector, rosterInstanceIdsWithSortIndexes, rosterTitles, questionnaire, getAnswer);
        }

        private RosterCalculationData CalculateRosterData(IReadOnlyInterviewStateDependentOnAnswers state, IQuestionnaire questionnare,
            List<Guid> rosterIds, decimal[] nearestToOuterRosterVector, Dictionary<decimal, int?> rosterInstanceIdsWithSortIndexes,
            Dictionary<decimal, string> rosterTitles,
            IQuestionnaire questionnaire,
            Func<IReadOnlyInterviewStateDependentOnAnswers, Identity, object> getAnswer)
        {
            List<RosterIdentity> rosterInstancesToAdd, rosterInstancesToRemove, rosterInstancesToChange = new List<RosterIdentity>();

            List<RosterCalculationData> rosterInstancesFromNestedLevels;
            this.CalculateChangesInRosterInstances(state, questionnare, rosterIds, nearestToOuterRosterVector,
                rosterInstanceIdsWithSortIndexes,
                getAnswer,
                out rosterInstancesToAdd, out rosterInstancesToRemove, out rosterInstancesFromNestedLevels);

            List<decimal> rosterInstanceIdsBeingRemoved = rosterInstancesToRemove.Select(instance => instance.RosterInstanceId).ToList();

            List<Identity> answersToRemoveByDecreasedRosterSize = this.GetAnswersToRemoveIfRosterInstancesAreRemoved(state,
                rosterIds, rosterInstanceIdsBeingRemoved, nearestToOuterRosterVector, questionnaire);

            List<Identity> disabledAnswersToEnableByDecreasedRosterSize = GetDisabledAnswersToEnableByDecreasedRosterSize(state,
                rosterIds, rosterInstanceIdsBeingRemoved, nearestToOuterRosterVector, questionnaire);

            List<Identity> disabledGroupsToEnableByDecreasedRosterSize = GetDisabledGroupsToEnableByDecreasedRosterSize(state,
                rosterInstancesToRemove, questionnaire);

            return new RosterCalculationData(rosterInstancesToAdd, rosterInstancesToRemove, rosterInstancesToChange,
                answersToRemoveByDecreasedRosterSize, disabledAnswersToEnableByDecreasedRosterSize, disabledGroupsToEnableByDecreasedRosterSize,
                rosterTitles, rosterInstancesFromNestedLevels);
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
                        new RosterIdentity(instanceDto.RosterId, instanceDto.OuterScopeRosterVector, instanceDto.RosterInstanceId,
                            instanceDto.SortIndex))
                .ToList();

            Dictionary<decimal, string> titlesForRosterInstancesToAdd = rosterInstancesDto.ToDictionary(
                dtoInstance => dtoInstance.RosterInstanceId,
                dtoInstance => dtoInstance.RosterTitle);

            return new RosterCalculationData(rosterInstancesToAdd, titlesForRosterInstancesToAdd);
        }

        private void CalculateChangesInRosterInstances(IReadOnlyInterviewStateDependentOnAnswers state, IQuestionnaire questionnaire,
            IEnumerable<Guid> rosterIds, decimal[] nearestToOuterRosterVector,
            Dictionary<decimal, int?> rosterInstanceIdsWithSortIndexes, Func<IReadOnlyInterviewStateDependentOnAnswers, Identity, object> getAnswer,
            out List<RosterIdentity> rosterInstancesToAdd, out List<RosterIdentity> rosterInstancesToRemove,
            out List<RosterCalculationData> rosterInstantiatesFromNestedLevels)
        {
            rosterInstancesToAdd = new List<RosterIdentity>();
            rosterInstancesToRemove = new List<RosterIdentity>();
            rosterInstantiatesFromNestedLevels = new List<RosterCalculationData>();

            IEnumerable<decimal> rosterInstanceIds = rosterInstanceIdsWithSortIndexes.Keys.ToList();
            foreach (var rosterId in rosterIds)
            {
                Guid[] rosterGroupsStartingFromTop = questionnaire.GetRostersFromTopToSpecifiedGroup(rosterId).ToArray();

                var outerVectorsForExtend =
                    GetOuterVectorForParentRoster(state, rosterGroupsStartingFromTop,
                        nearestToOuterRosterVector);

                foreach (var outerVectorForExtend in outerVectorsForExtend)
                {
                    var rosterInstanceIdsBeingAdded = GetRosterInstanceIdsBeingAdded(
                        existingRosterInstanceIds: this.interviewState.GetRosterInstanceIds(rosterId, outerVectorForExtend),
                        newRosterInstanceIds: rosterInstanceIds).Select(rosterInstanceId =>
                            new RosterIdentity(rosterId, outerVectorForExtend, rosterInstanceId,
                                sortIndex: rosterInstanceIdsWithSortIndexes[rosterInstanceId])).ToList();

                    rosterInstancesToAdd.AddRange(rosterInstanceIdsBeingAdded);

                    var listOfRosterInstanceIdsForRemove =
                        GetRosterInstanceIdsBeingRemoved(
                            this.interviewState.GetRosterInstanceIds(rosterId, outerVectorForExtend), rosterInstanceIds).Select(rosterInstanceId =>
                                new RosterIdentity(rosterId, outerVectorForExtend, rosterInstanceId)).ToList();

                    rosterInstancesToRemove.AddRange(listOfRosterInstanceIdsForRemove);

                    foreach (var rosterInstanceIdBeingAdded in rosterInstanceIdsBeingAdded)
                    {
                        var outerRosterVector = ((RosterVector) rosterInstanceIdBeingAdded.OuterRosterVector).ExtendWithOneCoordinate(rosterInstanceIdBeingAdded.RosterInstanceId);
                        var calculateDynamicRostersData = this.CalculateDynamicRostersData(state, questionnaire, outerRosterVector, rosterId, getAnswer);
                        rosterInstantiatesFromNestedLevels.AddRange(calculateDynamicRostersData);
                    }

                    RosterCalculationData nestedRostersDataForDelete = this.CalculateNestedRostersDataForDelete(state, questionnaire, rosterId,
                                                            listOfRosterInstanceIdsForRemove.Select(i => i.RosterInstanceId).ToList(), outerVectorForExtend);
                    rosterInstantiatesFromNestedLevels.Add(nestedRostersDataForDelete);
                }
            }
        }

        private RosterCalculationData CalculateNestedRostersDataForDelete(IReadOnlyInterviewStateDependentOnAnswers state,
            IQuestionnaire questionnaire, Guid rosterId, List<decimal> rosterInstanceIdsBeingRemoved, decimal[] nearestToOuterRosterVector)
        {
            var nestedRosterIds = questionnaire.GetNestedRostersOfGroupById(rosterId).ToList();

            int indexOfRosterInRosterVector = GetIndexOfRosterInRosterVector(rosterId, questionnaire);

            var listOfRosterInstanceIdsForRemove = new List<RosterIdentity>();

            List<Identity> answersToRemoveByDecreasedRosterSize = new List<Identity>();

            var rosterInstantiatesFromNestedLevels = new List<RosterCalculationData>();
            foreach (var nestedRosterId in nestedRosterIds)
            {
                Guid[] rosterGroupsStartingFromTop = questionnaire.GetRostersFromTopToSpecifiedGroup(nestedRosterId).ToArray();
                IEnumerable<RosterVector> outerVectorsForExtend = this.ExtendRosterVector(state, nearestToOuterRosterVector, rosterGroupsStartingFromTop.Length, rosterGroupsStartingFromTop);

                foreach (RosterVector outerVectorForExtend in outerVectorsForExtend)
                {
                    if (!rosterInstanceIdsBeingRemoved.Contains(outerVectorForExtend[indexOfRosterInRosterVector]))
                        continue;

                    var rosterIdForDelete = new RosterIdentity(nestedRosterId,
                        outerVectorForExtend.Take(outerVectorForExtend.Length - 1).ToArray(), outerVectorForExtend.Last(), null);

                    listOfRosterInstanceIdsForRemove.Add(rosterIdForDelete);

                    answersToRemoveByDecreasedRosterSize.AddRange(
                        this.GetAnswersToRemoveIfRosterInstancesAreRemoved(
                            state,
                            rosterIdForDelete.GroupId,
                            new List<decimal> { rosterIdForDelete.RosterInstanceId },
                            rosterIdForDelete.OuterRosterVector,
                            questionnaire));

                    rosterInstantiatesFromNestedLevels.Add(this.CalculateNestedRostersDataForDelete(state, questionnaire, nestedRosterId,
                        new List<decimal> { rosterIdForDelete.RosterInstanceId }, outerVectorForExtend));
                }
            }

            var disabledAnswersToEnableByDecreasedRosterSize = this.GetDisabledAnswersToEnableByDecreasedRosterSize(state,
                nestedRosterIds, rosterInstanceIdsBeingRemoved, nearestToOuterRosterVector, questionnaire);

            var disabledGroupsToEnableByDecreasedRosterSize = this.GetDisabledGroupsToEnableByDecreasedRosterSize(state,
                listOfRosterInstanceIdsForRemove, questionnaire);

            return new RosterCalculationData(
                new List<RosterIdentity>(), 
                listOfRosterInstanceIdsForRemove, 
                new List<RosterIdentity>(),
                answersToRemoveByDecreasedRosterSize, 
                disabledAnswersToEnableByDecreasedRosterSize,
                disabledGroupsToEnableByDecreasedRosterSize,
                new Dictionary<decimal, string>(),
                rosterInstantiatesFromNestedLevels);
        }
      
        private static List<RosterIdentity> CalculateRosterInstancesWhichTitlesAreAffected(Guid questionId, RosterVector rosterVector,
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
                throw new InterviewException(string.Format("Interview was completed by interviewer and cannot be deleted. InterviewId: {0}", EventSourceId));
        }

        private void ThrowIfQuestionDoesNotExist(Guid questionId, IQuestionnaire questionnaire)
        {
            if (!questionnaire.HasQuestion(questionId))
                throw new InterviewException(string.Format("Question with id '{0}' is not found. InterviewId: {1}", questionId, EventSourceId));
        }

        private void ThrowIfRosterVectorIsIncorrect(IReadOnlyInterviewStateDependentOnAnswers state, Guid questionId, RosterVector rosterVector, IQuestionnaire questionnaire)
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

        private void ThrowIfRosterVectorIsNull(Guid questionId, RosterVector rosterVector, IQuestionnaire questionnaire)
        {
            if (rosterVector == null)
                throw new InterviewException(string.Format(
                    "Roster information for question {0} is missing. Roster vector cannot be null. InterviewId: {1}",
                    FormatQuestionForException(questionId, questionnaire), EventSourceId));
        }

        private void ThrowIfRosterVectorLengthDoesNotCorrespondToParentRosterGroupsCount(
            Guid questionId, RosterVector rosterVector, Guid[] parentRosterGroups, IQuestionnaire questionnaire)
        {
            if (!DoesRosterVectorLengthCorrespondToParentRosterGroupsCount(rosterVector, parentRosterGroups))
                throw new InterviewException(string.Format(
                    "Roster information for question {0} is incorrect. " +
                        "Roster vector has {1} elements, but parent roster groups count is {2}. InterviewId: {3}",
                    FormatQuestionForException(questionId, questionnaire), rosterVector.Length, parentRosterGroups.Length, this.EventSourceId));
        }

        private void ThrowIfSomeOfRosterVectorValuesAreInvalid(IReadOnlyInterviewStateDependentOnAnswers state,
            Guid questionId, RosterVector rosterVector, Guid[] parentRosterGroupIdsStartingFromTop, IQuestionnaire questionnaire)
        {
            for (int indexOfRosterVectorElement = 0; indexOfRosterVectorElement < rosterVector.Length; indexOfRosterVectorElement++)
            {
                decimal rosterInstanceId = rosterVector[indexOfRosterVectorElement];
                Guid rosterGroupId = parentRosterGroupIdsStartingFromTop[indexOfRosterVectorElement];

                int rosterGroupOuterScopeRosterLevel = indexOfRosterVectorElement;
                decimal[] rosterGroupOuterScopeRosterVector = rosterVector.Shrink(rosterGroupOuterScopeRosterLevel);
                IEnumerable<decimal> rosterInstanceIds = state.GetRosterInstanceIds(rosterGroupId, rosterGroupOuterScopeRosterVector);

                if (!rosterInstanceIds.Contains(rosterInstanceId))
                    throw new InterviewException(string.Format(
                        "Roster information for question {0} is incorrect. " +
                            "Roster vector element with index [{1}] refers to instance of roster group {2} by instance id [{3}] " +
                            "but roster group has only following roster instances: {4}. InterviewId: {5}",
                        FormatQuestionForException(questionId, questionnaire), indexOfRosterVectorElement,
                        FormatGroupForException(rosterGroupId, questionnaire), rosterInstanceId,
                        string.Join(", ", rosterInstanceIds), this.EventSourceId));
            }
        }

        protected bool DoesRosterInstanceExist(IReadOnlyInterviewStateDependentOnAnswers state, RosterVector rosterVector, Guid[] parentRosterIdsStartingFromTop)
        {
            for (int indexOfRosterVectorElement = 0; indexOfRosterVectorElement < rosterVector.Length; indexOfRosterVectorElement++)
            {
                decimal rosterInstanceId = rosterVector[indexOfRosterVectorElement];
                Guid rosterGroupId = parentRosterIdsStartingFromTop[indexOfRosterVectorElement];

                int rosterGroupOuterScopeRosterLevel = indexOfRosterVectorElement;
                decimal[] rosterGroupOuterScopeRosterVector = rosterVector.Shrink(rosterGroupOuterScopeRosterLevel);
                IEnumerable<decimal> rosterInstanceIds = state.GetRosterInstanceIds(rosterGroupId, rosterGroupOuterScopeRosterVector);

                var rosterInstanceExists = rosterInstanceIds.Contains(rosterInstanceId);
                if (!rosterInstanceExists)
                    return false;
            }

            return true;
        }

        protected static bool DoesRosterVectorLengthCorrespondToParentRosterGroupsCount(RosterVector rosterVector, Guid[] parentRosterGroups)
        {
            return rosterVector.Length == parentRosterGroups.Length;
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

        private void ThrowIfLinkedQuestionDoesNotHaveAnswer(IReadOnlyInterviewStateDependentOnAnswers state, Identity answeredQuestion,
            Identity answeredLinkedQuestion, IQuestionnaire questionnaire)
        {
            if (!state.WasQuestionAnswered(answeredLinkedQuestion))
            {
                throw new InterviewException(string.Format(
                    "Could not set answer for question {0} because his dependent linked question {1} does not have answer. InterviewId: {2}",
                    FormatQuestionForException(answeredQuestion, questionnaire),
                    FormatQuestionForException(answeredLinkedQuestion, questionnaire),
                    this.EventSourceId));
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

        private void ThrowIfCascadingQuestionValueIsNotOneOfParentAvailableOptions(IReadOnlyInterviewStateDependentOnAnswers interviewState, Identity answeredQuestion, RosterVector rosterVector, decimal value, IQuestionnaire questionnaire)
        {
            var questionId = answeredQuestion.Id;
            Guid? cascadingId = questionnaire.GetCascadingQuestionParentId(questionId);

            if (!cascadingId.HasValue) return;

            decimal childParentValue = questionnaire.GetCascadingParentValue(questionId, value);

            var questionIdentity = GetInstanceOfQuestionWithSameAndUpperRosterLevelOrThrow(cascadingId.Value, rosterVector, questionnaire);

            if (!interviewState.WasQuestionAnswered(questionIdentity))
                return;

            object answer = interviewState.GetAnswerSupportedInExpressions(questionIdentity);
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

        private void ThrowIfQuestionOrParentGroupIsDisabled(IReadOnlyInterviewStateDependentOnAnswers state, Identity question, IQuestionnaire questionnaire)
        {
            if (state.IsQuestionDisabled(question))
                throw new InterviewException(string.Format(
                    "Question {1} is disabled by it's following enablement condition:{0}{2}{0}InterviewId: {3}",
                    Environment.NewLine,
                    FormatQuestionForException(question, questionnaire),
                    questionnaire.GetCustomEnablementConditionForQuestion(question.Id),
                    this.EventSourceId));

            IEnumerable<Guid> parentGroupIds = questionnaire.GetAllParentGroupsForQuestion(question.Id);
            IEnumerable<Identity> parentGroups = GetInstancesOfGroupsWithSameAndUpperRosterLevelOrThrow(parentGroupIds,
                question.RosterVector, questionnaire);

            foreach (Identity parentGroup in parentGroups)
            {
                if (state.IsGroupDisabled(parentGroup))
                    throw new InterviewException(string.Format(
                        "Question {1} is disabled because parent group {2} is disabled by it's following enablement condition:{0}{3}{0}InterviewId: {4}",
                        Environment.NewLine,
                        FormatQuestionForException(question, questionnaire),
                        FormatGroupForException(parentGroup, questionnaire),
                        questionnaire.GetCustomEnablementConditionForGroup(parentGroup.Id),
                        this.EventSourceId));
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
                        "Answer '{0}' for question {1}  is incorrect because has more decimal places than allowed by questionnaire. Allowed amount of decimal places is {2}. InterviewId: {3}", 
                        answer,
                        FormatQuestionForException(questionId, questionnaire),
                        countOfDecimalPlacesAllowed.Value,
                        EventSourceId));
        }

        private void ThrowIfRosterSizeAnswerIsNegativeOrGreaterThenMaxRosterRowCount(Guid questionId, int answer,
            IQuestionnaire questionnaire)
        {
            if (answer < 0)
                throw new InterviewException(string.Format(
                    "Answer '{0}' for question {1} is incorrect because question is used as size of roster and specified answer is negative. InterviewId: {2}",
                    answer, FormatQuestionForException(questionId, questionnaire), EventSourceId));

            var maxRosterRowCount = questionnaire.GetMaxRosterRowCount();
            if (answer > maxRosterRowCount)
                throw new InterviewException(string.Format(
                    "Answer '{0}' for question {1} is incorrect because question is used as size of roster and specified answer is greater than {3}. InterviewId: {2}",
                    answer, FormatQuestionForException(questionId, questionnaire), EventSourceId, maxRosterRowCount));
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

        private void ThrowIfTryAssignToSameInterviewer(Guid interviewerIdToAssign)
        {
            if (this.status == InterviewStatus.InterviewerAssigned && this.interviewerId == interviewerIdToAssign)
                throw new InterviewException(string.Format(
                    "Interview has assigned on this interviewer already. InterviewId: {0}, InterviewerId: {1}",
                    EventSourceId, this.interviewerId));
        }

        protected void ThrowIfInterviewHardDeleted()
        {
            if (this.wasHardDeleted)
                throw new InterviewException(string.Format("Interview {0} status is hard deleted.", EventSourceId), InterviewDomainExceptionType.InterviewHardDeleted);
        }
        protected void ThrowIfInterviewReceivedByInterviewer()
        {
            if (this.receivedByInterviewer) 
                throw new InterviewException($"Can't modify Interview {this.EventSourceId} on server, because it received by interviewer.");
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

        protected IQuestionnaire GetQuestionnaireOrThrow(Guid id, long version)
        {
            IQuestionnaire questionnaire = this.questionnaireRepository.GetHistoricalQuestionnaire(id, version);

            if (questionnaire == null)
                throw new InterviewException(string.Format("Questionnaire with id '{0}' of version {1} is not found.", id, version), InterviewDomainExceptionType.QuestionnaireIsMissing);

            return questionnaire;
        }

        private Guid GetLinkedQuestionIdOrThrow(Guid questionId, IQuestionnaire questionnaire)
        {
            return questionnaire.GetQuestionReferencedByLinkedQuestion(questionId);
        }

        private Dictionary<decimal, Tuple<string, int?>> GetRosterInstancesById(IReadOnlyInterviewStateDependentOnAnswers state,
            IQuestionnaire questionnaire, Guid rosterId,
            decimal[] outerRosterVector, Func<IReadOnlyInterviewStateDependentOnAnswers, Identity, object> getAnswer)
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
                            this.logger.Error("invalid cast of int answer on trigger question", e);
                        }
                    }

                    break;
                case QuestionType.MultyOption:
                    if (questionnaire.IsQuestionYesNo(rosterSizeQuestionId.Value))
                    {
                        var yesNoAnswer = answerOnRosterSizeQuestion as AnsweredYesNoOption[];
                        if (yesNoAnswer != null)
                        {
                            return yesNoAnswer.Where(a => a.Yes).Select(a => a.OptionValue).ToDictionary(
                                x => x,
                                x => new Tuple<string, int?>(questionnaire.GetAnswerOptionTitle(rosterSizeQuestionId.Value, x), (int?)x));
                        }
                    }
                    else
                    {
                        var multyOptionAnswer = answerOnRosterSizeQuestion as decimal[];
                        if (multyOptionAnswer != null)
                        {
                            return multyOptionAnswer.ToDictionary(
                                x => x,
                                x => new Tuple<string, int?>(questionnaire.GetAnswerOptionTitle(rosterSizeQuestionId.Value, x), (int?) x));
                        }
                    }
                    break;
                case QuestionType.TextList:

                    var currentAnswer = answerOnRosterSizeQuestion as Tuple<decimal, string>[];
                    if (currentAnswer != null)
                    {
                        return currentAnswer.ToDictionary(x => x.Item1, x => new Tuple<string, int?>(x.Item2, (int?)x.Item1));
                    }

                    var textListAnswer = state.GetTextListAnswer(rosterSizeQuestionIdentity);
                    if (textListAnswer != null)
                    {
                        return textListAnswer.ToDictionary(x => x.Item1,
                            x => new Tuple<string, int?>(x.Item2, (int?)x.Item1));
                    }
                    break;
            }
            return new Dictionary<decimal, Tuple<string, int?>>();
        }


        private void ValidatePrefilledQuestions(IQuestionnaire questionnaire, Dictionary<Guid, object> answersToFeaturedQuestions,
            RosterVector rosterVector = null, IReadOnlyInterviewStateDependentOnAnswers currentInterviewState = null, bool applyStrongChecks = true)
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
                        if (questionnaire.IsQuestionYesNo(questionId))
                        {
                            this.CheckYesNoQuestionInvariants(new Identity(questionId, currentRosterVector), (AnsweredYesNoOption[]) answer, questionnaire, currentInterviewState);
                        }
                        else
                        {
                            this.CheckMultipleOptionQuestionInvariants(questionId, currentRosterVector, (decimal[]) answer, questionnaire, answeredQuestion, currentInterviewState, applyStrongChecks);
                        }
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

        private List<Identity> GetAnswersToRemoveIfRosterInstancesAreRemoved(IReadOnlyInterviewStateDependentOnAnswers state,
            IEnumerable<Guid> rosterIds, List<decimal> rosterInstanceIdsBeingRemoved, decimal[] nearestToOuterRosterVector,
            IQuestionnaire questionnaire)
        {
            if (rosterInstanceIdsBeingRemoved.Count == 0)
                return new List<Identity>();

            var answersToRemoveIfRosterInstancesAreRemoved = rosterIds
                .SelectMany(rosterId => this.GetAnswersToRemoveIfRosterInstancesAreRemoved(state, rosterId, rosterInstanceIdsBeingRemoved, nearestToOuterRosterVector, questionnaire))
                .ToList();

            return answersToRemoveIfRosterInstancesAreRemoved;
        }

        private List<Identity> GetDisabledGroupsToEnableByDecreasedRosterSize(
            IReadOnlyInterviewStateDependentOnAnswers state,
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

                    var underlyingGroupInstances = this.GetInstancesOfGroupsWithSameAndDeeperRosterLevelOrThrow(state,
                        underlyingChildGroupIds, rosterAsGroupIdentity.RosterVector, questionnaire)
                        .ToList();

                    var underlyingDisabledQuestionsByRemovedRosterInstances = (
                        from @group in underlyingGroupInstances
                        where state.IsGroupDisabled(@group)
                        select @group
                        ).ToList();

                    if (state.IsGroupDisabled(rosterAsGroupIdentity))
                    {
                        underlyingDisabledQuestionsByRemovedRosterInstances.Add(rosterAsGroupIdentity);
                    }

                    return underlyingDisabledQuestionsByRemovedRosterInstances;
                }).ToList();
        }

        private List<Identity> GetDisabledAnswersToEnableByDecreasedRosterSize(
            IReadOnlyInterviewStateDependentOnAnswers state,
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

                     IEnumerable<Identity> underlyingQuestionInstances = this.GetInstancesOfQuestionsWithSameAndDeeperRosterLevelOrThrow(state,
                         underlyingQuestionIds, nearestToOuterRosterVector, questionnaire);

                     var underlyingDisabledQuestionsByRemovedRosterInstances = (
                         from question in underlyingQuestionInstances
                         where rosterInstanceIdsBeingRemoved.Contains(question.RosterVector[indexOfRosterInRosterVector])
                         where state.IsQuestionDisabled(question)
                         select question
                         ).ToList();

                     return underlyingDisabledQuestionsByRemovedRosterInstances;
                 }).ToList();
        }


        private IEnumerable<Identity> GetAnswersToRemoveIfRosterInstancesAreRemoved(IReadOnlyInterviewStateDependentOnAnswers state,
            Guid rosterId, List<decimal> rosterInstanceIdsBeingRemoved, decimal[] nearestToOuterRosterVector,
            IQuestionnaire questionnaire)
        {
            int indexOfRosterInRosterVector = GetIndexOfRosterInRosterVector(rosterId, questionnaire);

            IEnumerable<Guid> underlyingQuestionIds = questionnaire.GetAllUnderlyingQuestions(rosterId);

            IEnumerable<Identity> underlyingQuestionInstances = this.GetInstancesOfQuestionsWithSameAndDeeperRosterLevelOrThrow(state,
                underlyingQuestionIds, nearestToOuterRosterVector, questionnaire);

            IEnumerable<Identity> underlyingQuestionsBeingRemovedByRemovedRosterInstances = (
                from question in underlyingQuestionInstances
                where state.WasQuestionAnswered(question)
                where rosterInstanceIdsBeingRemoved.Contains(question.RosterVector[indexOfRosterInRosterVector])
                select question
                ).ToList();
         
            IEnumerable<Identity> linkedQuestionsWithNoLongerValidAnswersBecauseOfSelectedOptionBeingRemoved =
                this.GetAnswersForLinkedQuestionsToRemoveBecauseOfRemovedQuestionAnswers(state,
                    underlyingQuestionsBeingRemovedByRemovedRosterInstances, questionnaire);

            IEnumerable<Identity> linkedQuestionsWithNoLongerValidAnswersBecauseOfRosterRowBeingRemoved =
        this.GetAnswersForLinkedQuestionsToRemoveBecauseOfRemovedRow(state, questionnaire,
            rosterId, rosterInstanceIdsBeingRemoved, nearestToOuterRosterVector);
            return Enumerable.Concat(Enumerable.Concat(
                underlyingQuestionsBeingRemovedByRemovedRosterInstances,
                linkedQuestionsWithNoLongerValidAnswersBecauseOfSelectedOptionBeingRemoved),
                linkedQuestionsWithNoLongerValidAnswersBecauseOfRosterRowBeingRemoved);
        }

        private static IEnumerable<decimal> GetRosterInstanceIdsBeingAdded(IEnumerable<decimal> existingRosterInstanceIds, IEnumerable<decimal> newRosterInstanceIds)
        {
            return newRosterInstanceIds.Where(newRosterInstanceId => !existingRosterInstanceIds.Contains(newRosterInstanceId)).ToList();
        }

        private static IEnumerable<decimal> GetRosterInstanceIdsBeingRemoved(IEnumerable<decimal> existingRosterInstanceIds, IEnumerable<decimal> newRosterInstanceIds)
        {
            return existingRosterInstanceIds.Where(existingRosterInstanceId => !newRosterInstanceIds.Contains(existingRosterInstanceId)).ToList();
        }

        private IEnumerable<Identity> GetAnswersForLinkedQuestionsToRemoveBecauseOfRemovedQuestionAnswers(
            IReadOnlyInterviewStateDependentOnAnswers state,
            IEnumerable<Identity> questionsToRemove, IQuestionnaire questionnaire)
        {
            bool nothingGoingToBeRemoved = !questionsToRemove.Any();
            if (nothingGoingToBeRemoved)
                return Enumerable.Empty<Identity>();

            return this.GetAnswersForLinkedQuestionsToRemoveBecauseOfReferencedAnswersGoingToDisappear(state, questionnaire,
                isQuestionAnswerGoingToDisappear: 
                    question => questionsToRemove.Any(questionToRemove => question == questionToRemove));
        }

        private List<Identity> GetAnswersForLinkedQuestionsToRemoveBecauseOfDisabledGroupsOrQuestions(
            IReadOnlyInterviewStateDependentOnAnswers state,
            IEnumerable<Identity> groupsToBeDisabled, IEnumerable<Identity> questionsToBeDisabled, IQuestionnaire questionnaire)
        {
            bool nothingGoingToBeDisabled = !groupsToBeDisabled.Any() && !questionsToBeDisabled.Any();
            if (nothingGoingToBeDisabled)
                return new List<Identity>();

            return this.GetAnswersForLinkedQuestionsToRemoveBecauseOfReferencedAnswersGoingToDisappear(state, questionnaire,
                isQuestionAnswerGoingToDisappear:
                    question => IsQuestionGoingToBeDisabled(question, groupsToBeDisabled, questionsToBeDisabled, questionnaire));
        }
        private List<Identity> GetAnswersForLinkedQuestionsToRemoveBecauseOfRemovedRow(
          IReadOnlyInterviewStateDependentOnAnswers state,
          IQuestionnaire questionnaire, Guid rosterId, List<decimal> rosterInstanceIdsBeingRemoved, decimal[] nearestToOuterRosterVector)
        {
            var answersToRemove = new List<Identity>();
            var rosterVectorsToRemove =
                rosterInstanceIdsBeingRemoved.Select(x => nearestToOuterRosterVector.Union(new[] {x}).ToArray())
                    .ToArray();
            foreach (Tuple<Identity, RosterVector> linkedSingleOptionAnswer in state.GetAllLinkedToRosterSingleOptionAnswers(questionnaire))
            {
                var linkedQuestion = linkedSingleOptionAnswer.Item1;
                decimal[] linkedQuestionSelectedOption = linkedSingleOptionAnswer.Item2;

                var linkedToRoster = questionnaire.GetRosterReferencedByLinkedQuestion(linkedQuestion.Id);
                if(linkedToRoster!= rosterId)
                    continue;

                if (rosterVectorsToRemove.Any(x => x.SequenceEqual(linkedQuestionSelectedOption)))
                {
                    answersToRemove.Add(linkedQuestion);
                }
            }

            var allLinkedMultipleOptionsAnswers = state.GetAllLinkedToRosterMultipleOptionsAnswers(questionnaire);

            foreach (Tuple<Identity, RosterVector[]> linkedMultipleOptionsAnswer in allLinkedMultipleOptionsAnswers)
            {
                var linkedQuestion = linkedMultipleOptionsAnswer.Item1;
                RosterVector[] linkedQuestionSelectedOptions = linkedMultipleOptionsAnswer.Item2;

                var linkedToRoster = questionnaire.GetRosterReferencedByLinkedQuestion(linkedQuestion.Id);
                if (linkedToRoster != rosterId)
                    continue;

                foreach (var linkedQuestionSelectedOption in linkedQuestionSelectedOptions)
                {
                    if (rosterVectorsToRemove.Any(x => x.SequenceEqual(linkedQuestionSelectedOption)))
                    {
                        answersToRemove.Add(linkedQuestion);
                        break;
                    }
                }
            }

            return answersToRemove;
        }
        private List<Identity> GetAnswersForLinkedQuestionsToRemoveBecauseOfReferencedAnswersGoingToDisappear(
            IReadOnlyInterviewStateDependentOnAnswers state,
            IQuestionnaire questionnaire,
            Func<Identity, bool> isQuestionAnswerGoingToDisappear)
        {
            var answersToRemove = new List<Identity>();

            foreach (Tuple<Identity, RosterVector> linkedSingleOptionAnswer in state.GetAllLinkedSingleOptionAnswers(questionnaire))
            {
                var linkedQuestion = linkedSingleOptionAnswer.Item1;
                decimal[] linkedQuestionSelectedOption = linkedSingleOptionAnswer.Item2;

                IEnumerable<Identity> questionsReferencedByLinkedQuestion =
                    this.GetQuestionsReferencedByLinkedQuestion(state, linkedQuestion, questionnaire);

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

            var allLinkedMultipleOptionsAnswers = state.GetAllLinkedMultipleOptionsAnswers(questionnaire);

            foreach (Tuple<Identity, RosterVector[]> linkedMultipleOptionsAnswer in allLinkedMultipleOptionsAnswers)
            {
                var linkedQuestion = linkedMultipleOptionsAnswer.Item1;
                RosterVector[] linkedQuestionSelectedOptions = linkedMultipleOptionsAnswer.Item2;

                IEnumerable<Identity> questionsReferencedByLinkedQuestion =
                    this.GetQuestionsReferencedByLinkedQuestion(state, linkedQuestion, questionnaire);

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

        private IEnumerable<Identity> GetQuestionsReferencedByLinkedQuestion(IReadOnlyInterviewStateDependentOnAnswers state,
            Identity linkedQuestion, IQuestionnaire questionnaire)
        {
            Guid referencedQuestionId = questionnaire.GetQuestionReferencedByLinkedQuestion(linkedQuestion.Id);

            var rosterScopesForReferencedQuestion = questionnaire.GetRosterGroupsByRosterSizeQuestion(referencedQuestionId);
            var rosterScopeForLinkedQuestion = questionnaire.GetRosterGroupsByRosterSizeQuestion(linkedQuestion.Id);

            var commonRosterVectorBaseLength = rosterScopesForReferencedQuestion.Intersect(rosterScopeForLinkedQuestion).Count();

            var baseOfRosterVectorOfReferencedQuestion = new RosterVector(linkedQuestion.RosterVector.Take(commonRosterVectorBaseLength));

            var referencedQuestionIdentities = GetInstancesOfQuestionsWithSameAndDeeperRosterLevelOrThrow(state, referencedQuestionId, baseOfRosterVectorOfReferencedQuestion, questionnaire);

            return referencedQuestionIdentities;
        }

        private bool IsQuestionGoingToBeDisabled(Identity question,
            IEnumerable<Identity> groupsToBeDisabled, IEnumerable<Identity> questionsToBeDisabled, IQuestionnaire questionnaire)
        {
            bool questionIsListedToBeDisabled =
                questionsToBeDisabled.Any(questionToBeDisabled => question == questionToBeDisabled);

            IEnumerable<Guid> parentGroupIds = questionnaire.GetAllParentGroupsForQuestion(question.Id);
            IEnumerable<Identity> parentGroups = GetInstancesOfGroupsWithSameAndUpperRosterLevelOrThrow(parentGroupIds,
                question.RosterVector, questionnaire);

            bool someOfQuestionParentGroupsAreListedToBeDisabled = parentGroups.Any(parentGroup =>
                groupsToBeDisabled.Any(groupToBeDisabled => parentGroup == groupToBeDisabled));

            return questionIsListedToBeDisabled || someOfQuestionParentGroupsAreListedToBeDisabled;
        }

        private static int GetIndexOfRosterInRosterVector(Guid rosterId, IQuestionnaire questionnaire)
        {
            return questionnaire
                .GetRostersFromTopToSpecifiedGroup(rosterId)
                .ToList()
                .IndexOf(rosterId);
        }


        private InterviewChanges UpdateExpressionStateWithAnswersAndGetChanges(InterviewChangeStructures interviewChanges,
            IEnumerable<RosterCalculationData> rosterDatas)
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

                        var rosterInstanceTitle = changes.RosterCalculationData.GetRosterInstanceTitle(r.GroupId, r.RosterInstanceId);

                        if (rosterInstanceTitle != null)
                        {
                            expressionProcessorState.UpdateRosterTitle(r.GroupId, r.OuterRosterVector,
                                r.RosterInstanceId,
                                rosterInstanceTitle);
                        }
                    }
                    changes.RosterCalculationData.RosterInstancesToRemove.ForEach(r => expressionProcessorState.RemoveRoster(r.GroupId, r.OuterRosterVector, r.RosterInstanceId));
                }
            }

            foreach (var rosterData in rosterDatas)
            {
                foreach (var r in rosterData.RosterInstancesToAdd)
                {
                    expressionProcessorState.AddRoster(r.GroupId, r.OuterRosterVector, r.RosterInstanceId, r.SortIndex);

                    var rosterInstanceTitle = rosterData.GetRosterInstanceTitle(r.GroupId, r.RosterInstanceId);

                    if (rosterInstanceTitle != null)
                    {
                        expressionProcessorState.UpdateRosterTitle(r.GroupId, r.OuterRosterVector,
                            r.RosterInstanceId,
                            rosterInstanceTitle);
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
                null,
                null);
            return enablementAndValidityChanges;
        }

        private void UpdateExpressionProcessorStateWithAnswerChange(AnswerChange answerChange,
            IInterviewExpressionStateV6 expressionProcessorState)
        {
            switch (answerChange.InterviewChangeType)
            {
                case AnswerChangeType.RemoveAnswer:
                    RemoveAnswerFromExpressionProcessorState(expressionProcessorState, answerChange.QuestionId,
                        answerChange.RosterVector);
                    break;
                case AnswerChangeType.Text:
                    expressionProcessorState.UpdateTextAnswer(answerChange.QuestionId, answerChange.RosterVector,
                        (string) answerChange.Answer);
                    break;
                case AnswerChangeType.DateTime:
                    expressionProcessorState.UpdateDateAnswer(answerChange.QuestionId, answerChange.RosterVector,
                        (DateTime) answerChange.Answer);
                    break;
                case AnswerChangeType.TextList:
                    expressionProcessorState.UpdateTextListAnswer(answerChange.QuestionId, answerChange.RosterVector,
                        (Tuple<decimal, string>[]) answerChange.Answer);
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
                    expressionProcessorState.UpdateQrBarcodeAnswer(answerChange.QuestionId, answerChange.RosterVector,
                        (string) answerChange.Answer);
                    break;
                case AnswerChangeType.NumericInteger:
                    expressionProcessorState.UpdateNumericIntegerAnswer(answerChange.QuestionId,
                        answerChange.RosterVector, (int) answerChange.Answer);
                    break;
                case AnswerChangeType.NumericReal:
                    expressionProcessorState.UpdateNumericRealAnswer(answerChange.QuestionId, answerChange.RosterVector,
                        Convert.ToDouble(answerChange.Answer));
                    break;
                case AnswerChangeType.SingleOptionLinked:
                    expressionProcessorState.UpdateLinkedSingleOptionAnswer(answerChange.QuestionId,
                        answerChange.RosterVector,
                        (decimal[]) answerChange.Answer);
                    break;
                case AnswerChangeType.SingleOption:
                    expressionProcessorState.UpdateSingleOptionAnswer(answerChange.QuestionId, answerChange.RosterVector,
                        (decimal) answerChange.Answer);
                    break;
                case AnswerChangeType.MultipleOptionsLinked:
                    expressionProcessorState.UpdateLinkedMultiOptionAnswer(answerChange.QuestionId,
                        answerChange.RosterVector,
                        (decimal[][]) answerChange.Answer);
                    break;
                case AnswerChangeType.MultipleOptions:
                    expressionProcessorState.UpdateMultiOptionAnswer(answerChange.QuestionId, answerChange.RosterVector,
                        (decimal[]) answerChange.Answer);
                    break;
                case AnswerChangeType.YesNo:
                    expressionProcessorState.UpdateYesNoAnswer(answerChange.QuestionId, answerChange.RosterVector, ConvertToYesNoAnswersOnly((AnsweredYesNoOption[])answerChange.Answer));
                    break;
            }
        }

        private bool HasInvalidAnswers()
        {
            return this.interviewState.InvalidAnsweredQuestions.Any();
        }

        private static YesNoAnswersOnly ConvertToYesNoAnswersOnly(AnsweredYesNoOption[] answeredOptions)
        {
            var yesAnswers = answeredOptions.Where(x => x.Yes).Select(x => x.OptionValue).ToArray();
            var noAnswers = answeredOptions.Where(x => !x.Yes).Select(x => x.OptionValue).ToArray();
            return new YesNoAnswersOnly(yesAnswers, noAnswers);
        }
    }
}