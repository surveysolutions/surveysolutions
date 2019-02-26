using System;

namespace WB.Services.Export.Events.Interview.Base
{
    public abstract class QuestionAnswered : QuestionActiveEvent
    {
        public DateTime? AnswerTimeUtc { get; set; }
    }
}
