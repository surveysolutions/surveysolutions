using System;
using System.Globalization;

namespace WB.Services.Export.Questionnaire
{
    public class Answer
    {
        public string AnswerText { get; set; } = String.Empty;

        public string AnswerValue { get; set; } = String.Empty;

        public decimal GetParsedValue()
        {
            return decimal.Parse(this.AnswerValue, NumberStyles.Number, CultureInfo.InvariantCulture);
        }
    }
}
