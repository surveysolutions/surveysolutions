using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Views.SynchronizationLog;
using WB.Core.SharedKernels.SurveyManagement.Web.Code;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Api.Interviewer.v2
{
    [ApiBasicAuth(new[] { UserRoles.Operator })]
    public class AttachmentsApiV2Controller : ApiController
    {
        private readonly IPlainStorageAccessor<AttachmentContent> attachmentContentStorage;

        public AttachmentsApiV2Controller(IPlainStorageAccessor<AttachmentContent> attachmentContentStorage)
        {
            this.attachmentContentStorage = attachmentContentStorage;
        }

        [HttpGet]
        [WriteToSyncLog(SynchronizationLogType.GetAttachmentContent)]
        public HttpResponseMessage GetAttachmentContent(string id)
        {
            var attachmentContent = this.attachmentContentStorage.GetById(id);

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(attachmentContent.Content)
            };

            response.Content.Headers.ContentType = new MediaTypeHeaderValue(attachmentContent.ContentType);
            response.Headers.ETag = new EntityTagHeaderValue("\"" + id + "\"");

            response.Headers.CacheControl = new CacheControlHeaderValue
            {
                Public = true,
                MaxAge = TimeSpan.FromDays(10)
            };

            return response;
        }
    }
}