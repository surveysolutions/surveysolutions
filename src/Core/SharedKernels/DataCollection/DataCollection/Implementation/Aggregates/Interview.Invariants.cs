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
        private void ValidatePreloadValues(InterviewTree tree, IQuestionnaire questionnaire, Dictionary<Guid, AbstractAnswer> answersToFeaturedQuestions, RosterVector rosterVector = null)
        {
            var currentRosterVector = rosterVector ?? (decimal[])RosterVector.Empty;
            foreach (KeyValuePair<Guid, AbstractAnswer> answerToFeaturedQuestion in answersToFeaturedQuestions)
            {
                Guid questionId = answerToFeaturedQuestion.Key;
                AbstractAnswer answer = answerToFeaturedQuestion.Value;

                var questionIdentity = new Identity(questionId, currentRosterVector);

                QuestionType questionType = questionnaire.GetQuestionType(questionId);

                switch (questionType)
                {
                    case QuestionType.Text:
                        RequireTextPreloadValueAllowed(questionIdentity, questionnaire, tree);
                        break;

                    case QuestionType.Numeric:
                        if (questionnaire.IsQuestionInteger(questionId))
                            RequireNumericIntegerPreloadValueAllowed(questionIdentity, ((NumericIntegerAnswer) answer).Value, questionnaire, tree);
                        else
                            RequireNumericRealPreloadValueAllowed(questionIdentity, questionnaire, tree);
                        break;

                    case QuestionType.DateTime:
                        RequireDateTimePreloadValueAllowed(questionIdentity, questionnaire, tree);
                        break;

                    case QuestionType.SingleOption:
                        RequireFixedSingleOptionPreloadValueAllowed(questionIdentity, ((CategoricalFixedSingleOptionAnswer) answer).SelectedValue, questionnaire, tree);
                        break;

                    case QuestionType.MultyOption:
                        if (questionnaire.IsQuestionYesNo(questionId))
                            RequireYesNoPreloadValueAllowed(questionIdentity, (YesNoAnswer) answer, questionnaire, tree);
                        else
                            RequireFixedMultipleOptionsPreloadValueAllowed(questionIdentity, ((CategoricalFixedMultiOptionAnswer) answer).CheckedValues, questionnaire, tree);
                        break;
                    case QuestionType.QRBarcode:
                        CheckQRBarcodeInvariants(questionIdentity, questionnaire, tree, applyStrongChecks: false);
                        break;
                    case QuestionType.GpsCoordinates:
                        RequireGpsCoordinatesPreloadValueAllowed(questionIdentity, questionnaire, tree);
                        break;
                    case QuestionType.TextList:
                        RequireTextListPreloadValueAllowed(questionIdentity, ((TextListAnswer)answer).ToTupleArray(), questionnaire, tree);
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

                var questionIdentity = new Identity(questionId, currentRosterVector);

                QuestionType questionType = questionnaire.GetQuestionType(questionId);

                switch (questionType)
                {
                    case QuestionType.Text:
                        RequireTextAnswerAllowed(questionIdentity, questionnaire, tree);
                        break;

                    case QuestionType.Numeric:
                        if (questionnaire.IsQuestionInteger(questionId))
                            RequireNumericIntegerAnswerAllowed(questionIdentity, ((NumericIntegerAnswer) answer).Value, questionnaire, tree);
                        else
                            RequireNumericRealAnswerAllowed(questionIdentity, ((NumericRealAnswer) answer).Value, questionnaire, tree);
                        break;

                    case QuestionType.DateTime:
                        RequireDateTimeAnswerAllowed(questionIdentity, questionnaire, tree);
                        break;

                    case QuestionType.SingleOption:
                        this.RequireFixedSingleOptionAnswerAllowed(questionIdentity, ((CategoricalFixedSingleOptionAnswer) answer).SelectedValue, questionnaire, tree);
                        break;

                    case QuestionType.MultyOption:
                        if (questionnaire.IsQuestionYesNo(questionId))
                            RequireYesNoAnswerAllowed(new Identity(questionId, currentRosterVector), (YesNoAnswer) answer, questionnaire, tree);
                        else
                            RequireFixedMultipleOptionsAnswerAllowed(questionIdentity, ((CategoricalFixedMultiOptionAnswer) answer).CheckedValues, questionnaire, tree);
                        break;
                    case QuestionType.QRBarcode:
                        CheckQRBarcodeInvariants(questionIdentity, questionnaire, tree, applyStrongChecks: true);
                        break;
                    case QuestionType.GpsCoordinates:
                        RequireGpsCoordinatesAnswerAllowed(questionIdentity, questionnaire, tree);
                        break;
                    case QuestionType.TextList:
                        RequireTextListAnswerAllowed(questionIdentity, ((TextListAnswer)answer).ToTupleArray(), questionnaire, tree);
                        break;

                    default:
                        throw new InterviewException(
                            $"Question {questionId} has type {questionType} which is not supported as initial pre-filled question. InterviewId: {this.EventSourceId}");
                }
            }
        }

        private static void RequireTextPreloadValueAllowed(Identity questionIdentity,
            IQuestionnaire questionnaire, InterviewTree tree)
        {
            new InterviewQuestionInvariants(questionIdentity, questionnaire, tree)
                .RequireQuestion(QuestionType.Text);
        }

        private static void RequireTextAnswerAllowed(Identity questionIdentity,
            IQuestionnaire questionnaire, InterviewTree tree)
        {
            new InterviewQuestionInvariants(questionIdentity, questionnaire, tree)
                .RequireQuestion(QuestionType.Text)
                .RequireQuestionInstanceExists()
                .RequireQuestionIsEnabled();
        }

        private static void RequireNumericIntegerPreloadValueAllowed(Identity questionIdentity, int answer,
            IQuestionnaire questionnaire, InterviewTree tree)
        {
            new InterviewQuestionInvariants(questionIdentity, questionnaire, tree)
                .RequireQuestion(QuestionType.Numeric)
                .RequireNumericIntegerQuestion()
                .RequireRosterSizeAnswerNotNegative(answer)
                .RequireRosterSizeAnswerRespectsMaxRosterRowCount(answer);
        }

        private static void RequireNumericIntegerAnswerAllowed(Identity questionIdentity, int answer,
            IQuestionnaire questionnaire, InterviewTree tree)
        {
            new InterviewQuestionInvariants(questionIdentity, questionnaire, tree)
                .RequireQuestion(QuestionType.Numeric)
                .RequireNumericIntegerQuestion()
                .RequireRosterSizeAnswerNotNegative(answer)
                .RequireRosterSizeAnswerRespectsMaxRosterRowCount(answer)
                .RequireQuestionInstanceExists()
                .RequireQuestionIsEnabled();
        }

        private static void RequireNumericRealPreloadValueAllowed(Identity questionIdentity,
            IQuestionnaire questionnaire, InterviewTree tree)
        {
            new InterviewQuestionInvariants(questionIdentity, questionnaire, tree)
                .RequireQuestion(QuestionType.Numeric)
                .RequireNumericRealQuestion();
        }

        private static void RequireNumericRealAnswerAllowed(Identity questionIdentity, double answer,
            IQuestionnaire questionnaire, InterviewTree tree)
        {
            new InterviewQuestionInvariants(questionIdentity, questionnaire, tree)
                .RequireQuestion(QuestionType.Numeric)
                .RequireNumericRealQuestion()
                .RequireAllowedDecimalPlaces(answer)
                .RequireQuestionInstanceExists()
                .RequireQuestionIsEnabled();
        }

        private static void RequireDateTimePreloadValueAllowed(Identity questionIdentity,
            IQuestionnaire questionnaire, InterviewTree tree)
        {
            new InterviewQuestionInvariants(questionIdentity, questionnaire, tree)
                .RequireQuestion(QuestionType.DateTime);
        }

        private static void RequireDateTimeAnswerAllowed(Identity questionIdentity,
            IQuestionnaire questionnaire, InterviewTree tree)
        {
            new InterviewQuestionInvariants(questionIdentity, questionnaire, tree)
                .RequireQuestion(QuestionType.DateTime)
                .RequireQuestionInstanceExists()
                .RequireQuestionIsEnabled();
        }

        private static void RequireFixedSingleOptionPreloadValueAllowed(Identity questionIdentity, decimal selectedValue,
            IQuestionnaire questionnaire, InterviewTree tree)
        {
            new InterviewQuestionInvariants(questionIdentity, questionnaire, tree)
                .RequireQuestion(QuestionType.SingleOption)
                .RequireOptionExists(selectedValue);
        }

        private void RequireFixedSingleOptionAnswerAllowed(Identity questionIdentity, decimal selectedValue,
            IQuestionnaire questionnaire, InterviewTree tree)
        {
            new InterviewQuestionInvariants(questionIdentity, questionnaire, tree)
                .RequireQuestion(QuestionType.SingleOption)
                .RequireOptionExists(selectedValue)
                .RequireQuestionInstanceExists()
                .RequireQuestionIsEnabled()
                .RequireCascadingQuestionAnswerCorrespondsToParentAnswer(selectedValue, this.QuestionnaireIdentity, questionnaire.Translation);
        }

        private static void RequireLinkedToListSingleOptionAnswerAllowed(Identity questionIdentity, decimal selectedValue,
            IQuestionnaire questionnaire, InterviewTree tree)
        {
            new InterviewQuestionInvariants(questionIdentity, questionnaire, tree)
                .RequireQuestion(QuestionType.SingleOption)
                .RequireQuestionInstanceExists()
                .RequireQuestionIsEnabled()
                .RequireLinkedToListOptionIsAvailable(selectedValue);
        }

        private static void RequireLinkedToRosterSingleOptionAnswerAllowed(Identity questionIdentity, decimal[] selectedLinkedOption,
            IQuestionnaire questionnaire, InterviewTree tree)
        {
            new InterviewQuestionInvariants(questionIdentity, questionnaire, tree)
                .RequireQuestion(QuestionType.SingleOption)
                .RequireQuestionInstanceExists()
                .RequireQuestionIsEnabled()
                .RequireLinkedOptionIsAvailable(selectedLinkedOption);
        }

        private static void RequireFixedMultipleOptionsPreloadValueAllowed(Identity questionIdentity, IReadOnlyCollection<int> selectedValues, IQuestionnaire questionnaire, InterviewTree tree)
        {
            new InterviewQuestionInvariants(questionIdentity, questionnaire, tree)
                .RequireQuestion(QuestionType.MultyOption)
                .RequireOptionsExist(selectedValues)
                .RequireNotYesNoMultipleOptionsQuestion()
                .RequireRosterSizeAnswerNotNegative(selectedValues.Count)
                .RequireRosterSizeAnswerRespectsMaxRosterRowCount(selectedValues.Count)
                .RequireMaxAnswersCountLimit(selectedValues.Count);
        }

        private static void RequireFixedMultipleOptionsAnswerAllowed(Identity questionIdentity, IReadOnlyCollection<int> selectedValues, IQuestionnaire questionnaire, InterviewTree tree)
        {
            new InterviewQuestionInvariants(questionIdentity, questionnaire, tree)
                .RequireQuestion(QuestionType.MultyOption)
                .RequireOptionsExist(selectedValues)
                .RequireNotYesNoMultipleOptionsQuestion()
                .RequireRosterSizeAnswerNotNegative(selectedValues.Count)
                .RequireRosterSizeAnswerRespectsMaxRosterRowCount(selectedValues.Count)
                .RequireQuestionInstanceExists()
                .RequireMaxAnswersCountLimit(selectedValues.Count)
                .RequireQuestionIsEnabled();
        }

        private static void RequireLinkedToListMultipleOptionsAnswerAllowed(Identity questionIdentity, IReadOnlyCollection<int> selectedValues, IQuestionnaire questionnaire, InterviewTree tree)
        {
            new InterviewQuestionInvariants(questionIdentity, questionnaire, tree)
                .RequireQuestion(QuestionType.MultyOption)
                .RequireLinkedToListOptionsAreAvailable(selectedValues)
                .RequireNotYesNoMultipleOptionsQuestion()
                .RequireRosterSizeAnswerNotNegative(selectedValues.Count)
                .RequireRosterSizeAnswerRespectsMaxRosterRowCount(selectedValues.Count)
                .RequireQuestionInstanceExists()
                .RequireMaxAnswersCountLimit(selectedValues.Count)
                .RequireQuestionIsEnabled();
        }

        private static void RequireLinkedToRosterMultipleOptionsAnswerAllowed(Identity questionIdentity, decimal[][] selectedLinkedOptions, IQuestionnaire questionnaire, InterviewTree tree)
        {
            new InterviewQuestionInvariants(questionIdentity, questionnaire, tree)
                .RequireQuestion(QuestionType.MultyOption)
                .RequireQuestionInstanceExists()
                .RequireQuestionIsEnabled()
                .RequireLinkedOptionsAreAvailable(selectedLinkedOptions)
                .RequireMaxAnswersCountLimit(selectedLinkedOptions.Length);
        }

        private static void RequireYesNoPreloadValueAllowed(Identity questionIdentity, YesNoAnswer answer, IQuestionnaire questionnaire, InterviewTree tree)
        {
            int[] selectedValues = answer.CheckedOptions.Select(answeredOption => answeredOption.Value).ToArray();
            var yesAnswersCount = answer.CheckedOptions.Count(answeredOption => answeredOption.Yes);

            new InterviewQuestionInvariants(questionIdentity, questionnaire, tree)
                .RequireQuestion(QuestionType.MultyOption)
                .RequireOptionsExist(selectedValues)
                .RequireRosterSizeAnswerNotNegative(yesAnswersCount)
                .RequireRosterSizeAnswerRespectsMaxRosterRowCount(yesAnswersCount)
                .RequireMaxAnswersCountLimit(yesAnswersCount);
        }

        private static void RequireYesNoAnswerAllowed(Identity questionIdentity, YesNoAnswer answer, IQuestionnaire questionnaire, InterviewTree tree)
        {
            int[] selectedValues = answer.CheckedOptions.Select(answeredOption => answeredOption.Value).ToArray();
            var yesAnswersCount = answer.CheckedOptions.Count(answeredOption => answeredOption.Yes);

            new InterviewQuestionInvariants(questionIdentity, questionnaire, tree)
                .RequireQuestion(QuestionType.MultyOption)
                .RequireOptionsExist(selectedValues)
                .RequireRosterSizeAnswerNotNegative(yesAnswersCount)
                .RequireRosterSizeAnswerRespectsMaxRosterRowCount(yesAnswersCount)
                .RequireMaxAnswersCountLimit(yesAnswersCount)
                .RequireQuestionInstanceExists()
                .RequireQuestionIsEnabled();
        }

        private static void RequireTextListPreloadValueAllowed(Identity questionIdentity, Tuple<decimal, string>[] answers, IQuestionnaire questionnaire, InterviewTree tree)
        {
            new InterviewQuestionInvariants(questionIdentity, questionnaire, tree)
                .RequireQuestion(QuestionType.TextList)
                .RequireRosterSizeAnswerNotNegative(answers.Length)
                .RequireRosterSizeAnswerRespectsMaxRosterRowCount(answers.Length)
                .RequireMaxAnswersCountLimit(answers.Length);
        }

        private static void RequireTextListAnswerAllowed(Identity questionIdentity, Tuple<decimal, string>[] answers, IQuestionnaire questionnaire, InterviewTree tree)
        {
            new InterviewQuestionInvariants(questionIdentity, questionnaire, tree)
                .RequireQuestion(QuestionType.TextList)
                .RequireRosterSizeAnswerNotNegative(answers.Length)
                .RequireRosterSizeAnswerRespectsMaxRosterRowCount(answers.Length)
                .RequireMaxAnswersCountLimit(answers.Length)
                .RequireQuestionInstanceExists()
                .RequireQuestionIsEnabled()
                .RequireUniqueValues(answers)
                .RequireNotEmptyTexts(answers)
                .RequireMaxAnswersCountLimit(answers);
        }

        private static void RequireGpsCoordinatesPreloadValueAllowed(Identity questionIdentity, IQuestionnaire questionnaire, InterviewTree tree)
        {
            new InterviewQuestionInvariants(questionIdentity, questionnaire, tree)
                .RequireQuestion(QuestionType.GpsCoordinates);
        }

        private static void RequireGpsCoordinatesAnswerAllowed(Identity questionIdentity, IQuestionnaire questionnaire, InterviewTree tree)
        {
            new InterviewQuestionInvariants(questionIdentity, questionnaire, tree)
                .RequireQuestion(QuestionType.GpsCoordinates)
                .RequireQuestionInstanceExists()
                .RequireQuestionIsEnabled();
        }

        private static void CheckQRBarcodeInvariants(Identity questionIdentity,
            IQuestionnaire questionnaire, InterviewTree tree, bool applyStrongChecks = true)
        {
            var questionInvariants = new InterviewQuestionInvariants(questionIdentity, questionnaire, tree);

            questionInvariants.RequireQuestion(QuestionType.QRBarcode);

            if (applyStrongChecks)
            {
                questionInvariants.RequireQuestionInstanceExists();
                questionInvariants.RequireQuestionIsEnabled();
            }
        }
    }
}
