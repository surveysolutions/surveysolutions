using System;
using System.Linq;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;

namespace WB.Core.SharedKernels.Enumerator.Entities.Interview
{
    public class YesNoAnswer : BaseInterviewAnswer
    {
        public AnsweredYesNoOption[] Answers { get; private set; }

        public YesNoAnswer() { }
        public YesNoAnswer(Guid id, decimal[] rosterVector)
            : base(id, rosterVector)
        {
        }

        public void SetAnswers(AnsweredYesNoOption[] answer)
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