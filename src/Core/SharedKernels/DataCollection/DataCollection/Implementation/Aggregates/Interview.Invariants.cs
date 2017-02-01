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
                        {
                            CheckYesNoQuestionInvariants(questionIdentity, (YesNoAnswer) answer, questionnaire, tree, applyStrongChecks: false);
                        }
                        else
                        {
                            this.CheckMultipleOptionQuestionInvariants(questionIdentity, ((CategoricalFixedMultiOptionAnswer)answer).CheckedValues, questionnaire, tree, isLinkedToList: false, applyStrongChecks: false);
                        }
                        break;
                    case QuestionType.QRBarcode:
                        CheckQRBarcodeInvariants(questionIdentity, questionnaire, tree, applyStrongChecks: false);
                        break;
                    case QuestionType.GpsCoordinates:
                        CheckGpsCoordinatesInvariants(questionIdentity, questionnaire, tree, applyStrongChecks: false);
                        break;
                    case QuestionType.TextList:
                        CheckTextListInvariants(questionIdentity, ((TextListAnswer)answer).ToTupleArray(), questionnaire, tree, applyStrongChecks: false);
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
                        {
                            CheckYesNoQuestionInvariants(new Identity(questionId, currentRosterVector), (YesNoAnswer) answer, questionnaire, tree, applyStrongChecks: true);
                        }
                        else
                        {
                            this.CheckMultipleOptionQuestionInvariants(questionIdentity, ((CategoricalFixedMultiOptionAnswer)answer).CheckedValues, questionnaire, tree, isLinkedToList: false, applyStrongChecks: true);
                        }
                        break;
                    case QuestionType.QRBarcode:
                        CheckQRBarcodeInvariants(questionIdentity, questionnaire, tree, applyStrongChecks: true);
                        break;
                    case QuestionType.GpsCoordinates:
                        CheckGpsCoordinatesInvariants(questionIdentity, questionnaire, tree, applyStrongChecks: true);
                        break;
                    case QuestionType.TextList:
                        CheckTextListInvariants(questionIdentity, ((TextListAnswer)answer).ToTupleArray(), questionnaire, tree, applyStrongChecks: true);
                        break;

                    default:
                        throw new InterviewException(
                            $"Question {questionId} has type {questionType} which is not supported as initial pre-filled question. InterviewId: {this.EventSourceId}");
                }
            }
        }

        private static void CheckLinkedMultiOptionQuestionInvariants(Identity questionIdentity, decimal[][] linkedQuestionSelectedOptions,
            IQuestionnaire questionnaire, InterviewTree tree, bool applyStrongChecks = true)
        {
            var questionInvariants = new InterviewQuestionInvariants(questionIdentity, questionnaire, tree);

            questionInvariants.RequireQuestion(QuestionType.MultyOption);

            if (applyStrongChecks)
            {
                questionInvariants.RequireQuestionInstanceExists();
                questionInvariants.RequireQuestionIsEnabled();
            }

            if (!linkedQuestionSelectedOptions.Any())
                return;

            foreach (var selectedRosterVector in linkedQuestionSelectedOptions)
            {
                questionInvariants.RequireLinkedOptionIsAvailable(selectedRosterVector);
            }

            questionInvariants.RequireMaxAnswersCountLimit(linkedQuestionSelectedOptions.Length);
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
                .RequireRosterSizeAnswerRespectsMaxRosterRowCount(
                    answer,
                    questionnaire.IsQuestionIsRosterSizeForLongRoster(questionIdentity.Id)
                        ? questionnaire.GetMaxLongRosterRowCount()
                        : questionnaire.GetMaxRosterRowCount());
        }

        private static void RequireNumericIntegerAnswerAllowed(Identity questionIdentity, int answer,
            IQuestionnaire questionnaire, InterviewTree tree)
        {
            new InterviewQuestionInvariants(questionIdentity, questionnaire, tree)
                .RequireQuestion(QuestionType.Numeric)
                .RequireNumericIntegerQuestion()
                .RequireRosterSizeAnswerNotNegative(answer)
                .RequireRosterSizeAnswerRespectsMaxRosterRowCount(
                    answer,
                    questionnaire.IsQuestionIsRosterSizeForLongRoster(questionIdentity.Id)
                        ? questionnaire.GetMaxLongRosterRowCount()
                        : questionnaire.GetMaxRosterRowCount())
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

        private void CheckMultipleOptionQuestionInvariants(Identity questionIdentity, IReadOnlyCollection<int> selectedValues,
            IQuestionnaire questionnaire, InterviewTree tree, bool isLinkedToList, bool applyStrongChecks = true)
        {
            var questionInvariants = new InterviewQuestionInvariants(questionIdentity, questionnaire, tree);

            questionInvariants.RequireQuestion(QuestionType.MultyOption);

            if (isLinkedToList)
            {
                foreach (var selectedValue in selectedValues)
                {
                    questionInvariants.RequireLinkedToListOptionIsAvailable(selectedValue);
                }
            }
            else
            {
                questionInvariants.RequireOptionsExist(selectedValues);
            }

            if (questionnaire.IsQuestionYesNo(questionIdentity.Id))
            {
                throw new InterviewException($"Question {questionIdentity.Id} has Yes/No type, but command is sent to Multiopions type. questionnaireId: {this.QuestionnaireId}, interviewId {this.EventSourceId}");
            }

            questionInvariants.RequireRosterSizeAnswerNotNegative(selectedValues.Count);
            var maxSelectedAnswerOptions = questionnaire.GetMaxSelectedAnswerOptions(questionIdentity.Id);
            questionInvariants.RequireRosterSizeAnswerRespectsMaxRosterRowCount(selectedValues.Count, maxSelectedAnswerOptions ?? questionnaire.GetMaxRosterRowCount());

            if (applyStrongChecks)
            {
                questionInvariants.RequireQuestionInstanceExists();
                questionInvariants.RequireMaxAnswersCountLimit(selectedValues.Count);
                questionInvariants.RequireQuestionIsEnabled();
            }
        }

        private static void CheckYesNoQuestionInvariants(Identity questionIdentity, YesNoAnswer answer,
            IQuestionnaire questionnaire, InterviewTree tree, bool applyStrongChecks = true)
        {
            int[] selectedValues = answer.CheckedOptions.Select(answeredOption => answeredOption.Value).ToArray();
            var yesAnswersCount = answer.CheckedOptions.Count(answeredOption => answeredOption.Yes);

            var questionInvariants = new InterviewQuestionInvariants(questionIdentity, questionnaire, tree);

            questionInvariants.RequireQuestion(QuestionType.MultyOption);
            questionInvariants.RequireOptionsExist(selectedValues);

            questionInvariants.RequireRosterSizeAnswerNotNegative(yesAnswersCount);
            var maxSelectedAnswerOptions = questionnaire.GetMaxSelectedAnswerOptions(questionIdentity.Id);
            questionInvariants.RequireRosterSizeAnswerRespectsMaxRosterRowCount(yesAnswersCount, maxSelectedAnswerOptions ?? questionnaire.GetMaxRosterRowCount());

            questionInvariants.RequireMaxAnswersCountLimit(yesAnswersCount);

            if (applyStrongChecks)
            {
                questionInvariants.RequireQuestionInstanceExists();
                questionInvariants.RequireQuestionIsEnabled();
            }
        }

        private static void CheckTextListInvariants(Identity questionIdentity, Tuple<decimal, string>[] answers,
            IQuestionnaire questionnaire, InterviewTree tree, bool applyStrongChecks = true)
        {
            var questionInvariants = new InterviewQuestionInvariants(questionIdentity, questionnaire, tree);

            questionInvariants.RequireQuestion(QuestionType.TextList);

            questionInvariants.RequireRosterSizeAnswerNotNegative(answers.Length);
            var maxSelectedAnswerOptions = questionnaire.GetMaxSelectedAnswerOptions(questionIdentity.Id);
            questionInvariants.RequireRosterSizeAnswerRespectsMaxRosterRowCount(answers.Length, maxSelectedAnswerOptions ?? questionnaire.GetMaxRosterRowCount());

            if (applyStrongChecks)
            {
                questionInvariants.RequireQuestionInstanceExists();
                questionInvariants.RequireQuestionIsEnabled();
                questionInvariants.RequireUniqueValues(answers);
                questionInvariants.RequireNotEmptyTexts(answers);
                var maxAnswersCountLimit = questionnaire.GetListSizeForListQuestion(questionIdentity.Id);
                questionInvariants.RequireMaxAnswersCountLimit(answers, maxAnswersCountLimit);
            }
        }

        private static void CheckGpsCoordinatesInvariants(Identity questionIdentity,
            IQuestionnaire questionnaire, InterviewTree tree, bool applyStrongChecks = true)
        {
            var questionInvariants = new InterviewQuestionInvariants(questionIdentity, questionnaire, tree);

            questionInvariants.RequireQuestion(QuestionType.GpsCoordinates);

            if (applyStrongChecks)
            {
                questionInvariants.RequireQuestionInstanceExists();
                questionInvariants.RequireQuestionIsEnabled();
            }
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
