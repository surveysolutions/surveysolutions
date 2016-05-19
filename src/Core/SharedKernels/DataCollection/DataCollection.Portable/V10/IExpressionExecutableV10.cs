using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.V9;

namespace WB.Core.SharedKernels.DataCollection.V10
{
    public interface IExpressionExecutableV10 : IExpressionExecutableV9
    {
        IExpressionExecutableV10 CopyMembers(Func<Identity[], Guid, IEnumerable<IExpressionExecutableV10>> getInstances);
        void SetParent(IExpressionExecutableV10 parentLevel);

        new IExpressionExecutableV10 GetParent();
        new IExpressionExecutableV10 CreateChildRosterInstance(Guid rosterId, decimal[] rosterVector, Identity[] rosterIdentityKey);
    }
}