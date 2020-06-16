using WB.Services.Export.Events.Interview.Base;
using WB.Services.Export.Events.Interview.Dtos;

namespace WB.Services.Export.Events.Interview
{
    public class RosterInstancesRemoved : InterviewPassiveEvent
    {
        public RosterInstance[] Instances { get; set; } = new RosterInstance[0];
    }
}
