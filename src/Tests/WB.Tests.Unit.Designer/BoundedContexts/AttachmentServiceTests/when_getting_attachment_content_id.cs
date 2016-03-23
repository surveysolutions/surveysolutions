using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Implementation.Services.AttachmentService;

namespace WB.Tests.Unit.Designer.BoundedContexts.AttachmentServiceTests
{
    internal class when_getting_attachment_content_id : AttachmentServiceTestContext
    {
        Establish context = () =>
        {
            attachmentService = Create.AttachmentService();
        };

        Because of = () =>
            expectedAttachmentContentId = attachmentService.GetAttachmentContentId(fileBytes);

        It should_return_sha1_hash_of_file_bytes = () => expectedAttachmentContentId.ShouldEqual("7037807198C22A7D2B0807371D763779A84FDFCF");

        private static string expectedAttachmentContentId;
        private static readonly byte[] fileBytes = { 1, 2, 3};
        private static AttachmentService attachmentService;
    }
}