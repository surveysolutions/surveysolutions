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

                var questionInvariants = new InterviewQuestionInvariants(questionIdentity, questionnaire, tree);

                switch (questionType)
                {
                    case QuestionType.Text:
                        questionInvariants.RequireTextPreloadValueAllowed();
                        break;

                    case QuestionType.Numeric:
                        if (questionnaire.IsQuestionInteger(questionId))
                            questionInvariants.RequireNumericIntegerPreloadValueAllowed(((NumericIntegerAnswer) answer).Value);
                        else
                            questionInvariants.RequireNumericRealPreloadValueAllowed();
                        break;

                    case QuestionType.DateTime:
                        questionInvariants.RequireDateTimePreloadValueAllowed();
                        break;

                    case QuestionType.SingleOption:
                        questionInvariants.RequireFixedSingleOptionPreloadValueAllowed(((CategoricalFixedSingleOptionAnswer) answer).SelectedValue);
                        break;

                    case QuestionType.MultyOption:
                        if (questionnaire.IsQuestionYesNo(questionId))
                            questionInvariants.RequireYesNoPreloadValueAllowed((YesNoAnswer) answer);
                        else
                            questionInvariants.RequireFixedMultipleOptionsPreloadValueAllowed(((CategoricalFixedMultiOptionAnswer) answer).CheckedValues);
                        break;

                    case QuestionType.QRBarcode:
                        questionInvariants.RequireQRBarcodePreloadValueAllowed();
                        break;

                    case QuestionType.GpsCoordinates:
                        questionInvariants.RequireGpsCoordinatesPreloadValueAllowed();
                        break;

                    case QuestionType.TextList:
                        questionInvariants.RequireTextListPreloadValueAllowed(((TextListAnswer)answer).ToTupleArray());
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

                var questionInvariants = new InterviewQuestionInvariants(questionIdentity, questionnaire, tree);

                switch (questionType)
                {
                    case QuestionType.Text:
                        questionInvariants.RequireTextAnswerAllowed();
                        break;

                    case QuestionType.Numeric:
                        if (questionnaire.IsQuestionInteger(questionId))
                            questionInvariants.RequireNumericIntegerAnswerAllowed(((NumericIntegerAnswer) answer).Value);
                        else
                            questionInvariants.RequireNumericRealAnswerAllowed(((NumericRealAnswer) answer).Value);
                        break;

                    case QuestionType.DateTime:
                        questionInvariants.RequireDateTimeAnswerAllowed();
                        break;

                    case QuestionType.SingleOption:
                        questionInvariants.RequireFixedSingleOptionAnswerAllowed(((CategoricalFixedSingleOptionAnswer) answer).SelectedValue, this.QuestionnaireIdentity);
                        break;

                    case QuestionType.MultyOption:
                        if (questionnaire.IsQuestionYesNo(questionId))
                            questionInvariants.RequireYesNoAnswerAllowed((YesNoAnswer) answer);
                        else
                            questionInvariants.RequireFixedMultipleOptionsAnswerAllowed(((CategoricalFixedMultiOptionAnswer) answer).CheckedValues);
                        break;

                    case QuestionType.QRBarcode:
                        questionInvariants.RequireQRBarcodeAnswerAllowed();
                        break;

                    case QuestionType.GpsCoordinates:
                        questionInvariants.RequireGpsCoordinatesAnswerAllowed();
                        break;

                    case QuestionType.TextList:
                        questionInvariants.RequireTextListAnswerAllowed(((TextListAnswer)answer).ToTupleArray());
                        break;

                    default:
                        throw new InterviewException(
                            $"Question {questionId} has type {questionType} which is not supported as initial pre-filled question. InterviewId: {this.EventSourceId}");
                }
            }
        }
    }
}
