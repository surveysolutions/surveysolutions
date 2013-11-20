using System;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview.Base
{
    public abstract class AnswerQuestionCommand : QuestionCommand
    {
        public DateTime AnswerTime { get; private set; }

        protected AnswerQuestionCommand(Guid interviewId, Guid userId, Guid questionId, int[] rosterVector, DateTime answerTime)
            : base(interviewId, userId, questionId, rosterVector)
        {
            this.AnswerTime = answerTime;
        }
    }
}