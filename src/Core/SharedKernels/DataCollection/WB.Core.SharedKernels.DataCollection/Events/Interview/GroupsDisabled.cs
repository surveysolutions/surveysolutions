using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class GroupsDisabled : GroupsPassiveEvent
    {
        public GroupsDisabled(Identity[] groups)
            : base(groups) {}
    }
}