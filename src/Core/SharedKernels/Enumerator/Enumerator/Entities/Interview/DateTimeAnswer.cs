using System;

namespace WB.Core.SharedKernels.Enumerator.Entities.Interview
{
    public class DateTimeAnswer : BaseInterviewAnswer
    {
        public DateTime? Answer { get; private set; }

        public DateTimeAnswer() { }
        public DateTimeAnswer(Guid id, decimal[] rosterVector)
            : base(id, rosterVector)
        {
        }

        public void SetAnswer(DateTime? answer)
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