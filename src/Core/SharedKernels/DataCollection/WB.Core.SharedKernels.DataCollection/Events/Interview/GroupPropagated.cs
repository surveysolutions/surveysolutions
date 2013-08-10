using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class GroupPropagated : InterviewPassiveEvent
    {
        public Guid GroupId { get; private set; }
        public int[] OuterScopePropagationVector { get; private set; }
        public int Count { get; private set; }

        public GroupPropagated(Guid groupId, int[] outerScopePropagationVector, int count)
        {
            this.GroupId = groupId;
            this.OuterScopePropagationVector = outerScopePropagationVector;
            this.Count = count;
        }
    }
}