using System;
using System.Linq;
using WB.Services.Export.Events.Interview.Base;
using WB.Services.Export.Events.Interview.Dtos;

namespace WB.Services.Export.Events.Interview
{
    public class RosterInstancesAdded : InterviewPassiveEvent
    {
        public AddedRosterInstance[] Instances { get; private set; }

        public RosterInstancesAdded(AddedRosterInstance[] instances, DateTimeOffset originDate) 
        {
            this.Instances = instances.ToArray();
        }
    }
}
