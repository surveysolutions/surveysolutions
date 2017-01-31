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
        private void ValidatePreloadedValues(InterviewTree tree, IQuestionnaire questionnaire, Dictionary<Guid, AbstractAnswer> answersToFeaturedQuestions, RosterVector rosterVector = null)
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
                        this.RequireTextPreloadedValueAllowed(questionId, questionnaire);
                        break;

                    case QuestionType.Numeric:
                        if (questionnaire.IsQuestionInteger(questionId))
                            this.RequireNumericIntegerPreloadedValueAllowed(questionId, ((NumericIntegerAnswer) answer).Value, questionnaire);
                        else
                            this.RequireNumericRealPreloadedValueAllowed(questionId, questionnaire);
                        break;

                    case QuestionType.DateTime:
                        this.CheckDateTimeQuestionInvariants(questionId, currentRosterVector, questionnaire, answeredQuestion, tree, applyStrongChecks: false);
                        break;

                    case QuestionType.SingleOption:
                        this.CheckSingleOptionQuestionInvariants(questionId, currentRosterVector, ((CategoricalFixedSingleOptionAnswer)answer).SelectedValue, questionnaire,
                            answeredQuestion, tree, false, applyStrongChecks: false);
                        break;

                    case QuestionType.MultyOption:
                        if (questionnaire.IsQuestionYesNo(questionId))
                        {
                            this.CheckYesNoQuestionInvariants(new Identity(questionId, currentRosterVector), (YesNoAnswer) answer, questionnaire, tree, applyStrongChecks: false);
                        }
                        else
                        {
                            this.CheckMultipleOptionQuestionInvariants(questionId, currentRosterVector, ((CategoricalFixedMultiOptionAnswer)answer).CheckedValues, questionnaire, answeredQuestion, tree, isLinkedToList: false, applyStrongChecks: false);
                        }
                        break;
                    case QuestionType.QRBarcode:
                        this.CheckQRBarcodeInvariants(questionId, currentRosterVector, questionnaire, answeredQuestion, tree, applyStrongChecks: false);
                        break;
                    case QuestionType.GpsCoordinates:
                        this.CheckGpsCoordinatesInvariants(questionId, currentRosterVector, questionnaire, answeredQuestion, tree, applyStrongChecks: false);
                        break;
                    case QuestionType.TextList:
                        this.CheckTextListInvariants(questionId, currentRosterVector, questionnaire, answeredQuestion, ((TextListAnswer)answer).ToTupleArray(), tree, applyStrongChecks: false);
                        break;

                    default:
                        throw new InterviewException(
                            $"Question {questionId} has type {questionType} which is not supported as initial pre-filled question. InterviewId: {this.EventSourceId}");
                }
            }
        }

        private void ValidatePrefilledAnswers(InterviewTree tree, IQuestionnaire questionnaire, Dictionary<Guid, AbstractAnswer> answersToFeaturedQuestions, RosterVector rosterVector = null)
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
                        this.RequireTextAnswerAllowed(questionId, currentRosterVector, questionnaire, answeredQuestion, tree);
                        break;

                    case QuestionType.Numeric:
                        if (questionnaire.IsQuestionInteger(questionId))
                            this.RequireNumericIntegerAnswerAllowed(questionId, currentRosterVector, ((NumericIntegerAnswer) answer).Value, answeredQuestion, questionnaire, tree);
                        else
                            this.RequireNumericRealAnswerAllowed(questionId, currentRosterVector, ((NumericRealAnswer) answer).Value, answeredQuestion, questionnaire, tree);
                        break;

                    case QuestionType.DateTime:
                        this.CheckDateTimeQuestionInvariants(questionId, currentRosterVector, questionnaire, answeredQuestion, tree, applyStrongChecks: true);
                        break;

                    case QuestionType.SingleOption:
                        this.CheckSingleOptionQuestionInvariants(questionId, currentRosterVector, ((CategoricalFixedSingleOptionAnswer)answer).SelectedValue, questionnaire,
                            answeredQuestion, tree, false, applyStrongChecks: true);
                        break;

                    case QuestionType.MultyOption:
                        if (questionnaire.IsQuestionYesNo(questionId))
                        {
                            this.CheckYesNoQuestionInvariants(new Identity(questionId, currentRosterVector), (YesNoAnswer) answer, questionnaire, tree, applyStrongChecks: true);
                        }
                        else
                        {
                            this.CheckMultipleOptionQuestionInvariants(questionId, currentRosterVector, ((CategoricalFixedMultiOptionAnswer)answer).CheckedValues, questionnaire, answeredQuestion, tree, isLinkedToList: false, applyStrongChecks: true);
                        }
                        break;
                    case QuestionType.QRBarcode:
                        this.CheckQRBarcodeInvariants(questionId, currentRosterVector, questionnaire, answeredQuestion, tree, applyStrongChecks: true);
                        break;
                    case QuestionType.GpsCoordinates:
                        this.CheckGpsCoordinatesInvariants(questionId, currentRosterVector, questionnaire, answeredQuestion, tree, applyStrongChecks: true);
                        break;
                    case QuestionType.TextList:
                        this.CheckTextListInvariants(questionId, currentRosterVector, questionnaire, answeredQuestion, ((TextListAnswer)answer).ToTupleArray(), tree, applyStrongChecks: true);
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

            questionInvariants.RequireQuestion(QuestionType.MultyOption);

            if (applyStrongChecks)
            {
                treeInvariants.RequireQuestionInstanceExists(questionId, rosterVector);
                treeInvariants.RequireQuestionIsEnabled(answeredQuestion);
            }

            if (!linkedQuestionSelectedOptions.Any())
                return;

            var linkedQuestionIdentity = new Identity(questionId, rosterVector);

            foreach (var selectedRosterVector in linkedQuestionSelectedOptions)
            {
                treeInvariants.RequireLinkedOptionIsAvailable(linkedQuestionIdentity, selectedRosterVector);
            }

            questionInvariants.RequireMaxAnswersCountLimit(linkedQuestionSelectedOptions.Length);
        }

        private void CheckLinkedSingleOptionQuestionInvariants(Guid questionId, RosterVector rosterVector, decimal[] linkedQuestionSelectedOption, IQuestionnaire questionnaire, Identity answeredQuestion, InterviewTree tree, bool applyStrongChecks = true)
        {
            var treeInvariants = new InterviewTreeInvariants(tree);
            var questionInvariants = new InterviewQuestionInvariants(this.properties.Id, questionId, questionnaire);

            questionInvariants.RequireQuestion(QuestionType.SingleOption);

            if (applyStrongChecks)
            {
                treeInvariants.RequireQuestionInstanceExists(questionId, rosterVector);
                treeInvariants.RequireQuestionIsEnabled(answeredQuestion);
            }

            var linkedQuestionIdentity = new Identity(questionId, rosterVector);

            treeInvariants.RequireLinkedOptionIsAvailable(linkedQuestionIdentity, linkedQuestionSelectedOption);
        }

        private void RequireTextPreloadedValueAllowed(Guid questionId, IQuestionnaire questionnaire)
        {
            new InterviewQuestionInvariants(this.properties.Id, questionId, questionnaire)
                .RequireQuestion(QuestionType.Text);
        }

        private void RequireTextAnswerAllowed(Guid questionId, RosterVector rosterVector, IQuestionnaire questionnaire,
            Identity answeredQuestion, InterviewTree tree)
        {
            new InterviewQuestionInvariants(this.properties.Id, questionId, questionnaire)
                .RequireQuestion(QuestionType.Text);

            new InterviewTreeInvariants(tree)
                .RequireQuestionInstanceExists(questionId, rosterVector)
                .RequireQuestionIsEnabled(answeredQuestion);
        }

        private void RequireNumericIntegerPreloadedValueAllowed(Guid questionId, int answer, IQuestionnaire questionnaire)
        {
            new InterviewQuestionInvariants(this.properties.Id, questionId, questionnaire)
                .RequireQuestion(QuestionType.Numeric)
                .RequireNumericIntegerQuestion()
                .RequireRosterSizeAnswerNotNegative(answer)
                .RequireRosterSizeAnswerRespectsMaxRosterRowCount(
                    answer,
                    questionnaire.IsQuestionIsRosterSizeForLongRoster(questionId)
                        ? questionnaire.GetMaxLongRosterRowCount()
                        : questionnaire.GetMaxRosterRowCount());
        }

        private void RequireNumericIntegerAnswerAllowed(Guid questionId, RosterVector rosterVector, int answer, Identity answeredQuestion,
            IQuestionnaire questionnaire, InterviewTree tree)
        {
            new InterviewQuestionInvariants(this.properties.Id, questionId, questionnaire)
                .RequireQuestion(QuestionType.Numeric)
                .RequireNumericIntegerQuestion()
                .RequireRosterSizeAnswerNotNegative(answer)
                .RequireRosterSizeAnswerRespectsMaxRosterRowCount(
                    answer,
                    questionnaire.IsQuestionIsRosterSizeForLongRoster(questionId)
                        ? questionnaire.GetMaxLongRosterRowCount()
                        : questionnaire.GetMaxRosterRowCount());

            new InterviewTreeInvariants(tree)
                .RequireQuestionInstanceExists(questionId, rosterVector)
                .RequireQuestionIsEnabled(answeredQuestion);
        }

        private void RequireNumericRealPreloadedValueAllowed(Guid questionId, IQuestionnaire questionnaire)
        {
            new InterviewQuestionInvariants(this.properties.Id, questionId, questionnaire)
                .RequireQuestion(QuestionType.Numeric)
                .RequireNumericRealQuestion();
        }

        private void RequireNumericRealAnswerAllowed(Guid questionId, RosterVector rosterVector, double answer, Identity answeredQuestion,
            IQuestionnaire questionnaire, InterviewTree tree)
        {
            new InterviewQuestionInvariants(this.properties.Id, questionId, questionnaire)
                .RequireQuestion(QuestionType.Numeric)
                .RequireNumericRealQuestion()
                .RequireAllowedDecimalPlaces(answer);

            new InterviewTreeInvariants(tree)
                .RequireQuestionInstanceExists(questionId, rosterVector)
                .RequireQuestionIsEnabled(answeredQuestion);
        }

        private void CheckDateTimeQuestionInvariants(Guid questionId, RosterVector rosterVector, IQuestionnaire questionnaire,
            Identity answeredQuestion, InterviewTree tree, bool applyStrongChecks = true)
        {
            var treeInvariants = new InterviewTreeInvariants(tree);

            new InterviewQuestionInvariants(this.properties.Id, questionId, questionnaire)
                .RequireQuestion(QuestionType.DateTime);

            if (applyStrongChecks)
            {
                treeInvariants.RequireQuestionInstanceExists(questionId, rosterVector);
                treeInvariants.RequireQuestionIsEnabled(answeredQuestion);
            }
        }

        private void CheckSingleOptionQuestionInvariants(Guid questionId, RosterVector rosterVector, decimal selectedValue,
            IQuestionnaire questionnaire, Identity answeredQuestion, InterviewTree tree, bool isLinkedToList,
            bool applyStrongChecks = true)
        {
            var treeInvariants = new InterviewTreeInvariants(tree);
            var questionInvariants = new InterviewQuestionInvariants(this.properties.Id, questionId, questionnaire);

            questionInvariants.RequireQuestion(QuestionType.SingleOption);

            if (isLinkedToList)
            {
                var linkedQuestionIdentity = new Identity(questionId, rosterVector);
                treeInvariants.RequireLinkedToListOptionIsAvailable(linkedQuestionIdentity, selectedValue);
            }
            else
            {
                questionInvariants.RequireOptionExists(selectedValue);
            }

            if (applyStrongChecks)
            {
                treeInvariants.RequireCascadingQuestionAnswerCorrespondsToParentAnswer(answeredQuestion, selectedValue, this.QuestionnaireIdentity, questionnaire.Translation);
                treeInvariants.RequireQuestionInstanceExists(questionId, rosterVector);
                treeInvariants.RequireQuestionIsEnabled(answeredQuestion);
            }
        }

        private void CheckMultipleOptionQuestionInvariants(Guid questionId, RosterVector rosterVector, IReadOnlyCollection<int> selectedValues,
            IQuestionnaire questionnaire, Identity answeredQuestion, InterviewTree tree, bool isLinkedToList,
            bool applyStrongChecks = true)
        {
            var treeInvariants = new InterviewTreeInvariants(tree);
            var questionInvariants = new InterviewQuestionInvariants(this.properties.Id, questionId, questionnaire);

            questionInvariants.RequireQuestion(QuestionType.MultyOption);

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
                questionInvariants.RequireOptionsExist(selectedValues);
            }

            if (questionnaire.IsQuestionYesNo(questionId))
            {
                throw new InterviewException($"Question {questionId} has Yes/No type, but command is sent to Multiopions type. questionnaireId: {this.QuestionnaireId}, interviewId {this.EventSourceId}");
            }

            questionInvariants.RequireRosterSizeAnswerNotNegative(selectedValues.Count);
            var maxSelectedAnswerOptions = questionnaire.GetMaxSelectedAnswerOptions(questionId);
            questionInvariants.RequireRosterSizeAnswerRespectsMaxRosterRowCount(selectedValues.Count, maxSelectedAnswerOptions ?? questionnaire.GetMaxRosterRowCount());

            if (applyStrongChecks)
            {
                treeInvariants.RequireQuestionInstanceExists(questionId, rosterVector);
                questionInvariants.RequireMaxAnswersCountLimit(selectedValues.Count);
                treeInvariants.RequireQuestionIsEnabled(answeredQuestion);
            }
        }

        private void CheckYesNoQuestionInvariants(Identity question, YesNoAnswer answer, IQuestionnaire questionnaire, InterviewTree tree, bool applyStrongChecks = true)
        {
            int[] selectedValues = answer.CheckedOptions.Select(answeredOption => answeredOption.Value).ToArray();
            var yesAnswersCount = answer.CheckedOptions.Count(answeredOption => answeredOption.Yes);

            var treeInvariants = new InterviewTreeInvariants(tree);
            var questionInvariants = new InterviewQuestionInvariants(this.properties.Id, question.Id, questionnaire);

            questionInvariants.RequireQuestion(QuestionType.MultyOption);
            questionInvariants.RequireOptionsExist(selectedValues);

            questionInvariants.RequireRosterSizeAnswerNotNegative(yesAnswersCount);
            var maxSelectedAnswerOptions = questionnaire.GetMaxSelectedAnswerOptions(question.Id);
            questionInvariants.RequireRosterSizeAnswerRespectsMaxRosterRowCount(yesAnswersCount, maxSelectedAnswerOptions ?? questionnaire.GetMaxRosterRowCount());

            questionInvariants.RequireMaxAnswersCountLimit(yesAnswersCount);

            if (applyStrongChecks)
            {
                treeInvariants.RequireQuestionInstanceExists(question.Id, question.RosterVector);
                treeInvariants.RequireQuestionIsEnabled(question);
            }
        }

        private void CheckTextListInvariants(Guid questionId, RosterVector rosterVector, IQuestionnaire questionnaire, Identity answeredQuestion, Tuple<decimal, string>[] answers, InterviewTree tree, bool applyStrongChecks = true)
        {
            var treeInvariants = new InterviewTreeInvariants(tree);
            var questionInvariants = new InterviewQuestionInvariants(this.properties.Id, questionId, questionnaire);

            questionInvariants.RequireQuestion(QuestionType.TextList);

            questionInvariants.RequireRosterSizeAnswerNotNegative(answers.Length);
            var maxSelectedAnswerOptions = questionnaire.GetMaxSelectedAnswerOptions(questionId);
            questionInvariants.RequireRosterSizeAnswerRespectsMaxRosterRowCount(answers.Length, maxSelectedAnswerOptions ?? questionnaire.GetMaxRosterRowCount());

            if (applyStrongChecks)
            {
                treeInvariants.RequireQuestionInstanceExists(questionId, rosterVector);
                treeInvariants.RequireQuestionIsEnabled(answeredQuestion);
                questionInvariants.RequireUniqueValues(answers);
                questionInvariants.RequireNotEmptyTexts(answers);
                var maxAnswersCountLimit = questionnaire.GetListSizeForListQuestion(questionId);
                questionInvariants.RequireMaxAnswersCountLimit(answers, maxAnswersCountLimit);
            }
        }

        private void CheckGpsCoordinatesInvariants(Guid questionId, RosterVector rosterVector, IQuestionnaire questionnaire, Identity answeredQuestion, InterviewTree tree, bool applyStrongChecks = true)
        {
            var treeInvariants = new InterviewTreeInvariants(tree);

            new InterviewQuestionInvariants(this.properties.Id, questionId, questionnaire)
                .RequireQuestion(QuestionType.GpsCoordinates);

            if (applyStrongChecks)
            {
                treeInvariants.RequireQuestionInstanceExists(questionId, rosterVector);
                treeInvariants.RequireQuestionIsEnabled(answeredQuestion);
            }
        }

        private void CheckQRBarcodeInvariants(Guid questionId, RosterVector rosterVector, IQuestionnaire questionnaire,
            Identity answeredQuestion, InterviewTree tree, bool applyStrongChecks = true)
        {
            var treeInvariants = new InterviewTreeInvariants(tree);

            new InterviewQuestionInvariants(this.properties.Id, questionId, questionnaire)
                .RequireQuestion(QuestionType.QRBarcode);

            if (applyStrongChecks)
            {
                treeInvariants.RequireQuestionInstanceExists(questionId, rosterVector);
                treeInvariants.RequireQuestionIsEnabled(answeredQuestion);
            }
        }
    }
}
