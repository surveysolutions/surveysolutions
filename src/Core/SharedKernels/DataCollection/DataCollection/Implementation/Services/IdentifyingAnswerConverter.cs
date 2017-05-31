using System;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Services;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Services
{
    public class IdentifyingAnswerConverter : IIdentifyingAnswerConverter
    {
        public AbstractAnswer GetAbstractAnswer(IQuestion question, string answer)
        {
            if (question == null || answer == null)
                return null;

            switch (question.QuestionType)
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
                    throw new ArgumentException(string.Format("Question of type {0} can't be identifier question", question.QuestionType));
            }
        }
    }
}