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
                if (questionDescription == null || !questionDescription.Options.Any())
                    return decimalAnswer.ToString(CultureInfo.InvariantCulture);

                return questionDescription.Options[decimalAnswer];
            }

            if (typeof (T) == typeof (DateTime))
            {
                return (Convert.ToDateTime(answer)).ToUniversalTime().ToShortDateString();
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
                if (multiAnswer == null || questionDescription==null)
                    return string.Empty;

                return string.Join(",", multiAnswer.Select(answerValue => questionDescription.Options[answerValue]));
            }

            return answer.ToString();
        }
    }
}
