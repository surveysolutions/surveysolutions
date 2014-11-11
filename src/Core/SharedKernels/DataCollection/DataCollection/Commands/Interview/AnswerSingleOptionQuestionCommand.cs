using System;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class AnswerSingleOptionQuestionCommand : AnswerQuestionCommand
    {
        public decimal SelectedValue { get; private set; }

        public AnswerSingleOptionQuestionCommand(Guid interviewId, Guid userId, Guid questionId, decimal[] rosterVector, DateTime answerTime, decimal selectedValue)
            : base(interviewId, userId, questionId, rosterVector, answerTime)
        {
            this.SelectedValue = selectedValue;
        }
    }
}