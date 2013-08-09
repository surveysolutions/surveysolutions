using System;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    [MapsToAggregateRootMethod(typeof(Implementation.Aggregates.Interview), "CommentAnswer")]
    public class CommentAnswerCommand : QuestionCommand
    {
        public string Comment { get; private set; }

        public CommentAnswerCommand(Guid interviewId, Guid userId, Guid questionId, string comment)
            : base(interviewId, userId, questionId)
        {
            this.Comment = comment;
        }
    }
}