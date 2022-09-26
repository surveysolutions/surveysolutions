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
using WB.UI.Designer.Controllers.Api.Designer;
using WB.UI.Designer.Extensions;
using WB.UI.Designer.Services.AttachmentPreview;

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

            if (attachmentContent.Content == null)
                throw new FileNotFoundException($"Attachment content is missing. AttachmentId: {attachmentId}, ContentId: {contentId}");
                
            var attachmentPreviewHelper = htmlHelper.ViewContext.HttpContext.RequestServices.GetRequiredService<IAttachmentPreviewHelper>();
            var previewContent = attachmentPreviewHelper.GetPreviewImage(attachmentContent, sizeToScale);

            if (previewContent == null)
                throw new FileNotFoundException($"Attachment content is missing. AttachmentId: {attachmentId}, ContentId: {contentId}");

            string initialContent = "data:" + previewContent.ContentType + ";base64,";
            return initialContent + Convert.ToBase64String(previewContent.Content);
        }
    }
}
