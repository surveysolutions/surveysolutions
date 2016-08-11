using System;
using System.Globalization;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.SharedKernels.Questionnaire.Documents
{
    public static class AnswerExtentions
    {
        public static CategoricalOption ToCategoricalOption(this Answer answer)
        {
            if (answer == null)
                return null;

            return new CategoricalOption()
            {
                Title = answer.AnswerText,
                Value = answer.AnswerCode.HasValue ? 
                            Convert.ToInt32(answer.AnswerCode.Value) : 
                            Convert.ToInt32(decimal.Parse(answer.AnswerValue, NumberStyles.Number, CultureInfo.InvariantCulture)),
                ParentValue = answer.ParentCode.HasValue ? 
                            Convert.ToInt32(answer.ParentCode):
                            ParseValue(answer.ParentValue)
            };
        }

        private static int? ParseValue(string stringValue)
        {
            decimal parsedValue;

            if (decimal.TryParse(stringValue, NumberStyles.Number, CultureInfo.InvariantCulture, out parsedValue))
                return Convert.ToInt32(parsedValue);

            return (int?) null;
        }
    }
}
