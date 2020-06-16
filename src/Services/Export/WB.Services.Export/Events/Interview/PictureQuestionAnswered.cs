using System;
using WB.Services.Export.Events.Interview.Base;

namespace WB.Services.Export.Events.Interview
{
    public class PictureQuestionAnswered : QuestionAnswered
    {
        public string PictureFileName { get; set; } = String.Empty;

    }
}
