using System;

namespace WB.UI.Headquarters.Models.Api
{
    public class IdentifyingAnswerApiView
    {
        public Guid QuestionId { get; set; }
        public string Answer { get; set; }
    }
}