using WB.Services.Export.Events.Interview.Base;
using WB.Services.Export.Events.Interview.Dtos;

namespace WB.Services.Export.Events.Interview
{
    public class RosterInstancesAdded : InterviewPassiveEvent
    {
        public AddedRosterInstance[] Instances { get; set; } = new AddedRosterInstance[0];
    }
}
