using System;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class AnswerMultipleOptionsLinkedQuestionCommand: AnswerQuestionCommand
    {
        public RosterVector[] SelectedRosterVectors { get; private set; }

        public AnswerMultipleOptionsLinkedQuestionCommand(Guid interviewId, Guid userId, Guid questionId, 
            RosterVector rosterVector, RosterVector[] selectedRosterVectors)
            : base(interviewId, userId, questionId, rosterVector)
        {
            this.SelectedRosterVectors = selectedRosterVectors;
        }
    }
}
