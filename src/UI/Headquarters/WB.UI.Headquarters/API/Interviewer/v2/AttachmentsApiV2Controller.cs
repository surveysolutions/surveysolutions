using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.Core.SharedKernels.SurveyManagement.Web.Code;
using WB.UI.Headquarters.Code;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Api.Interviewer.v2
{
    [ApiBasicAuth(new[] { UserRoles.Operator })]
    public class AttachmentsApiV2Controller : ApiController
    {
        private readonly IAttachmentContentService attachmentContentService;

        public AttachmentsApiV2Controller(IAttachmentContentService attachmentContentService)
        {
            this.attachmentContentService = attachmentContentService;
        }

        [HttpGet]
        [WriteToSyncLog(SynchronizationLogType.GetAttachmentContent)]
        public HttpResponseMessage GetAttachmentContent(string id)
        {
            var attachmentContent = this.attachmentContentService.GetAttachmentContent(id);

            if (attachmentContent == null)
                return Request.CreateResponse(HttpStatusCode.NotFound);

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(attachmentContent.Content)
            };

            response.Content.Headers.ContentType = new MediaTypeHeaderValue(attachmentContent.ContentType);
            response.Headers.ETag = new EntityTagHeaderValue("\"" + attachmentContent.ContentHash + "\"");

            response.Headers.CacheControl = new CacheControlHeaderValue
            {
                Public = true,
                MaxAge = TimeSpan.FromDays(10)
            };

            return response;
        }
    }
}