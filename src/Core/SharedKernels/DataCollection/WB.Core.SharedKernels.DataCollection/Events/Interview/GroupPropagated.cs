using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class GroupPropagated : GroupPassiveEvent
    {
        public int Count { get; private set; }

        public GroupPropagated(Guid groupId, int[] propagationVector, int count)
            : base(groupId, propagationVector)
        {
            this.Count = count;
        }
    }
}