using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Portable;

namespace WB.Core.SharedKernels.DataCollection.ExpressionStorage
{
    public interface IInterviewStateForExpressions
    {
        T GetAnswer<T>(Guid questionId, IEnumerable<int> rosterVector);

        T GetVariable<T>(Guid questionId, IEnumerable<int> rosterVector);

        IEnumerable<Identity> FindEntitiesFromSameOrDeeperLevel(Guid entityIdToSearch, Identity startingSearchPointIdentity);

        int GetRosterIndex(Identity rosterIdentity);

        string GetRosterTitle(Identity rosterIdentity);

        IInterviewPropertiesForExpressions Properties { get; }

        Section GetSection(Guid sectionId, IEnumerable<int> rosterVector);
    }

    public interface IInterviewPropertiesForExpressions
    {
        double Random { get; }

        Guid SupervisorId { get; }

        Guid InterviewerId { get; }

        string InterviewId { get; }
    }
}
