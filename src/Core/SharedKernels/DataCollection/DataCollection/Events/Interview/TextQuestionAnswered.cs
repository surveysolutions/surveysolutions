using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class TextQuestionAnswered : QuestionAnswered
    {
        public string Answer { get; private set; }

        public TextQuestionAnswered(Guid userId, Guid questionId, decimal[] rosterVector, 
            DateTimeOffset originDate, string answer)
            : base(userId, questionId, rosterVector, originDate)
        {
            this.Answer = answer;
        }
    }
}
