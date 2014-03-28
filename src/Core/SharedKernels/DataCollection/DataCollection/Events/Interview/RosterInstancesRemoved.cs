using System.Linq;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class RosterInstancesRemoved : InterviewPassiveEvent
    {
        public RosterInstanceIdentity[] Instances { get; private set; }

        public RosterInstancesRemoved(RosterInstanceIdentity[] instances)
        {
            this.Instances = instances.ToArray();
        }
    }
}