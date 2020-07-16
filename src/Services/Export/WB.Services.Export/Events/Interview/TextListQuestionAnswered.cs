using System;
using WB.Services.Export.Events.Interview.Base;

namespace WB.Services.Export.Events.Interview
{
    public class TextListQuestionAnswered : QuestionAnswered
    {
        public Tuple<decimal, string>[] Answers { get; set; } = new Tuple<decimal, string>[0];

    }
}
