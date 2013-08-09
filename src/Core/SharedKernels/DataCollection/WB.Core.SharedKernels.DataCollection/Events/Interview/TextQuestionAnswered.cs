using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class TextQuestionAnswered : QuestionAnswered
    {
        public string Answer { get; private set; }

        public TextQuestionAnswered(Guid userId, Guid questionId, int[] propagationVector, DateTime answerTime, string answer)
            : base(userId, questionId, propagationVector, answerTime)
        {
            this.Answer = answer;
        }
    }
}