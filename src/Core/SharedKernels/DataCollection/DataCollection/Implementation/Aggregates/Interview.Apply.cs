using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.CustomCollections;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Utils;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    public partial class Interview
    {
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
                        this.ExpressionProcessorStatePrototype.UpdateYesNoAnswer(question.Id, questionRosterVector, YesNoAnswer.FromAnsweredYesNoOptions((AnsweredYesNoOption[])question.Answer).ToYesNoAnswersOnly());
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

            var yesNoAnswers = YesNoAnswer.FromAnsweredYesNoOptions(@event.AnsweredOptions).ToYesNoAnswersOnly();
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
                this.ExpressionProcessorStatePrototype.RemoveAnswer(new Identity(identity.Id, identity.RosterVector));
            }
        }

        public virtual void Apply(AnswerRemoved @event)
        {
            this.interviewState.RemoveAnswers(new[] { new Identity(@event.QuestionId, @event.RosterVector) });
            this.ExpressionProcessorStatePrototype.RemoveAnswer(new Identity(@event.QuestionId, @event.RosterVector));
        }
    }
}
