using System;

namespace WB.Core.SharedKernels.Enumerator.Entities.Interview
{
    public class SingleOptionAnswer : BaseInterviewAnswer
    {
        public virtual decimal? Answer { get; private set; }

        public SingleOptionAnswer() { }
        public SingleOptionAnswer(Guid id, decimal[] rosterVector)
            : base(id, rosterVector)
        {
        }

        public void SetAnswer(decimal? answer)
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