using System;
using System.Diagnostics;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    [DebuggerDisplay("ChangedInstancesCount = {ChangedInstances.Length}")]
    public class RosterInstancesTitleChanged : InterviewPassiveEvent
    {
        public ChangedRosterInstanceTitleDto[] ChangedInstances { get; private set; }

        public RosterInstancesTitleChanged(ChangedRosterInstanceTitleDto[] changedInstances, DateTimeOffset originDate) : base(originDate)
        {
            this.ChangedInstances = changedInstances;
        }
    }
}
