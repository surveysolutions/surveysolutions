using System;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    [MapsToAggregateRootMethod(typeof(Implementation.Aggregates.Interview), "RemoveFlagFromAnswer")]
    public class RemoveFlagFromAnswerCommand : QuestionCommand
    {
        public RemoveFlagFromAnswerCommand(Guid interviewId, Guid userId, Guid questionId)
            : base(interviewId, userId, questionId) {}
    }
}