using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class GroupDisabled : GroupPassiveEvent
    {
        public GroupDisabled(Guid groupId, int[] propagationVector)
            : base(groupId, propagationVector) {}
    }
}