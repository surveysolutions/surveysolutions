using System;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class AnswerNumericIntegerQuestionCommand : AnswerQuestionCommand
    {
        public int Answer { get; private set; }

        public AnswerNumericIntegerQuestionCommand(Guid interviewId, Guid userId, Guid questionId, decimal[] rosterVector, int answer)
            : base(interviewId, userId, questionId, rosterVector)
        {
            this.Answer = answer;
        }
    }
}
