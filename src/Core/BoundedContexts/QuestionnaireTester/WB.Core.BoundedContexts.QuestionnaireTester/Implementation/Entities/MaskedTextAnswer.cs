using System;

using WB.Core.GenericSubdomains.Utils;

namespace WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities
{
    public class MaskedTextAnswer : BaseInterviewAnswer
    {
        public string Answer { get; private set; }

        public MaskedTextAnswer() { }
        public MaskedTextAnswer(Guid id, decimal[] rosterVector)
            : base(id, rosterVector)
        {
        }

        public void SetAnswer(string answer)
        {
            this.Answer = answer;

            if (this.Answer.IsNullOrEmpty())
                this.MarkUnAnswered();
            else
                this.MarkAnswered();
        }
    }
}