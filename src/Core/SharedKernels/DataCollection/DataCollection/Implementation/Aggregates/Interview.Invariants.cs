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
                        RequireQRBarcodePreloadValueAllowed(questionIdentity, questionnaire, tree);
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
                        RequireQRBarcodeAnswerAllowed(questionIdentity, questionnaire, tree);
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
                .RequireTextPreloadValueAllowed();
        }

        private static void RequireTextAnswerAllowed(Identity questionIdentity,
            IQuestionnaire questionnaire, InterviewTree tree)
        {
            new InterviewQuestionInvariants(questionIdentity, questionnaire, tree)
                .RequireTextAnswerAllowed();
        }

        private static void RequireNumericIntegerPreloadValueAllowed(Identity questionIdentity, int answer,
            IQuestionnaire questionnaire, InterviewTree tree)
        {
            new InterviewQuestionInvariants(questionIdentity, questionnaire, tree)
                .RequireNumericIntegerPreloadValueAllowed(answer);
        }

        private static void RequireNumericIntegerAnswerAllowed(Identity questionIdentity, int answer,
            IQuestionnaire questionnaire, InterviewTree tree)
        {
            new InterviewQuestionInvariants(questionIdentity, questionnaire, tree)
                .RequireNumericIntegerAnswerAllowed(answer);
        }

        private static void RequireNumericRealPreloadValueAllowed(Identity questionIdentity,
            IQuestionnaire questionnaire, InterviewTree tree)
        {
            new InterviewQuestionInvariants(questionIdentity, questionnaire, tree)
                .RequireNumericRealPreloadValueAllowed();
        }

        private static void RequireNumericRealAnswerAllowed(Identity questionIdentity, double answer,
            IQuestionnaire questionnaire, InterviewTree tree)
        {
            new InterviewQuestionInvariants(questionIdentity, questionnaire, tree)
                .RequireNumericRealAnswerAllowed(answer);
        }

        private static void RequireDateTimePreloadValueAllowed(Identity questionIdentity,
            IQuestionnaire questionnaire, InterviewTree tree)
        {
            new InterviewQuestionInvariants(questionIdentity, questionnaire, tree)
                .RequireDateTimePreloadValueAllowed();
        }

        private static void RequireDateTimeAnswerAllowed(Identity questionIdentity,
            IQuestionnaire questionnaire, InterviewTree tree)
        {
            new InterviewQuestionInvariants(questionIdentity, questionnaire, tree)
                .RequireDateTimeAnswerAllowed();
        }

        private static void RequireFixedSingleOptionPreloadValueAllowed(Identity questionIdentity, decimal selectedValue,
            IQuestionnaire questionnaire, InterviewTree tree)
        {
            new InterviewQuestionInvariants(questionIdentity, questionnaire, tree)
                .RequireFixedSingleOptionPreloadValueAllowed(selectedValue);
        }

        private void RequireFixedSingleOptionAnswerAllowed(Identity questionIdentity, decimal selectedValue,
            IQuestionnaire questionnaire, InterviewTree tree)
        {
            new InterviewQuestionInvariants(questionIdentity, questionnaire, tree)
                .RequireFixedSingleOptionAnswerAllowed(selectedValue, this.QuestionnaireIdentity);
        }

        private static void RequireLinkedToListSingleOptionAnswerAllowed(Identity questionIdentity, decimal selectedValue,
            IQuestionnaire questionnaire, InterviewTree tree)
        {
            new InterviewQuestionInvariants(questionIdentity, questionnaire, tree)
                .RequireLinkedToListSingleOptionAnswerAllowed(selectedValue);
        }

        private static void RequireLinkedToRosterSingleOptionAnswerAllowed(Identity questionIdentity, decimal[] selectedLinkedOption,
            IQuestionnaire questionnaire, InterviewTree tree)
        {
            new InterviewQuestionInvariants(questionIdentity, questionnaire, tree)
                .RequireLinkedToRosterSingleOptionAnswerAllowed(selectedLinkedOption);
        }

        private static void RequireFixedMultipleOptionsPreloadValueAllowed(Identity questionIdentity, IReadOnlyCollection<int> selectedValues, IQuestionnaire questionnaire, InterviewTree tree)
        {
            new InterviewQuestionInvariants(questionIdentity, questionnaire, tree)
                .RequireFixedMultipleOptionsPreloadValueAllowed(selectedValues);
        }

        private static void RequireFixedMultipleOptionsAnswerAllowed(Identity questionIdentity, IReadOnlyCollection<int> selectedValues, IQuestionnaire questionnaire, InterviewTree tree)
        {
            new InterviewQuestionInvariants(questionIdentity, questionnaire, tree)
                .RequireFixedMultipleOptionsAnswerAllowed(selectedValues);
        }

        private static void RequireLinkedToListMultipleOptionsAnswerAllowed(Identity questionIdentity, IReadOnlyCollection<int> selectedValues, IQuestionnaire questionnaire, InterviewTree tree)
        {
            new InterviewQuestionInvariants(questionIdentity, questionnaire, tree)
                .RequireLinkedToListMultipleOptionsAnswerAllowed(selectedValues);
        }

        private static void RequireLinkedToRosterMultipleOptionsAnswerAllowed(Identity questionIdentity, decimal[][] selectedLinkedOptions, IQuestionnaire questionnaire, InterviewTree tree)
        {
            new InterviewQuestionInvariants(questionIdentity, questionnaire, tree)
                .RequireLinkedToRosterMultipleOptionsAnswerAllowed(selectedLinkedOptions);
        }

        private static void RequireYesNoPreloadValueAllowed(Identity questionIdentity, YesNoAnswer answer, IQuestionnaire questionnaire, InterviewTree tree)
        {
            new InterviewQuestionInvariants(questionIdentity, questionnaire, tree)
                .RequireYesNoPreloadValueAllowed(answer);
        }

        private static void RequireYesNoAnswerAllowed(Identity questionIdentity, YesNoAnswer answer, IQuestionnaire questionnaire, InterviewTree tree)
        {
            new InterviewQuestionInvariants(questionIdentity, questionnaire, tree)
                .RequireYesNoAnswerAllowed(answer);
        }

        private static void RequireTextListPreloadValueAllowed(Identity questionIdentity, Tuple<decimal, string>[] answers, IQuestionnaire questionnaire, InterviewTree tree)
        {
            new InterviewQuestionInvariants(questionIdentity, questionnaire, tree)
                .RequireTextListPreloadValueAllowed(answers);
        }

        private static void RequireTextListAnswerAllowed(Identity questionIdentity, Tuple<decimal, string>[] answers, IQuestionnaire questionnaire, InterviewTree tree)
        {
            new InterviewQuestionInvariants(questionIdentity, questionnaire, tree)
                .RequireTextListAnswerAllowed(answers);
        }

        private static void RequireGpsCoordinatesPreloadValueAllowed(Identity questionIdentity, IQuestionnaire questionnaire, InterviewTree tree)
        {
            new InterviewQuestionInvariants(questionIdentity, questionnaire, tree)
                .RequireGpsCoordinatesPreloadValueAllowed();
        }

        private static void RequireGpsCoordinatesAnswerAllowed(Identity questionIdentity, IQuestionnaire questionnaire, InterviewTree tree)
        {
            new InterviewQuestionInvariants(questionIdentity, questionnaire, tree)
                .RequireGpsCoordinatesAnswerAllowed();
        }

        private static void RequireQRBarcodePreloadValueAllowed(Identity questionIdentity, IQuestionnaire questionnaire, InterviewTree tree)
        {
            new InterviewQuestionInvariants(questionIdentity, questionnaire, tree)
                .RequireQRBarcodePreloadValueAllowed();
        }

        private static void RequireQRBarcodeAnswerAllowed(Identity questionIdentity, IQuestionnaire questionnaire, InterviewTree tree)
        {
            new InterviewQuestionInvariants(questionIdentity, questionnaire, tree)
                .RequireQRBarcodeAnswerAllowed();
        }
    }
}
