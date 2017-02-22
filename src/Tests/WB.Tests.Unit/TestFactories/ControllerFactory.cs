using WB.Core.BoundedContexts.Headquarters.Services;

namespace WB.Tests.Unit.TestFactories
{
    internal class ControllerFactory
    {
        public Core.SharedKernels.SurveyManagement.Web.Controllers.AttachmentsController AttachmentsController(IAttachmentContentService attachmentContentService)
            => new WB.Core.SharedKernels.SurveyManagement.Web.Controllers.AttachmentsController(attachmentContentService);
    }
}
