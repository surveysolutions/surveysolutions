using System;
using System.Collections.Generic;

namespace WB.Core.SharedKernels.DataCollection.V2
{
    public abstract class AbstractInterviewExpressionStateV2 : AbstractInterviewExpressionState, IInterviewExpressionStateV2
    {
        public abstract void UpdateRosterTitle(Guid rosterId, decimal[] outerRosterVector, decimal rosterInstanceId,
            string rosterTitle);

        IInterviewExpressionStateV2 IInterviewExpressionStateV2.Clone()
        {
            return Clone() as IInterviewExpressionStateV2;
        }
    }
}