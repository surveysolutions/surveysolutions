using System;
using System.IO;
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
        private const string thumbnailTransform = "thumbnail";
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
                    Return404(context);
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
                Return404(context);
            }
            else
            {
                if (transform == TransformationType.thumbnail)
                    TransformContent(context, attachment, transformationType);
                else
                    GetOriginalContent(context, attachment);
            }
        }

        private static void Return404(HttpContext context)
        {
            context.Response.ClearHeaders();
            context.Response.Clear();

            context.Response.StatusCode = 404;
            context.Response.SuppressContent = true;
        }

        private static void GetOriginalContent(HttpContext context, QuestionnaireAttachment attachment)
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

        private static void TransformContent(HttpContext context, QuestionnaireAttachment attachment, string transformation)
        {
            //later should handle video and produce image preview 

            var defaultSizeToScale = defaultImageSizeToScale;
            int size;
            if (int.TryParse(context.Request.RequestContext.RouteData.Values["size"].ToString(), out size))
                defaultSizeToScale = size;

            var resizeSettings = new ResizeSettings
            {
                MaxWidth = defaultSizeToScale,
                MaxHeight = defaultSizeToScale
            };

            byte[] resultStream = GetTrasformedContent(attachment, resizeSettings);

            TimeSpan cacheTime = new TimeSpan(2, 0, 0);
            context.Response.Cache.VaryByParams["*"] = true;
            context.Response.Cache.SetExpires(DateTime.Now.Add(cacheTime));
            context.Response.Cache.SetMaxAge(cacheTime);
            context.Response.Cache.SetCacheability(HttpCacheability.Public);

            context.Response.Cache.SetETag(attachment.AttachmentContentId);
            context.Response.ContentType = attachment.ContentType;
            context.Response.AddHeader("content-disposition", $"attachment; filename={attachment.FileName}");
            context.Response.BinaryWrite(resultStream);
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
    }

    public enum TransformationType
    {
        unknown = 0,
        thumbnail = 1
    }

}