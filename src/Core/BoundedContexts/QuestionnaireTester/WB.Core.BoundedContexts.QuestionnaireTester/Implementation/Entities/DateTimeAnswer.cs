using System;

namespace WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities
{
    public class DateTimeAnswer : BaseInterviewAnswer
    {
        public DateTime Answer { get; private set; }

        public DateTimeAnswer() { }
        public DateTimeAnswer(Guid id, decimal[] rosterVector)
            : base(id, rosterVector)
        {
        }

        public void SetAnswer(DateTime answer)
        {
            this.Answer = answer;
            this.MarkAnswered();
        }
    }
}