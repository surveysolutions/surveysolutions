using System;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class AnswerMultipleOptionsLinkedQuestionCommand: AnswerQuestionCommand
    {
        public decimal[][] SelectedRosterVectors { get; private set; }

        public AnswerMultipleOptionsLinkedQuestionCommand(Guid interviewId, Guid userId, Guid questionId, decimal[] rosterVector, DateTime answerTime, decimal[][] selectedRosterVectors)
            : base(interviewId, userId, questionId, rosterVector, answerTime)
        {
            this.SelectedRosterVectors = selectedRosterVectors;
        }
    }
}
