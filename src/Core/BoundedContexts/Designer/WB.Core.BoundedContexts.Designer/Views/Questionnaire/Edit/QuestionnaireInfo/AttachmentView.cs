using System;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo
{
    public class AttachmentView
    {
        public string ItemId { get; set; }
        public string Name { get; set; }
        public string FileName { get; set; }
        public object Meta { get; set; }
        public long? SizeInBytes { get; set; }
        public DateTime? LastUpdated { get; set; }
        public string Type { get; set; }
    }
}