using WB.Core.BoundedContexts.Headquarters.Services;

namespace WB.Tests.Abc.TestFactories
{
    internal class ControllerFactory
    {
        public Core.SharedKernels.SurveyManagement.Web.Controllers.AttachmentsController AttachmentsController(IAttachmentContentService attachmentContentService)
            => new WB.Core.SharedKernels.SurveyManagement.Web.Controllers.AttachmentsController(attachmentContentService);
    }
}
