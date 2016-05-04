using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.V5;
using WB.Core.SharedKernels.DataCollection.V6;

namespace WB.Core.SharedKernels.DataCollection.V7
{
    public interface IExpressionExecutableV7 : IExpressionExecutableV6
    {
        IExpressionExecutableV7 CopyMembers(Func<Identity[], Guid, IEnumerable<IExpressionExecutableV7>> getInstances);

        List<LinkedQuestionFilterResult> ExecuteLinkedQuestionFilters();

        void SetParent(IExpressionExecutableV7 parentLevel);
        new IExpressionExecutableV7 GetParent();
        new IExpressionExecutableV7 CreateChildRosterInstance(Guid rosterId, decimal[] rosterVector, Identity[] rosterIdentityKey);
    }
}