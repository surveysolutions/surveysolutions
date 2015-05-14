using System;

namespace WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities
{
    public class RealNumericAnswer : BaseInterviewAnswer
    {
        public decimal? Answer { get; private set; }

        public RealNumericAnswer() { }
        public RealNumericAnswer(Guid id, decimal[] rosterVector)
            : base(id, rosterVector)
        {
        }

        public void SetAnswer(decimal? answer)
        {
            this.Answer = answer;

            if (this.Answer.HasValue)
                this.MarkAnswered();
            else
                this.MarkUnAnswered();
        }
    }
}