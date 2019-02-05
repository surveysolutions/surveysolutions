using System;
using WB.Services.Export.Events.Interview.Base;

namespace WB.Services.Export.Events.Interview
{
    public class DateTimeQuestionAnswered : QuestionAnswered
    {
        public DateTime Answer { get; private set; }
    }
}
