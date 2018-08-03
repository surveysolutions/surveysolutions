using System;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class AnswerTextListQuestionCommand : AnswerQuestionCommand
    {
        public Tuple<decimal, string>[] Answers { get; private set; }

        public AnswerTextListQuestionCommand(Guid interviewId, Guid userId, Guid questionId, decimal[] rosterVector, Tuple<decimal, string>[] answers)
            : base(interviewId, userId, questionId, rosterVector)
        {
            this.Answers = answers;
        }
    }
}
