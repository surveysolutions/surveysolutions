using System;

namespace WB.Core.SharedKernels.Enumerator.Entities.Interview
{
    public class RealNumericAnswer : BaseInterviewAnswer
    {
        public double? Answer { get; private set; }

        public RealNumericAnswer() { }
        public RealNumericAnswer(Guid id, decimal[] rosterVector)
            : base(id, rosterVector) { }

        public void SetAnswer(double? answer) => this.Answer = answer;

        public override bool IsAnswered => this.Answer.HasValue;

        public override void RemoveAnswer() => this.Answer = null;
    }
}