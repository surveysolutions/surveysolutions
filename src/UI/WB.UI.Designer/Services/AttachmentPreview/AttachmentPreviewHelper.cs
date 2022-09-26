using System.IO;
using Microsoft.AspNetCore.Hosting;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using WB.Core.BoundedContexts.Designer.Implementation.Services.AttachmentService;
using WB.UI.Designer.Extensions;

namespace WB.UI.Designer.Services.AttachmentPreview;

public class AttachmentPreviewHelper : IAttachmentPreviewHelper
{
    private readonly IWebHostEnvironment webHostEnvironment;

    public AttachmentPreviewHelper(IWebHostEnvironment webHostEnvironment)
    {
        this.webHostEnvironment = webHostEnvironment;
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
        
        return new AttachmentPreviewContent()
        {
            ContentType = contentType,
            Content = bytes,
        };
    }
    
    private static byte[] GetTransformedContent(byte[] source, int? sizeToScale = null)
    {
        if (!sizeToScale.HasValue) return source;
        using (var outputStream = new MemoryStream())
        {
            using (Image image = Image.Load(source, out var format))
            {
                var opt = new ResizeOptions()
                {
                    Mode = ResizeMode.Max,
                    Size = new Size(sizeToScale.Value)
                };
                image.Mutate(ctx => ctx.Resize(opt)); 
                image.Save(outputStream, format); 
            } 

            return outputStream.ToArray();
        }
    }
}