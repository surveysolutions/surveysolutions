using System;
using System.Linq;

namespace WB.Services.Export.Events.Interview.Base
{
    public abstract class QuestionsPassiveEvent : InterviewPassiveEvent
    {
        public Identity[] Questions { get; set; }
    }
}
