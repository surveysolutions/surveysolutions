using System;

using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities
{
    public class TextAnswer : BaseInterviewAnswer
    {
        public string Answer { get; private set; }

        public TextAnswer() { }
        public TextAnswer(Guid id, decimal[] rosterVector)
            : base(id, rosterVector)
        {
        }

        public void SetAnswer(string answer)
        {
            this.Answer = answer;
        }

        public override bool IsAnswered
        {
            get { return !this.Answer.IsNullOrEmpty(); }
        }

        public override void RemoveAnswer()
        {
            this.Answer = null;
        }
    }
}