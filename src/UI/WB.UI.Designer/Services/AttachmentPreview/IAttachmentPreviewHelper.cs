using WB.Core.BoundedContexts.Designer.Implementation.Services.AttachmentService;

namespace WB.UI.Designer.Services.AttachmentPreview;

public interface IAttachmentPreviewHelper
{
    AttachmentPreviewContent? GetPreviewImage(AttachmentContent attachmentContent, int? sizeToScale);
}