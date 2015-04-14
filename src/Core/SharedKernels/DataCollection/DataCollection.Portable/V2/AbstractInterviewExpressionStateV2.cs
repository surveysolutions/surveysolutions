using System;

namespace WB.Core.SharedKernels.DataCollection
{
    public abstract class AbstractInterviewExpressionStateV2 : AbstractInterviewExpressionState, IInterviewExpressionStateV2
    {
        public abstract void UpdateRosterTitle(Guid rosterId, decimal[] outerRosterVector, decimal rosterInstanceId,
            string rosterTitle);

        public abstract IInterviewExpressionStateV2 CloneV2();

        public override IInterviewExpressionState Clone()
        {
            return CloneV2();
        }
    }
}