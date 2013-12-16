using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace WB.Core.BoundedContexts.Capi.ModelUtils
{
    public static class QuestionUtils
    {
        public static decimal[] ExtractSelectedOptions(object answer)
        {
            return CastAnswerToSingleDimensionalArray<decimal>(answer, decimal.TryParse) ?? CastAnswerToDecimal(answer);
        }

        public static decimal[][] ExtractSelectedOptionsOfLinkedQuestion(object answer)
        {
            if (answer == null)
                return null;

            var selectedAnswer = CastAnswerToSingleDimensionalArray<decimal>(answer, decimal.TryParse);
            if (selectedAnswer != null)
                return new decimal[][] { selectedAnswer };

            decimal[][] typedAnswers = CastAnswerTo2DimensionalArray(answer);
            if (typedAnswers != null)
                return typedAnswers;

            return new decimal[0][];
        }

        private static T[] CastAnswerToSingleDimensionalArray<T>(object answer, TryParseDelegate<T> tryParseFunction) where T : struct 
        {
            var decimalCast = answer as IEnumerable<T>;
            if (decimalCast != null)
                return decimalCast.ToArray();

            var objectCast = CastAnswerFormObjectToIntArray<T>(answer, tryParseFunction);
            if (objectCast != null)
                return objectCast;

            var jArrayCast = GetValueFromJArray<T>(answer);
            if (jArrayCast.Length > 0)
                return jArrayCast;
            return null;
        }

        private static T[] CastAnswerFormObjectToIntArray<T>(object answer, TryParseDelegate<T> tryParseFunction) where T : struct
        {
            var objectCast = answer as IEnumerable<object>;
            if (objectCast == null)
                return null;

            var result =
                objectCast.Select(obj => ConvertObjectToAnswer<T>(obj, tryParseFunction)).Where(a => a.HasValue).Select(a => a.Value).ToArray();
            if (result.Length == 0)
                return null;

            return result;
        }

        private static T? ConvertObjectToAnswer<T>(object answer, TryParseDelegate<T> tryParseFunction) where T : struct 
        {
            if (answer == null)
                return null;
            T value;
            if (tryParseFunction(answer.ToString(), out value))
                return value;

            return null;
        }

        private static decimal[] CastAnswerToDecimal(object answer)
        {
            var decimalAnswer = ConvertObjectToAnswer<decimal>(answer, decimal.TryParse);
            if (decimalAnswer.HasValue)
                return new decimal[] { decimalAnswer.Value };
            return null;
        }

        private static decimal[][] CastAnswerTo2DimensionalArray(object answer)
        {
            var intCast = answer as IEnumerable<decimal[]>;
            if (intCast != null)
                return intCast.ToArray();

            var objectCast = answer as IEnumerable<object>;
            if (objectCast != null)
            {
                var result =
                    objectCast.Select((obj) => CastAnswerFormObjectToIntArray<decimal>(obj, decimal.TryParse)).Where(i => i != null).ToArray();
             
                if (result.Length > 0)
                    return result;
            }

            var jArrayCast = GetValueFromJArray<decimal[]>(answer);
            if (jArrayCast.Length > 0)
                return jArrayCast;
            return null;
        }

        private static T[] GetValueFromJArray<T>(object answer)
        {
            try
            {
                return ((JArray)answer).ToObject<T[]>();
            }
            catch (Exception)
            {
                return new T[0];
            }
        }
    }

    internal delegate bool TryParseDelegate<T>(string answerString, out T value) where T : struct;
}
