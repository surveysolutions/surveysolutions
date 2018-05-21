using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;

namespace WB.UI.Headquarters.API.PublicApi.Models
{
    public class QuestionApiView
    {
        public string Type { get; set; }
        public Guid PublicKey { get; set; }
        public string StataExportCaption { get; set; }
        public string QuestionText { get; set; }
        public List<QuestionAnswerView> Answers { get; set; }
        public string[] Breadcrumbs { get; set; }
        public string Label { get; set; }
        public bool HasTotal { get; set; }
        public bool SupportConditions { get; set; }
        public bool Pivotable { get; set; }
    }
}
