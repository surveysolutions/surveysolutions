using System;

namespace WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities
{
    public class SingleOptionAnswer : BaseInterviewAnswer
    {
        public decimal Answer { get; private set; }

        public SingleOptionAnswer() { }
        public SingleOptionAnswer(Guid id, decimal[] rosterVector)
            : base(id, rosterVector)
        {
        }

        public void SetAnswer(decimal answer)
        {
            this.Answer = answer;
            this.MarkAnswered();
        }
    }
}