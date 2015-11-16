using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;

namespace WB.Core.SharedKernels.Enumerator.Entities.Interview
{
    public class YesNoQuestionAnswer : BaseInterviewAnswer
    {
        public AnsweredYesNoOption[] Answers { get; private set; }

        public YesNoQuestionAnswer() { }
        public YesNoQuestionAnswer(Guid id, decimal[] rosterVector)
            : base(id, rosterVector)
        {
        }

        public void SetAnswers(IEnumerable<AnsweredYesNoOption> answers)
        {
            this.Answers = answers.ToArray();
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