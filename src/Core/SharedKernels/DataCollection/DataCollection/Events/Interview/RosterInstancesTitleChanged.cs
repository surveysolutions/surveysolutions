using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class RosterInstancesTitleChanged : InterviewPassiveEvent
    {
        public ChangedRosterInstanceTitleDto[] ChangedInstances { get; private set; }

        public RosterInstancesTitleChanged(ChangedRosterInstanceTitleDto[] changedInstances)
        {
            this.ChangedInstances = changedInstances;
        }
    }
}