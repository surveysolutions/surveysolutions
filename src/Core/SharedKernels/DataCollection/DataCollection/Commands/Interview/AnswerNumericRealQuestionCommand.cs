using System;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class AnswerNumericRealQuestionCommand : AnswerQuestionCommand
    {
        public double Answer { get; private set; }

        public AnswerNumericRealQuestionCommand(Guid interviewId, Guid userId, Guid questionId, decimal[] rosterVector, double answer)
            : base(interviewId, userId, questionId, rosterVector)
        {
            this.Answer = answer;
        }
    }
}
