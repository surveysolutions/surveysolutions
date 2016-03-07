using System;
using System.Web;
using System.Web.Routing;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.Designer.Implementation.Services.AttachmentService;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.UI.Designer
{
    public class AttachmentHandler : IHttpHandler
    {
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
                context.Response.ClearHeaders();
                context.Response.Clear();

                context.Response.StatusCode = 404;
                context.Response.SuppressContent = true;
            }
            else
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
        }

        public bool IsReusable => false;
    }
}