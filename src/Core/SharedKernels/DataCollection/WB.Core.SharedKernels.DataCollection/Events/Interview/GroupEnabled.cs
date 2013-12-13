using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class GroupEnabled : GroupPassiveEvent
    {
        public GroupEnabled(Guid groupId, decimal[] propagationVector)
            : base(groupId, propagationVector) {}
    }
}