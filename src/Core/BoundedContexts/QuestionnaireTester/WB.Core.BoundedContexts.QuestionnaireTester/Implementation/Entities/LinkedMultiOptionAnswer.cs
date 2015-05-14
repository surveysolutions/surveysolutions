using System;
using System.Linq;

namespace WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities
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

            if (this.Answers.Any())
                this.MarkAnswered();
            else
                this.MarkUnAnswered();
        }
    }
}