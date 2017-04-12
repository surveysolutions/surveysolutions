using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class AreaQuestionAnswered : QuestionAnswered
    {
        public string Answer { get; private set; }

        public AreaQuestionAnswered(Guid userId, Guid questionId, decimal[] rosterVector,
                                    DateTime answerTimeUtc, string answer)
            : base(userId, questionId, rosterVector, answerTimeUtc)
        {
            this.Answer = answer;
        }
    }
}