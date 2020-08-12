using System;
using WB.Services.Export.Events.Interview.Base;

namespace WB.Services.Export.Events.Interview
{
    public class QRBarcodeQuestionAnswered : QuestionAnswered
    {
        public string Answer { get; set; } = String.Empty;
    }
}
