using System;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class AnswerSingleOptionLinkedQuestionCommand : AnswerQuestionCommand
    {
        public decimal[] SelectedRosterVector { get; private set; }

        public AnswerSingleOptionLinkedQuestionCommand(Guid interviewId, Guid userId, Guid questionId, decimal[] rosterVector, DateTime answerTime, decimal[] selectedRosterVector)
            : base(interviewId, userId, questionId, rosterVector, answerTime)
        {
            this.SelectedRosterVector = selectedRosterVector;
        }
    }
}