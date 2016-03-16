using System;
using System.Net.Http;
using System.Web.Http;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Web.Code;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Api.Interviewer.v2
{
    [ApiBasicAuth(new[] { UserRoles.Operator })]
    public class AttachmentsApiV2Controller : AttachmentsControllerBase
    {
        public AttachmentsApiV2Controller(IPlainStorageAccessor<AttachmentContent> attachmentContentStorage) 
            : base(attachmentContentStorage)
        {
        }

        [HttpGet]
        public override HttpResponseMessage GetAttachmentContent(string id) => base.GetAttachmentContent(id);
    }
}