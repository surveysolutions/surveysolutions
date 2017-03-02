using System;
using System.Collections.Generic;

namespace WB.Core.SharedKernels.DataCollection.V11
{
    public interface IInterviewState
    {
        T GetAnswer<T>(Guid questionId, IEnumerable<int> rosterVector);

        int GetRosterIndex(Identity rosterIdentity);

        string GetRosterTitle(Identity rosterIdentity);
    }
}