using System;
using System.Collections.Generic;

namespace WB.Core.SharedKernels.DataCollection.ExpressionStorage
{
    public interface IInterviewStateForExpressions
    {
        T GetAnswer<T>(Guid questionId, IEnumerable<int> rosterVector) where T: class;

        T GetVariable<T>(Guid questionId, IEnumerable<int> rosterVector) where T: class;

        IEnumerable<Identity> FindEntitiesFromSameOrDeeperLevel(Guid entityIdToSearch, Identity startingSearchPointIdentity);

        int GetRosterIndex(Identity rosterIdentity);

        string GetRosterTitle(Identity rosterIdentity);

        IInterviewPropertiesForExpressions Properties { get; }
    }

    public interface IInterviewPropertiesForExpressions
    {
        double Random { get; }

        Guid SupervisorId { get; }

        Guid InterviewerId { get; }

        string InterviewId { get; }
    }
}