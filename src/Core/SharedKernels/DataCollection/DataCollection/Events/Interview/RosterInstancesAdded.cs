using System.Linq;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class RosterInstancesAdded : InterviewPassiveEvent
    {
        public AddedRosterInstanceDto[] AddedInstances { get; private set; }

        public RosterInstancesAdded(AddedRosterInstanceDto[] addedInstances)
        {
            this.AddedInstances = addedInstances.ToArray();
        }
    }
}