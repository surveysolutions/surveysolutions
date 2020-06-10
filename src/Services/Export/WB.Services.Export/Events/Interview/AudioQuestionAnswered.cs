using System;
using WB.Services.Export.Events.Interview.Base;

namespace WB.Services.Export.Events.Interview
{
    public class AudioQuestionAnswered : QuestionAnswered
    {
        public TimeSpan Length { get; set; }
        public string FileName { get; set; } = null!;
    }
}
