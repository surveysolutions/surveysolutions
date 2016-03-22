using WB.Core.BoundedContexts.Designer.Implementation.Services.AttachmentService;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo
{
    public class AttachmentView
    {
        public string AttachmentId { get; set; }
        public string Name { get; set; }
        public AttachmentContentView Content { get; set; }
        public AttachmentMeta Meta { get; set; }
    }
}