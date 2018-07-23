using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class FlagSetToAnswer : QuestionActiveEvent
    {
        public FlagSetToAnswer(Guid userId, Guid questionId, decimal[] rosterVector, DateTimeOffset originDate)
            : base(userId, questionId, rosterVector, originDate) {}
    }
}
