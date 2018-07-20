using System;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class AnswerDateTimeQuestionCommand : AnswerQuestionCommand
    {
        public DateTime Answer { get; private set; }

        public AnswerDateTimeQuestionCommand(Guid interviewId, Guid userId, Guid questionId, decimal[] rosterVector, 
            DateTime answer)
            : base(interviewId, userId, questionId, rosterVector)
        {
            this.Answer = answer;
        }
    }
}
