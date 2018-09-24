using System.Globalization;

namespace WB.Services.Export.Questionnaire
{
    public class Answer
    {
        public string AnswerText { get; set; }

        public string AnswerValue { get; set; }

        public decimal GetParsedValue()
        {
            return decimal.Parse(this.AnswerValue, NumberStyles.Number, CultureInfo.InvariantCulture);
        }
    }
}
