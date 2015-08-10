using System;
using System.Linq;

namespace WB.Core.SharedKernels.Enumerator.Entities.Interview
{
    public class LinkedMultiOptionAnswer : BaseInterviewAnswer
    {
        public decimal[][] Answers { get; private set; }

        public LinkedMultiOptionAnswer() { }
        public LinkedMultiOptionAnswer(Guid id, decimal[] rosterVector)
            : base(id, rosterVector)
        {
        }

        public void SetAnswers(decimal[][] answer)
        {
            this.Answers = answer;
        }

        public override bool IsAnswered
        {
            get { return this.Answers != null && this.Answers.Any(); }
        }

        public override void RemoveAnswer()
        {
            this.Answers = null;
        }
    }
}