using System;
using System.Linq;

namespace WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities
{
    public class TextListAnswer : BaseInterviewAnswer
    {
        public virtual Tuple<decimal, string>[] Answers { get; private set; }

        public TextListAnswer() { }
        public TextListAnswer(Guid id, decimal[] rosterVector)
            : base(id, rosterVector)
        {
        }

        public void SetAnswers(Tuple<decimal, string>[] answer)
        {
            this.Answers = answer;
        }

        public override bool IsAnswered
        {
            get { return Answers != null && Answers.Any(); }
        }

        public override void RemoveAnswer()
        {
            this.Answers = null;
        }
    }
}