using System;

namespace WB.Core.SharedKernels.Enumerator.Entities.Interview
{
    public class LinkedSingleOptionAnswer : BaseInterviewAnswer
    {
        public decimal[] Answer { get; private set; }

        public LinkedSingleOptionAnswer() { }
        public LinkedSingleOptionAnswer(Guid id, decimal[] rosterVector)
            : base(id, rosterVector)
        {
        }

        public void SetAnswer(decimal[] answer)
        {
            this.Answer = answer;
        }

        public override bool IsAnswered
        {
            get { return this.Answer != null; }
        }

        public override void RemoveAnswer()
        {
            this.Answer = null;
        }
    }
}