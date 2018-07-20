using System;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class CommentAnswerCommand : QuestionCommand
    {
        public string Comment { get; private set; }

        public CommentAnswerCommand(Guid interviewId, Guid userId, Guid questionId, 
            decimal[] rosterVector, string comment)
            : base(interviewId, userId, questionId, rosterVector)
        {
            this.Comment = comment;
        }
    }
}
