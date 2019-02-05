using System;

namespace WB.Services.Export.Events.Interview.Base
{
    public abstract class GroupPassiveEvent : InterviewPassiveEvent
    {
        public Guid GroupId { get; private set; }
        public decimal[] RosterVector { get; private set; }
    }
}
