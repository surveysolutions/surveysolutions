using System;

namespace WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities
{
    public class SingleOptionAnswer : BaseInterviewAnswer
    {
        public virtual decimal Answer { get; private set; }

        public SingleOptionAnswer() { }
        public SingleOptionAnswer(Guid id, decimal[] rosterVector)
            : base(id, rosterVector)
        {
        }

        public void SetAnswer(decimal answer)
        {
            this.Answer = answer;
            this.IsAnswered = true;
        }

        public override void RemoveAnswer()
        {
            this.IsAnswered = false;
            this.Answer = default(decimal);
        }
    }
}