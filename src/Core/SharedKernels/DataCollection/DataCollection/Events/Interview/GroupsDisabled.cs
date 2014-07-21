using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class GroupsDisabled : GroupsPassiveEvent
    {
        public GroupsDisabled(Dtos.Identity[] groups)
            : base(groups) {}
    }
}