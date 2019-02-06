using System;

namespace WB.Services.Export.Events.Interview.Base
{
    public abstract class QuestionActiveEvent : InterviewActiveEvent
    {
        public Guid QuestionId { get; set; }
        public int[] RosterVector { get; set; }
    }
}
