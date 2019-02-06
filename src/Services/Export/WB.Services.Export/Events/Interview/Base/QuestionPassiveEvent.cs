using System;

namespace WB.Services.Export.Events.Interview.Base
{
    public abstract class QuestionPassiveEvent : InterviewPassiveEvent
    {
        public Guid QuestionId { get; set; }
        public decimal[] RosterVector { get; set; }
    }
}
