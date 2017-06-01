using System;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Services;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Services
{
    public class IdentifyingAnswerConverter : IIdentifyingAnswerConverter
    {
        public AbstractAnswer GetAbstractAnswer(QuestionType questionType, string answer)
        {
            switch (questionType)
            {
                case QuestionType.Text:
                    return TextAnswer.FromString(answer);
                case QuestionType.Numeric:
                    return NumericIntegerAnswer.FromInt(int.Parse(answer));
                case QuestionType.SingleOption:
                    return CategoricalFixedSingleOptionAnswer.FromDecimal(decimal.Parse(answer));
                case QuestionType.DateTime:
                    return DateTimeAnswer.FromDateTime(DateTime.Parse(answer));
                case QuestionType.GpsCoordinates:
                    return GpsAnswer.FromGeoPosition(GeoPosition.FromString(answer));

                default:
                    throw new ArgumentException($"Unknown question of type {questionType} can't be identifier question");
            }
        }
    }
}