using NSubstitute;
using WB.Core.BoundedContexts.Designer.Services;
using WB.UI.Designer.Controllers.Api.Tester;

namespace WB.Tests.Unit.Designer.Api.Tester.AttachmentsControllerTests
{
    public class AttachmentsControllerTestsContext
    {
        public static AttachmentController CreateAttachmentController(IAttachmentService attachmentService = null)
        {
            return new AttachmentController(
                attachmentService: attachmentService ?? Substitute.For<IAttachmentService>());
        }
    }
}
