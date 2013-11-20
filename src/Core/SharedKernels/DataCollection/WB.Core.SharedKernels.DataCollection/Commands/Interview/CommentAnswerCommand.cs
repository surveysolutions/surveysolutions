using System;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    [MapsToAggregateRootMethod(typeof(Implementation.Aggregates.Interview), "CommentAnswer")]
    public class CommentAnswerCommand : QuestionCommand
    {
        public DateTime CommentTime { get; private set; }

        public string Comment { get; private set; }

        public CommentAnswerCommand(Guid interviewId, Guid userId, Guid questionId, int[] rosterVector, DateTime commentTime, string comment)
            : base(interviewId, userId, questionId, rosterVector)
        {
            this.Comment = comment;
            CommentTime = commentTime;
        }
    }
}