using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class FlagRemovedFromAnswer : QuestionActiveEvent
    {
        public FlagRemovedFromAnswer(Guid userId, Guid questionId, decimal[] propagationVector)
            : base(userId, questionId, propagationVector) {}
    }
}