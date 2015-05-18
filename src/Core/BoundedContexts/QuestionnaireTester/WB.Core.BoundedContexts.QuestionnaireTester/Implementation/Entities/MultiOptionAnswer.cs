using System;
using System.Linq;

namespace WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities
{
    public class MultiOptionAnswer : BaseInterviewAnswer
    {
        public decimal[] Answers { get; private set; }

        public MultiOptionAnswer() { }
        public MultiOptionAnswer(Guid id, decimal[] rosterVector)
            : base(id, rosterVector)
        {
        }

        public void SetAnswers(decimal[] answer)
        {
            this.Answers = answer;
            this.IsAnswered = this.Answers.Any();
        }

        public override void RemoveAnswer()
        {
            this.IsAnswered = false;
            this.Answers = null;
        }
    }
}