using System;

namespace WB.Services.Export.Events.Interview.Base
{
    public abstract class RosterRowEvent : InterviewPassiveEvent
    {
        public Guid GroupId { get; private set; }
        public decimal[] OuterRosterVector { get; private set; }
        public decimal RosterInstanceId { get; private set; }
       
    }
}
