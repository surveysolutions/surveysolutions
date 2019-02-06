using System;
using WB.Services.Export.Events.Interview.Base;

namespace WB.Services.Export.Events.Interview
{
    public class NumericRealQuestionAnswered : QuestionAnswered
    {
        public decimal Answer { get; set; }
        
    }
}
