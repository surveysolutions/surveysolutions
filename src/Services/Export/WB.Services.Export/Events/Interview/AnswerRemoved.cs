using System;
using WB.Services.Export.Events.Interview.Base;

namespace WB.Services.Export.Events.Interview
{
    [Obsolete("Since v6.0")]
    public class AnswerRemoved : QuestionActiveEvent
    {
        public DateTime? RemoveTimeUtc { get; set; }
    }
}
