using System;
using System.Collections.Generic;

namespace WB.UI.Headquarters.API.PublicApi.Models.Statistics
{
    public class QuestionDto
    {
        public Guid Id { get; set; }
        public string VariableName { get; set; }
        public string QuestionText { get; set; }
        public List<QuestionAnswerView> Answers { get; set; }
        public string[] Breadcrumbs { get; set; }
        public string Label { get; set; }
        public string Type { get; set; }
        public bool HasTotal { get; set; }
        public bool SupportConditions { get; set; }
    }
}
