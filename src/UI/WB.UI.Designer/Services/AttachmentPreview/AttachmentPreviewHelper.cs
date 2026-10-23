using System.IO;
using Microsoft.AspNetCore.Hosting;
using WB.Core.BoundedContexts.Designer.Implementation.Services.AttachmentService;
using WB.UI.Designer.Extensions;
using WB.UI.Shared.Web.Services;

namespace WB.UI.Designer.Services.AttachmentPreview;

public class AttachmentPreviewHelper : IAttachmentPreviewHelper
{
    private readonly IWebHostEnvironment webHostEnvironment;
    private readonly IImageProcessingService imageProcessingService;

    public AttachmentPreviewHelper(IWebHostEnvironment webHostEnvironment, IImageProcessingService imageProcessingService)
    {
        this.webHostEnvironment = webHostEnvironment;
        this.imageProcessingService = imageProcessingService;
    }

    public AttachmentPreviewContent? GetPreviewImage(AttachmentContent attachmentContent, int? sizeToScale)
    {
        byte[]? bytes = attachmentContent.Content;
        if (bytes == null)
            return null;

        string contentType = attachmentContent.ContentType;

        if (sizeToScale.HasValue)
        {
            contentType = "image/jpg";
            byte[]? thumbBytes = null;

            if (attachmentContent.Details.Thumbnail == null || attachmentContent.Details.Thumbnail.Length == 0)
            {
                if (attachmentContent.IsImage())
                {
                    thumbBytes = attachmentContent.Content;
                }

                if (attachmentContent.IsAudio())
                {
                    thumbBytes = System.IO.File.ReadAllBytes(webHostEnvironment.MapPath("images/icons-files-audio.png"));
                    contentType = @"image/png";
                }

                if (attachmentContent.IsPdf())
                {
                    thumbBytes = System.IO.File.ReadAllBytes(webHostEnvironment.MapPath(@"images/icons-files-pdf.png"));
                    contentType = @"image/png";
                }
            }
            else
            {
                thumbBytes = attachmentContent.Details.Thumbnail;
            }

            if (thumbBytes == null)
            {
                return null;
            }

            if (sizeToScale != null && contentType == "image/jpg")
            {
                thumbBytes = GetTransformedContent(thumbBytes, sizeToScale);
            }

            bytes = thumbBytes;
        }

        return new AttachmentPreviewContent(contentType, bytes);
    }
    
    private byte[] GetTransformedContent(byte[] source, int? sizeToScale = null)
    {
        if (!sizeToScale.HasValue) return source;

        return imageProcessingService.ResizeImageKeepingFormat(source, sizeToScale.Value);
    }
}
