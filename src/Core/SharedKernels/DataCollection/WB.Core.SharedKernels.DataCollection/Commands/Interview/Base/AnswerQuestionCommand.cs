using System;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview.Base
{
    public abstract class AnswerQuestionCommand : InterviewCommand
    {
        public Guid QuestionId { get; private set; }
        public DateTime AnswerTime { get; private set; }

        protected AnswerQuestionCommand(Guid interviewId, Guid questionId, DateTime answerTime)
            : base(interviewId)
        {
            this.QuestionId = questionId;
            this.AnswerTime = answerTime;
        }
    }
}