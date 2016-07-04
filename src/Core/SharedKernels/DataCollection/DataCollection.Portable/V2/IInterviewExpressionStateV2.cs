using System;

namespace WB.Core.SharedKernels.DataCollection.V2
{
    public interface IInterviewExpressionStateV2 : IInterviewExpressionState
    {
        [Obsolete("Obsolete in 5.10. Used for unsupported @rowname property in rosters")]
        void UpdateRosterTitle(Guid rosterId, decimal[] outerRosterVector, decimal rosterInstanceId, string rosterTitle);
        new IInterviewExpressionStateV2 Clone();
    }
}