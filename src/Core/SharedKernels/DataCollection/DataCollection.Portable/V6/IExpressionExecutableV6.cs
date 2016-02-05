using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.V5;

namespace WB.Core.SharedKernels.DataCollection.V6
{
    public interface IExpressionExecutableV6 : IExpressionExecutableV5
    {
        
        IExpressionExecutableV6 CopyMembers(Func<Identity[], Guid, IEnumerable<IExpressionExecutableV6>> getInstances);

        void SetParent(IExpressionExecutableV6 parentLevel);
        new IExpressionExecutableV6 GetParent();
        new IExpressionExecutableV6 CreateChildRosterInstance(Guid rosterId, decimal[] rosterVector, Identity[] rosterIdentityKey);

        ValidityChanges ProcessValidationExpressions();
    }
}