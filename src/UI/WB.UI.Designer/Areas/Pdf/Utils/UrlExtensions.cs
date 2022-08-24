using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using WB.Core.BoundedContexts.Designer.Implementation.Services.AttachmentService;
using WB.Core.BoundedContexts.Designer.Services;
using WB.UI.Designer.Extensions;

namespace WB.UI.Designer.Code
{
    public static class UrlExtensions
    {
        public static string ContentAbsolute(this IHtmlHelper htmlHelper, string contentPath)
        {
            string root = "";
            if (htmlHelper.ViewData.ContainsKey("webRoot"))
            {
                root = htmlHelper.ViewData["webRoot"]?.ToString() ?? "";
            }
            else
            {
                var urlHelperFactory = htmlHelper.ViewContext.HttpContext.RequestServices.GetRequiredService<IUrlHelperFactory>();
                var actionContextAccessor = htmlHelper.ViewContext.HttpContext.RequestServices.GetRequiredService<IActionContextAccessor>();
                
                if (actionContextAccessor?.ActionContext == null)
                    throw new Exception("Invalid context");
                
                var urlHelper = urlHelperFactory.GetUrlHelper(actionContextAccessor.ActionContext);
                root = urlHelper.Content("~")!;
            }
            return ConvertToAbsoluteUrl(root, contentPath);
        }

        private static string ConvertToAbsoluteUrl(string webRoot, string path)
        {
            if (path.StartsWith("~"))
            {
                return path.Replace("~", webRoot);
            }

            return webRoot + path;
        }
        
        public static string EmbedImageContent(this IHtmlHelper htmlHelper, string contentPath)
        {
            var webHostEnvironment = htmlHelper.ViewContext.HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();
            var filePath = Path.Combine(webHostEnvironment.WebRootPath, contentPath);
            if (!File.Exists(filePath))
                throw new FileNotFoundException(filePath);

            var imageType = Path.GetExtension(filePath).TrimStart('.');
            if (imageType == "svg")
                imageType = "svg+xml";
            string initialContent = "data:image/" + imageType + ";base64,";
            var bytes = File.ReadAllBytes(filePath);
            return initialContent + Convert.ToBase64String(bytes);
        }

        public static string EmbedAttachmentImage(this IHtmlHelper htmlHelper, Guid attachmentId, int? sizeToScale = null)
        {
            var attachmentService = htmlHelper.ViewContext.HttpContext.RequestServices.GetRequiredService<IAttachmentService>();
            var contentId = attachmentService.GetAttachmentContentId(attachmentId);
            if (contentId == null) 
                throw new FileNotFoundException($"Attachment contentId don't found. AttachmentId: {attachmentId}");
            AttachmentContent? attachmentContent = attachmentService.GetContent(contentId);
            if (attachmentContent == null) 
                throw new FileNotFoundException($"Attachment content don't found. AttachmentId: {attachmentId}, ContentId: {contentId}");

            string contentType = attachmentContent.ContentType;
            if (attachmentContent.Content == null)
                throw new FileNotFoundException($"Attachment content is missing. AttachmentId: {attachmentId}, ContentId: {contentId}");
                
            byte[] bytes = attachmentContent.Content;

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
                        var environment = htmlHelper.ViewContext.HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();
                        thumbBytes = System.IO.File.ReadAllBytes(environment.MapPath("images/icons-files-audio.png"));
                        contentType = @"image/png";
                    }

                    if (attachmentContent.IsPdf())
                    {
                        var environment = htmlHelper.ViewContext.HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();
                        thumbBytes = System.IO.File.ReadAllBytes(environment.MapPath(@"images/icons-files-pdf.png"));
                        contentType = @"image/png";
                    }
                }
                else
                {
                    thumbBytes = attachmentContent.Details.Thumbnail;
                }

                if (thumbBytes == null)
                {
                    throw new FileNotFoundException($"Attachment content is missing. AttachmentId: {attachmentId}, ContentId: {contentId}");
                }

                if (sizeToScale != null && contentType == "image/jpg")
                {
                    thumbBytes = GetTransformedContent(thumbBytes, sizeToScale);
                }

                bytes = thumbBytes;
            }

            string initialContent = "data:" + contentType + ";base64,";
            return initialContent + Convert.ToBase64String(bytes);
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
}
