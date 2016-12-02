using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Invariants;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    public partial class Interview
    {
        private void ValidatePrefilledQuestions(InterviewTree tree, IQuestionnaire questionnaire, Dictionary<Guid, AbstractAnswer> answersToFeaturedQuestions, RosterVector rosterVector = null, bool applyStrongChecks = true)
        {
            var currentRosterVector = rosterVector ?? (decimal[])RosterVector.Empty;
            foreach (KeyValuePair<Guid, AbstractAnswer> answerToFeaturedQuestion in answersToFeaturedQuestions)
            {
                Guid questionId = answerToFeaturedQuestion.Key;
                AbstractAnswer answer = answerToFeaturedQuestion.Value;

                var answeredQuestion = new Identity(questionId, currentRosterVector);

                QuestionType questionType = questionnaire.GetQuestionType(questionId);

                switch (questionType)
                {
                    case QuestionType.Text:
                        this.CheckTextQuestionInvariants(questionId, currentRosterVector, questionnaire, answeredQuestion, tree, applyStrongChecks);
                        break;

                    case QuestionType.Numeric:
                        if (questionnaire.IsQuestionInteger(questionId))
                            this.CheckNumericIntegerQuestionInvariants(questionId, currentRosterVector, ((NumericIntegerAnswer)answer).Value, questionnaire,
                                answeredQuestion, tree, applyStrongChecks);
                        else
                            this.CheckNumericRealQuestionInvariants(questionId, currentRosterVector, ((NumericRealAnswer)answer).Value, questionnaire,
                                answeredQuestion, tree, applyStrongChecks);
                        break;

                    case QuestionType.DateTime:
                        this.CheckDateTimeQuestionInvariants(questionId, currentRosterVector, questionnaire, answeredQuestion, tree, applyStrongChecks);
                        break;

                    case QuestionType.SingleOption:
                        this.CheckSingleOptionQuestionInvariants(questionId, currentRosterVector, ((CategoricalFixedSingleOptionAnswer)answer).SelectedValue, questionnaire,
                            answeredQuestion, tree, false ,applyStrongChecks);
                        break;

                    case QuestionType.MultyOption:
                        if (questionnaire.IsQuestionYesNo(questionId))
                        {
                            this.CheckYesNoQuestionInvariants(new Identity(questionId, currentRosterVector), (YesNoAnswer) answer, questionnaire, tree, applyStrongChecks);
                        }
                        else
                        {
                            this.CheckMultipleOptionQuestionInvariants(questionId, currentRosterVector, ((CategoricalFixedMultiOptionAnswer)answer).CheckedValues, questionnaire, answeredQuestion, tree, applyStrongChecks);
                        }
                        break;
                    case QuestionType.QRBarcode:
                        this.CheckQRBarcodeInvariants(questionId, currentRosterVector, questionnaire, answeredQuestion, tree, applyStrongChecks);
                        break;
                    case QuestionType.GpsCoordinates:
                        this.CheckGpsCoordinatesInvariants(questionId, currentRosterVector, questionnaire, answeredQuestion, tree, applyStrongChecks);
                        break;
                    case QuestionType.TextList:
                        this.CheckTextListInvariants(questionId, currentRosterVector, questionnaire, answeredQuestion, ((TextListAnswer)answer).ToTupleArray(), tree, applyStrongChecks);
                        break;

                    default:
                        throw new InterviewException(
                            $"Question {questionId} has type {questionType} which is not supported as initial pre-filled question. InterviewId: {this.EventSourceId}");
                }
            }
        }

        private void CheckLinkedMultiOptionQuestionInvariants(Guid questionId, RosterVector rosterVector,
            decimal[][] linkedQuestionSelectedOptions, IQuestionnaire questionnaire, Identity answeredQuestion,
            InterviewTree tree, bool applyStrongChecks = true)
        {
            var treeInvariants = new InterviewTreeInvariants(tree);
            var questionInvariants = new InterviewQuestionInvariants(this.properties.Id, questionId, questionnaire);

            questionInvariants.RequireQuestionExists();
            questionInvariants.RequireQuestionType(QuestionType.MultyOption);

            if (applyStrongChecks)
            {
                treeInvariants.RequireRosterVectorQuestionInstanceExists(questionId, rosterVector);
                treeInvariants.RequireQuestionIsEnabled(answeredQuestion);
            }

            if (!linkedQuestionSelectedOptions.Any())
                return;

            var linkedQuestionIdentity = new Identity(questionId, rosterVector);

            foreach (var selectedRosterVector in linkedQuestionSelectedOptions)
            {
                treeInvariants.RequireLinkedOptionIsAvailable(linkedQuestionIdentity, selectedRosterVector);
            }

            questionInvariants.ThrowIfLengthOfSelectedValuesMoreThanMaxForSelectedAnswerOptions(linkedQuestionSelectedOptions.Length);
        }

        private void CheckLinkedSingleOptionQuestionInvariants(Guid questionId, RosterVector rosterVector, decimal[] linkedQuestionSelectedOption, IQuestionnaire questionnaire, Identity answeredQuestion, InterviewTree tree, bool applyStrongChecks = true)
        {
            var treeInvariants = new InterviewTreeInvariants(tree);
            var questionInvariants = new InterviewQuestionInvariants(this.properties.Id, questionId, questionnaire);

            questionInvariants.RequireQuestionExists();
            questionInvariants.RequireQuestionType(QuestionType.SingleOption);

            if (applyStrongChecks)
            {
                treeInvariants.RequireRosterVectorQuestionInstanceExists(questionId, rosterVector);
                treeInvariants.RequireQuestionIsEnabled(answeredQuestion);
            }

            var linkedQuestionIdentity = new Identity(questionId, rosterVector);

            treeInvariants.RequireLinkedOptionIsAvailable(linkedQuestionIdentity, linkedQuestionSelectedOption);
        }

        private void CheckNumericRealQuestionInvariants(Guid questionId, RosterVector rosterVector, double answer,
           IQuestionnaire questionnaire,
           Identity answeredQuestion, InterviewTree tree, bool applyStrongChecks = true)
        {
            var treeInvariants = new InterviewTreeInvariants(tree);
            var questionInvariants = new InterviewQuestionInvariants(this.properties.Id, questionId, questionnaire);

            questionInvariants.RequireQuestionExists();
            questionInvariants.RequireQuestionType(QuestionType.Numeric);
            questionInvariants.RequireNumericRealQuestion();

            if (applyStrongChecks)
            {
                treeInvariants.RequireRosterVectorQuestionInstanceExists(questionId, rosterVector);
                treeInvariants.RequireQuestionIsEnabled(answeredQuestion);
                questionInvariants.ThrowIfAnswerHasMoreDecimalPlacesThenAccepted(answer);
            }
        }

        private void CheckDateTimeQuestionInvariants(Guid questionId, RosterVector rosterVector, IQuestionnaire questionnaire,
            Identity answeredQuestion, InterviewTree tree, bool applyStrongChecks = true)
        {
            var treeInvariants = new InterviewTreeInvariants(tree);
            var questionInvariants = new InterviewQuestionInvariants(this.properties.Id, questionId, questionnaire);

            questionInvariants.RequireQuestionExists();
            questionInvariants.RequireQuestionType(QuestionType.DateTime);

            if (applyStrongChecks)
            {
                treeInvariants.RequireRosterVectorQuestionInstanceExists(questionId, rosterVector);
                treeInvariants.RequireQuestionIsEnabled(answeredQuestion);
            }
        }

        private void CheckSingleOptionQuestionInvariants(Guid questionId, RosterVector rosterVector, decimal selectedValue,
            IQuestionnaire questionnaire, Identity answeredQuestion, InterviewTree tree, bool isLinkedToList,
            bool applyStrongChecks = true)
        {
            var treeInvariants = new InterviewTreeInvariants(tree);
            var questionInvariants = new InterviewQuestionInvariants(this.properties.Id, questionId, questionnaire);

            questionInvariants.RequireQuestionExists();
            questionInvariants.RequireQuestionType(QuestionType.SingleOption);

            if (isLinkedToList)
            {
                var linkedQuestionIdentity = new Identity(questionId, rosterVector);
                treeInvariants.RequireLinkedToListOptionIsAvailable(linkedQuestionIdentity, selectedValue);
            }
            else
            {
                questionInvariants.ThrowIfValueIsNotOneOfAvailableOptions(selectedValue);
            }

            if (applyStrongChecks)
            {
                treeInvariants.RequireCascadingQuestionAnswerCorrespondsToParentAnswer(answeredQuestion, selectedValue, 
                    this.QuestionnaireIdentity, questionnaire.Translation);
                treeInvariants.RequireRosterVectorQuestionInstanceExists(questionId, rosterVector);
                treeInvariants.RequireQuestionIsEnabled(answeredQuestion);
            }
        }

        private void CheckMultipleOptionQuestionInvariants(Guid questionId, RosterVector rosterVector, IReadOnlyCollection<int> selectedValues,
            IQuestionnaire questionnaire, Identity answeredQuestion, InterviewTree tree, bool isLinkedToList,
            bool applyStrongChecks = true)
        {
            var treeInvariants = new InterviewTreeInvariants(tree);
            var questionInvariants = new InterviewQuestionInvariants(this.properties.Id, questionId, questionnaire);

            questionInvariants.RequireQuestionExists();
            questionInvariants.RequireQuestionType(QuestionType.MultyOption);

            if (isLinkedToList)
            {
                var linkedQuestionIdentity = new Identity(questionId, rosterVector);
                foreach (var selectedValue in selectedValues)
                {
                    treeInvariants.RequireLinkedToListOptionIsAvailable(linkedQuestionIdentity, selectedValue);
                }
            }
            else
            {
                questionInvariants.ThrowIfSomeValuesAreNotFromAvailableOptions(selectedValues);
            }

            if (questionnaire.IsQuestionYesNo(questionId))
            {
                throw new InterviewException($"Question {questionId} has Yes/No type, but command is sent to Multiopions type. questionnaireId: {this.QuestionnaireId}, interviewId {this.EventSourceId}");
            }

            if (questionnaire.ShouldQuestionSpecifyRosterSize(questionId))
            {
                questionInvariants.ThrowIfRosterSizeAnswerIsNegativeOrGreaterThenMaxRosterRowCount(selectedValues.Count);
                var maxSelectedAnswerOptions = questionnaire.GetMaxSelectedAnswerOptions(questionId);
                questionInvariants.ThrowIfRosterSizeAnswerIsGreaterThenMaxRosterRowCount(selectedValues.Count, maxSelectedAnswerOptions ?? questionnaire.GetMaxRosterRowCount());
            }

            if (applyStrongChecks)
            {
                treeInvariants.RequireRosterVectorQuestionInstanceExists(questionId, rosterVector);
                questionInvariants.ThrowIfLengthOfSelectedValuesMoreThanMaxForSelectedAnswerOptions(selectedValues.Count);
                treeInvariants.RequireQuestionIsEnabled(answeredQuestion);
            }
        }

        private void CheckYesNoQuestionInvariants(Identity question, YesNoAnswer answer, IQuestionnaire questionnaire, InterviewTree tree, bool applyStrongChecks = true)
        {
            int[] selectedValues = answer.CheckedOptions.Select(answeredOption => answeredOption.Value).ToArray();
            var yesAnswersCount = answer.CheckedOptions.Count(answeredOption => answeredOption.Yes);

            var treeInvariants = new InterviewTreeInvariants(tree);
            var questionInvariants = new InterviewQuestionInvariants(this.properties.Id, question.Id, questionnaire);

            questionInvariants.RequireQuestionExists();
            questionInvariants.RequireQuestionType(QuestionType.MultyOption);
            questionInvariants.ThrowIfSomeValuesAreNotFromAvailableOptions(selectedValues);

            if (questionnaire.ShouldQuestionSpecifyRosterSize(question.Id))
            {
                questionInvariants.ThrowIfRosterSizeAnswerIsNegativeOrGreaterThenMaxRosterRowCount(yesAnswersCount);
                var maxSelectedAnswerOptions = questionnaire.GetMaxSelectedAnswerOptions(question.Id);
                questionInvariants.ThrowIfRosterSizeAnswerIsGreaterThenMaxRosterRowCount(yesAnswersCount, maxSelectedAnswerOptions ?? questionnaire.GetMaxRosterRowCount());
            }

            questionInvariants.ThrowIfLengthOfSelectedValuesMoreThanMaxForSelectedAnswerOptions(yesAnswersCount);

            if (applyStrongChecks)
            {
                treeInvariants.RequireRosterVectorQuestionInstanceExists(question.Id, question.RosterVector);
                treeInvariants.RequireQuestionIsEnabled(question);
            }
        }

        private void CheckTextQuestionInvariants(Guid questionId, RosterVector rosterVector, IQuestionnaire questionnaire,
            Identity answeredQuestion, InterviewTree tree, bool applyStrongChecks = true)
        {
            var treeInvariants = new InterviewTreeInvariants(tree);
            var questionInvariants = new InterviewQuestionInvariants(this.properties.Id, questionId, questionnaire);

            questionInvariants.RequireQuestionExists();
            questionInvariants.RequireQuestionType(QuestionType.Text);

            if (applyStrongChecks)
            {
                treeInvariants.RequireRosterVectorQuestionInstanceExists(questionId, rosterVector);
                treeInvariants.RequireQuestionIsEnabled(answeredQuestion);
            }
        }

        private void CheckNumericIntegerQuestionInvariants(Guid questionId, RosterVector rosterVector, int answer, IQuestionnaire questionnaire,
            Identity answeredQuestion, InterviewTree tree, bool applyStrongChecks = true)
        {
            var treeInvariants = new InterviewTreeInvariants(tree);
            var questionInvariants = new InterviewQuestionInvariants(this.properties.Id, questionId, questionnaire);

            questionInvariants.RequireQuestionExists();
            questionInvariants.RequireQuestionType(QuestionType.Numeric);
            questionInvariants.RequireNumericIntegerQuestion();

            if (questionnaire.ShouldQuestionSpecifyRosterSize(questionId))
            {
                questionInvariants.ThrowIfRosterSizeAnswerIsNegativeOrGreaterThenMaxRosterRowCount(answer);
                questionInvariants.ThrowIfRosterSizeAnswerIsGreaterThenMaxRosterRowCount(answer, questionnaire.IsQuestionIsRosterSizeForLongRoster(questionId)
                    ? questionnaire.GetMaxLongRosterRowCount()
                    : questionnaire.GetMaxRosterRowCount());
            }

            if (applyStrongChecks)
            {
                treeInvariants.RequireRosterVectorQuestionInstanceExists(questionId, rosterVector);
                treeInvariants.RequireQuestionIsEnabled(answeredQuestion);
            }
        }

        private void CheckTextListInvariants(Guid questionId, RosterVector rosterVector, IQuestionnaire questionnaire, Identity answeredQuestion, Tuple<decimal, string>[] answers, InterviewTree tree, bool applyStrongChecks = true)
        {
            var treeInvariants = new InterviewTreeInvariants(tree);
            var questionInvariants = new InterviewQuestionInvariants(this.properties.Id, questionId, questionnaire);

            questionInvariants.RequireQuestionExists();
            questionInvariants.RequireQuestionType(QuestionType.TextList);

            if (questionnaire.ShouldQuestionSpecifyRosterSize(questionId))
            {
                questionInvariants.ThrowIfRosterSizeAnswerIsNegativeOrGreaterThenMaxRosterRowCount(answers.Length);
                var maxSelectedAnswerOptions = questionnaire.GetMaxSelectedAnswerOptions(questionId);
                questionInvariants.ThrowIfRosterSizeAnswerIsGreaterThenMaxRosterRowCount(answers.Length, maxSelectedAnswerOptions ?? questionnaire.GetMaxRosterRowCount());
            }

            if (applyStrongChecks)
            {
                treeInvariants.RequireRosterVectorQuestionInstanceExists(questionId, rosterVector);
                treeInvariants.RequireQuestionIsEnabled(answeredQuestion);
                questionInvariants.ThrowIfDecimalValuesAreNotUnique(answers);
                questionInvariants.ThrowIfStringValueAreEmptyOrWhitespaces(answers);
                var maxAnswersCountLimit = questionnaire.GetListSizeForListQuestion(questionId);
                questionInvariants.ThrowIfAnswersExceedsMaxAnswerCountLimit(answers, maxAnswersCountLimit);
            }
        }

        private void CheckGpsCoordinatesInvariants(Guid questionId, RosterVector rosterVector, IQuestionnaire questionnaire, Identity answeredQuestion, InterviewTree tree, bool applyStrongChecks = true)
        {
            var treeInvariants = new InterviewTreeInvariants(tree);
            var questionInvariants = new InterviewQuestionInvariants(this.properties.Id, questionId, questionnaire);

            questionInvariants.RequireQuestionExists();
            questionInvariants.RequireQuestionType(QuestionType.GpsCoordinates);

            if (applyStrongChecks)
            {
                treeInvariants.RequireRosterVectorQuestionInstanceExists(questionId, rosterVector);
                treeInvariants.RequireQuestionIsEnabled(answeredQuestion);
            }
        }

        private void CheckQRBarcodeInvariants(Guid questionId, RosterVector rosterVector, IQuestionnaire questionnaire,
            Identity answeredQuestion, InterviewTree tree, bool applyStrongChecks = true)
        {
            var treeInvariants = new InterviewTreeInvariants(tree);
            var questionInvariants = new InterviewQuestionInvariants(this.properties.Id, questionId, questionnaire);

            questionInvariants.RequireQuestionExists();
            questionInvariants.RequireQuestionType(QuestionType.QRBarcode);

            if (applyStrongChecks)
            {
                treeInvariants.RequireRosterVectorQuestionInstanceExists(questionId, rosterVector);
                treeInvariants.RequireQuestionIsEnabled(answeredQuestion);
            }
        }
    }
}
