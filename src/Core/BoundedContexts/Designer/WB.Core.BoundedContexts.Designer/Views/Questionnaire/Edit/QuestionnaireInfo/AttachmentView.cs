using System;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo
{
    public class AttachmentView
    {
        public string ItemId { get; set; }
        public string Name { get; set; }
        public string FileName { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public long SizeInBytes { get; set; }
        public string Format { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}