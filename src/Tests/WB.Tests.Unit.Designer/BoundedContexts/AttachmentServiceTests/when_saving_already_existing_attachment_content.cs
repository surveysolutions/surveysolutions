using System;
using System.Linq;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Designer.Implementation.Services.AttachmentService;
using WB.Core.Infrastructure.PlainStorage;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.AttachmentServiceTests
{
    internal class when_saving_already_existing_attachment_content : AttachmentServiceTestContext
    {
        Establish context = () =>
        {
            attachmentContentStorage
                .Setup(x => x.Query(Moq.It.IsAny<Func<IQueryable<AttachmentContent>, bool>>()))
                .Returns(true);

            attachmentService = Create.AttachmentService(attachmentContentStorage: attachmentContentStorage.Object);
        };

        Because of = () =>
            attachmentService.SaveContent(attachmentContentId, contentType, fileContent);

        It should_not_save_content_to__storage = () =>
            attachmentContentStorage.Verify(x => x.Store(Moq.It.IsAny<AttachmentContent>(), Moq.It.IsAny<string>()), Times.Never);
        

        private static AttachmentService attachmentService;
        private static readonly byte[] fileContent = new byte[] { 96, 97, 98, 99, 100 };
        private static readonly string attachmentContentId = "ABECA98D65F866DFCD292BC973BDACF5954B916D";
        private static readonly string contentType = "image/png";
        private static readonly Mock<IPlainStorageAccessor<AttachmentContent>> attachmentContentStorage = new Mock<IPlainStorageAccessor<AttachmentContent>>();
    }
}