using System;
using System.Collections.Generic;
using System.Globalization;

namespace Main.Core.Entities.SubEntities
{
    public class Answer
    {
        private decimal? answerCode;
        private decimal? parentCode;

        public string AnswerText { get; set; } = String.Empty;

        public string AnswerValue
        {
            get => answerCode.HasValue ? answerCode.Value.ToString(CultureInfo.InvariantCulture) : String.Empty;
            set => answerCode = string.IsNullOrEmpty(value) 
                ? null 
                : decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal parsedDecimal)
                    ? parsedDecimal
                    : null;
        }

        public string? ParentValue
        {
            get => parentCode.HasValue ? parentCode.Value.ToString(CultureInfo.InvariantCulture) : String.Empty;
            set => parentCode = string.IsNullOrEmpty(value) ? null : decimal.Parse(value, NumberStyles.Number, CultureInfo.InvariantCulture);
        }

        public decimal? AnswerCode
        {
            get => answerCode;// ?? (string.IsNullOrEmpty(AnswerValue) ? null : GetParsedValue());
            set => answerCode = value;
        }

        public decimal? ParentCode
        {
            get => parentCode;// ?? (string.IsNullOrEmpty(ParentValue) ? null : GetParsedParentValue());
            set => parentCode = value;
        }

        public string? AttachmentName { get; set; }

        public Answer Clone()
        {
            var answer = this.MemberwiseClone() as Answer;
            if(answer == null)
                throw new InvalidOperationException("Cloned object is not an Answer.");
            return answer;
        }

        public decimal GetParsedValue()
        {
            return (decimal)this.AnswerCode!;// ?? decimal.Parse(this.AnswerValue, NumberStyles.Number, CultureInfo.InvariantCulture);
        }

        public int? GetParsedParentValue()
        {
            if (this.ParentCode.HasValue)
                return (int) this.ParentCode;

            return null;
            //if (string.IsNullOrEmpty(this.ParentValue)) return null;

            //int.TryParse(this.ParentValue, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsedParentValue);
            //return parsedParentValue;
        }

        public bool HasValue() => AnswerCode.HasValue;// || !string.IsNullOrEmpty(AnswerValue);

        public override bool Equals(object? obj)
        {
            return obj is Answer answer 
                   && EqualityComparer<decimal?>.Default.Equals(
                       HasValue() ? GetParsedValue() : null, 
                       answer.HasValue() ? answer.GetParsedValue() : null) 
                   && EqualityComparer<decimal?>.Default.Equals(GetParsedParentValue(), answer.GetParsedParentValue()) 
                   && AnswerText == answer.AnswerText 
                   && AttachmentName == answer.AttachmentName;
        }

        public override int GetHashCode()
        {
            var hashCode = 1711232258;
            hashCode = hashCode * -1521134295 + EqualityComparer<string?>.Default.GetHashCode(AnswerText);
            hashCode = hashCode * -1521134295 + EqualityComparer<decimal?>.Default.GetHashCode(GetParsedValue());
            
            var parsedParentValue = GetParsedParentValue();
            if (parsedParentValue.HasValue)
                hashCode = hashCode * -1521134295 + EqualityComparer<decimal?>.Default.GetHashCode(parsedParentValue);
            
            var attachmentName = AttachmentName;
            if (attachmentName != null)
                hashCode = hashCode * -1521134295 + EqualityComparer<string?>.Default.GetHashCode(attachmentName);
            
            return hashCode;
        }
    }
}
