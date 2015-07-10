using System;

namespace WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities
{
    public class IntegerNumericAnswer : BaseInterviewAnswer
    {
        public virtual int? Answer { get; private set; }

        public IntegerNumericAnswer() { }
        public IntegerNumericAnswer(Guid id, decimal[] rosterVector)
            : base(id, rosterVector)
        {
        }

        public void SetAnswer(int? answer)
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