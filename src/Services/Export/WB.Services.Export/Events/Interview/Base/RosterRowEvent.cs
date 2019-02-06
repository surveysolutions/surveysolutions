using System;

namespace WB.Services.Export.Events.Interview.Base
{
    public abstract class RosterRowEvent : InterviewPassiveEvent
    {
        public Guid GroupId { get; set; }
        public decimal[] OuterRosterVector { get; set; }
        public decimal RosterInstanceId { get; set; }
       
    }
}
