using System;

namespace WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities
{
    public class IntegerNumericAnswer : BaseInterviewAnswer
    {
        public int? Answer { get; private set; }

        public IntegerNumericAnswer() { }
        public IntegerNumericAnswer(Guid id, decimal[] rosterVector)
            : base(id, rosterVector)
        {
        }

        public void SetAnswer(int? answer)
        {
            this.Answer = answer;

            if (this.Answer.HasValue)
                this.MarkAnswered();
            else
                this.MarkUnAnswered();
        }
    }
}