using Main.Core.Domain.Exceptions;

namespace Main.Core.Entities.SubEntities.Complete.Question
{
    using System;
    using System.Collections.Generic;

    public sealed class NumericCompleteQuestion : AbstractCompleteQuestion, INumericQuestion,
        ICompelteValueQuestion<double?>
    {
        public NumericCompleteQuestion()
            : base() {}

        public NumericCompleteQuestion(string text)
            : base(text) {}

        public override void AddAnswer(IAnswer answer)
        {
            throw new NotImplementedException();
        }

        public override object GetAnswerObject()
        {
            return this.Answer;
        }

        public override bool IsAnswered()
        {
            return this.Answer != null;
        }

        public override string GetAnswerString()
        {
            return this.Answer.HasValue ? this.Answer.Value.ToString() : string.Empty;
        }

        public override void SetAnswer(List<Guid> answer, string answerValue)
        {
            if (string.IsNullOrWhiteSpace(answerValue))
            {
                this.Answer = null;
            }
            else
            {
                double value;
                if (double.TryParse(answerValue.Trim(), out value))
                {
                    this.Answer = value;
                }
            }
        }

        public override void ThrowDomainExceptionIfAnswerInvalid(List<Guid> answerKeys, string answerValue)
        {
            if (string.IsNullOrWhiteSpace(answerValue))
            {
                return;
            }
            double value;
            if (!double.TryParse(answerValue.Trim(), out value))
            {
                throw new InterviewException("value must be numeric");
            }

        }

        public double? Answer { get; set; }

        public bool IsInteger { get; set; }
    }
}