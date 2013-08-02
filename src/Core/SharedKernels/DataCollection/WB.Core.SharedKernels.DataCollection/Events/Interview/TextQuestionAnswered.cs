using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class TextQuestionAnswered : QuestionAnswered
    {
        public string Answer { get; private set; }

        public TextQuestionAnswered(Guid userId, Guid questionId, DateTime answerTime, string answer)
            : base(userId, questionId, answerTime)
        {
            this.Answer = answer;
        }
    }
}