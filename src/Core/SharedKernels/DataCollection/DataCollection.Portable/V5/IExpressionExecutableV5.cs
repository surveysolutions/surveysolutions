using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.V4;

namespace WB.Core.SharedKernels.DataCollection.V5
{
    public interface IExpressionExecutableV5 : IExpressionExecutableV2
    {
        void UpdateYesNoAnswer(Guid questionId, YesNoAnswersOnly answers);

        IExpressionExecutableV5 CopyMembers(Func<Identity[], Guid, IEnumerable<IExpressionExecutableV5>> getInstances);

        void SetParent(IExpressionExecutableV5 parentLevel);
        new IExpressionExecutableV5 GetParent();
        new IExpressionExecutableV5 CreateChildRosterInstance(Guid rosterId, decimal[] rosterVector, Identity[] rosterIdentityKey);
    }
}