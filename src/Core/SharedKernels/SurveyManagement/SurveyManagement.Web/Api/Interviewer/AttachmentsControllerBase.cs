using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Views.SynchronizationLog;
using WB.Core.SharedKernels.SurveyManagement.Web.Code;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Api.Interviewer
{
    public class AttachmentsControllerBase : ApiController
    {
        private readonly IPlainStorageAccessor<AttachmentContent> attachmentContentStorage;

        public AttachmentsControllerBase(IPlainStorageAccessor<AttachmentContent> attachmentContentStorage)
        {
            this.attachmentContentStorage = attachmentContentStorage;
        }


        [WriteToSyncLog(SynchronizationLogType.GetAttachmentContent)]
        public virtual HttpResponseMessage GetAttachmentContent(string id)
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