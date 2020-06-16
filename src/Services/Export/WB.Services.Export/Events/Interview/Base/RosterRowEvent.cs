using System;
using WB.Services.Export.Interview.Entities;
using WB.Services.Infrastructure.EventSourcing;

namespace WB.Services.Export.Events.Interview.Base
{
    public abstract class RosterRowEvent : InterviewPassiveEvent
    {
        public Guid GroupId { get; set; }
        public RosterVector OuterRosterVector { get; set; } = RosterVector.Empty;

        public int RosterInstanceId { get; set; }
    }
}
