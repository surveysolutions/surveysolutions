using System;

namespace WB.Core.BoundedContexts.Tester.Implementation.Entities
{
    public class SingleOptionAnswer : BaseInterviewAnswer
    {
        public virtual decimal? Answer { get; private set; }

        public SingleOptionAnswer() { }
        public SingleOptionAnswer(Guid id, decimal[] rosterVector)
            : base(id, rosterVector)
        {
        }

        public void SetAnswer(decimal answer)
        {
            this.Answer = answer;
        }

        public override bool IsAnswered
        {
            get { return Answer.HasValue; }
        }

        public override void RemoveAnswer()
        {
            this.Answer = null;
        }
    }
}