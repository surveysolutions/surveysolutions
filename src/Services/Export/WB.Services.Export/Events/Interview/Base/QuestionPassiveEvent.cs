using System;

namespace WB.Services.Export.Events.Interview.Base
{
    public abstract class QuestionPassiveEvent : InterviewPassiveEvent
    {
        public Guid QuestionId { get; private set; }
        public decimal[] RosterVector { get; private set; }
    }
}
