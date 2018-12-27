using System;
using System.Globalization;

namespace Main.Core.Entities.SubEntities
{
    public class Answer
    {
        public string AnswerText { get; set; }

        public string AnswerValue { get; set; }

        public string ParentValue { get; set; }
       
        public decimal? AnswerCode { get; set; }

        public decimal? ParentCode { get; set; }

        public Answer Clone()
        {
            return this.MemberwiseClone() as Answer;
        }

        public decimal GetParsedValue()
        {
            return this.AnswerCode ?? decimal.Parse(this.AnswerValue, NumberStyles.Number, CultureInfo.InvariantCulture);
        }

        public int? GetParsedParentValue()
        {
            if (this.ParentCode.HasValue)
                return (int) this.ParentCode;

            if (string.IsNullOrEmpty(this.ParentValue)) return null;

            int.TryParse(this.ParentValue, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsedParentValue);
            return parsedParentValue;
        }

        public static Answer CreateFromOther(Answer answer)
        {
            return new Answer
            {
                AnswerText = answer.AnswerText,
                AnswerValue = answer.AnswerValue,
                ParentValue = answer.ParentValue,
                AnswerCode = answer.AnswerCode
            };
        }
    }
}
