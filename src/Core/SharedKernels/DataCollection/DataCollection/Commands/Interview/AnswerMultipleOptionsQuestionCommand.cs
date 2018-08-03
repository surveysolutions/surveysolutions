using System;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class AnswerMultipleOptionsQuestionCommand : AnswerQuestionCommand
    {
        public int[] SelectedValues { get; private set; }

        public AnswerMultipleOptionsQuestionCommand(Guid interviewId, Guid userId, Guid questionId, decimal[] rosterVector, int[] selectedValues)
            : base(interviewId, userId, questionId, rosterVector)
        {
            this.SelectedValues = selectedValues;
        }
    }
}
