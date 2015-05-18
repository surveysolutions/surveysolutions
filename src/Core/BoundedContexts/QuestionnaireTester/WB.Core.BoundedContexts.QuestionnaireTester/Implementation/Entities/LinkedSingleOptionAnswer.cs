using System;
using System.Linq;

namespace WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities
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
            this.IsAnswered = this.Answer.Any();
        }

        public override void RemoveAnswer()
        {
            this.IsAnswered = false;
            this.Answer = null;
        }
    }
}