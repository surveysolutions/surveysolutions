using System;
using System.Globalization;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.SharedKernels.Questionnaire.Documents
{
    public static class AnswerExtentions
    {
        public static CategoricalOption? ToCategoricalOption(this Answer answer)
        {
            if (answer == null)
                return null;

            return new CategoricalOption
            {
                Title = answer.AnswerText,
                Value = Convert.ToInt32(answer.GetParsedValue()),
                ParentValue = answer.ParentCode.HasValue ? 
                            Convert.ToInt32(answer.ParentCode):
                            ParseValue(answer.ParentValue),
                AttachmentName = answer.AttachmentName
            };
        }

        private static int? ParseValue(string? stringValue)
        {
            decimal parsedValue;

            if (decimal.TryParse(stringValue, NumberStyles.Number, CultureInfo.InvariantCulture, out parsedValue))
                return Convert.ToInt32(parsedValue);

            return null;
        }
    }
}
