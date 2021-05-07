namespace WB.Core.BoundedContexts.Headquarters.Invitations
{
    public enum EmailAttachmentDisposition
    {
        Attachment,
        Inline
    }
    
    public class EmailAttachment
    {
        public string Filename { get; set; }
        public byte[] Content { get; set; }
        public string ContentType { get; set; }
        public string ContentId { get; set; }
        public EmailAttachmentDisposition Disposition { get; set; } = EmailAttachmentDisposition.Attachment;
    }
}