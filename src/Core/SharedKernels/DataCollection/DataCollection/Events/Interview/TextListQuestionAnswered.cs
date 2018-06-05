using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class TextListQuestionAnswered : QuestionAnswered
    {
        public Tuple<decimal, string>[] Answers { get; private set; }

        public TextListQuestionAnswered(Guid userId, Guid questionId, decimal[] rosterVector, 
            DateTimeOffset originDate, Tuple<decimal, string>[] answers, DateTime? answerTimeUtc = null)
            : base(userId, questionId, rosterVector, originDate, answerTimeUtc)
        {
            this.Answers = answers;
        }
    }
}
