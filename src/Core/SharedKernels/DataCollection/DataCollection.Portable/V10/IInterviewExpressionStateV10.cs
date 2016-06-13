using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.V9;

namespace WB.Core.SharedKernels.DataCollection.V10
{
    public interface IInterviewExpressionStateV10 : IInterviewExpressionStateV9
    {
        IEnumerable<CategoricalOption> FilterOptionsForQuestion(Identity questionIdentity, IEnumerable<CategoricalOption> options);
        void RemoveRosterAndItsDependencies(Identity[] rosterKey, Guid rosterSorceId, decimal rosterInstanceId);
        new IInterviewExpressionStateV10 Clone();
    }
}