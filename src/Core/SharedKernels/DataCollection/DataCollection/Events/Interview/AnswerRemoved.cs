using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class AnswerRemoved : QuestionAnswered
    {
        public AnswerRemoved(Guid userId, Guid questionId, decimal[] rosterVector, DateTime answerTimeUtc)
            : base(userId, questionId, rosterVector, answerTimeUtc)
        {
        }
    }
}