using WB.Core.BoundedContexts.Designer.Implementation.Services.AttachmentService;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo
{
    public class AttachmentView
    {
        public AttachmentView(string attachmentId, string name, AttachmentContent? content, AttachmentMeta? meta)
        {
            AttachmentId = attachmentId;
            Name = name;
            Content = content;
            Meta = meta;
        }

        public string AttachmentId { get; set; }
        public string Name { get; set; }
        public AttachmentContent? Content { get; set; }
        public AttachmentMeta? Meta { get; set; }
    }
}
