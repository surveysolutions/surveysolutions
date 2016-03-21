using System;
using System.IO;
using System.Security.Cryptography;
using System.Web;
using System.Web.Routing;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.Designer.Implementation.Services.AttachmentService;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.Infrastructure.PlainStorage;
using ImageResizer;

namespace WB.UI.Designer
{
    public class AttachmentHandler : IHttpHandler
    {
        private const int defaultImageSizeToScale = 156;

        private IAttachmentService attachmentService => ServiceLocator.Current.GetInstance<IAttachmentService>();

        IPlainTransactionManager TransactionManager => ServiceLocator.Current.GetInstance<IPlainTransactionManager>();

        protected RequestContext RequestContext { get; set; }

         public AttachmentHandler(RequestContext requestContext) 
        { 
            this.RequestContext = requestContext; 
        }

        public void ProcessRequest(HttpContext context)
        {
            var attachmentId = Guid.Parse(context.Request.RequestContext.RouteData.Values["attachmentId"].ToString());
            var transformationType = context.Request.RequestContext.RouteData.Values["transform"].ToString();

            TransformationType transform = TransformationType.unknown;
            if (!string.IsNullOrWhiteSpace(transformationType))
            {
                if (!Enum.TryParse(transformationType, out transform))
                {
                    Write404ToResponse(context);
                    return;
                }
            }

            QuestionnaireAttachment attachment;
            try
            {
                this.TransactionManager.BeginTransaction();
                attachment = this.attachmentService.GetAttachment(attachmentId);
                this.TransactionManager.CommitTransaction();
            }
            catch (Exception exception)
            {
                this.TransactionManager.RollbackTransaction();
                throw exception;
            }
            if (attachment == null)
            {
                Write404ToResponse(context);
                return;
            }
            else
            {
                if (transform == TransformationType.thumbnail)
                    ResizeAndWriteContentToResponse(context, attachment);
                else
                    WriteOriginalContentToResponse(context, attachment);
            }
        }

        private static void Write404ToResponse(HttpContext context)
        {
            context.Response.ClearHeaders();
            context.Response.Clear();

            context.Response.StatusCode = 404;
            context.Response.SuppressContent = true;
        }

        private static void WriteOriginalContentToResponse(HttpContext context, QuestionnaireAttachment attachment)
        {
            TimeSpan cacheTime = new TimeSpan(2, 0, 0);
            context.Response.Cache.VaryByParams["*"] = true;
            context.Response.Cache.SetExpires(DateTime.Now.Add(cacheTime));
            context.Response.Cache.SetMaxAge(cacheTime);
            context.Response.Cache.SetCacheability(HttpCacheability.Public);

            context.Response.Cache.SetETag(attachment.AttachmentContentId);
            context.Response.ContentType = attachment.ContentType;
            context.Response.AddHeader("content-disposition", $"attachment; filename={attachment.FileName}");
            context.Response.BinaryWrite(attachment.Content);
        }

        private static void ResizeAndWriteContentToResponse(HttpContext context, QuestionnaireAttachment attachment)
        {
            var sizeToScale = defaultImageSizeToScale;
            int size;
            if (int.TryParse(context.Request.RequestContext.RouteData.Values["size"].ToString(), out size))
                sizeToScale = size;

            var resizeSettings = new ResizeSettings
            {
                MaxWidth = sizeToScale,
                MaxHeight = sizeToScale
            };

            byte[] transformedContent = GetTrasformedContent(attachment, resizeSettings);

            string transformedContentHash;
            using (SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider())
            {
                transformedContentHash = BitConverter.ToString(sha1.ComputeHash(transformedContent)).Replace("-", string.Empty);
            }

            TimeSpan cacheTime = new TimeSpan(2, 0, 0);
            context.Response.Cache.VaryByParams["*"] = true;
            context.Response.Cache.SetExpires(DateTime.Now.Add(cacheTime));
            context.Response.Cache.SetMaxAge(cacheTime);
            context.Response.Cache.SetCacheability(HttpCacheability.Public);

            context.Response.Cache.SetETag(transformedContentHash);
            context.Response.ContentType = attachment.ContentType;
            context.Response.AddHeader("content-disposition", $"attachment; filename={attachment.FileName}");
            context.Response.BinaryWrite(transformedContent);
        }

        private static byte[] GetTrasformedContent(QuestionnaireAttachment attachment, ResizeSettings resizeSettings)
        {
            using (var outputStream = new MemoryStream())
            {
                ImageBuilder.Current.Build(attachment.Content, outputStream, resizeSettings);
                return outputStream.ToArray();
            }
        }

        public bool IsReusable => false;

        private enum TransformationType
        {
            unknown = 0,
            thumbnail = 1
        }
    }
}