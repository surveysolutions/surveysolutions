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
                        this.RequireTextPreloadValueAllowed(questionId, questionnaire);
                        break;

                    case QuestionType.Numeric:
                        if (questionnaire.IsQuestionInteger(questionId))
                            this.RequireNumericIntegerPreloadValueAllowed(questionId, ((NumericIntegerAnswer) answer).Value, questionnaire);
                        else
                            this.RequireNumericRealPreloadValueAllowed(questionId, questionnaire);
                        break;

                    case QuestionType.DateTime:
                        this.RequireDateTimePreloadValueAllowed(questionId, questionnaire);
                        break;

                    case QuestionType.SingleOption:
                        this.RequireFixedSingleOptionPreloadValueAllowed(questionId, ((CategoricalFixedSingleOptionAnswer) answer).SelectedValue, questionnaire);
                        break;

                    case QuestionType.MultyOption:
                        if (questionnaire.IsQuestionYesNo(questionId))
                        {
                            this.CheckYesNoQuestionInvariants(new Identity(questionId, currentRosterVector), (YesNoAnswer) answer, questionnaire, tree, applyStrongChecks: false);
                        }
                        else
                        {
                            this.CheckMultipleOptionQuestionInvariants(questionIdentity, ((CategoricalFixedMultiOptionAnswer)answer).CheckedValues, questionnaire, tree, isLinkedToList: false, applyStrongChecks: false);
                        }
                        break;
                    case QuestionType.QRBarcode:
                        this.CheckQRBarcodeInvariants(questionIdentity, questionnaire, tree, applyStrongChecks: false);
                        break;
                    case QuestionType.GpsCoordinates:
                        this.CheckGpsCoordinatesInvariants(questionIdentity, questionnaire, tree, applyStrongChecks: false);
                        break;
                    case QuestionType.TextList:
                        this.CheckTextListInvariants(questionIdentity, ((TextListAnswer)answer).ToTupleArray(), questionnaire, tree, applyStrongChecks: false);
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
                        this.RequireTextAnswerAllowed(questionIdentity, questionnaire, tree);
                        break;

                    case QuestionType.Numeric:
                        if (questionnaire.IsQuestionInteger(questionId))
                            this.RequireNumericIntegerAnswerAllowed(questionIdentity, ((NumericIntegerAnswer) answer).Value, questionnaire, tree);
                        else
                            this.RequireNumericRealAnswerAllowed(questionIdentity, ((NumericRealAnswer) answer).Value, questionnaire, tree);
                        break;

                    case QuestionType.DateTime:
                        this.RequireDateTimeAnswerAllowed(questionIdentity, questionnaire, tree);
                        break;

                    case QuestionType.SingleOption:
                        this.RequireFixedSingleOptionAnswerAllowed(questionIdentity, ((CategoricalFixedSingleOptionAnswer) answer).SelectedValue, questionnaire, tree);
                        break;

                    case QuestionType.MultyOption:
                        if (questionnaire.IsQuestionYesNo(questionId))
                        {
                            this.CheckYesNoQuestionInvariants(new Identity(questionId, currentRosterVector), (YesNoAnswer) answer, questionnaire, tree, applyStrongChecks: true);
                        }
                        else
                        {
                            this.CheckMultipleOptionQuestionInvariants(questionIdentity, ((CategoricalFixedMultiOptionAnswer)answer).CheckedValues, questionnaire, tree, isLinkedToList: false, applyStrongChecks: true);
                        }
                        break;
                    case QuestionType.QRBarcode:
                        this.CheckQRBarcodeInvariants(questionIdentity, questionnaire, tree, applyStrongChecks: true);
                        break;
                    case QuestionType.GpsCoordinates:
                        this.CheckGpsCoordinatesInvariants(questionIdentity, questionnaire, tree, applyStrongChecks: true);
                        break;
                    case QuestionType.TextList:
                        this.CheckTextListInvariants(questionIdentity, ((TextListAnswer)answer).ToTupleArray(), questionnaire, tree, applyStrongChecks: true);
                        break;

                    default:
                        throw new InterviewException(
                            $"Question {questionId} has type {questionType} which is not supported as initial pre-filled question. InterviewId: {this.EventSourceId}");
                }
            }
        }

        private void CheckLinkedMultiOptionQuestionInvariants(Identity questionIdentity, decimal[][] linkedQuestionSelectedOptions, IQuestionnaire questionnaire, InterviewTree tree, bool applyStrongChecks = true)
        {
            var treeInvariants = new InterviewTreeInvariants(questionIdentity, tree);
            var questionInvariants = new InterviewQuestionInvariants(this.properties.Id, questionIdentity.Id, questionnaire);

            questionInvariants.RequireQuestion(QuestionType.MultyOption);

            if (applyStrongChecks)
            {
                treeInvariants.RequireQuestionInstanceExists();
                treeInvariants.RequireQuestionIsEnabled();
            }

            if (!linkedQuestionSelectedOptions.Any())
                return;

            foreach (var selectedRosterVector in linkedQuestionSelectedOptions)
            {
                treeInvariants.RequireLinkedOptionIsAvailable(selectedRosterVector);
            }

            questionInvariants.RequireMaxAnswersCountLimit(linkedQuestionSelectedOptions.Length);
        }

        private void RequireTextPreloadValueAllowed(Guid questionId, IQuestionnaire questionnaire)
        {
            new InterviewQuestionInvariants(this.properties.Id, questionId, questionnaire)
                .RequireQuestion(QuestionType.Text);
        }

        private void RequireTextAnswerAllowed(Identity questionIdentity, IQuestionnaire questionnaire, InterviewTree tree)
        {
            new InterviewQuestionInvariants(this.properties.Id, questionIdentity.Id, questionnaire)
                .RequireQuestion(QuestionType.Text);

            new InterviewTreeInvariants(questionIdentity, tree)
                .RequireQuestionInstanceExists()
                .RequireQuestionIsEnabled();
        }

        private void RequireNumericIntegerPreloadValueAllowed(Guid questionId, int answer, IQuestionnaire questionnaire)
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

        private void RequireNumericIntegerAnswerAllowed(Identity questionIdentity, int answer, IQuestionnaire questionnaire, InterviewTree tree)
        {
            new InterviewQuestionInvariants(this.properties.Id, questionIdentity.Id, questionnaire)
                .RequireQuestion(QuestionType.Numeric)
                .RequireNumericIntegerQuestion()
                .RequireRosterSizeAnswerNotNegative(answer)
                .RequireRosterSizeAnswerRespectsMaxRosterRowCount(
                    answer,
                    questionnaire.IsQuestionIsRosterSizeForLongRoster(questionIdentity.Id)
                        ? questionnaire.GetMaxLongRosterRowCount()
                        : questionnaire.GetMaxRosterRowCount());

            new InterviewTreeInvariants(questionIdentity, tree)
                .RequireQuestionInstanceExists()
                .RequireQuestionIsEnabled();
        }

        private void RequireNumericRealPreloadValueAllowed(Guid questionId, IQuestionnaire questionnaire)
        {
            new InterviewQuestionInvariants(this.properties.Id, questionId, questionnaire)
                .RequireQuestion(QuestionType.Numeric)
                .RequireNumericRealQuestion();
        }

        private void RequireNumericRealAnswerAllowed(Identity questionIdentity, double answer, IQuestionnaire questionnaire, InterviewTree tree)
        {
            new InterviewQuestionInvariants(this.properties.Id, questionIdentity.Id, questionnaire)
                .RequireQuestion(QuestionType.Numeric)
                .RequireNumericRealQuestion()
                .RequireAllowedDecimalPlaces(answer);

            new InterviewTreeInvariants(questionIdentity, tree)
                .RequireQuestionInstanceExists()
                .RequireQuestionIsEnabled();
        }

        private void RequireDateTimePreloadValueAllowed(Guid questionId, IQuestionnaire questionnaire)
        {
            new InterviewQuestionInvariants(this.properties.Id, questionId, questionnaire)
                .RequireQuestion(QuestionType.DateTime);
        }

        private void RequireDateTimeAnswerAllowed(Identity questionIdentity, IQuestionnaire questionnaire, InterviewTree tree)
        {
            new InterviewQuestionInvariants(this.properties.Id, questionIdentity.Id, questionnaire)
                .RequireQuestion(QuestionType.DateTime);

            new InterviewTreeInvariants(questionIdentity, tree)
                .RequireQuestionInstanceExists()
                .RequireQuestionIsEnabled();
        }

        private void RequireFixedSingleOptionPreloadValueAllowed(Guid questionId, decimal selectedValue, IQuestionnaire questionnaire)
        {
            new InterviewQuestionInvariants(this.properties.Id, questionId, questionnaire)
                .RequireQuestion(QuestionType.SingleOption)
                .RequireOptionExists(selectedValue);
        }

        private void RequireFixedSingleOptionAnswerAllowed(Identity questionIdentity, decimal selectedValue, IQuestionnaire questionnaire, InterviewTree tree)
        {
            new InterviewQuestionInvariants(this.properties.Id, questionIdentity.Id, questionnaire)
                .RequireQuestion(QuestionType.SingleOption)
                .RequireOptionExists(selectedValue);

            new InterviewTreeInvariants(questionIdentity, tree)
                .RequireQuestionInstanceExists()
                .RequireQuestionIsEnabled()
                .RequireCascadingQuestionAnswerCorrespondsToParentAnswer(selectedValue, this.QuestionnaireIdentity, questionnaire.Translation);
        }

        private void RequireLinkedToListSingleOptionAnswerAllowed(Identity questionIdentity, decimal selectedValue, IQuestionnaire questionnaire, InterviewTree tree)
        {
            new InterviewQuestionInvariants(this.properties.Id, questionIdentity.Id, questionnaire)
                .RequireQuestion(QuestionType.SingleOption);

            new InterviewTreeInvariants(questionIdentity, tree)
                .RequireQuestionInstanceExists()
                .RequireQuestionIsEnabled()
                .RequireLinkedToListOptionIsAvailable(selectedValue);
        }

        private void RequireLinkedToRosterSingleOptionAnswerAllowed(Identity questionIdentity, decimal[] selectedLinkedOption, IQuestionnaire questionnaire, InterviewTree tree)
        {
            new InterviewQuestionInvariants(this.properties.Id, questionIdentity.Id, questionnaire)
                .RequireQuestion(QuestionType.SingleOption);

            new InterviewTreeInvariants(questionIdentity, tree)
                .RequireQuestionInstanceExists()
                .RequireQuestionIsEnabled()
                .RequireLinkedOptionIsAvailable(selectedLinkedOption);
        }

        private void CheckMultipleOptionQuestionInvariants(Identity questionIdentity, IReadOnlyCollection<int> selectedValues, IQuestionnaire questionnaire, InterviewTree tree, bool isLinkedToList, bool applyStrongChecks = true)
        {
            var treeInvariants = new InterviewTreeInvariants(questionIdentity, tree);
            var questionInvariants = new InterviewQuestionInvariants(this.properties.Id, questionIdentity.Id, questionnaire);

            questionInvariants.RequireQuestion(QuestionType.MultyOption);

            if (isLinkedToList)
            {
                foreach (var selectedValue in selectedValues)
                {
                    treeInvariants.RequireLinkedToListOptionIsAvailable(selectedValue);
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
                treeInvariants.RequireQuestionInstanceExists();
                questionInvariants.RequireMaxAnswersCountLimit(selectedValues.Count);
                treeInvariants.RequireQuestionIsEnabled();
            }
        }

        private void CheckYesNoQuestionInvariants(Identity questionIdentity, YesNoAnswer answer, IQuestionnaire questionnaire, InterviewTree tree, bool applyStrongChecks = true)
        {
            int[] selectedValues = answer.CheckedOptions.Select(answeredOption => answeredOption.Value).ToArray();
            var yesAnswersCount = answer.CheckedOptions.Count(answeredOption => answeredOption.Yes);

            var treeInvariants = new InterviewTreeInvariants(questionIdentity, tree);
            var questionInvariants = new InterviewQuestionInvariants(this.properties.Id, questionIdentity.Id, questionnaire);

            questionInvariants.RequireQuestion(QuestionType.MultyOption);
            questionInvariants.RequireOptionsExist(selectedValues);

            questionInvariants.RequireRosterSizeAnswerNotNegative(yesAnswersCount);
            var maxSelectedAnswerOptions = questionnaire.GetMaxSelectedAnswerOptions(questionIdentity.Id);
            questionInvariants.RequireRosterSizeAnswerRespectsMaxRosterRowCount(yesAnswersCount, maxSelectedAnswerOptions ?? questionnaire.GetMaxRosterRowCount());

            questionInvariants.RequireMaxAnswersCountLimit(yesAnswersCount);

            if (applyStrongChecks)
            {
                treeInvariants.RequireQuestionInstanceExists();
                treeInvariants.RequireQuestionIsEnabled();
            }
        }

        private void CheckTextListInvariants(Identity questionIdentity, Tuple<decimal, string>[] answers, IQuestionnaire questionnaire, InterviewTree tree, bool applyStrongChecks = true)
        {
            var treeInvariants = new InterviewTreeInvariants(questionIdentity, tree);
            var questionInvariants = new InterviewQuestionInvariants(this.properties.Id, questionIdentity.Id, questionnaire);

            questionInvariants.RequireQuestion(QuestionType.TextList);

            questionInvariants.RequireRosterSizeAnswerNotNegative(answers.Length);
            var maxSelectedAnswerOptions = questionnaire.GetMaxSelectedAnswerOptions(questionIdentity.Id);
            questionInvariants.RequireRosterSizeAnswerRespectsMaxRosterRowCount(answers.Length, maxSelectedAnswerOptions ?? questionnaire.GetMaxRosterRowCount());

            if (applyStrongChecks)
            {
                treeInvariants.RequireQuestionInstanceExists();
                treeInvariants.RequireQuestionIsEnabled();
                questionInvariants.RequireUniqueValues(answers);
                questionInvariants.RequireNotEmptyTexts(answers);
                var maxAnswersCountLimit = questionnaire.GetListSizeForListQuestion(questionIdentity.Id);
                questionInvariants.RequireMaxAnswersCountLimit(answers, maxAnswersCountLimit);
            }
        }

        private void CheckGpsCoordinatesInvariants(Identity questionIdentity, IQuestionnaire questionnaire, InterviewTree tree, bool applyStrongChecks = true)
        {
            var treeInvariants = new InterviewTreeInvariants(questionIdentity, tree);

            new InterviewQuestionInvariants(this.properties.Id, questionIdentity.Id, questionnaire)
                .RequireQuestion(QuestionType.GpsCoordinates);

            if (applyStrongChecks)
            {
                treeInvariants.RequireQuestionInstanceExists();
                treeInvariants.RequireQuestionIsEnabled();
            }
        }

        private void CheckQRBarcodeInvariants(Identity questionIdentity, IQuestionnaire questionnaire, InterviewTree tree, bool applyStrongChecks = true)
        {
            var treeInvariants = new InterviewTreeInvariants(questionIdentity, tree);

            new InterviewQuestionInvariants(this.properties.Id, questionIdentity.Id, questionnaire)
                .RequireQuestion(QuestionType.QRBarcode);

            if (applyStrongChecks)
            {
                treeInvariants.RequireQuestionInstanceExists();
                treeInvariants.RequireQuestionIsEnabled();
            }
        }
    }
}
