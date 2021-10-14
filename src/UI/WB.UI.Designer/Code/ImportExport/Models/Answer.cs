using System;
using System.Collections.Generic;
using System.Globalization;

namespace WB.UI.Designer.Code.ImportExport.Models
{
    public class Answer
    {
        public string AnswerText { get; set; } = String.Empty;

        public string AnswerValue { get; set; } = String.Empty;

        public string? ParentValue { get; set; }
       
        public decimal? AnswerCode { get; set; }

        public decimal? ParentCode { get; set; }

        public Answer Clone()
        {
            var answer = this.MemberwiseClone() as Answer;
            if(answer == null)
                throw new InvalidOperationException("Cloned object is not an Answer.");
            return answer;
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
    }
}
