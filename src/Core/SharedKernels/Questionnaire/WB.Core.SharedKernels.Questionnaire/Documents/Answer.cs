using System;
using System.Collections.Generic;
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

        public bool HasValue() => AnswerCode.HasValue || !string.IsNullOrEmpty(AnswerValue);

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

        public override bool Equals(object obj)
        {
            return obj is Answer answer &&
                   AnswerText == answer.AnswerText &&
                   EqualityComparer<decimal?>.Default.Equals(GetParsedValue(), answer.GetParsedValue()) &&
                   EqualityComparer<decimal?>.Default.Equals(GetParsedParentValue(), answer.GetParsedParentValue());
        }

        public override int GetHashCode()
        {
            var hashCode = 1711232258;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(AnswerText);
            hashCode = hashCode * -1521134295 + EqualityComparer<decimal?>.Default.GetHashCode(GetParsedValue());
            hashCode = hashCode * -1521134295 + EqualityComparer<decimal?>.Default.GetHashCode(GetParsedParentValue());
            return hashCode;
        }
    }
}
