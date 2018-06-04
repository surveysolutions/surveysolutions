using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class GroupsEnabled : GroupsPassiveEvent
    {
        public GroupsEnabled(Identity[] groups, DateTimeOffset originDate)
            : base(groups, originDate) {}
    }
}
