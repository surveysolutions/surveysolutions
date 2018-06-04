using System;
using System.Linq;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class RosterInstancesRemoved : InterviewPassiveEvent
    {
        public RosterInstance[] Instances { get; private set; }

        public RosterInstancesRemoved(RosterInstance[] instances, DateTimeOffset originDate) : base(originDate)
        {
            this.Instances = instances.ToArray();
        }
    }
}
