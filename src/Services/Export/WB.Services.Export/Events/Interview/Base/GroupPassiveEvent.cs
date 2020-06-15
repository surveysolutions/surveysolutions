using System;
using WB.Services.Export.Interview.Entities;
using WB.Services.Infrastructure.EventSourcing;

namespace WB.Services.Export.Events.Interview.Base
{
    public abstract class GroupPassiveEvent : InterviewPassiveEvent
    {
        public Guid GroupId { get; set; }
        public RosterVector? RosterVector { get; set; }
    }
}
