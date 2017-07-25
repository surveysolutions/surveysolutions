using System;
using System.Net.Http.Headers;
using Machine.Specifications;
using Moq;
using MultipartDataMediaFormatter.Infrastructure;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Attachments;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.UI.Designer.Api;
using WB.UI.Shared.Web.CommandDeserialization;


namespace WB.Tests.Unit.Designer.Applications.CommandApiControllerTests
{
    internal class when_posting_updated_attachment : CommandApiControllerTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var updateAttachmentCommand = Create.Command.AddOrUpdateAttachment(questionnaireId, attachmentId, attachmentContentId, responsibleId, name);

            attachmentServiceMock.Setup(x => x.CreateAttachmentContentId(fileBytes)).Returns(attachmentContentId);
            attachmentServiceMock.Setup(x => x.SaveContent(attachmentContentId, contentType, fileBytes)).Verifiable();
            attachmentServiceMock.Setup(x => x.SaveMeta(attachmentId, questionnaireId, attachmentContentId, fileName)).Verifiable();

            var commandDeserializerMock = new Mock<ICommandDeserializer>();
            
            commandDeserializerMock
                .Setup(x => x.Deserialize(typeof (AddOrUpdateAttachment).Name, serializedUpdateAttachmentCommand))
                .Returns(updateAttachmentCommand);
            
            controller = CreateCommandController(
                commandDeserializer: commandDeserializerMock.Object,
                attachmentService: attachmentServiceMock.Object,
                commandService: mockOfCommandService.Object);
            BecauseOf();
        }

        private void BecauseOf() =>
            controller.UpdateAttachment(new CommandController.AttachmentModel { File = new HttpFile { Buffer = fileBytes, FileName = fileName , MediaType = contentType }, Command = serializedUpdateAttachmentCommand });

        [NUnit.Framework.Test] public void should_save_attachment_content_with_specified_params () =>
            attachmentServiceMock.Verify(
                x => x.SaveContent(attachmentContentId, contentType, fileBytes), Times.Once);

        [NUnit.Framework.Test] public void should_save_attachment_meta_with_specified_params () =>
            attachmentServiceMock.Verify(
                x => x.SaveMeta(attachmentId, questionnaireId, attachmentContentId, fileName), Times.Once);

        [NUnit.Framework.Test] public void should_execute_AddOrUpdateAttachment_command () =>
            mockOfCommandService.Verify(
                x => x.Execute(Moq.It.IsAny<AddOrUpdateAttachment>(), Moq.It.IsAny<string>()), Times.Once);

        private static CommandController controller;
        private static readonly Mock<IAttachmentService> attachmentServiceMock = new Mock<IAttachmentService>();
        private static readonly Mock<ICommandService> mockOfCommandService = new Mock<ICommandService>();
        private static string serializedUpdateAttachmentCommand = "hello";
        private static byte[] fileBytes = new byte[] { 96, 97, 98, 99, 100 };
        private static readonly string attachmentContentId = "ABECA98D65F866DFCD292BC973BDACF5954B916D";
        private static readonly string name = "Attachment";
        private static readonly string fileName = "Attachment.PNG";
        private static readonly Guid responsibleId = Guid.Parse("DDDD0000000000000000000000000000");
        private static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid attachmentId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static string contentType = "image/png";
    }
}