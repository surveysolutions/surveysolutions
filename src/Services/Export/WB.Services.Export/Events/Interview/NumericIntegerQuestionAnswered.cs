using System;
using WB.Services.Export.Events.Interview.Base;

namespace WB.Services.Export.Events.Interview
{
    public class NumericIntegerQuestionAnswered : QuestionAnswered
    {
        public int Answer { get; private set; }
       
    }
}
