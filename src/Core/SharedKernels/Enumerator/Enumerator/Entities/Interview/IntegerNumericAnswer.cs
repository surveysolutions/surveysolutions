using System;

namespace WB.Core.SharedKernels.Enumerator.Entities.Interview
{
    public class IntegerNumericAnswer : BaseInterviewAnswer
    {
        public virtual int? Answer { get; private set; }

        public IntegerNumericAnswer() { }
        public IntegerNumericAnswer(Guid id, decimal[] rosterVector)
            : base(id, rosterVector)
        {
        }

        public void SetAnswer(int? answer)
        {
            this.Answer = answer;
        }

        public override bool IsAnswered
        {
            get { return this.Answer.HasValue; }
        }

        public override void RemoveAnswer()
        {
            this.Answer = null;
        }
    }
}