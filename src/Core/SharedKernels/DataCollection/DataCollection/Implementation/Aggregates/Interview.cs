using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Ncqrs.Domain;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.CustomCollections;
using WB.Core.GenericSubdomains.Portable.Services;
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

        public virtual void Apply(InterviewReceivedByInterviewer @event)
        {
            this.properties.IsReceivedByInterviewer = true;
        }

        public virtual void Apply(InterviewReceivedBySupervisor @event)
        {
            this.properties.IsReceivedByInterviewer = false;
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
            this.properties.Status = @event.InterviewData.Status;
            this.properties.WasCompleted = @event.InterviewData.WasCompleted;
            
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
                            ((decimal[][])question.Answer).Select(rosterVector => (RosterVector)rosterVector).ToArray()));

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

            var orderedRosterInstances = @event.InterviewData.RosterGroupInstances.SelectMany(x => x.Value).OrderBy(x => x.OuterScopeRosterVector.Length).ToList();
            foreach (RosterSynchronizationDto roster in orderedRosterInstances)
            {
                this.ExpressionProcessorStatePrototype.AddRoster(roster.RosterId, roster.OuterScopeRosterVector, roster.RosterInstanceId, roster.SortIndex);
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
            if (@event.InterviewData.LinkedQuestionOptions != null)
            {
                this.interviewState.LinkedQuestionOptions.Clear();

                var changedLinkedOptions = @event.InterviewData.LinkedQuestionOptions.Select(x => new ChangedLinkedOptions(new Identity(x.Key.Id, x.Key.InterviewItemRosterVector), x.Value)).ToArray();

                this.interviewState.ApplyLinkedOptionQuestionChanges(changedLinkedOptions);
            }
            if (@event.InterviewData.Variables != null)
            {
                this.interviewState.VariableValues.Clear();

                this.interviewState.ChangeVariables(
                    @event.InterviewData.Variables.Select(
                        x => new ChangedVariable(new Identity(x.Key.Id, x.Key.InterviewItemRosterVector), x.Value))
                        .ToArray());
            }
            if (@event.InterviewData.DisabledVariables != null && @event.InterviewData.DisabledVariables.Any())
            {
                this.ExpressionProcessorStatePrototype.DisableVariables(
                    @event.InterviewData.DisabledVariables.Select(x => new Identity(x.Id, x.InterviewItemRosterVector))
                        .ToArray());
            }
            this.ExpressionProcessorStatePrototype.DeclareAnswersValid(@event.InterviewData.ValidAnsweredQuestions.Select(validAnsweredQuestion => new Identity(validAnsweredQuestion.Id, validAnsweredQuestion.InterviewItemRosterVector)));
            //should call this.ExpressionProcessorStatePrototype.ApplyFailedValidations(...) when sync is ready
            this.ExpressionProcessorStatePrototype.DeclareAnswersInvalid(@event.InterviewData.InvalidAnsweredQuestions.Select(validAnsweredQuestion => new Identity(validAnsweredQuestion.Id, validAnsweredQuestion.InterviewItemRosterVector)));

            this.ExpressionProcessorStatePrototype.DisableQuestions(@event.InterviewData.DisabledQuestions.Select(validAnsweredQuestion => new Identity(validAnsweredQuestion.Id, validAnsweredQuestion.InterviewItemRosterVector)));
            this.ExpressionProcessorStatePrototype.DisableGroups(@event.InterviewData.DisabledGroups.Select(validAnsweredQuestion => new Identity(validAnsweredQuestion.Id, validAnsweredQuestion.InterviewItemRosterVector)));
            if (@event.InterviewData.DisabledStaticTexts != null)
                this.ExpressionProcessorStatePrototype.DisableStaticTexts(@event.InterviewData.DisabledStaticTexts);

            this.interviewState.DisabledGroups = ToHashSetOfIdAndRosterVectorStrings(@event.InterviewData.DisabledGroups);
            this.interviewState.DisabledQuestions = ToHashSetOfIdAndRosterVectorStrings(@event.InterviewData.DisabledQuestions);
            this.interviewState.RosterGroupInstanceIds = BuildRosterInstanceIdsFromSynchronizationDto(@event.InterviewData);
            this.interviewState.ValidAnsweredQuestions = new ConcurrentHashSet<Identity>(@event.InterviewData.ValidAnsweredQuestions.Select(x => new Identity(x.Id, x.InterviewItemRosterVector)));
            this.interviewState.InvalidAnsweredQuestions = @event.InterviewData.FailedValidationConditions.ToDictionary();

            this.interviewState.DisabledStaticTexts = new ConcurrentHashSet<Identity>(@event.InterviewData?.DisabledStaticTexts ?? new List<Identity>());
            this.interviewState.ValidStaticTexts = new ConcurrentHashSet<Identity>(@event.InterviewData?.ValidStaticTexts ?? new List<Identity>());
            this.interviewState.InvalidStaticTexts = @event.InterviewData.InvalidStaticTexts.ToDictionary();

            this.interviewState.RosterTitles.Clear();
            var changedRosterTitles =
                @event.InterviewData.RosterGroupInstances.SelectMany(x => x.Value).Select(
                    r =>
                        new ChangedRosterInstanceTitleDto(
                            new RosterInstance(r.RosterId, r.OuterScopeRosterVector, r.RosterInstanceId), r.RosterTitle))
                    .ToArray();
            this.interviewState.ChangeRosterTitles(changedRosterTitles);
        }

        public virtual void Apply(SynchronizationMetadataApplied @event)
        {
            this.questionnaireId = @event.QuestionnaireId;
            this.questionnaireVersion = @event.QuestionnaireVersion;
            this.properties.Status = @event.Status;
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
                (RosterVector)@event.SelectedRosterVector);

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
            this.interviewState.DeclareAnswersInvalid(@event.FailedValidationConditions);

            if (@event.FailedValidationConditions.Count > 0)
            {
                this.ExpressionProcessorStatePrototype.ApplyFailedValidations(@event.FailedValidationConditions);
            }
            else //handling of old events
            {
                this.ExpressionProcessorStatePrototype.DeclareAnswersInvalid(@event.FailedValidationConditions.Keys);
            }
        }

        public virtual void Apply(StaticTextsDeclaredValid @event)
        {
            this.interviewState.DeclareStaticTextValid(@event.StaticTexts);
            this.ExpressionProcessorStatePrototype.DeclareStaticTextValid(@event.StaticTexts);
        }

        public virtual void Apply(StaticTextsDeclaredInvalid @event)
        {
            var staticTextsConditions = @event.GetFailedValidationConditionsDictionary();

            this.interviewState.DeclareStaticTextInvalid(staticTextsConditions);
            this.ExpressionProcessorStatePrototype.ApplyStaticTextFailedValidations(staticTextsConditions);
        }

        public void Apply(LinkedOptionsChanged @event)
        {
            this.interviewState.ApplyLinkedOptionQuestionChanges(@event.ChangedLinkedQuestions);
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

        public virtual void Apply(VariablesDisabled @event)
        {
            this.ExpressionProcessorStatePrototype.DisableVariables(@event.Variables);
            this.interviewState.DisableVariables(@event.Variables);
        }

        public virtual void Apply(VariablesEnabled @event)
        {
            this.ExpressionProcessorStatePrototype.EnableVariables(@event.Variables);
            this.interviewState.EnableVariables(@event.Variables);
        }

        public virtual void Apply(VariablesChanged @event)
        {
            this.interviewState.ChangeVariables(@event.ChangedVariables);

            foreach (var changedVariableValueDto in @event.ChangedVariables)
            {
                this.ExpressionProcessorStatePrototype.UpdateVariableValue(changedVariableValueDto.Identity, changedVariableValueDto.NewValue);
            }
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

        public virtual void Apply(StaticTextsEnabled @event)
        {
            this.interviewState.EnableStaticTexts(@event.StaticTexts);
            this.ExpressionProcessorStatePrototype.EnableStaticTexts(@event.StaticTexts);
        }

        public virtual void Apply(StaticTextsDisabled @event)
        {
            this.interviewState.DisableStaticTexts(@event.StaticTexts);
            this.ExpressionProcessorStatePrototype.DisableStaticTexts(@event.StaticTexts);
        }

        public virtual void Apply(AnswerCommented @event)
        {
            this.interviewState.AnswerComments.Add(new AnswerComment(@event.UserId, @event.CommentTime, @event.Comment, @event.QuestionId, @event.RosterVector));
        }

        public virtual void Apply(FlagSetToAnswer @event) { }

        public virtual void Apply(TranslationSwitched @event)
        {
            this.language = @event.Language;
        }

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
            this.properties.Status = @event.Status;
        }

        public virtual void Apply(SupervisorAssigned @event)
        {
        }

        public virtual void Apply(InterviewerAssigned @event)
        {
            this.properties.InterviewerId = @event.InterviewerId;
            this.properties.IsReceivedByInterviewer = false;
        }

        public virtual void Apply(InterviewDeleted @event) { }

        public virtual void Apply(InterviewHardDeleted @event)
        {
            this.properties.IsHardDeleted = true;
        }

        public virtual void Apply(InterviewSentToHeadquarters @event) { }

        public virtual void Apply(InterviewRestored @event) { }

        public virtual void Apply(InterviewCompleted @event)
        {
            this.properties.WasCompleted = true;
        }

        public virtual void Apply(InterviewRestarted @event) { }

        public virtual void Apply(InterviewApproved @event) { }

        public virtual void Apply(InterviewApprovedByHQ @event) { }

        public virtual void Apply(UnapprovedByHeadquarters @event) { }

        public virtual void Apply(InterviewRejected @event)
        {
            this.properties.WasCompleted = false;
        }

        public virtual void Apply(InterviewRejectedByHQ @event) { }

        public virtual void Apply(InterviewDeclaredValid @event) { }

        public virtual void Apply(InterviewDeclaredInvalid @event) { }

        public virtual void Apply(AnswersRemoved @event)
        {
            this.interviewState.RemoveAnswers(@event.Questions);

            foreach (var identity in @event.Questions)
            {
                RemoveAnswerFromExpressionProcessorState(this.ExpressionProcessorStatePrototype, identity.Id, identity.RosterVector);
            }
        }

        public virtual void Apply(AnswerRemoved @event)
        {
            this.interviewState.RemoveAnswers(new[] { new Identity(@event.QuestionId, @event.RosterVector) });
            RemoveAnswerFromExpressionProcessorState(this.ExpressionProcessorStatePrototype, @event.QuestionId, @event.RosterVector);
        }

        private void RemoveAnswerFromExpressionProcessorState(ILatestInterviewExpressionState state, Guid questionId, RosterVector rosterVector)
        {
            state.RemoveAnswer(new Identity(questionId, rosterVector));
        }

        #region Dependencies

        private readonly ILogger logger;

        /// <remarks>
        /// Repository operations are time-consuming.
        /// So this repository may be used only in command handlers.
        /// And should never be used in event handlers!!
        /// </remarks>
        private readonly IQuestionnaireStorage questionnaireRepository;

        private readonly IInterviewExpressionStatePrototypeProvider expressionProcessorStatePrototypeProvider;

        #endregion

        public Interview(ILogger logger, IQuestionnaireStorage questionnaireRepository, IInterviewExpressionStatePrototypeProvider expressionProcessorStatePrototypeProvider)
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

        public void CreateInterviewWithPreloadedData(CreateInterviewWithPreloadedData command)
        {
            this.SetQuestionnaireProperties(command.QuestionnaireId, command.Version);

            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow(command.QuestionnaireId, command.Version, language: null);

            var interviewChangeStructures = new InterviewChangeStructures();

            var fixedRosterCalculationDatas = this.CalculateFixedRostersData(interviewChangeStructures.State, questionnaire);

            foreach (var fixedRosterCalculationData in fixedRosterCalculationDatas)
            {
                var fixedRosterChanges = new InterviewChanges(
                    null, null, null, fixedRosterCalculationData, null, null, null, null, null, null, null);
                interviewChangeStructures.State.ApplyInterviewChanges(fixedRosterChanges);
                interviewChangeStructures.Changes.Add(fixedRosterChanges);
            }

            var orderedData = command.PreloadedData.Data.OrderBy(x => x.RosterVector.Length).ToArray();

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

                this.CalculateChangesByFeaturedQuestion(interviewExpressionStateForPreloading, interviewChangeStructures, command.UserId, questionnaire,
                    answersToFeaturedQuestions, command.AnswersTime, newAnswers, preloadedLevel.RosterVector);
            }

            var enablementAndValidityChanges = this.UpdateExpressionStateWithAnswersAndGetChanges(
                this.ExpressionProcessorStatePrototype.Clone(),
                interviewChangeStructures,
                fixedRosterCalculationDatas,
                questionnaire,
                command.AnswersTime,
                command.UserId);

            //apply events
            this.ApplyEvent(new InterviewFromPreloadedDataCreated(command.UserId, questionnaireId, questionnaire.Version));
            this.ApplyInterviewChanges(interviewChangeStructures.Changes);
            this.ApplyInterviewChanges(enablementAndValidityChanges);
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

            this.ValidatePrefilledQuestions(questionnaire, answersToFeaturedQuestions, RosterVector.Empty, state);

            var sourceInterviewTree = this.BuildInterviewTree(questionnaire, state);

            var newAnswers =
                answersToFeaturedQuestions.ToDictionary(
                    answersToFeaturedQuestion => new Identity(answersToFeaturedQuestion.Key, RosterVector.Empty),
                    answersToFeaturedQuestion => answersToFeaturedQuestion.Value);

            var changedInterviewTree = this.BuildInterviewTree(questionnaire, state);

            foreach (var newAnswer in newAnswers)
            {
                var question = changedInterviewTree.GetQuestion(newAnswer.Key);
                var questionId = newAnswer.Key.Id;
                var answer = newAnswer.Value;

                QuestionType questionType = questionnaire.GetQuestionType(questionId);
                switch (questionType)
                {
                    case QuestionType.Text:
                        question.AsText.SetAnswer(answer as string);
                        break;
                    case QuestionType.Numeric:
                        if (questionnaire.IsQuestionInteger(questionId))
                            question.AsInteger.SetAnswer((int)answer);
                        else
                            question.AsDouble.SetAnswer((double)answer);
                        break;
                    case QuestionType.DateTime:
                        question.AsDateTime.SetAnswer((DateTime)answer);
                        break;
                    case QuestionType.SingleOption:
                        question.AsSingleOption.SetAnswer((int)answer);
                        break;

                    case QuestionType.MultyOption:
                        question.AsMultiOption.SetAnswer((decimal[])answer);
                        break;
                    case QuestionType.QRBarcode:
                        question.AsQRBarcode.SetAnswer(answer as string);
                        break;
                    case QuestionType.GpsCoordinates:
                        question.AsGps.SetAnswer((GeoPosition)answer);
                        break;
                    case QuestionType.TextList:
                        question.AsTextList.SetAnswer((Tuple<decimal, string>[])answer);
                        break;

                    default:
                        throw new InterviewException(string.Format(
                            "Question {0} has type {1} which is not supported as initial pre-filled question. InterviewId: {2}",
                            questionId, questionType, this.EventSourceId));
                }
            }

            var changedQuestionIdentities = newAnswers.Keys.ToList();
            this.UpdateTreeWithNewAnswers(changedInterviewTree, questionnaire, changedQuestionIdentities.ToArray());

            ILatestInterviewExpressionState expressionProcessorState = this.ExpressionProcessorStatePrototype.Clone();

            this.UpdateExpressionState(sourceInterviewTree, changedInterviewTree, expressionProcessorState);

            expressionProcessorState.SaveAllCurrentStatesAsPrevious();
            EnablementChanges enablementChanges = expressionProcessorState.ProcessEnablementConditions();

            this.UpdateTreeWithEnablementChanges(changedInterviewTree, enablementChanges);

            this.UpdateTreeWithStructuralChanges(changedInterviewTree, expressionProcessorState.GetStructuralChanges());

            this.UpdateRosterTitles(changedInterviewTree, questionnaire);

            this.UpdateLinkedQuestions(changedInterviewTree, expressionProcessorState);

            VariableValueChanges variableValueChanges = expressionProcessorState.ProcessVariables();

            ValidityChanges validationChanges = expressionProcessorState.ProcessValidationExpressions();
            this.UpdateTreeWithValidationChanges(changedInterviewTree, validationChanges);

            //apply events
            this.ApplyEvent(new InterviewCreated(userId, questionnaireId, questionnaire.Version));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.Created, comment: null));

            this.ApplyEvents(sourceInterviewTree, changedInterviewTree, userId);

            if (variableValueChanges?.ChangedVariableValues?.Count > 0)
            {
                this.ApplyEvent(new VariablesChanged(variableValueChanges.ChangedVariableValues.Select(c => new ChangedVariable(c.Key, c.Value)).ToArray()));
            }

            this.ApplyEvent(new SupervisorAssigned(userId, supervisorId));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.SupervisorAssigned, comment: null));
        }

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

        private void UpdateRosterTitles(InterviewTree tree, IQuestionnaire questionnaire)
        {
            foreach (var roster in tree.FindRosters().Where(x => x.IsList))
            {
                var sizeQuestionId = roster.AsList.RosterSizeQuestion;
                var rosterSizeQuestion = roster.GetQuestionFromThisOrUpperLevel(sizeQuestionId);
                roster.SetRosterTitle(rosterSizeQuestion.AsTextList.GetTitleByItemCode(roster.Identity.RosterVector.Last()));
            }

            foreach (var roster in tree.FindRosters().Where(x => x.IsNumeric))
            {
                var titleQuestion = tree.GetQuestion(roster.AsNumeric.RosterTitleQuestionIdentity);
                if (titleQuestion==null) continue;
                var rosterTitle = titleQuestion.IsAnswered() 
                    ? titleQuestion.GetAnswerAsString(answerOptionValue => questionnaire.GetAnswerOptionTitle(titleQuestion.Identity.Id, answerOptionValue)) 
                    : null;
                roster.SetRosterTitle(rosterTitle);
            }
        }

        private void UpdateLinkedQuestions(InterviewTree tree, ILatestInterviewExpressionState interviewExpressionState)
        {
            if (!interviewExpressionState.AreLinkedQuestionsSupported())
            {
                var linkedQuestions = tree.FindQuestions().Where(x => x.IsLinked);
                foreach (InterviewTreeQuestion linkedQuestion in linkedQuestions)
                {
                    linkedQuestion.UpdateLinkedOptions();
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
                    var previousOptions = linkedQuestion.AsLinked.Options;
                    linkedQuestion.AsLinked.SetOptions(linkedQuestionWithOptions.Value);
                    if (previousOptions.SequenceEqual(linkedQuestionWithOptions.Value))
                        continue;

                    if (linkedQuestion.IsMultiLinkedOption)
                        linkedQuestion.AsMultiLinkedOption.RemoveAnswer();
                    else
                        linkedQuestion.AsSingleLinkedOption.RemoveAnswer();
                }
            }
        }

        public void CreateInterviewOnClient(QuestionnaireIdentity questionnaireIdentity, Guid supervisorId, DateTime answersTime, Guid userId)
        {
            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version, language: null);
            this.SetQuestionnaireProperties(questionnaireIdentity.QuestionnaireId, questionnaire.Version);

            InterviewChangeStructures interviewChangeStructures = new InterviewChangeStructures();

            var sourceInterviewTree = this.BuildInterviewTree(questionnaire, interviewChangeStructures.State);
            var changedInterview = this.BuildInterviewTree(questionnaire, interviewChangeStructures.State);

            this.UpdateTreeWithNewAnswers(changedInterview, questionnaire, new Identity[0]);

            var fixedRosterCalculationDatas = this.CalculateFixedRostersData(interviewChangeStructures.State, questionnaire);

            var enablementAndValidityChanges = this.UpdateExpressionStateWithAnswersAndGetChanges(
                this.ExpressionProcessorStatePrototype.Clone(),
                interviewChangeStructures,
                fixedRosterCalculationDatas,
                questionnaire,
                answersTime,
                userId);

            //apply events
            this.ApplyEvent(new InterviewOnClientCreated(userId, questionnaireIdentity.QuestionnaireId, questionnaire.Version));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.Created, comment: null));

            this.ApplyInterviewChanges(interviewChangeStructures.Changes);

            this.ApplyEvents(sourceInterviewTree, changedInterview, userId);
            this.UpdateExpressionState(sourceInterviewTree, changedInterview, this.GetClonedExpressionState());

            this.ApplyInterviewChanges(enablementAndValidityChanges);
            this.ApplyEvent(new SupervisorAssigned(userId, supervisorId));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.SupervisorAssigned, comment: null));

            this.ApplyEvent(new InterviewerAssigned(userId, userId, answersTime));
            this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.InterviewerAssigned, comment: null));
        }

        private class RosterNodeDescriptor
        {
            public Identity Identity { get; set; }
            public string Title { get; set; }

            public RosterType Type { get; set; }

            public InterviewTreeQuestion SizeQuestion { get; set; }
            public Identity RosterTitleQuestionIdentity { get; set; }
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
                YesNoAnswersOnly yesNoAnswersOnly = changedYesNoQuestion.Value;
                var yesNoOptionsList = new List<AnsweredYesNoOption>();
                yesNoOptionsList.AddRange(yesNoAnswersOnly.Yes.Select(yesOption => new AnsweredYesNoOption(yesOption, true)));
                yesNoOptionsList.AddRange(yesNoAnswersOnly.No.Select(noOption => new AnsweredYesNoOption(noOption, false)));
                tree.GetQuestion(changedYesNoQuestion.Key).AsYesNo.SetAnswer(yesNoOptionsList.ToArray());
            }

            foreach (var removedRosterIdentity in structuralChanges.RemovedRosters)
            {
                tree.RemoveNode(removedRosterIdentity);
            }
        }

        private void UpdateTreeWithNewAnswers(
            InterviewTree tree,
            IQuestionnaire questionnaire,
            Identity[] changedQuestions)
        {
            var itemsQueue = new Queue<IInterviewTreeNode>(tree.Sections);

            while (itemsQueue.Count > 0)
            {
                var currentItem = itemsQueue.Dequeue();

                if (currentItem is InterviewTreeGroup)
                {
                    var group = currentItem as InterviewTreeGroup;
                    var parentRosterVector = group.Identity.RosterVector;
                    var childEntityIds = questionnaire.GetChildEntityIds(currentItem.Identity.Id);

                    foreach (var childEntityId in childEntityIds)
                    {
                        if (questionnaire.IsRosterGroup(childEntityId))
                        {
                            Guid rosterId = childEntityId;
                            List<RosterNodeDescriptor> expectedRosterIdentitiesWithTitles = new List<RosterNodeDescriptor>();
                            if (questionnaire.IsFixedRoster(childEntityId))
                            {
                                var rosterTitles = questionnaire.GetFixedRosterTitles(childEntityId);

                                expectedRosterIdentitiesWithTitles =
                                    rosterTitles
                                    .Select((x, i) => new RosterNodeDescriptor
                                    {
                                        Identity = new RosterIdentity(childEntityId, parentRosterVector, x.Value).ToIdentity(),
                                        Title = x.Title,
                                        Type = RosterType.Fixed
                                    }).ToList();
                            }
                            else
                            {
                                Guid sourceQuestionId = questionnaire.GetRosterSizeQuestion(rosterId);
                                var questionaType = questionnaire.GetQuestionType(sourceQuestionId);
                                var rosterSizeQuestion = group.GetQuestionFromThisOrUpperLevel(sourceQuestionId);
                                switch (questionaType)
                                {
                                    case QuestionType.MultyOption:
                                        if (questionnaire.IsQuestionYesNo(sourceQuestionId))
                                        {
                                            var newYesNoAnswer = rosterSizeQuestion.AsYesNo.IsAnswered ? rosterSizeQuestion.AsYesNo.GetAnswer() : new AnsweredYesNoOption[0];
                                            expectedRosterIdentitiesWithTitles = CreateRosterIdentitiesForYesNoQuestion(newYesNoAnswer, rosterId, parentRosterVector, rosterSizeQuestion, questionnaire);
                                        }
                                        else
                                        {
                                            var newMultiAnswer = rosterSizeQuestion.AsMultiOption.IsAnswered ?  rosterSizeQuestion.AsMultiOption.GetAnswer() : new decimal[0];
                                            expectedRosterIdentitiesWithTitles = CreateRosterIdentitiesForMultiQuestion(newMultiAnswer, rosterId, parentRosterVector, rosterSizeQuestion, questionnaire);
                                        }
                                        break;
                                    case QuestionType.Numeric:
                                        var rosterTitleQuestionId = questionnaire.GetRosterTitleQuestionId(rosterId);
                                        var integerAnswer = (rosterSizeQuestion!=null && rosterSizeQuestion.AsInteger.IsAnswered) ? rosterSizeQuestion.AsInteger.GetAnswer() : 0;
                                        expectedRosterIdentitiesWithTitles = CreateRosterIdentitiesForNumericQuestion
                                            (
                                                integerAnswer,
                                                rosterId,
                                                parentRosterVector,
                                                rosterSizeQuestion,
                                                rosterTitleQuestionId);
                                        
                                        break;
                                    case QuestionType.TextList:
                                        var listAnswer = rosterSizeQuestion.AsTextList.IsAnswered ? rosterSizeQuestion.AsTextList.GetAnswer(): new Tuple<decimal, string>[0];
                                        expectedRosterIdentitiesWithTitles = CreateRosterIdentitiesForListQuestion(listAnswer, rosterId, currentItem.Identity.RosterVector, rosterSizeQuestion);
                                        break;
                                }
                            }

                            var actualRosterIdentities = group.Children.Where(x => x.Identity.Id == rosterId).Select(x => x.Identity).ToList();

                            foreach (var actualRosterIdentity in actualRosterIdentities)
                            {
                                if (expectedRosterIdentitiesWithTitles.Any(x => x.Identity.Equals(actualRosterIdentity)))
                                    continue;
                                group.RemoveChildren(actualRosterIdentity);
                            }

                            for (int index = 0; index < expectedRosterIdentitiesWithTitles.Count; index++)
                            {
                                var rosterIdentityWithTitle = expectedRosterIdentitiesWithTitles[index];
                                if (@group.HasChild(rosterIdentityWithTitle.Identity))
                                    continue;
                                var rosterNode = new InterviewTreeRoster(
                                    rosterIdentityWithTitle.Identity,
                                    Enumerable.Empty<IInterviewTreeNode>(),
                                    isDisabled: false,
                                    sortIndex: index,
                                    rosterTitle: rosterIdentityWithTitle.Title,
                                    rosterType: rosterIdentityWithTitle.Type,
                                    rosterSizeQuestion: rosterIdentityWithTitle.SizeQuestion?.Identity.Id,
                                    rosterTitleQuestionIdentity: rosterIdentityWithTitle.RosterTitleQuestionIdentity);

                                @group.AddChildren(rosterNode);
                                itemsQueue.Enqueue(rosterNode);
                            }
                        }
                        else if (questionnaire.IsSubSection(childEntityId))
                        {
                            var subSectionIdentity = new Identity(childEntityId, parentRosterVector);
                            if (!group.HasChild(subSectionIdentity))
                            {
                                group.AddChildren(new InterviewTreeSubSection(subSectionIdentity, Enumerable.Empty<IInterviewTreeNode>(), false));
                            }
                        }
                        else if (questionnaire.IsStaticText(childEntityId))
                        {
                            var staticTextIdentity = new Identity(childEntityId, parentRosterVector);
                            if (!group.HasChild(staticTextIdentity))
                            {
                                group.AddChildren(new InterviewTreeStaticText(staticTextIdentity, false));
                            }
                        }
                        else if (questionnaire.IsQuestion(childEntityId))
                        {
                            var questionIdentity = new Identity(childEntityId, parentRosterVector);
                            if (!group.HasChild(questionIdentity))
                            {
                                group.AddChildren(BuildInterviewTreeQuestion(questionIdentity, null, false, new RosterVector[0], questionnaire));
                            }
                        } else if (questionnaire.IsVariable(childEntityId))
                        {
                            var variableIdentity = new Identity(childEntityId, parentRosterVector);
                            if (!group.HasChild(variableIdentity))
                            {
                                group.AddChildren(new InterviewTreeVariable(variableIdentity, false, null));
                            }
                        }
                    }

                    var childItems = currentItem.Children;

                    if (childItems != null)
                    {
                        foreach (var childItem in childItems)
                        {
                            itemsQueue.Enqueue(childItem);
                        }
                    }
                }
            }
        }

        private static List<RosterNodeDescriptor> CreateRosterIdentitiesForMultiQuestion(decimal[] newMultiAnswer, Guid rosterId, RosterVector parentRosterVector, InterviewTreeQuestion rosterSizeQuestion, IQuestionnaire questionnaire)
        {
            return newMultiAnswer.Select((optionValue, index) => new RosterNodeDescriptor
            {
                Identity = new RosterIdentity(rosterId, parentRosterVector, optionValue, index).ToIdentity(),
                Title = questionnaire.GetAnswerOptionTitle(rosterSizeQuestion.Identity.Id, optionValue),
                Type = RosterType.Multi,
                SizeQuestion = rosterSizeQuestion
            }).ToList();
        }

        private static List<RosterNodeDescriptor> CreateRosterIdentitiesForYesNoQuestion(AnsweredYesNoOption[] newYesNoAnswer, Guid rosterId, RosterVector parentRosterVector, InterviewTreeQuestion rosterSizeQuestion, IQuestionnaire questionnaire)
        {
            return newYesNoAnswer
                .Where(x => x.Yes)
                .Select((selectedYesOption, index) => new RosterNodeDescriptor
                {
                    Identity = new RosterIdentity(rosterId, parentRosterVector, selectedYesOption.OptionValue, index).ToIdentity(),
                    Title = questionnaire.GetAnswerOptionTitle(rosterSizeQuestion.Identity.Id, selectedYesOption.OptionValue),
                    Type = RosterType.YesNo,
                    SizeQuestion = rosterSizeQuestion
                }).ToList();
        }

        private static List<RosterNodeDescriptor> CreateRosterIdentitiesForNumericQuestion(int answer, Guid rosterId, RosterVector parentRosterVector, InterviewTreeQuestion rosterSizeQuestion, Guid? rosterTitleQuestionId)
        {
            return Enumerable.Range(0, answer)
                .Select(index => new RosterIdentity(rosterId, parentRosterVector, index, index))
                .Select(x => new RosterNodeDescriptor
                {
                    Identity = x.ToIdentity(),
                    Title = (x.RosterInstanceId + 1).ToString(CultureInfo.InvariantCulture),
                    Type = RosterType.Numeric,
                    SizeQuestion = rosterSizeQuestion,
                    RosterTitleQuestionIdentity = rosterTitleQuestionId.HasValue ? new Identity(rosterTitleQuestionId.Value, x.ToIdentity().RosterVector) : null
                }).ToList();
        }

        private static List<RosterNodeDescriptor> CreateRosterIdentitiesForListQuestion(Tuple<decimal, string>[] answers, Guid rosterId, RosterVector parentRosterVector, InterviewTreeQuestion rosterSizeQuestion)
        {
            return answers
                .Select(answer => new RosterNodeDescriptor
                {
                    Identity = new RosterIdentity(rosterId, parentRosterVector, answer.Item1, 0).ToIdentity(),
                    Title = answer.Item2,
                    Type = RosterType.List,
                    SizeQuestion = rosterSizeQuestion
                }).ToList();
        }

        public void CreateInterviewCreatedOnClient(Guid questionnaireId, long questionnaireVersion,
            InterviewStatus interviewStatus,
            AnsweredQuestionSynchronizationDto[] featuredQuestionsMeta, bool isValid, Guid userId)
        {
            this.SetQuestionnaireProperties(questionnaireId, questionnaireVersion);

            this.GetQuestionnaireOrThrow(questionnaireId, questionnaireVersion, language: null);
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

        private static string FormatQuestionForException(Guid questionId, IQuestionnaire questionnaire)
        {
            return string.Format("'{0} [{1}]'",
                GetQuestionTitleForException(questionId, questionnaire),
                GetQuestionVariableNameForException(questionId, questionnaire));
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

        #endregion

        #region Handlers

        #region AnsweringQuestions
      

        private void ApplyQuestionAnswer(Guid userId, InterviewTree changedInterviewTree, IQuestionnaire questionnaire,
            List<Identity> changedQuestionIdentities, InterviewTree sourceInterviewTree)
        {
            var expressionProcessorState = this.GetClonedExpressionState();

            this.UpdateTreeWithNewAnswers(changedInterviewTree, questionnaire, changedQuestionIdentities.ToArray());

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

            ValidityChanges validationChanges = expressionProcessorState.ProcessValidationExpressions();
            this.UpdateTreeWithValidationChanges(changedInterviewTree, validationChanges);

            this.ApplySubstitutionEvents(changedInterviewTree, questionnaire, changedQuestionIdentities);

            this.ApplyEvents(sourceInterviewTree, changedInterviewTree, userId);

            if (variableValueChanges?.ChangedVariableValues?.Count > 0)
            {
                this.ApplyEvent(new VariablesChanged(variableValueChanges.ChangedVariableValues.Select(c => new ChangedVariable(c.Key, c.Value)).ToArray()));
            }
        }

        private void ApplySubstitutionEvents(InterviewTree tree, IQuestionnaire questionnaire, List<Identity> changedQuestionIdentities)
        {
            var changedQuestionTitles = new List<Identity>();
            var changedStaticTextTitles = new List<Identity>();
            var changedGroupTitles = new List<Identity>();
            foreach (var questionIdentity in changedQuestionIdentities)
            {
                var rosterLevel = questionIdentity.RosterVector.Length;

                var substitutedQuestionIds = questionnaire.GetSubstitutedQuestions(questionIdentity.Id);
                foreach (var substitutedQuestionId in substitutedQuestionIds)
                {
                    changedQuestionTitles.AddRange(tree.FindEntity(substitutedQuestionId)
                        .Select(x => x.Identity)
                        .Where(x => x.RosterVector.Take(rosterLevel).SequenceEqual(questionIdentity.RosterVector)));
                }

                var substitutedStaticTextIds = questionnaire.GetSubstitutedStaticTexts(questionIdentity.Id);
                foreach (var substitutedStaticTextId in substitutedStaticTextIds)
                {
                    changedStaticTextTitles.AddRange(tree.FindEntity(substitutedStaticTextId)
                        .Select(x => x.Identity)
                        .Where(x => x.RosterVector.Take(rosterLevel).SequenceEqual(questionIdentity.RosterVector)));
                }
             
                var substitutedGroupIds = questionnaire.GetSubstitutedGroups(questionIdentity.Id);
                foreach (var substitutedGroupId in substitutedGroupIds)
                {
                    changedGroupTitles.AddRange(tree.FindEntity(substitutedGroupId)
                        .Select(x => x.Identity)
                        .Where(x => x.RosterVector.Take(rosterLevel).SequenceEqual(questionIdentity.RosterVector)));
                }
            }

            if (changedQuestionTitles.Any() || changedStaticTextTitles.Any() || changedGroupTitles.Any())
            {
                this.ApplyEvent(new SubstitutionTitlesChanged(
                    changedQuestionTitles.ToArray(),
                    changedStaticTextTitles.ToArray(),
                    changedGroupTitles.ToArray()));
            }
        }

        #endregion

        
        //todo should respect changes calculated in ExpressionState
        public void ReevaluateSynchronizedInterview()
        {
            var propertiesInvariants = new InterviewPropertiesInvariants(this.properties);

            propertiesInvariants.ThrowIfInterviewHardDeleted();

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
            this.ApplyRosterTitleChanges(targetQuestionnaire);
        }

        protected void ApplyRosterTitleChanges(IQuestionnaire targetQuestionnaire)
        {
            var rosterInstances =
                this.GetInstancesOfGroupsWithSameAndDeeperRosterLevelOrThrow(this.interviewState,
                    targetQuestionnaire.GetRostersWithTitlesToChange(), RosterVector.Empty,
                    targetQuestionnaire).ToArray();

            var changedTitles = rosterInstances.Select(
                rosterInstance =>
                    new ChangedRosterInstanceTitleDto(
                        RosterInstance.CreateFromIdentity(rosterInstance),
                        this.GetRosterTitle(targetQuestionnaire, rosterInstance)))
                .ToArray();

            if (changedTitles.Any())
            {
                this.ApplyEvent(new RosterInstancesTitleChanged(changedTitles));
            }
        }

        private string GetRosterTitle(IQuestionnaire targetQuestionnaire, Identity rosterInstance)
        {
            if (targetQuestionnaire.IsFixedRoster(rosterInstance.Id))
            {
                return targetQuestionnaire.GetFixedRosterTitle(rosterInstance.Id,
                    rosterInstance.RosterVector.Coordinates.Last());
            }
            else if (targetQuestionnaire.IsNumericRoster(rosterInstance.Id))
            {
                var questionId = targetQuestionnaire.GetRosterTitleQuestionId(rosterInstance.Id);
                Identity rosterTitleQuestionIdentity = new Identity(questionId.Value, rosterInstance.RosterVector);
                var questionType = targetQuestionnaire.GetQuestionType(questionId.Value);
                var questionValue = this.interviewState.GetAnswerSupportedInExpressions(rosterTitleQuestionIdentity);

                switch (questionType)
                {
                    case QuestionType.SingleOption:
                    case QuestionType.MultyOption:
                        return AnswerUtils.AnswerToString(questionValue, x => targetQuestionnaire.GetAnswerOptionTitle(questionId.Value, x));
                    default:
                        return AnswerUtils.AnswerToString(questionValue);
                }
            }
            else
            {
                return targetQuestionnaire.GetAnswerOptionTitle(
                    targetQuestionnaire.GetRosterSizeQuestion(rosterInstance.Id),
                    rosterInstance.RosterVector.Last());
            }
        }

        public void CommentAnswer(Guid userId, Guid questionId, RosterVector rosterVector, DateTime commentTime, string comment)
        {
            new InterviewPropertiesInvariants(this.properties).RequireAnswerCanBeChanged();

            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion, this.language);

            var tree = this.BuildInterviewTree(questionnaire);
            var treeInvariants = new InterviewTreeInvariants(tree);

            this.ThrowIfQuestionDoesNotExist(questionId, questionnaire);
            treeInvariants.RequireRosterVectorQuestionInstanceExists(questionId, rosterVector);

            this.ApplyEvent(new AnswerCommented(userId, questionId, rosterVector, commentTime, comment));
        }

        public void SetFlagToAnswer(Guid userId, Guid questionId, RosterVector rosterVector)
        {
            new InterviewPropertiesInvariants(this.properties).RequireAnswerCanBeChanged();

            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion, this.language);

            var tree = this.BuildInterviewTree(questionnaire);
            var treeInvariants = new InterviewTreeInvariants(tree);

            this.ThrowIfQuestionDoesNotExist(questionId, questionnaire);
            treeInvariants.RequireRosterVectorQuestionInstanceExists(questionId, rosterVector);

            this.ApplyEvent(new FlagSetToAnswer(userId, questionId, rosterVector));
        }

        public void RemoveFlagFromAnswer(Guid userId, Guid questionId, RosterVector rosterVector)
        {
            new InterviewPropertiesInvariants(this.properties).RequireAnswerCanBeChanged();

            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion, this.language);

            var tree = this.BuildInterviewTree(questionnaire);
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

        public void CancelByHQSynchronization(Guid userId)
        {
            var propertiesInvariants = new InterviewPropertiesInvariants(this.properties);

            propertiesInvariants.ThrowIfInterviewHardDeleted();

            if (this.properties.Status != InterviewStatus.Completed)
            {
                this.ApplyEvent(new InterviewDeleted(userId));
                this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.Deleted, comment: null));
            }
        }

        public void MarkInterviewAsReceivedByInterviwer(Guid userId)
        {
            var propertiesInvariants = new InterviewPropertiesInvariants(this.properties);

            propertiesInvariants.ThrowIfInterviewHardDeleted();

            this.ApplyEvent(new InterviewReceivedByInterviewer());
        }

        public void MarkInterviewAsSentToHeadquarters(Guid userId)
        {
            if (!this.properties.IsHardDeleted)
            {
                this.ApplyEvent(new InterviewDeleted(userId));
                this.ApplyEvent(new InterviewStatusChanged(InterviewStatus.Deleted, comment: null));
            }
            this.ApplyEvent(new InterviewSentToHeadquarters());
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

        public void SynchronizeInterviewFromHeadquarters(Guid id, Guid userId, Guid supervisorId, InterviewSynchronizationDto interviewDto, DateTime synchronizationTime)
        {
            if (this.Version > 0)
            {
                throw new InterviewException(string.Format("Interview with id {0} already created", EventSourceId));
            }

            this.SetQuestionnaireProperties(interviewDto.QuestionnaireId, interviewDto.QuestionnaireVersion);

            var propertiesInvariants = new InterviewPropertiesInvariants(this.properties);

            propertiesInvariants.ThrowIfInterviewHardDeleted();

            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow(interviewDto.QuestionnaireId,
                interviewDto.QuestionnaireVersion, language: null);

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

            if (interviewChanges.AnswersToRemove != null)
                this.ApplyAnswersRemovanceEvents(interviewChanges.AnswersToRemove);

            if (interviewChanges.RosterInstancesWithAffectedTitles != null)
                this.ApplyRosterRowsTitleChangedEvents(interviewChanges.RosterInstancesWithAffectedTitles);

            if (interviewChanges.VariableValueChanges != null)
                this.ApplyVariableValueChangesEvents(interviewChanges.VariableValueChanges);

            if (interviewChanges.EnablementChanges != null)
                this.ApplyEnablementChangesEvents(interviewChanges.EnablementChanges);

            if (interviewChanges.ValidityChanges != null)
                this.ApplyValidityChangesEvents(interviewChanges.ValidityChanges);

            this.ApplySubstitutionChangesEvents(interviewChanges.ChangedQuestionTitles,
                interviewChanges.ChangedStaticTextTitles,
                interviewChanges.ChangedGroupTitles);

            if (interviewChanges.LinkedQuestionOptionsChanges != null)
            {
                this.ApplyLinkedQuestionOptionsChangesEvents(interviewChanges.LinkedQuestionOptionsChanges);
            }
        }

        private void ApplySubstitutionChangesEvents(Identity[] changedQuestionIds,
            Identity[] changedStaticTextIds,
            Identity[] changedGroupIds)
        {
            if (changedQuestionIds?.Length > 0 || changedStaticTextIds?.Length > 0 || changedGroupIds?.Length > 0)
            {
                this.ApplyEvent(new SubstitutionTitlesChanged(
                    changedQuestionIds ?? new Identity[] { },
                    changedStaticTextIds ?? new Identity[] { },
                    changedGroupIds ?? new Identity[] { }));
            }
        }

        private void ApplyVariableValueChangesEvents(VariableValueChanges variableValueChanges)
        {
            if (variableValueChanges.ChangedVariableValues.Count > 0)
            {
                this.ApplyEvent(new VariablesChanged(variableValueChanges.ChangedVariableValues.Select(c => new ChangedVariable(c.Key, c.Value)).ToArray()));
            }
        }

        private void ApplyLinkedQuestionOptionsChangesEvents(ChangedLinkedOptions[] linkedQuestionOptionsChanges)
        {
            if (linkedQuestionOptionsChanges.Length > 0)
            {
                this.ApplyEvent(new LinkedOptionsChanged(linkedQuestionOptionsChanges));
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

                if (interviewChanges.AnswersToRemove != null)
                    this.ApplyAnswersRemovanceEvents(interviewChanges.AnswersToRemove);

                if (interviewChanges.RosterInstancesWithAffectedTitles != null)
                    this.ApplyRosterRowsTitleChangedEvents(interviewChanges.RosterInstancesWithAffectedTitles);

                if (interviewChanges.VariableValueChanges != null)
                    this.ApplyVariableValueChangesEvents(interviewChanges.VariableValueChanges);

                if (interviewChanges.ValidityChanges != null)
                    validityChanges.Add(interviewChanges.ValidityChanges);

                if (interviewChanges.EnablementChanges != null)
                    eneblementChanges.Add(interviewChanges.EnablementChanges);
            }

            this.ApplyEnablementChangesEvents(EnablementChanges.Union(eneblementChanges));

            //merge changes, saving only last state - valid or invalid  
            validityChanges.ForEach(this.ApplyValidityChangesEvents);

        }

        private void ApplyEnablementChangesEvents(EnablementChanges enablementChanges)
        {
            //should be removed. bad tests setup
            if (enablementChanges == null)
                return;

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

            if (enablementChanges.StaticTextsToBeDisabled.Any())
            {
                this.ApplyEvent(new StaticTextsDisabled(enablementChanges.StaticTextsToBeDisabled.ToArray()));
            }

            if (enablementChanges.StaticTextsToBeEnabled.Any())
            {
                this.ApplyEvent(new StaticTextsEnabled(enablementChanges.StaticTextsToBeEnabled.ToArray()));
            }
            if (enablementChanges.VariablesToBeDisabled.Any())
            {
                this.ApplyEvent(new VariablesDisabled(enablementChanges.VariablesToBeDisabled.ToArray()));
            }
            if (enablementChanges.VariablesToBeEnabled.Any())
            {
                this.ApplyEvent(new VariablesEnabled(enablementChanges.VariablesToBeEnabled.ToArray()));
            }
        }

        private void ApplyValidityChangesEvents(ValidityChanges validityChanges)
        {
            if (validityChanges != null)
            {
                if (validityChanges.AnswersDeclaredValid.Any())
                {
                    this.ApplyEvent(new AnswersDeclaredValid(validityChanges.AnswersDeclaredValid.ToArray()));
                }

                if (validityChanges.FailedValidationConditionsForQuestions.Any())
                {
                    this.ApplyEvent(new AnswersDeclaredInvalid(validityChanges.FailedValidationConditionsForQuestions));
                }

                if (validityChanges.StaticTextsDeclaredValid.Any())
                {
                    this.ApplyEvent(new StaticTextsDeclaredValid(validityChanges.StaticTextsDeclaredValid.ToArray()));
                }

                if (validityChanges.FailedValidationConditionsForStaticTexts.Any())
                {
                    this.ApplyEvent(new StaticTextsDeclaredInvalid(validityChanges.FailedValidationConditionsForStaticTexts.ToList()));
                }
            }
        }

        private void ApplyAnswersRemovanceEvents(List<Identity> answersToRemove)
        {
            if (answersToRemove.Any())
            {
                this.ApplyEvent(new AnswersRemoved(answersToRemove.ToArray()));
            }
        }

        private void ApplyRosterRowsTitleChangedEvents(Dictionary<Identity, string> rostersWithTitles)
        {
            if (rostersWithTitles.Any())
            {
                List<ChangedRosterInstanceTitleDto> changedRosterInstances = new List<ChangedRosterInstanceTitleDto>();
                foreach (var rosterInstance in rostersWithTitles)
                {
                    var instance = new RosterInstance(rosterInstance.Key.Id,
                        rosterInstance.Key.RosterVector.WithoutLast().ToArray(),
                        rosterInstance.Key.RosterVector.Last());
                    var changedDto = new ChangedRosterInstanceTitleDto(
                        instance,
                        rosterInstance.Value);

                    changedRosterInstances.Add(changedDto);
                }

                this.ApplyEvent(new RosterInstancesTitleChanged(changedRosterInstances.ToArray()));
            }
        }

        private void ApplyRostersEvents(params RosterCalculationData[] rosterDatas)
        {
            var rosterInstancesToAdd = this.GetOrderedUnionOfUniqueRosterDataPropertiesByRosterAndNestedRosters(
                d => d.RosterInstancesToAdd, new RosterIdentityComparer(), rosterDatas);

            if (rosterInstancesToAdd.Any())
            {
                AddedRosterInstance[] instances = rosterInstancesToAdd
                    .Select(roster => new AddedRosterInstance(roster.GroupId, roster.OuterRosterVector, roster.RosterInstanceId, roster.SortIndex))
                    .ToArray();

                this.ApplyEvent(new RosterInstancesAdded(instances));
            }

            var rosterInstancesToRemove = this.GetOrderedUnionOfUniqueRosterDataPropertiesByRosterAndNestedRosters(
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

            this.ApplyAnswersRemovanceEvents(this.GetOrderedUnionOfUniqueRosterDataPropertiesByRosterAndNestedRosters(
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

        private List<T> GetOrderedUnionOfUniqueRosterDataPropertiesByRosterAndNestedRosters<T>(Func<RosterCalculationData, IEnumerable<T>> getProperty, IEqualityComparer<T> equalityComparer, params RosterCalculationData[] datas)
        {
            var result = new OrderedAdditiveSet<T>(equalityComparer);
            foreach (var data in datas)
            {
                var propertyValue = getProperty(data);

                propertyValue?.ForEach(x => result.Add(x));

                foreach (var rosterInstantiatesFromNestedLevel in data.RosterInstantiatesFromNestedLevels)
                {
                    this.GetOrderedUnionOfUniqueRosterDataPropertiesByRosterAndNestedRosters(getProperty, equalityComparer, rosterInstantiatesFromNestedLevel).ForEach(x => result.Add(x));
                }
            }

            return result.ToList();
        }

        private Dictionary<RosterIdentity, string> GetUnionOfUniqueRosterInstancesToAddWithRosterTitlesByRosterAndNestedRosters(
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

        private void CheckLinkedMultiOptionQuestionInvariants(Guid questionId, RosterVector rosterVector,
            decimal[][] linkedQuestionSelectedOptions, IQuestionnaire questionnaire, Identity answeredQuestion, 
            InterviewTree tree)
        {
            var treeInvariants = new InterviewTreeInvariants(tree);

            this.ThrowIfQuestionDoesNotExist(questionId, questionnaire);
            treeInvariants.RequireRosterVectorQuestionInstanceExists(questionId, rosterVector);
            this.ThrowIfQuestionTypeIsNotOneOfExpected(questionId, questionnaire, QuestionType.MultyOption);
            treeInvariants.RequireQuestionIsEnabled(answeredQuestion);

            if (!linkedQuestionSelectedOptions.Any())
                return;

            var linkedQuestionIdentity = new Identity(questionId, rosterVector);

            foreach (var selectedRosterVector in linkedQuestionSelectedOptions)
            {
                treeInvariants.RequireLinkedOptionIsAvailable(linkedQuestionIdentity, selectedRosterVector);
            }

            this.ThrowIfLengthOfSelectedValuesMoreThanMaxForSelectedAnswerOptions(questionId, linkedQuestionSelectedOptions.Length, questionnaire);
        }

        private void CheckLinkedSingleOptionQuestionInvariants(Guid questionId, RosterVector rosterVector, decimal[] linkedQuestionSelectedOption, IQuestionnaire questionnaire, Identity answeredQuestion)
        {
            var tree = this.BuildInterviewTree(questionnaire);
            var treeInvariants = new InterviewTreeInvariants(tree);

            this.ThrowIfQuestionDoesNotExist(questionId, questionnaire);
            treeInvariants.RequireRosterVectorQuestionInstanceExists(questionId, rosterVector);

            ThrowIfQuestionTypeIsNotOneOfExpected(questionId, questionnaire, QuestionType.SingleOption);
            treeInvariants.RequireQuestionIsEnabled(answeredQuestion);

            var linkedQuestionIdentity = new Identity(questionId, rosterVector);

            treeInvariants.RequireLinkedOptionIsAvailable(linkedQuestionIdentity, linkedQuestionSelectedOption);
        }

        private void CheckNumericRealQuestionInvariants(Guid questionId, RosterVector rosterVector, decimal answer,
           IQuestionnaire questionnaire,
           Identity answeredQuestion, InterviewStateDependentOnAnswers currentInterviewState, bool applyStrongChecks = true)
        {
            var tree = this.BuildInterviewTree(questionnaire, currentInterviewState);
            var treeInvariants = new InterviewTreeInvariants(tree);

            this.ThrowIfQuestionDoesNotExist(questionId, questionnaire);
            treeInvariants.RequireRosterVectorQuestionInstanceExists(questionId, rosterVector);
            ThrowIfQuestionTypeIsNotOneOfExpected(questionId, questionnaire, QuestionType.Numeric);
            ThrowIfNumericQuestionIsNotReal(questionId, questionnaire);
            if (applyStrongChecks)
            {
                treeInvariants.RequireQuestionIsEnabled(answeredQuestion);
                this.ThrowIfAnswerHasMoreDecimalPlacesThenAccepted(questionnaire, questionId, answer);
            }
        }

        private void CheckDateTimeQuestionInvariants(Guid questionId, RosterVector rosterVector, IQuestionnaire questionnaire,
            Identity answeredQuestion, InterviewStateDependentOnAnswers currentInterviewState, bool applyStrongChecks = true)
        {
            var tree = this.BuildInterviewTree(questionnaire, currentInterviewState);
            var treeInvariants = new InterviewTreeInvariants(tree);

            this.ThrowIfQuestionDoesNotExist(questionId, questionnaire);
            treeInvariants.RequireRosterVectorQuestionInstanceExists(questionId, rosterVector);
            this.ThrowIfQuestionTypeIsNotOneOfExpected(questionId, questionnaire, QuestionType.DateTime);
            if (applyStrongChecks)
            {
                treeInvariants.RequireQuestionIsEnabled(answeredQuestion);
            }
        }

        private void CheckSingleOptionQuestionInvariants(Guid questionId, RosterVector rosterVector, decimal selectedValue,
            IQuestionnaire questionnaire, Identity answeredQuestion, InterviewStateDependentOnAnswers currentInterviewState,
            bool applyStrongChecks = true)
        {
            var tree = this.BuildInterviewTree(questionnaire, currentInterviewState);
            var treeInvariants = new InterviewTreeInvariants(tree);

            this.ThrowIfQuestionDoesNotExist(questionId, questionnaire);
            treeInvariants.RequireRosterVectorQuestionInstanceExists(questionId, rosterVector);
            ThrowIfQuestionTypeIsNotOneOfExpected(questionId, questionnaire, QuestionType.SingleOption);
            ThrowIfValueIsNotOneOfAvailableOptions(questionId, selectedValue, questionnaire);
            treeInvariants.RequireCascadingQuestionAnswerCorrespondsToParentAnswer(answeredQuestion, selectedValue, questionnaire);
            if (applyStrongChecks)
            {
                treeInvariants.RequireQuestionIsEnabled(answeredQuestion);
            }
        }

        private void CheckMultipleOptionQuestionInvariants(Guid questionId, RosterVector rosterVector, decimal[] selectedValues,
            IQuestionnaire questionnaire, Identity answeredQuestion, InterviewStateDependentOnAnswers currentInterviewState, InterviewTree tree,
            bool applyStrongChecks = true)
        {
            var treeInvariants = new InterviewTreeInvariants(tree);

            this.ThrowIfQuestionDoesNotExist(questionId, questionnaire);
            treeInvariants.RequireRosterVectorQuestionInstanceExists(questionId, rosterVector);
            this.ThrowIfQuestionTypeIsNotOneOfExpected(questionId, questionnaire, QuestionType.MultyOption);
            this.ThrowIfSomeValuesAreNotFromAvailableOptions(questionId, selectedValues, questionnaire);

            if (questionnaire.IsQuestionYesNo(questionId))
            {
                throw new InterviewException($"Question {questionId} has Yes/No type, but command is sent to Multiopions type. questionnaireId: {this.questionnaireId}, interviewId {this.EventSourceId}");
            }

            if (questionnaire.ShouldQuestionSpecifyRosterSize(questionId))
            {
                this.ThrowIfRosterSizeAnswerIsNegativeOrGreaterThenMaxRosterRowCount(questionId, selectedValues.Length, questionnaire);
                var maxSelectedAnswerOptions = questionnaire.GetMaxSelectedAnswerOptions(questionId);
                this.ThrowIfRosterSizeAnswerIsGreaterThenMaxRosterRowCount(questionId, selectedValues.Length,
                    questionnaire,
                    maxSelectedAnswerOptions ?? questionnaire.GetMaxRosterRowCount());
            }

            if (applyStrongChecks)
            {
                this.ThrowIfLengthOfSelectedValuesMoreThanMaxForSelectedAnswerOptions(questionId, selectedValues.Length, questionnaire);
                treeInvariants.RequireQuestionIsEnabled(answeredQuestion);
            }
        }

        private void CheckYesNoQuestionInvariants(Identity question, AnsweredYesNoOption[] answeredOptions, IQuestionnaire questionnaire,
            InterviewStateDependentOnAnswers state, InterviewTree tree)
        {
            decimal[] selectedValues = answeredOptions.Select(answeredOption => answeredOption.OptionValue).ToArray();
            var yesAnswersCount = answeredOptions.Count(answeredOption => answeredOption.Yes);

            var treeInvariants = new InterviewTreeInvariants(tree);

            this.ThrowIfQuestionDoesNotExist(question.Id, questionnaire);
            treeInvariants.RequireRosterVectorQuestionInstanceExists(question.Id, question.RosterVector);
            this.ThrowIfQuestionTypeIsNotOneOfExpected(question.Id, questionnaire, QuestionType.MultyOption);
            this.ThrowIfSomeValuesAreNotFromAvailableOptions(question.Id, selectedValues, questionnaire);

            if (questionnaire.ShouldQuestionSpecifyRosterSize(question.Id))
            {
                this.ThrowIfRosterSizeAnswerIsNegativeOrGreaterThenMaxRosterRowCount(question.Id, yesAnswersCount, questionnaire);
                var maxSelectedAnswerOptions = questionnaire.GetMaxSelectedAnswerOptions(question.Id);
                this.ThrowIfRosterSizeAnswerIsGreaterThenMaxRosterRowCount(question.Id, selectedValues.Length,
                    questionnaire,
                    maxSelectedAnswerOptions ?? questionnaire.GetMaxRosterRowCount());
            }

            this.ThrowIfLengthOfSelectedValuesMoreThanMaxForSelectedAnswerOptions(question.Id, yesAnswersCount, questionnaire);
            treeInvariants.RequireQuestionIsEnabled(question);
        }

        private void CheckTextQuestionInvariants(Guid questionId, RosterVector rosterVector, IQuestionnaire questionnaire,
            Identity answeredQuestion, InterviewStateDependentOnAnswers currentInterviewState, InterviewTree tree, bool applyStrongChecks = true)
        {
            var treeInvariants = new InterviewTreeInvariants(tree);

            this.ThrowIfQuestionDoesNotExist(questionId, questionnaire);
            treeInvariants.RequireRosterVectorQuestionInstanceExists(questionId, rosterVector);
            this.ThrowIfQuestionTypeIsNotOneOfExpected(questionId, questionnaire, QuestionType.Text);
            if (applyStrongChecks)
            {
                treeInvariants.RequireQuestionIsEnabled(answeredQuestion);
            }
        }

        private void CheckNumericIntegerQuestionInvariants(Guid questionId, RosterVector rosterVector, int answer, IQuestionnaire questionnaire,
            Identity answeredQuestion, InterviewStateDependentOnAnswers currentInterviewState, InterviewTree tree, bool applyStrongChecks = true)
        {
            var treeInvariants = new InterviewTreeInvariants(tree);

            this.ThrowIfQuestionDoesNotExist(questionId, questionnaire);
            treeInvariants.RequireRosterVectorQuestionInstanceExists(questionId, rosterVector);
            this.ThrowIfQuestionTypeIsNotOneOfExpected(questionId, questionnaire, QuestionType.AutoPropagate, QuestionType.Numeric);
            this.ThrowIfNumericQuestionIsNotInteger(questionId, questionnaire);

            if (questionnaire.ShouldQuestionSpecifyRosterSize(questionId))
            {
                this.ThrowIfRosterSizeAnswerIsNegativeOrGreaterThenMaxRosterRowCount(questionId, answer, questionnaire);
                this.ThrowIfRosterSizeAnswerIsGreaterThenMaxRosterRowCount(questionId, answer, questionnaire,
                    questionnaire.IsQuestionIsRosterSizeForLongRoster(questionId)
                        ? questionnaire.GetMaxLongRosterRowCount()
                        : questionnaire.GetMaxRosterRowCount());
            }

            if (applyStrongChecks)
            {
                treeInvariants.RequireQuestionIsEnabled(answeredQuestion);
            }
        }

        private void CheckTextListInvariants(Guid questionId, RosterVector rosterVector, IQuestionnaire questionnaire, Identity answeredQuestion,
            InterviewStateDependentOnAnswers currentInterviewState, Tuple<decimal, string>[] answers, bool applyStrongChecks = true)
        {
            var tree = this.BuildInterviewTree(questionnaire, currentInterviewState);
            var treeInvariants = new InterviewTreeInvariants(tree);

            this.ThrowIfQuestionDoesNotExist(questionId, questionnaire);
            treeInvariants.RequireRosterVectorQuestionInstanceExists(questionId, rosterVector);
            this.ThrowIfQuestionTypeIsNotOneOfExpected(questionId, questionnaire, QuestionType.TextList);

            if (questionnaire.ShouldQuestionSpecifyRosterSize(questionId))
            {
                this.ThrowIfRosterSizeAnswerIsNegativeOrGreaterThenMaxRosterRowCount(questionId, answers.Length, questionnaire);
                var maxSelectedAnswerOptions = questionnaire.GetMaxSelectedAnswerOptions(questionId);
                this.ThrowIfRosterSizeAnswerIsGreaterThenMaxRosterRowCount(questionId, answers.Length,
                    questionnaire,
                    maxSelectedAnswerOptions ?? questionnaire.GetMaxRosterRowCount());
            }

            if (applyStrongChecks)
            {
                treeInvariants.RequireQuestionIsEnabled(answeredQuestion);
                this.ThrowIfDecimalValuesAreNotUnique(answers, questionId, questionnaire);
                this.ThrowIfStringValueAreEmptyOrWhitespaces(answers, questionId, questionnaire);
                var maxAnswersCountLimit = questionnaire.GetListSizeForListQuestion(questionId);
                this.ThrowIfAnswersExceedsMaxAnswerCountLimit(answers, maxAnswersCountLimit, questionId, questionnaire);
            }
        }

        private void CheckGpsCoordinatesInvariants(Guid questionId, RosterVector rosterVector, IQuestionnaire questionnaire, Identity answeredQuestion,
            InterviewStateDependentOnAnswers currentInterviewState, bool applyStrongChecks = true)
        {
            var tree = this.BuildInterviewTree(questionnaire, currentInterviewState);
            var treeInvariants = new InterviewTreeInvariants(tree);

            this.ThrowIfQuestionDoesNotExist(questionId, questionnaire);
            treeInvariants.RequireRosterVectorQuestionInstanceExists(questionId, rosterVector);
            this.ThrowIfQuestionTypeIsNotOneOfExpected(questionId, questionnaire, QuestionType.GpsCoordinates);
            if (applyStrongChecks)
            {
                treeInvariants.RequireQuestionIsEnabled(answeredQuestion);
            }
        }

        private void CheckQRBarcodeInvariants(Guid questionId, RosterVector rosterVector, IQuestionnaire questionnaire,
         Identity answeredQuestion, InterviewStateDependentOnAnswers currentInterviewState, bool applyStrongChecks = true)
        {
            var tree = this.BuildInterviewTree(questionnaire, currentInterviewState);
            var treeInvariants = new InterviewTreeInvariants(tree);

            this.ThrowIfQuestionDoesNotExist(questionId, questionnaire);
            treeInvariants.RequireRosterVectorQuestionInstanceExists(questionId, rosterVector);
            ThrowIfQuestionTypeIsNotOneOfExpected(questionId, questionnaire, QuestionType.QRBarcode);
            if (applyStrongChecks)
            {
                treeInvariants.RequireQuestionIsEnabled(answeredQuestion);
            }
        }

        #endregion

        #region Calculations

        // triggers roster
        private InterviewChanges CalculateInterviewChangesOnAnswerNumericIntegerQuestion(ILatestInterviewExpressionState expressionProcessorState,
            IReadOnlyInterviewStateDependentOnAnswers state, Guid userId, Guid questionId, RosterVector rosterVector, DateTime answerTime, int answer,
            Func<IReadOnlyInterviewStateDependentOnAnswers, Identity, object> getAnswer, IQuestionnaire questionnaire)
        {
            List<Guid> rosterIds = questionnaire.GetRosterGroupsByRosterSizeQuestion(questionId).ToList();
            int rosterSize = rosterIds.Any() ? ToRosterSize(answer) : 0;

            Func<Guid, decimal[], bool> isRoster = (groupId, groupOuterScopeRosterVector)
                => rosterIds.Contains(groupId)
                    && AreEqualRosterVectors(groupOuterScopeRosterVector, rosterVector);

            IEnumerable<decimal> rosterInstanceIds = Enumerable.Range(0, rosterSize).Select(index => (decimal)index).ToList();

            var rostersWithoutRosterTitleQuestions = rosterIds
                .Where(rosterId => !questionnaire.IsRosterTitleQuestionAvailable(rosterId));

            Dictionary<decimal, string> numericRosterTitles = null;

            if (rostersWithoutRosterTitleQuestions.Any())
            {
                numericRosterTitles = Enumerable.Range(0, rosterSize).ToDictionary(
                    index => (decimal)index,
                    index => (index + 1).ToString(CultureInfo.InvariantCulture));
            }

            //Update State
            expressionProcessorState.UpdateNumericIntegerAnswer(questionId, rosterVector, answer);

            var interviewByAnswerChange = new List<AnswerChange>
            {
                new AnswerChange(AnswerChangeType.NumericInteger, userId, questionId, rosterVector, answerTime, answer)
            };

            RosterCalculationData rosterCalculationData = this.CalculateRosterData(state, rosterIds, rosterVector, rosterInstanceIds, numericRosterTitles, questionnaire, getAnswer);

            var rosterInstancesToAdd = this.GetUnionOfUniqueRosterInstancesToAddWithRosterTitlesByRosterAndNestedRosters(rosterCalculationData);

            var rosterInstancesToRemove = this.GetOrderedUnionOfUniqueRosterDataPropertiesByRosterAndNestedRosters(
                d => d.RosterInstancesToRemove, new RosterIdentityComparer(), rosterCalculationData);


            rosterInstancesToAdd.ForEach(rosterIdentityToAdd => expressionProcessorState.AddRoster(rosterIdentityToAdd.Key.GroupId, rosterIdentityToAdd.Key.OuterRosterVector,
                    rosterIdentityToAdd.Key.RosterInstanceId, rosterIdentityToAdd.Key.SortIndex));

            rosterInstancesToRemove.ForEach(r => expressionProcessorState.RemoveRoster(r.GroupId, r.OuterRosterVector, r.RosterInstanceId));

            IReadOnlyInterviewStateDependentOnAnswers alteredState = state.Amend(getRosterInstanceIds: (groupId, groupOuterRosterVector)
                => isRoster(groupId, groupOuterRosterVector)
                    ? rosterInstanceIds
                    : state.GetRosterInstanceIds(groupId, groupOuterRosterVector));

            return this.CalculateInterviewStructuralChanges(questionId, rosterVector, answerTime, userId, questionnaire, expressionProcessorState,
                interviewByAnswerChange, rosterCalculationData, alteredState);

        }

        private InterviewChanges EmitInterviewStructuralChangesByExpressionStateRosterTitlesAndLinked(ILatestInterviewExpressionState expressionProcessorState,
            RosterCalculationData rosterCalculationData,
            IQuestionnaire questionnaire,
            List<AnswerChange> interviewByAnswerChange,
            Guid userId, DateTime answerTime,
            Guid questionId)
        {
            EnablementChanges enablementChanges = expressionProcessorState.ProcessEnablementConditions();

            if (rosterCalculationData != null)
            {
                enablementChanges.QuestionsToBeEnabled.AddRange(rosterCalculationData.DisabledAnswersToEnableByDecreasedRosterSize);
                enablementChanges.GroupsToBeEnabled.AddRange(rosterCalculationData.DisabledGroupsToEnableByDecreasedRosterSize);
            }

            var structuralChanges = expressionProcessorState.GetStructuralChanges();

            if (structuralChanges.ChangedMultiQuestions != null && structuralChanges.ChangedMultiQuestions.Count > 0)
            {
                interviewByAnswerChange.AddRange(
                    structuralChanges.ChangedMultiQuestions.Select(
                        x => new AnswerChange(AnswerChangeType.MultipleOptions, userId, x.Key.Id, x.Key.RosterVector, answerTime, x.Value.Select(Convert.ToDecimal).ToArray())));
            }

            if (structuralChanges.ChangedSingleQuestions != null && structuralChanges.ChangedSingleQuestions.Count > 0)
            {
                interviewByAnswerChange.AddRange(
                    structuralChanges.ChangedSingleQuestions.Select(
                        x => new AnswerChange(AnswerChangeType.SingleOption, userId, x.Key.Id, x.Key.RosterVector, answerTime, x.Value)));
            }

            if (structuralChanges.ChangedYesNoQuestions != null && structuralChanges.ChangedYesNoQuestions.Count > 0)
            {
                interviewByAnswerChange.AddRange(
                    structuralChanges.ChangedYesNoQuestions.Select(
                        x => new AnswerChange(AnswerChangeType.YesNo, userId, x.Key.Id, x.Key.RosterVector, answerTime, ConvertToAnsweredYesNoOptionArray(x.Value))));
            }

            var rostersWithTitles = this.GetUpdatedRosterTitles(questionnaire, interviewByAnswerChange, questionId);

            if (structuralChanges.RemovedRosters != null && structuralChanges.RemovedRosters.Count > 0)
            {
                if (rosterCalculationData == null)
                    rosterCalculationData = new RosterCalculationData();

                rosterCalculationData.RosterInstancesToRemove.AddRange(
                    structuralChanges.RemovedRosters.Select(x => new RosterIdentity(x.Id, x.RosterVector.Shrink(), x.RosterVector.Last())));
            }

            var changedLinkedOptions =
               this.CreateChangedLinkedOptions(expressionProcessorState,
               this.interviewState,
               questionnaire,
               interviewByAnswerChange,
               enablementChanges,
               rosterCalculationData,
               rostersWithTitles).ToArray();

            var linkedQuestionsAnswersToRemove = this.GetLinkedQuestionsAnswersToRemove(this.interviewState, changedLinkedOptions, questionnaire);

            var variableValuesChanges = expressionProcessorState.ProcessVariables();

            //should be executed after all changes 

            return new InterviewChanges(
                interviewByAnswerChange,
                enablementChanges,
                null,
                rosterCalculationData,
                linkedQuestionsAnswersToRemove,
                rostersWithTitles,
                null,
                null,
                null,
                changedLinkedOptions,
                variableValuesChanges);
        }

        private Dictionary<Identity, string> GetUpdatedRosterTitles(IQuestionnaire questionnaire, List<AnswerChange> interviewByAnswerChange, Guid questionId)
        {
            Dictionary<Identity, string> rostersWithTitles = new Dictionary<Identity, string>();

            var categoricalQuestionChangeTypes = new List<AnswerChangeType>
            {
                AnswerChangeType.MultipleOptions,
                AnswerChangeType.SingleOption,
                AnswerChangeType.YesNo
            };

            foreach (var answerChange in interviewByAnswerChange)
            {
                if (questionnaire.DoesQuestionSpecifyRosterTitle(answerChange.QuestionId))
                {
                    List<Identity> changedRosterTitles = CalculateRosterInstancesWhichTitlesAreAffected(
                        answerChange.QuestionId, answerChange.RosterVector, questionnaire);

                    foreach (var rosterInstanceWithAffectedTitle in changedRosterTitles)
                    {
                        string changedAnswerString = null;

                        if (answerChange.InterviewChangeType == AnswerChangeType.SingleOptionLinked)
                        {
                            decimal[] selectedRosterVector = answerChange.Answer as decimal[];
                            if (questionnaire.IsQuestionLinkedToRoster(questionId))
                            {
                                Guid linkedRosterId = questionnaire.GetRosterReferencedByLinkedQuestion(questionId);
                                changedAnswerString = this.interviewState.GetRosterTitle(linkedRosterId,
                                    new RosterVector(selectedRosterVector));
                            }
                            else
                            {
                                Guid linkedQuestionId = this.GetLinkedQuestionIdOrThrow(questionId, questionnaire);
                                var answeredLinkedQuestion = new Identity(linkedQuestionId, selectedRosterVector);
                                changedAnswerString = this.GetLinkedQuestionAnswerFormattedAsRosterTitle(this.interviewState,
                                    answeredLinkedQuestion, questionnaire);
                            }
                        }
                        else if (answerChange.InterviewChangeType == AnswerChangeType.MultipleOptionsLinked)
                        {
                            decimal[][] selectedRosterVectors = answerChange.Answer as decimal[][];
                            if (questionnaire.IsQuestionLinkedToRoster(questionId))
                            {
                                Guid linkedRosterId = questionnaire.GetRosterReferencedByLinkedQuestion(questionId);
                                changedAnswerString = string.Join(", ",
                                    selectedRosterVectors.Select(
                                        selectedRosterVector => this.interviewState.GetRosterTitle(linkedRosterId,
                                            new RosterVector(selectedRosterVector))));
                            }
                            else
                            {
                                Guid linkedQuestionId = this.GetLinkedQuestionIdOrThrow(questionId, questionnaire);
                                var answeredLinkedQuestions =
                                    selectedRosterVectors.Select(
                                        selectedRosterVector => new Identity(linkedQuestionId, selectedRosterVector));
                                changedAnswerString = string.Join(", ",
                                    answeredLinkedQuestions.Select(
                                        q =>
                                            this.GetLinkedQuestionAnswerFormattedAsRosterTitle(this.interviewState, q, questionnaire)));
                            }
                        }
                        else if (answerChange.InterviewChangeType == AnswerChangeType.DateTime)
                        {
                            DateTime dateAnswer = (DateTime)answerChange.Answer;
                            changedAnswerString = questionnaire.IsTimestampQuestion(answerChange.QuestionId)
                                ? dateAnswer.ToLocalTime().ToString(CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern)
                                : AnswerUtils.AnswerToString(dateAnswer);
                        }
                        else if (categoricalQuestionChangeTypes.Contains(answerChange.InterviewChangeType))
                        {
                            changedAnswerString = AnswerUtils.AnswerToString(answerChange.Answer,
                                answerOptionValue => questionnaire.GetAnswerOptionTitle(questionId, answerOptionValue));
                        }
                        else
                        {
                            changedAnswerString = AnswerUtils.AnswerToString(answerChange.Answer);
                        }

                        rostersWithTitles[rosterInstanceWithAffectedTitle] = changedAnswerString;
                    }
                }
            }
            return rostersWithTitles;
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

        private IEnumerable<Identity> GetAnswersToRemoveIfRosterInstancesAreRemoved(IReadOnlyInterviewStateDependentOnAnswers state,
           Guid rosterId, List<decimal> rosterInstanceIdsBeingRemoved, decimal[] nearestToOuterRosterVector,
           IQuestionnaire questionnaire)
        {
            int indexOfRosterInRosterVector = GetIndexOfRosterInRosterVector(rosterId, questionnaire);

            IEnumerable<Guid> underlyingQuestionIds = questionnaire.GetAllUnderlyingQuestions(rosterId);

            IEnumerable<Identity> underlyingQuestionInstances = this.GetInstancesOfEntitiesWithSameAndDeeperRosterLevelOrThrow(state,
                underlyingQuestionIds, nearestToOuterRosterVector, questionnaire);

            IEnumerable<Identity> underlyingQuestionsBeingRemovedByRemovedRosterInstances = (
                from question in underlyingQuestionInstances
                where state.WasQuestionAnswered(question)
                where rosterInstanceIdsBeingRemoved.Contains(question.RosterVector[indexOfRosterInRosterVector])
                select question
                ).ToList();

            return underlyingQuestionsBeingRemovedByRemovedRosterInstances;
        }

        private List<Identity> GetLinkedQuestionsAnswersToRemove(
            InterviewStateDependentOnAnswers currentState,
            ChangedLinkedOptions[] linkedOptionChanges,
            IQuestionnaire questionnaire)
        {
            var answersToRemove = new List<Identity>();

            foreach (Tuple<Identity, RosterVector> linkedSingleOptionAnswer in currentState.GetAllAnswersOnSingleOptionLinkedQuestions(questionnaire))
            {
                var linkedQuestion = linkedSingleOptionAnswer.Item1;
                var linkedQuestionSelectedOption = new RosterVector(linkedSingleOptionAnswer.Item2);
                var changedOptions = linkedOptionChanges.FirstOrDefault(q => q.QuestionId == linkedQuestion);
                if (changedOptions != null)
                {
                    if (!changedOptions.Options.Contains(linkedQuestionSelectedOption))
                        answersToRemove.Add(linkedQuestion);
                }
            }

            foreach (Tuple<Identity, RosterVector[]> linkedMultipleOptionsAnswer in currentState.LinkedMultipleOptionsAnswers.Values)
            {
                var linkedQuestion = linkedMultipleOptionsAnswer.Item1;
                RosterVector[] linkedQuestionSelectedOptions = linkedMultipleOptionsAnswer.Item2;

                var changedOptions = linkedOptionChanges.FirstOrDefault(q => q.QuestionId == linkedQuestion);
                if (changedOptions != null)
                {
                    foreach (var linkedQuestionSelectedOption in linkedQuestionSelectedOptions)
                    {
                        if (!changedOptions.Options.Contains(linkedQuestionSelectedOption))
                        {
                            answersToRemove.Add(linkedQuestion);
                            break;
                        }
                    }
                }
            }
            return answersToRemove;
        }

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

        private IEnumerable<Identity> GetQuestionsToRemoveAnswersFromDependingOnCascading(Guid questionId, RosterVector rosterVector,
            IQuestionnaire questionnaire, IReadOnlyInterviewStateDependentOnAnswers state)
        {
            IEnumerable<Guid> dependentQuestionIds = questionnaire.GetCascadingQuestionsThatDependUponQuestion(questionId);
            IEnumerable<Identity> questions = this.GetInstancesOfEntitiesWithSameAndDeeperRosterLevelOrThrow(state, dependentQuestionIds, rosterVector, questionnaire);

            return questions.Where(state.WasQuestionAnswered);
        }

        private void CalculateChangesByFeaturedQuestion(ILatestInterviewExpressionState expressionProcessorState, InterviewChangeStructures changeStructures, Guid userId,
            IQuestionnaire questionnaire, Dictionary<Guid, object> answersToFeaturedQuestions,
            DateTime answersTime, Dictionary<Identity, object> newAnswers, RosterVector rosterVector = null)
        {
            var currentQuestionRosterVector = rosterVector ?? (decimal[])RosterVector.Empty;
            Func<IReadOnlyInterviewStateDependentOnAnswers, Identity, object> getAnswer =
                (currentState, question) => newAnswers.Any(x => question == x.Key)
                    ? newAnswers.SingleOrDefault(x => question == x.Key).Value
                    : this.GetEnabledQuestionAnswerSupportedInExpressions(changeStructures.State, question, questionnaire);

            foreach (KeyValuePair<Guid, object> answerToFeaturedQuestion in answersToFeaturedQuestions)
            {
                Guid questionId = answerToFeaturedQuestion.Key;
                object answer = answerToFeaturedQuestion.Value;

                QuestionType questionType = questionnaire.GetQuestionType(questionId);

                InterviewChanges interviewChanges;

                switch (questionType)
                {
                    case QuestionType.Text:
                        interviewChanges =
                            this.CalculateInterviewChangesOnAnswerTextQuestion(expressionProcessorState, userId, questionId,
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
                                this.CalculateInterviewChangesOnAnswerNumericRealQuestion(expressionProcessorState, userId, questionId,
                                    currentQuestionRosterVector, answersTime, (decimal)answer, questionnaire);
                        break;

                    case QuestionType.DateTime:
                        interviewChanges =
                            this.CalculateInterviewChangesOnAnswerDateTimeQuestion(expressionProcessorState, userId, questionId,
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
                                questionId, currentQuestionRosterVector, (AnsweredYesNoOption[])answer,
                                answersTime, userId, questionnaire, expressionProcessorState, changeStructures.State, getAnswer)
                            : this.CalculateInterviewChangesOnAnswerMultipleOptionsQuestion(expressionProcessorState, changeStructures.State, userId, questionId,
                                currentQuestionRosterVector, answersTime, (decimal[])answer, getAnswer, questionnaire);
                        break;
                    case QuestionType.QRBarcode:
                        interviewChanges =
                            this.CalculateInterviewChangesOnAnswerQRBarcodeQuestion(expressionProcessorState, userId, questionId, currentQuestionRosterVector,
                                answersTime, (string)answer, questionnaire);
                        break;
                    case QuestionType.GpsCoordinates:
                        var geoAnswer = answer as GeoPosition;
                        interviewChanges =
                            this.CalculateInterviewChangesOnAnswerGeoLocationQuestion(expressionProcessorState, userId, questionId,
                                currentQuestionRosterVector, answersTime, geoAnswer.Latitude, geoAnswer.Longitude, geoAnswer.Accuracy,
                                geoAnswer.Altitude, geoAnswer.Timestamp, questionnaire);
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
                outerRosterVector = RosterVector.Empty;

            Func<IReadOnlyInterviewStateDependentOnAnswers, Identity, object> getAnswer = (currentState, question) => null;

            List<Guid> fixedRosterIds = questionnaire.GetFixedRosterGroups().ToList();

            Dictionary<Guid, Dictionary<decimal, string>> rosterTitlesGroupedByRosterId = CalculateFixedRosterData(fixedRosterIds, questionnaire);

            Func<Guid, IEnumerable<decimal>> getFixedRosterInstanceIds =
                fixedRosterId => rosterTitlesGroupedByRosterId[fixedRosterId].Keys.ToList();

            return fixedRosterIds.Select(fixedRosterId =>
                this.CalculateRosterData(
                    state: state,
                    rosterIds: new List<Guid> { fixedRosterId },
                    nearestToOuterRosterVector: outerRosterVector,
                    rosterInstanceIds: getFixedRosterInstanceIds(fixedRosterId),
                    rosterTitles: rosterTitlesGroupedByRosterId[fixedRosterId],
                    questionnaire: questionnaire,
                    getAnswer: getAnswer)
                ).ToList();
        }

        private IEnumerable<RosterCalculationData> CalculateDynamicRostersData(IReadOnlyInterviewStateDependentOnAnswers state,
            IQuestionnaire questionnaire, decimal[] outerRosterVector,
            Guid rosterId, Func<IReadOnlyInterviewStateDependentOnAnswers, Identity, object> getAnswer)
        {
            var nestedRosterIds = questionnaire.GetNestedRostersOfGroupById(rosterId);

            foreach (var nestedRosterId in nestedRosterIds)
            {
                var rosterInstanceIds = this.GetRosterInstancesById(state, questionnaire, nestedRosterId, outerRosterVector, getAnswer);

                var rosterTitles = rosterInstanceIds.Any(x => !string.IsNullOrEmpty(x.Value.Item1))
                    ? rosterInstanceIds.ToDictionary(x => x.Key, x => x.Value.Item1)
                    : null;

                var rosterInstanceIdsWithSortIndexes = rosterInstanceIds.ToDictionary(x => x.Key, x => x.Value.Item2);

                yield return this.CalculateRosterData(
                    state: state,
                    rosterIds: new List<Guid> { nestedRosterId },
                    nearestToOuterRosterVector: outerRosterVector,
                    rosterInstanceIdsWithSortIndexes: rosterInstanceIdsWithSortIndexes,
                    rosterTitles: rosterTitles,
                    questionnaire: questionnaire,
                    getAnswer: getAnswer);
            }
        }


        private RosterCalculationData CalculateRosterDataWithRosterTitlesFromTextListQuestions(IReadOnlyInterviewStateDependentOnAnswers state,
            RosterVector rosterVector,
            List<Guid> rosterIds, Dictionary<decimal, int?> rosterInstanceIdsWithSortIndexes,
            IQuestionnaire questionnaire,
            Func<IReadOnlyInterviewStateDependentOnAnswers, Identity, object> getAnswer,
            Tuple<decimal, string>[] answers, Tuple<decimal, string>[] changedAnswers)
        {
            RosterCalculationData rosterCalculationData = this.CalculateRosterData(state, rosterIds,
                rosterVector, rosterInstanceIdsWithSortIndexes, null, questionnaire, getAnswer);

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

        private RosterCalculationData CalculateRosterDataWithRosterTitlesFromYesNoQuestions(Guid questionId, RosterVector rosterVector, List<Guid> rosterIds,
             Dictionary<decimal, int?> rosterInstanceIdsWithSortIndexes, IQuestionnaire questionnaire,
             IReadOnlyInterviewStateDependentOnAnswers state, Func<IReadOnlyInterviewStateDependentOnAnswers, Identity, object> getAnswer)
        {
            RosterCalculationData rosterCalculationData = this.CalculateRosterData(state, rosterIds,
                rosterVector, rosterInstanceIdsWithSortIndexes, null, questionnaire, getAnswer);

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

        private RosterCalculationData CalculateRosterData(IReadOnlyInterviewStateDependentOnAnswers state,
            List<Guid> rosterIds, decimal[] nearestToOuterRosterVector, IEnumerable<decimal> rosterInstanceIds,
            Dictionary<decimal, string> rosterTitles, IQuestionnaire questionnaire,
            Func<IReadOnlyInterviewStateDependentOnAnswers, Identity, object> getAnswer)
        {
            Dictionary<decimal, int?> rosterInstanceIdsWithSortIndexes =
                rosterInstanceIds.ToDictionary(
                    rosterInstanceId => rosterInstanceId,
                    rosterInstanceId => (int?)null);

            return this.CalculateRosterData(state, rosterIds,
                nearestToOuterRosterVector, rosterInstanceIdsWithSortIndexes, rosterTitles, questionnaire, getAnswer);
        }

        private RosterCalculationData CalculateRosterData(IReadOnlyInterviewStateDependentOnAnswers state, List<Guid> rosterIds,
            decimal[] nearestToOuterRosterVector, Dictionary<decimal, int?> rosterInstanceIdsWithSortIndexes, Dictionary<decimal, string> rosterTitles,
            IQuestionnaire questionnaire,
            Func<IReadOnlyInterviewStateDependentOnAnswers, Identity, object> getAnswer)
        {
            List<RosterIdentity> rosterInstancesToAdd, rosterInstancesToRemove, rosterInstancesToChange = new List<RosterIdentity>();

            List<RosterCalculationData> rosterInstancesFromNestedLevels;
            this.CalculateChangesInRosterInstances(state, questionnaire, rosterIds, nearestToOuterRosterVector,
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

                var outerVectorsForExtend = GetOuterVectorForParentRoster(state, rosterGroupsStartingFromTop, nearestToOuterRosterVector);

                foreach (var outerVectorForExtend in outerVectorsForExtend)
                {
                    var isParentRosterBeingDeleted = false;
                    var isNestedRoster = rosterGroupsStartingFromTop.Length > 1;
                    if (isNestedRoster)
                    {
                        var parentRosterId = rosterGroupsStartingFromTop.Shrink().Last();
                        var parentRosterIdentityWithWrongSortNumber = new RosterIdentity(parentRosterId, outerVectorForExtend.Shrink(), outerVectorForExtend.Last(), null);

                        isParentRosterBeingDeleted = this.IsRosterBeingDeleted(parentRosterIdentityWithWrongSortNumber, rosterInstancesToRemove, rosterInstantiatesFromNestedLevels);
                    }

                    var rosterInstanceIdsBeingAdded = (isParentRosterBeingDeleted
                        ? Enumerable.Empty<RosterIdentity>()
                        : GetRosterInstanceIdsBeingAdded(
                        existingRosterInstanceIds: this.interviewState.GetRosterInstanceIds(rosterId, outerVectorForExtend),
                        newRosterInstanceIds: rosterInstanceIds).Select(rosterInstanceId =>
                            new RosterIdentity(rosterId, outerVectorForExtend, rosterInstanceId,
                                sortIndex: rosterInstanceIdsWithSortIndexes[rosterInstanceId]))).ToList();

                    rosterInstancesToAdd.AddRange(rosterInstanceIdsBeingAdded);

                    var listOfRosterInstanceIdsForRemove =
                        GetRosterInstanceIdsBeingRemoved(
                            this.interviewState.GetRosterInstanceIds(rosterId, outerVectorForExtend), rosterInstanceIds).Select(rosterInstanceId =>
                                new RosterIdentity(rosterId, outerVectorForExtend, rosterInstanceId)).ToList();

                    rosterInstancesToRemove.AddRange(listOfRosterInstanceIdsForRemove);

                    foreach (var rosterInstanceIdBeingAdded in rosterInstanceIdsBeingAdded)
                    {
                        var outerRosterVector = ((RosterVector)rosterInstanceIdBeingAdded.OuterRosterVector).ExtendWithOneCoordinate(rosterInstanceIdBeingAdded.RosterInstanceId);
                        var calculateDynamicRostersData = this.CalculateDynamicRostersData(state, questionnaire, outerRosterVector, rosterId, getAnswer);
                        rosterInstantiatesFromNestedLevels.AddRange(calculateDynamicRostersData);
                    }

                    RosterCalculationData nestedRostersDataForDelete = this.CalculateNestedRostersDataForDelete(state, questionnaire, rosterId,
                                                            listOfRosterInstanceIdsForRemove.Select(i => i.RosterInstanceId).ToList(), outerVectorForExtend);

                    rosterInstantiatesFromNestedLevels.Add(nestedRostersDataForDelete);
                }
            }
        }

        private bool IsRosterBeingDeleted(RosterIdentity rosterIdentity, IEnumerable<RosterIdentity> rosterInstancesToRemove, IEnumerable<RosterCalculationData> rosterInstantiatesFromNestedLevels)
        {
            return rosterInstantiatesFromNestedLevels
                    .SelectMany(x => x.RosterInstancesToRemove)
                    .Union(rosterInstancesToRemove)
                    .Any(x => x.GroupId == rosterIdentity.GroupId
                              && x.RosterInstanceId == rosterIdentity.RosterInstanceId
                              && x.OuterRosterVector.SequenceEqual(rosterIdentity.OuterRosterVector));
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
                IEnumerable<RosterVector> outerVectorsForExtend = ExtendRosterVector(state, nearestToOuterRosterVector, rosterGroupsStartingFromTop.Length, rosterGroupsStartingFromTop);

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

        private static List<Identity> CalculateRosterInstancesWhichTitlesAreAffected(Guid questionId, RosterVector rosterVector,
            IQuestionnaire questionnaire)
        {
            if (!questionnaire.DoesQuestionSpecifyRosterTitle(questionId))
                return new List<Identity>();

            Tuple<decimal[], decimal> splittedRosterVector = SplitRosterVectorOntoOuterVectorAndRosterInstanceId(rosterVector);

            return questionnaire
                .GetRostersAffectedByRosterTitleQuestion(questionId)
                .Select(rosterId => new Identity(rosterId, new RosterVector(splittedRosterVector.Item1.Concat(splittedRosterVector.Item2.ToEnumerable()))))
                .ToList();
        }

        #endregion

        #region ThrowIfs

        private void ThrowIfQuestionDoesNotExist(Guid questionId, IQuestionnaire questionnaire)
        {
            if (!questionnaire.HasQuestion(questionId))
                throw new InterviewException(string.Format("Question with id '{0}' is not found. InterviewId: {1}", questionId, EventSourceId));
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
                throw new AnswerNotAcceptedException(string.Format(
                    "Question {0} has type {1}. But one of the following types was expected: {2}. InterviewId: {3}",
                    FormatQuestionForException(questionId, questionnaire), questionType,
                    string.Join(", ", expectedQuestionTypes.Select(type => type.ToString())),
                    EventSourceId));
        }

        private void ThrowIfNumericQuestionIsNotReal(Guid questionId, IQuestionnaire questionnaire)
        {
            var isNotSupportReal = questionnaire.IsQuestionInteger(questionId);
            if (isNotSupportReal)
                throw new AnswerNotAcceptedException(string.Format(
                    "Question {0} doesn't support answer of type real. InterviewId: {1}",
                    FormatQuestionForException(questionId, questionnaire), EventSourceId));
        }

        private void ThrowIfNumericQuestionIsNotInteger(Guid questionId, IQuestionnaire questionnaire)
        {
            var isNotSupportInteger = !questionnaire.IsQuestionInteger(questionId);
            if (isNotSupportInteger)
                throw new AnswerNotAcceptedException(string.Format(
                    "Question {0} doesn't support answer of type integer. InterviewId: {1}",
                    FormatQuestionForException(questionId, questionnaire), EventSourceId));
        }

        private void ThrowIfValueIsNotOneOfAvailableOptions(Guid questionId, decimal value, IQuestionnaire questionnaire)
        {
            var availableValues = questionnaire.GetOptionForQuestionByOptionValue(questionId, value);

            if (availableValues == null)
                throw new AnswerNotAcceptedException(string.Format(
                    "For question {0} was provided selected value {1} as answer. InterviewId: {2}",
                    FormatQuestionForException(questionId, questionnaire), value, EventSourceId));
        }

        private void ThrowIfSomeValuesAreNotFromAvailableOptions(Guid questionId, decimal[] values, IQuestionnaire questionnaire)
        {
            IEnumerable<decimal> availableValues = questionnaire.GetMultiSelectAnswerOptionsAsValues(questionId);

            bool someValueIsNotOneOfAvailable = values.Any(value => !availableValues.Contains(value));
            if (someValueIsNotOneOfAvailable)
                throw new AnswerNotAcceptedException(string.Format(
                    "For question {0} were provided selected values {1} as answer. But only following values are allowed: {2}. InterviewId: {3}",
                    FormatQuestionForException(questionId, questionnaire), JoinDecimalsWithComma(values),
                    JoinDecimalsWithComma(availableValues),
                    EventSourceId));
        }

        private void ThrowIfLengthOfSelectedValuesMoreThanMaxForSelectedAnswerOptions(Guid questionId, int answersCount, IQuestionnaire questionnaire)
        {
            int? maxSelectedOptions = questionnaire.GetMaxSelectedAnswerOptions(questionId);

            if (maxSelectedOptions.HasValue && maxSelectedOptions > 0 && answersCount > maxSelectedOptions)
                throw new AnswerNotAcceptedException(string.Format(
                    "For question {0} number of answers is greater than the maximum number of selected answers. InterviewId: {1}",
                    FormatQuestionForException(questionId, questionnaire), EventSourceId));
        }

        private void ThrowIfAnswerHasMoreDecimalPlacesThenAccepted(IQuestionnaire questionnaire, Guid questionId, decimal answer)
        {
            int? countOfDecimalPlacesAllowed = questionnaire.GetCountOfDecimalPlacesAllowedByQuestion(questionId);
            if (!countOfDecimalPlacesAllowed.HasValue)
                return;

            var roundedAnswer = Math.Round(answer, countOfDecimalPlacesAllowed.Value);
            if (roundedAnswer != answer)
                throw new AnswerNotAcceptedException(
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
                throw new AnswerNotAcceptedException(
                    $"Answer '{answer}' for question {FormatQuestionForException(questionId, questionnaire)} is incorrect because question is used as size of roster and specified answer is negative. InterviewId: {this.EventSourceId}");
        }

        private void ThrowIfRosterSizeAnswerIsGreaterThenMaxRosterRowCount(Guid questionId, int answer,
           IQuestionnaire questionnaire, int maxRosterRowCount)
        {
            if (answer > maxRosterRowCount)
            {
                var message = string.Format(
                    "Answer '{0}' for question {1} is incorrect because question is used as size of roster and specified answer is greater than {3}. InterviewId: {2}",
                    answer, FormatQuestionForException(questionId, questionnaire), this.EventSourceId, maxRosterRowCount);
                throw new AnswerNotAcceptedException(message);
            }
        }

        #endregion

        protected IQuestionnaire GetQuestionnaireOrThrow(Guid id, long version, string language)
        {
            IQuestionnaire questionnaire = this.questionnaireRepository.GetQuestionnaire(new QuestionnaireIdentity(id, version), language);

            if (questionnaire == null)
                throw new InterviewException($"Questionnaire '{new QuestionnaireIdentity(id, version)}' was not found. InterviewId {EventSourceId}", InterviewDomainExceptionType.QuestionnaireIsMissing);

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
            if (questionnaire.IsFixedRoster(rosterId))
                return questionnaire.GetFixedRosterTitles(rosterId)
                    .ToDictionary(x => x.Value, x => new Tuple<string, int?>(x.Title, null));

            Guid rosterSizeQuestionId = questionnaire.GetRosterSizeQuestion(rosterId);

            var rosterSizeQuestionIdentity = new Identity(rosterSizeQuestionId,
                outerRosterVector.Take(questionnaire.GetRosterLevelForQuestion(rosterSizeQuestionId)).ToArray());

            var answerOnRosterSizeQuestion = getAnswer(state, rosterSizeQuestionIdentity);

            var questionType = questionnaire.GetQuestionType(rosterSizeQuestionId);
            switch (questionType)
            {
                case QuestionType.Numeric:
                    if (questionnaire.IsQuestionInteger(rosterSizeQuestionId))
                    {
                        if (answerOnRosterSizeQuestion == null)
                            return new Dictionary<decimal, Tuple<string, int?>>();
                        try
                        {
                            var isRosterTitleAvaliable = questionnaire.IsRosterTitleQuestionAvailable(rosterId);
                            var intAnswer = Convert.ToInt32(answerOnRosterSizeQuestion);
                            return Enumerable.Range(0, intAnswer)
                                .ToDictionary(x => (decimal)x,
                                    x =>
                                        new Tuple<string, int?>(
                                            isRosterTitleAvaliable
                                                ? null
                                                : (x + 1).ToString(CultureInfo.InvariantCulture), x));
                        }
                        catch (InvalidCastException e)
                        {
                            this.logger.Error("invalid cast of int answer on trigger question", e);
                        }
                    }

                    break;
                case QuestionType.MultyOption:
                    if (questionnaire.IsQuestionYesNo(rosterSizeQuestionId))
                    {
                        var yesNoAnswer = answerOnRosterSizeQuestion as AnsweredYesNoOption[];
                        if (yesNoAnswer != null)
                        {
                            return yesNoAnswer.Where(a => a.Yes).Select(a => a.OptionValue).ToDictionary(
                                x => x,
                                x => new Tuple<string, int?>(questionnaire.GetAnswerOptionTitle(rosterSizeQuestionId, x), (int?)x));
                        }
                    }
                    else
                    {
                        var multyOptionAnswer = answerOnRosterSizeQuestion as decimal[];
                        if (multyOptionAnswer != null)
                        {
                            return multyOptionAnswer.ToDictionary(
                                x => x,
                                x => new Tuple<string, int?>(questionnaire.GetAnswerOptionTitle(rosterSizeQuestionId, x), (int?)x));
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
            RosterVector rosterVector = null, InterviewStateDependentOnAnswers currentInterviewState = null, bool applyStrongChecks = true)
        {
            var currentRosterVector = rosterVector ?? (decimal[])RosterVector.Empty;
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
                            currentInterviewState, this.BuildInterviewTree(questionnaire, currentInterviewState), applyStrongChecks);
                        break;

                    case QuestionType.AutoPropagate:
                        this.CheckNumericIntegerQuestionInvariants(questionId, currentRosterVector, (int)answer, questionnaire,
                            answeredQuestion, currentInterviewState, this.BuildInterviewTree(questionnaire, currentInterviewState), applyStrongChecks);
                        break;
                    case QuestionType.Numeric:
                        if (questionnaire.IsQuestionInteger(questionId))
                            this.CheckNumericIntegerQuestionInvariants(questionId, currentRosterVector, (int)answer, questionnaire,
                                answeredQuestion, currentInterviewState, this.BuildInterviewTree(questionnaire, currentInterviewState), applyStrongChecks);
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
                            this.CheckYesNoQuestionInvariants(new Identity(questionId, currentRosterVector), (AnsweredYesNoOption[])answer, questionnaire, currentInterviewState, this.BuildInterviewTree(questionnaire, currentInterviewState));
                        }
                        else
                        {
                            this.CheckMultipleOptionQuestionInvariants(questionId, currentRosterVector, (decimal[])answer, questionnaire, answeredQuestion,
                                currentInterviewState, this.BuildInterviewTree(questionnaire, currentInterviewState), applyStrongChecks);
                        }
                        break;
                    case QuestionType.QRBarcode:
                        this.CheckQRBarcodeInvariants(questionId, currentRosterVector, questionnaire, answeredQuestion, currentInterviewState, applyStrongChecks);
                        break;
                    case QuestionType.GpsCoordinates:
                        this.CheckGpsCoordinatesInvariants(questionId, currentRosterVector, questionnaire, answeredQuestion, currentInterviewState, applyStrongChecks);
                        break;
                    case QuestionType.TextList:
                        this.CheckTextListInvariants(questionId, currentRosterVector, questionnaire, answeredQuestion, currentInterviewState,
                            (Tuple<decimal, string>[])answer, applyStrongChecks);
                        break;

                    default:
                        throw new InterviewException(string.Format(
                            "Question {0} has type {1} which is not supported as initial pre-filled question. InterviewId: {2}",
                            questionId, questionType, this.EventSourceId));
                }
            }
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

                    IEnumerable<Identity> underlyingQuestionInstances = this.GetInstancesOfEntitiesWithSameAndDeeperRosterLevelOrThrow(state,
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

        private static IEnumerable<decimal> GetRosterInstanceIdsBeingAdded(IEnumerable<decimal> existingRosterInstanceIds, IEnumerable<decimal> newRosterInstanceIds)
        {
            return newRosterInstanceIds.Where(newRosterInstanceId => !existingRosterInstanceIds.Contains(newRosterInstanceId)).ToList();
        }

        private static IEnumerable<decimal> GetRosterInstanceIdsBeingRemoved(IEnumerable<decimal> existingRosterInstanceIds, IEnumerable<decimal> newRosterInstanceIds)
        {
            return existingRosterInstanceIds.Where(existingRosterInstanceId => !newRosterInstanceIds.Contains(existingRosterInstanceId)).ToList();
        }

        private static int GetIndexOfRosterInRosterVector(Guid rosterId, IQuestionnaire questionnaire)
        {
            return questionnaire
                .GetRostersFromTopToSpecifiedGroup(rosterId)
                .ToList()
                .IndexOf(rosterId);
        }

        private InterviewChanges UpdateExpressionStateWithAnswersAndGetChanges(
            ILatestInterviewExpressionState expressionProcessorState,
            InterviewChangeStructures interviewChanges,
            IEnumerable<RosterCalculationData> rosterDatas,
            IQuestionnaire questionnaire,
            DateTime answerTime,
            Guid userId)
        {
            foreach (var changes in interviewChanges.Changes)
            {
                changes.ValidityChanges?.Clear();

                changes.EnablementChanges?.Clear();

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
                        expressionProcessorState.AddRoster(r.GroupId, r.OuterRosterVector, r.RosterInstanceId, r.SortIndex);
                    }
                    changes.RosterCalculationData.RosterInstancesToRemove.ForEach(r => expressionProcessorState.RemoveRoster(r.GroupId, r.OuterRosterVector, r.RosterInstanceId));
                }
            }

            var rosterInstancesToAdd = GetOrderedUnionOfUniqueRosterDataPropertiesByRosterAndNestedRosters(x => x.RosterInstancesToAdd, new RosterIdentityComparer(), rosterDatas.ToArray());
            var rosterInstancesToRemove = GetOrderedUnionOfUniqueRosterDataPropertiesByRosterAndNestedRosters(x => x.RosterInstancesToRemove, new RosterIdentityComparer(), rosterDatas.ToArray());

            rosterInstancesToAdd.ForEach(r => expressionProcessorState.AddRoster(r.GroupId, r.OuterRosterVector, r.RosterInstanceId, r.SortIndex));
            rosterInstancesToRemove.ForEach(r => expressionProcessorState.RemoveRoster(r.GroupId, r.OuterRosterVector, r.RosterInstanceId));

            expressionProcessorState.SaveAllCurrentStatesAsPrevious();
            EnablementChanges enablementChanges = expressionProcessorState.ProcessEnablementConditions();

            //calculate state
            var structuralChanges = expressionProcessorState.GetStructuralChanges();

            List<AnswerChange> interviewByAnswerChange = null;
            RosterCalculationData rosterCalculationData = null;

            if (structuralChanges.ChangedMultiQuestions != null && structuralChanges.ChangedMultiQuestions.Count > 0)
            {
                if (interviewByAnswerChange == null)
                    interviewByAnswerChange = new List<AnswerChange>();

                interviewByAnswerChange.AddRange(
                    structuralChanges.ChangedMultiQuestions.Select(
                        x => new AnswerChange(AnswerChangeType.MultipleOptions, userId, x.Key.Id, x.Key.RosterVector, answerTime, x.Value.Select(Convert.ToDecimal).ToArray())));
            }

            if (structuralChanges.ChangedSingleQuestions != null && structuralChanges.ChangedSingleQuestions.Count > 0)
            {
                if (interviewByAnswerChange == null)
                    interviewByAnswerChange = new List<AnswerChange>();

                interviewByAnswerChange.AddRange(
                    structuralChanges.ChangedSingleQuestions.Select(
                        x => new AnswerChange(AnswerChangeType.SingleOption, userId, x.Key.Id, x.Key.RosterVector, answerTime, x.Value)));
            }

            if (structuralChanges.ChangedYesNoQuestions != null && structuralChanges.ChangedYesNoQuestions.Count > 0)
            {
                if (interviewByAnswerChange == null)
                    interviewByAnswerChange = new List<AnswerChange>();

                interviewByAnswerChange.AddRange(
                    structuralChanges.ChangedYesNoQuestions.Select(
                        x => new AnswerChange(AnswerChangeType.YesNo, userId, x.Key.Id, x.Key.RosterVector, answerTime, ConvertToAnsweredYesNoOptionArray(x.Value))));
            }

            if (structuralChanges.RemovedRosters != null && structuralChanges.RemovedRosters.Count > 0)
            {
                if (rosterCalculationData == null)
                    rosterCalculationData = new RosterCalculationData();

                rosterCalculationData.RosterInstancesToRemove.AddRange(
                    structuralChanges.RemovedRosters.Select(x => new RosterIdentity(x.Id, x.RosterVector.Shrink(), x.RosterVector.Last())));
            }

            var rostersWithTitles = new Dictionary<Identity, string>();

            var changedLinkedOptions =
              this.CreateChangedLinkedOptions(expressionProcessorState,
              this.interviewState,
              questionnaire,
              interviewByAnswerChange,
              enablementChanges,
              rosterCalculationData,
              rostersWithTitles).ToArray();

            var linkedQuestionsAnswersToRemove = this.GetLinkedQuestionsAnswersToRemove(this.interviewState, changedLinkedOptions, questionnaire);

            VariableValueChanges variableValueChanges = expressionProcessorState.ProcessVariables();

            ValidityChanges validationChanges = expressionProcessorState.ProcessValidationExpressions();

            var enablementAndValidityChanges = new InterviewChanges(
                interviewByAnswerChange,
                enablementChanges,
                validationChanges,
                rosterCalculationData,
                linkedQuestionsAnswersToRemove,
                null,
                null,
                null,
                null,
                changedLinkedOptions,
                variableValueChanges);

            return enablementAndValidityChanges;
        }

        private void UpdateExpressionProcessorStateWithAnswerChange(AnswerChange answerChange,
            ILatestInterviewExpressionState expressionProcessorState)
        {
            switch (answerChange.InterviewChangeType)
            {
                case AnswerChangeType.RemoveAnswer:
                    RemoveAnswerFromExpressionProcessorState(expressionProcessorState, answerChange.QuestionId,
                        answerChange.RosterVector);
                    break;
                case AnswerChangeType.Text:
                    expressionProcessorState.UpdateTextAnswer(answerChange.QuestionId, answerChange.RosterVector,
                        (string)answerChange.Answer);
                    break;
                case AnswerChangeType.DateTime:
                    expressionProcessorState.UpdateDateAnswer(answerChange.QuestionId, answerChange.RosterVector,
                        (DateTime)answerChange.Answer);
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
                    expressionProcessorState.UpdateQrBarcodeAnswer(answerChange.QuestionId, answerChange.RosterVector,
                        (string)answerChange.Answer);
                    break;
                case AnswerChangeType.NumericInteger:
                    expressionProcessorState.UpdateNumericIntegerAnswer(answerChange.QuestionId,
                        answerChange.RosterVector, (int)answerChange.Answer);
                    break;
                case AnswerChangeType.NumericReal:
                    expressionProcessorState.UpdateNumericRealAnswer(answerChange.QuestionId, answerChange.RosterVector,
                        Convert.ToDouble(answerChange.Answer));
                    break;
                case AnswerChangeType.SingleOptionLinked:
                    expressionProcessorState.UpdateLinkedSingleOptionAnswer(answerChange.QuestionId,
                        answerChange.RosterVector,
                        (decimal[])answerChange.Answer);
                    break;
                case AnswerChangeType.SingleOption:
                    expressionProcessorState.UpdateSingleOptionAnswer(answerChange.QuestionId, answerChange.RosterVector,
                        (decimal)answerChange.Answer);
                    break;
                case AnswerChangeType.MultipleOptionsLinked:
                    expressionProcessorState.UpdateLinkedMultiOptionAnswer(answerChange.QuestionId,
                        answerChange.RosterVector,
                        (decimal[][])answerChange.Answer);
                    break;
                case AnswerChangeType.MultipleOptions:
                    expressionProcessorState.UpdateMultiOptionAnswer(answerChange.QuestionId, answerChange.RosterVector,
                        (decimal[])answerChange.Answer);
                    break;
                case AnswerChangeType.YesNo:
                    expressionProcessorState.UpdateYesNoAnswer(answerChange.QuestionId, answerChange.RosterVector, ConvertToYesNoAnswersOnly((AnsweredYesNoOption[])answerChange.Answer));
                    break;
            }
        }

        public virtual IEnumerable<CategoricalOption> GetFilteredOptionsForQuestion(Identity question, int? parentQuestionValue, string filter)
        {
            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion, this.language);
            var filteredOptions = questionnaire.GetOptionsForQuestion(question.Id, parentQuestionValue, filter);

            if (questionnaire.IsSupportFilteringForOptions(question.Id))
                return this.ExpressionProcessorStatePrototype.FilterOptionsForQuestion(question, filteredOptions);
            else
                return filteredOptions;
        }

        public CategoricalOption GetOptionForQuestionWithoutFilter(Identity question, int value, int? parentQuestionValue = null)
        {
            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion, this.language);

            return questionnaire.GetOptionsForQuestion(question.Id, parentQuestionValue, string.Empty).FirstOrDefault(x => x.Value == value);
        }

        public CategoricalOption GetOptionForQuestionWithFilter(Identity question, string optionText, int? parentQuestionValue = null)
        {
            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion, this.language);
            var filteredOption = questionnaire.GetOptionForQuestionByOptionText(question.Id, optionText);

            if (filteredOption == null)
                return null;

            if (questionnaire.IsSupportFilteringForOptions(question.Id))
                return this.ExpressionProcessorStatePrototype.FilterOptionsForQuestion(question, Enumerable.Repeat(filteredOption, 1)).SingleOrDefault();
            else
                return filteredOption;
        }

        protected bool HasInvalidAnswers() => this.interviewState.InvalidAnsweredQuestions.Any(x => !this.interviewState.DisabledQuestions.Contains(ConversionHelper.ConvertIdentityToString(x.Key)));
        protected bool HasInvalidStaticTexts => this.interviewState.InvalidStaticTexts.Any(x => !this.interviewState.DisabledStaticTexts.Contains(x.Key));

        private static AnsweredYesNoOption[] ConvertToAnsweredYesNoOptionArray(YesNoAnswersOnly answeredOptions)
        {
            return
                answeredOptions.No.Select(x => new AnsweredYesNoOption(x, false))
                    .Union(answeredOptions.Yes.Select(x => new AnsweredYesNoOption(x, true)))
                    .ToArray();
        }

        protected InterviewTree BuildInterviewTree(IQuestionnaire questionnaire, InterviewStateDependentOnAnswers interviewState = null)
        {
            var sections = this.BuildInterviewTreeSections(questionnaire, interviewState).ToList();

            return new InterviewTree(this.EventSourceId, sections);
        }

        private IEnumerable<InterviewTreeSection> BuildInterviewTreeSections(IQuestionnaire questionnaire, InterviewStateDependentOnAnswers interviewState)
        {
            var sectionIds = questionnaire.GetAllSections();

            foreach (var sectionId in sectionIds)
            {
                var sectionIdentity = new Identity(sectionId, RosterVector.Empty);
                var section = this.BuildInterviewTreeSection(sectionIdentity, questionnaire, interviewState);

                yield return section;
            }
        }

        private InterviewTreeSection BuildInterviewTreeSection(Identity sectionIdentity, IQuestionnaire questionnaire, InterviewStateDependentOnAnswers interviewState)
        {
            interviewState = interviewState ?? this.interviewState;

            var children = BuildInterviewTreeGroupChildren(sectionIdentity, questionnaire, interviewState).ToList();
            bool isDisabled = interviewState.IsGroupDisabled(sectionIdentity);

            return new InterviewTreeSection(sectionIdentity, children, isDisabled: isDisabled);
        }

        private InterviewTreeSubSection BuildInterviewTreeSubSection(Identity groupIdentity, IQuestionnaire questionnaire, InterviewStateDependentOnAnswers interviewState)
        {
            var children = BuildInterviewTreeGroupChildren(groupIdentity, questionnaire, interviewState).ToList();
            bool isDisabled = interviewState.IsGroupDisabled(groupIdentity);

            return new InterviewTreeSubSection(groupIdentity, children, isDisabled: isDisabled);
        }

        private InterviewTreeRoster BuildInterviewTreeRoster(Identity rosterIdentity, IQuestionnaire questionnaire, InterviewStateDependentOnAnswers interviewState)
        {
            var children = BuildInterviewTreeGroupChildren(rosterIdentity, questionnaire, interviewState).ToList();
            bool isDisabled = interviewState.IsGroupDisabled(rosterIdentity);
            string rosterGroupKey = ConversionHelper.ConvertIdAndRosterVectorToString(rosterIdentity.Id, rosterIdentity.RosterVector);
            string rosterTitle = this.interviewState.RosterTitles.ContainsKey(rosterGroupKey) 
                ? this.interviewState.RosterTitles[rosterGroupKey]
                : null;

            var rosterTitleQuestionId = questionnaire.GetRosterTitleQuestionId(rosterIdentity.Id);
            Identity rosterTitleQuestionIdentity = null;
            if (rosterTitleQuestionId.HasValue)
                rosterTitleQuestionIdentity = new Identity(rosterTitleQuestionId.Value, rosterIdentity.RosterVector);
            RosterType rosterType = RosterType.Fixed;
            Guid? sourceQuestionId = null;
            if (questionnaire.IsFixedRoster(rosterIdentity.Id))
                rosterType = RosterType.Fixed;
            else
            {
                sourceQuestionId = questionnaire.GetRosterSizeQuestion(rosterIdentity.Id);
                var questionaType = questionnaire.GetQuestionType(sourceQuestionId.Value);
                switch (questionaType)
                {
                    case QuestionType.MultyOption:
                        rosterType = questionnaire.IsQuestionYesNo(sourceQuestionId.Value) ? RosterType.YesNo : RosterType.Multi;
                        break;
                    case QuestionType.Numeric:
                        rosterType = RosterType.Numeric;
                        break;
                    case QuestionType.TextList:
                        rosterType = RosterType.List;
                        break;
                }
            }

            return new InterviewTreeRoster(rosterIdentity, children,
                rosterType: rosterType, 
                isDisabled: isDisabled, 
                rosterTitle: rosterTitle,
                rosterTitleQuestionIdentity: rosterTitleQuestionIdentity,
                rosterSizeQuestion: sourceQuestionId);
        }

        private static InterviewTreeQuestion BuildInterviewTreeQuestion(Identity questionIdentity, object answer, bool isQuestionDisabled, IReadOnlyCollection<RosterVector> linkedOptions, IQuestionnaire questionnaire)
        {
            QuestionType questionType = questionnaire.GetQuestionType(questionIdentity.Id);
            bool isDisabled = isQuestionDisabled;
            string title = questionnaire.GetQuestionTitle(questionIdentity.Id);
            string variableName = questionnaire.GetQuestionVariableName(questionIdentity.Id);
            bool isYesNoQuestion = questionnaire.IsQuestionYesNo(questionIdentity.Id);
            bool isDecimalQuestion = !questionnaire.IsQuestionInteger(questionIdentity.Id);
            bool isLinkedQuestion = questionnaire.IsQuestionLinked(questionIdentity.Id) || questionnaire.IsQuestionLinkedToRoster(questionIdentity.Id);
            var linkedSourceEntityId = isLinkedQuestion ?
                (questionnaire.IsQuestionLinked(questionIdentity.Id)
                  ? questionnaire.GetQuestionReferencedByLinkedQuestion(questionIdentity.Id)
                  : questionnaire.GetRosterReferencedByLinkedQuestion(questionIdentity.Id))
                  : (Guid?)null;

            Guid? commonParentRosterIdForLinkedQuestion = isLinkedQuestion ? questionnaire.GetCommontParentForLinkedQuestionAndItSource(questionIdentity.Id) : null;
            Identity commonParentIdentity = null;
            if (isLinkedQuestion && commonParentRosterIdForLinkedQuestion.HasValue)
            {
                var level = questionnaire.GetRosterLevelForEntity(commonParentRosterIdForLinkedQuestion.Value);
                var commonParentRosterVector = questionIdentity.RosterVector.Take(level).ToArray();
                commonParentIdentity = new Identity(commonParentRosterIdForLinkedQuestion.Value, commonParentRosterVector);
            }

            Guid ? cascadingParentQuestionId = questionnaire.GetCascadingQuestionParentId(questionIdentity.Id);
            var cascadingParentQuestionIdentity = cascadingParentQuestionId.HasValue
                ? GetInstanceOfQuestionWithSameAndUpperRosterLevelOrThrow(cascadingParentQuestionId.Value, questionIdentity.RosterVector, questionnaire)
                : null;

            return new InterviewTreeQuestion(
                questionIdentity,
                questionType: questionType,
                isDisabled: isDisabled,
                title: title,
                variableName: variableName,
                answer: answer,
                linkedOptions: linkedOptions,
                cascadingParentQuestionIdentity: cascadingParentQuestionIdentity,
                isYesNo: isYesNoQuestion,
                isDecimal: isDecimalQuestion,
                linkedSourceId: linkedSourceEntityId,
                commonParentRosterIdForLinkedQuestion: commonParentIdentity);
        }

        private static InterviewTreeStaticText BuildInterviewTreeStaticText(Identity staticTextIdentity, IQuestionnaire questionnaire, InterviewStateDependentOnAnswers interviewState)
        {
            bool isDisabled = interviewState.IsStaticTextDisabled(staticTextIdentity);

            var interviewStaticText =  new InterviewTreeStaticText(staticTextIdentity, isDisabled);
            if (interviewState.InvalidStaticTexts.ContainsKey(staticTextIdentity))
                interviewStaticText.MarkAsInvalid(interviewState.InvalidStaticTexts[staticTextIdentity]);
            return interviewStaticText;
        }

        private IEnumerable<IInterviewTreeNode> BuildInterviewTreeGroupChildren(Identity groupIdentity, IQuestionnaire questionnaire, InterviewStateDependentOnAnswers interviewState)
        {
            var childIds = questionnaire.GetChildEntityIds(groupIdentity.Id);

            foreach (var childId in childIds)
            {
                if (questionnaire.IsRosterGroup(childId))
                {
                    Guid[] rostersStartingFromTop = questionnaire.GetRostersFromTopToSpecifiedGroup(childId).ToArray();

                    IEnumerable<RosterVector> childRosterVectors = ExtendRosterVector(
                        interviewState, groupIdentity.RosterVector, rostersStartingFromTop.Length, rostersStartingFromTop);

                    foreach (var childRosterVector in childRosterVectors)
                    {
                        var childRosterIdentity = new Identity(childId, childRosterVector);
                        yield return BuildInterviewTreeRoster(childRosterIdentity, questionnaire, interviewState);
                    }
                }
                else if (questionnaire.HasGroup(childId))
                {
                    var childGroupIdentity = new Identity(childId, groupIdentity.RosterVector);
                    yield return BuildInterviewTreeSubSection(childGroupIdentity, questionnaire, interviewState);
                }
                else if (questionnaire.HasQuestion(childId))
                {
                    var childQuestionIdentity = new Identity(childId, groupIdentity.RosterVector);
                    var answer = interviewState.GetAnswer(childQuestionIdentity);

                    var interviewTreeQuestion = BuildInterviewTreeQuestion(childQuestionIdentity, answer,
                        interviewState.IsQuestionDisabled(childQuestionIdentity),
                        interviewState.GetOptionsForLinkedQuestion(childQuestionIdentity), questionnaire);

                    if (interviewState.InvalidAnsweredQuestions.ContainsKey(childQuestionIdentity))
                        interviewTreeQuestion.MarkAsInvalid(interviewState.InvalidAnsweredQuestions[childQuestionIdentity]);

                    yield return interviewTreeQuestion;
                }
                else if (questionnaire.IsStaticText(childId))
                {
                    var childStaticTextIdentity = new Identity(childId, groupIdentity.RosterVector);
                    yield return BuildInterviewTreeStaticText(childStaticTextIdentity, questionnaire, interviewState);
                }
                else if (questionnaire.IsVariable(childId))
                {
                    var childVariableIdentity = new Identity(childId, groupIdentity.RosterVector);
                    bool isDisabled = interviewState.IsVariableDisabled(childVariableIdentity);
                    yield return new InterviewTreeVariable(childVariableIdentity, isDisabled, null);
                }
            }
        }
    }
}