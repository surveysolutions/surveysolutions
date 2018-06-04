using System;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class AnswerQRBarcodeQuestionCommand : AnswerQuestionCommand
    {
        public string Answer { get; private set; }

        public AnswerQRBarcodeQuestionCommand(Guid interviewId, Guid userId, Guid questionId, decimal[] rosterVector, string answer)
            : base(interviewId, userId, questionId, rosterVector)
        {
            this.Answer = answer;
        }
    }
}
