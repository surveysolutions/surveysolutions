using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class TextListQuestionAnswered : QuestionAnswered
    {
        public Tuple<decimal, string>[] Answers { get; private set; }

        public TextListQuestionAnswered(Guid userId, Guid questionId, decimal[] rosterVector, 
            DateTimeOffset originDate, Tuple<decimal, string>[] answers)
            : base(userId, questionId, rosterVector, originDate)
        {
            this.Answers = answers;
        }
    }
}
