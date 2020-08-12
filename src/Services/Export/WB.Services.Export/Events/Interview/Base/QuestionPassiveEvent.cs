using System;
using WB.Services.Export.Interview.Entities;
using WB.Services.Infrastructure.EventSourcing;

namespace WB.Services.Export.Events.Interview.Base
{
    public abstract class QuestionPassiveEvent : InterviewPassiveEvent
    {
        public Guid QuestionId { get; set; }
        public RosterVector? RosterVector { get; set; }
    }
}
