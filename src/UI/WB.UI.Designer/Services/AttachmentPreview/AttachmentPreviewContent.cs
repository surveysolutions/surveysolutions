namespace WB.UI.Designer.Services.AttachmentPreview;

public class AttachmentPreviewContent
{
    public AttachmentPreviewContent(string contentType, byte[] content)
    {
        ContentType = contentType;
        Content = content;
    }

    public string ContentType { get; set; }
    public byte[] Content { get; set; }
}