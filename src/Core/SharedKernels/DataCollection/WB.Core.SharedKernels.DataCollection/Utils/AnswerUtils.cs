using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

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
                return ((DateTime)answer).ToString("M/d/yyyy", CultureInfo.InvariantCulture);

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
                    ? string.Empty
                    : string.Join(", ", multiAnswer.Select(getCategoricalAnswerOptionText));
            }

            if (answer is GeoPosition)
            {
                return ((GeoPosition) answer).ToString();
            }

            return answer.ToString();
        }
    }
}
