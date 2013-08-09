using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class GroupPropagated : GroupPassiveEvent
    {
        public int Count { get; set; }

        public GroupPropagated(Guid groupId, int count)
            : base(groupId)
        {
            this.Count = count;
        }
    }
}