using System;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class AnswerTextQuestionCommand : AnswerQuestionCommand
    {
        public string Answer { get; private set; }

        public AnswerTextQuestionCommand(Guid interviewId, Guid userId, Guid questionId, decimal[] rosterVector, DateTime answerTime, string answer)
            : base(interviewId, userId, questionId, rosterVector, answerTime)
        {
            this.Answer = answer;
        }
    }
}