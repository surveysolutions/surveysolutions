using System;
using System.Globalization;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Views.Interview;

namespace WB.Core.SharedKernels.DataCollection.Utils
{
    public static class AnswerUtils
    {
        public static string AnswerToString(object answer, Func<decimal, string> getCategoricalAnswerOptionText = null)
        {
            if (answer == null)
                return string.Empty;

            if (answer is string)
                return (string) answer;

            if (answer is int)
                return ((int) answer).ToString(CultureInfo.InvariantCulture);

            if (answer is DateTime)
                return ((DateTime)answer).ToString(CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern, CultureInfo.InvariantCulture);

            if (answer is decimal)
            {
                var decimalAnswer = (decimal) answer;

                return getCategoricalAnswerOptionText == null
                    ? decimalAnswer.ToString(CultureInfo.InvariantCulture)
                    : getCategoricalAnswerOptionText(decimalAnswer);
            }

            if (answer is decimal[])
            {
                var multiAnswer = (decimal[]) answer;

                return getCategoricalAnswerOptionText == null
                    ? string.Join(", ", multiAnswer)
                    : string.Join(", ", multiAnswer.Select(getCategoricalAnswerOptionText));
            }
            if (answer is AnsweredYesNoOption[])
            {
                var yesNoAnswer = (AnsweredYesNoOption[])answer;
                var yesAnsweredValues = yesNoAnswer.Where(a => a.Yes).Select(a => a.OptionValue);

                return getCategoricalAnswerOptionText == null
                    ? string.Join(", ", yesAnsweredValues)
                    : string.Join(", ", yesAnsweredValues.Select(getCategoricalAnswerOptionText));
            }
            if (answer is decimal[][])
            {
                var multiLinkAnswer = (decimal[][])answer;
                return string.Join("|", multiLinkAnswer.Select(a => string.Join(", ", a)));
            }
            if (answer is GeoPosition)
            {
                var geoAnswer = answer as GeoPosition;
                return string.Format(CultureInfo.InvariantCulture, "[{0};{1}]", geoAnswer.Latitude, geoAnswer.Longitude);
                //return ((GeoPosition) answer).ToString();
            }

            if (answer is GeoLocationPoint)
            {
                var geoAnswer = answer as GeoLocationPoint;
                return string.Format(CultureInfo.InvariantCulture, "[{0};{1}]", geoAnswer.Latitude, geoAnswer.Longitude);
            }
            if (answer is InterviewTextListAnswers)
            {
                return string.Join("|", ((InterviewTextListAnswers) answer).Answers.Select(x => x.Answer));
            }
            if (answer is Tuple<decimal, string>[])
            {
                var answers = answer as Tuple<decimal, string>[];
                var selectedValues = answers.Select(x => x.Item1).ToArray();
                return AnswerToString(selectedValues, answerOptionValue => answers.Single(x => x.Item1 == answerOptionValue).Item2);
            }

            return answer.ToString();
        }
    }
}
