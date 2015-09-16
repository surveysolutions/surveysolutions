using System;
using System.Collections.Generic;

namespace WB.Core.SharedKernels.DataCollection.V4
{
    public interface IExpressionExecutableV2 : IExpressionExecutable
    {
        void SetInterviewProperties(IInterviewProperties interviewProperties);

        IExpressionExecutableV2 CopyMembers(Func<Identity[], Guid, IEnumerable<IExpressionExecutableV2>> getInstances);

        void SetParent(IExpressionExecutableV2 parentLevel);
        new IExpressionExecutableV2 GetParent();
        new IExpressionExecutableV2 CreateChildRosterInstance(Guid rosterId, decimal[] rosterVector, Identity[] rosterIdentityKey);
    }
}