using System;

namespace WB.Core.BoundedContexts.Tester.Implementation.Entities
{
    public class DateTimeAnswer : BaseInterviewAnswer
    {
        public DateTime? Answer { get; private set; }

        public DateTimeAnswer() { }
        public DateTimeAnswer(Guid id, decimal[] rosterVector)
            : base(id, rosterVector)
        {
        }

        public void SetAnswer(DateTime answer)
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