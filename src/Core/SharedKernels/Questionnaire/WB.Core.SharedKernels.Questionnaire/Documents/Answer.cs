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

        public int GetParsedParentValue()
        {
            decimal parsedParentValue = this.ParentCode ?? decimal.Parse(this.ParentValue, NumberStyles.Number, CultureInfo.InvariantCulture);
            return Convert.ToInt32(parsedParentValue);
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