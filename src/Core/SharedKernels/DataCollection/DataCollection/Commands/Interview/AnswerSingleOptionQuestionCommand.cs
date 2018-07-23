using System;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class AnswerSingleOptionQuestionCommand : AnswerQuestionCommand
    {
        public int SelectedValue { get; private set; }

        public AnswerSingleOptionQuestionCommand(Guid interviewId, Guid userId, Guid questionId, decimal[] rosterVector, int selectedValue)
            : base(interviewId, userId, questionId, rosterVector)
        {
            this.SelectedValue = selectedValue;
        }
    }
}
