using System;

namespace WB.UI.Headquarters.API.PublicApi.Models
{
    public class QuestionApiView
    {
        public string Type { get; set; }
        public Guid PublicKey { get; set; }
        public string QuestionText { get; set; }
        public string Label { get; set; }
    }
}
