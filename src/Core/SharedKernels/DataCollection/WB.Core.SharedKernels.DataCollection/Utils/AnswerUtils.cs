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
        public static string AnswerToString<T>(T answer, RosterTitleQuestionDescription questionDescription)
        {
            Func<decimal, string> getCategoricalAnswerOptionText =
                questionDescription == null || !questionDescription.Options.Any()
                    ? (Func<decimal, string>) null
                    : value => questionDescription.Options[value];

            return AnswerToString(answer, getCategoricalAnswerOptionText);
        }

        public static string AnswerToString<T>(T answer, Func<decimal, string> getCategoricalAnswerOptionText = null)
        {
            if (typeof (T) == typeof (string))
            {
                return answer as string;
            }

            if (typeof (T) == typeof (int))
            {
                return (Convert.ToInt32(answer)).ToString(CultureInfo.InvariantCulture);
            }

            if (typeof (T) == typeof (decimal))
            {
                var decimalAnswer = Convert.ToDecimal(answer);

                return getCategoricalAnswerOptionText == null
                    ? decimalAnswer.ToString(CultureInfo.InvariantCulture)
                    : getCategoricalAnswerOptionText(decimalAnswer);
            }

            if (typeof (T) == typeof (DateTime))
            {
                return (Convert.ToDateTime(answer)).ToString("M/d/yyyy", CultureInfo.InvariantCulture);
            }

            if (typeof (T) == typeof (GeoPosition))
            {
                var geoAnswer = answer as GeoPosition;
                if (geoAnswer == null)
                    return string.Empty;
                return geoAnswer.ToString();
            }

            if (typeof (T) == typeof (decimal[]))
            {
                var multiAnswer = answer as decimal[];

                return multiAnswer == null || getCategoricalAnswerOptionText == null
                    ? string.Empty
                    : string.Join(", ", multiAnswer.Select(getCategoricalAnswerOptionText));
            }

            return answer.ToString();
        }
    }
}
