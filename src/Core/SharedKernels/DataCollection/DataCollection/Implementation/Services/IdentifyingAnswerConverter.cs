using System;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Services;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Services
{
    public class IdentifyingAnswerConverter : IIdentifyingAnswerConverter
    {
        public AbstractAnswer GetAbstractAnswer(IQuestionnaire questionnaire, Guid questionId, string answer)
        {
            var questionType = questionnaire.GetQuestionType(questionId);

            switch (questionType)
            {
                case QuestionType.Text:
                    return TextAnswer.FromString(answer);
                case QuestionType.Numeric:
                    if (questionnaire.IsQuestionInteger(questionId))
                        return NumericIntegerAnswer.FromInt(int.Parse(answer));
                    else
                        return NumericRealAnswer.FromDecimal(decimal.Parse(answer));
                case QuestionType.SingleOption:
                    return CategoricalFixedSingleOptionAnswer.FromDecimal(decimal.Parse(answer));
                case QuestionType.DateTime:
                    return DateTimeAnswer.FromDateTime(DateTime.Parse(answer));
                case QuestionType.GpsCoordinates:
                    return GpsAnswer.FromGeoPosition(GeoPosition.FromString(answer));

                default:
                    return null;
                    //throw new ArgumentException($"Unknown question of type {questionType} can't be identifier question");
            }
        }
    }
}