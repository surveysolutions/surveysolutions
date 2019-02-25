using System;
using WB.Services.Export.Events.Interview.Base;

namespace WB.Services.Export.Events.Interview
{
    public class AnswersRemoved : QuestionsPassiveEvent
    {
        public DateTime? RemoveTime { get; set; }
    }
}
