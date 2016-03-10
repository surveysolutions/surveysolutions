namespace WB.Core.BoundedContexts.Designer.Implementation.Services.AttachmentService
{
    public class QuestionnaireAttachment
    {
        public virtual string AttachmentId { get; set; }
        public virtual string FileName { get; set; }
        public virtual string ContentType { get; set; }
        public virtual string AttachmentContentId { get; set; }
        public virtual byte[] Content { get; set; }
    }
}