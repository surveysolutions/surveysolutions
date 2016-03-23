using System;
using System.Linq;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Designer.Implementation.Services.AttachmentService;
using WB.Core.Infrastructure.PlainStorage;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.AttachmentServiceTests
{
    internal class when_saving_attachment_content_and_unexpected_content_type : AttachmentServiceTestContext
    {
        Establish context = () =>
        {
            attachmentContentStorage
                .Setup(x => x.Query(Moq.It.IsAny<Func<IQueryable<AttachmentContent>, bool>>()))
                .Returns(false);

            attachmentService = Create.AttachmentService(attachmentContentStorage: attachmentContentStorage.Object);
        };

        Because of = () =>
            exception = Catch.Exception(()=> attachmentService.SaveContent(attachmentContentId, contentType, fileContent));

        It should_throw_format_exception = () =>
            exception.ShouldBeOfExactType<FormatException>();
        

        private static AttachmentService attachmentService;
        private static Exception exception;

        private static readonly byte[] fileContent = {255, 216, 255};
        private static readonly string attachmentContentId = "ABECA98D65F866DFCD292BC973BDACF5954B916D";
        private static readonly string contentType = "text/txt";
        private static readonly Mock<IPlainStorageAccessor<AttachmentContent>> attachmentContentStorage = new Mock<IPlainStorageAccessor<AttachmentContent>>();
    }
}