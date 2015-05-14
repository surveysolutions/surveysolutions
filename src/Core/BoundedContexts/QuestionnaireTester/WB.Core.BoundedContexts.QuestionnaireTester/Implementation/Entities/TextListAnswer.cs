using System;
using System.Linq;

namespace WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities
{
    public class TextListAnswer : BaseInterviewAnswer
    {
        public Tuple<decimal, string>[] Answers { get; private set; }

        public TextListAnswer() { }
        public TextListAnswer(Guid id, decimal[] rosterVector)
            : base(id, rosterVector)
        {
        }

        public void SetAnswers(Tuple<decimal, string>[] answer)
        {
            this.Answers = answer;

            if (this.Answers.Any())
                this.MarkUnAnswered();
            else
                this.MarkAnswered();
        }
    }
}