using System;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    [MapsToAggregateRootMethod(typeof(Implementation.Aggregates.Interview), "SetFlagToAnswer")]
    public class SetFlagToAnswerCommand : QuestionCommand
    {
        public SetFlagToAnswerCommand(Guid interviewId, Guid userId, Guid questionId, int[] propagationVector)
            : base(interviewId, userId, questionId, propagationVector) {}
    }
}