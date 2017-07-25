using System;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Attachments;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.UI.Designer.Api;
using WB.UI.Shared.Web.CommandDeserialization;


namespace WB.Tests.Unit.Designer.Applications.CommandApiControllerTests
{
    internal class when_posting_updated_attachment_without_file : CommandApiControllerTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var updateAttachmentCommand = Create.Command.AddOrUpdateAttachment(questionnaireId, attachmentId, null, responsibleId, name, oldAttachmentId);

            attachmentServiceMock.Setup(x => x.GetAttachmentContentId(oldAttachmentId)).Returns(attachmentContentId);

            var commandDeserializerMock = new Mock<ICommandDeserializer>();

            commandDeserializerMock
                .Setup(x => x.Deserialize(typeof(AddOrUpdateAttachment).Name, serializedUpdateAttachmentCommand))
                .Returns(updateAttachmentCommand);

            controller = CreateCommandController(
                commandDeserializer: commandDeserializerMock.Object,
                attachmentService: attachmentServiceMock.Object,
                commandService: mockOfCommandService.Object);
            BecauseOf();
        }

        private void BecauseOf() =>
            controller.UpdateAttachment(new CommandController.AttachmentModel { Command = serializedUpdateAttachmentCommand, FileName = fileName});

        [NUnit.Framework.Test] public void should_get_content_id_by_attachmentId () =>
            attachmentServiceMock.Verify(x=>x.GetAttachmentContentId(oldAttachmentId), Times.Once);

        [NUnit.Framework.Test] public void should_not_save_content () =>
            attachmentServiceMock.Verify(x => x.SaveContent(Moq.It.IsAny<string>(), Moq.It.IsAny<string>(), Moq.It.IsAny<byte[]>()), Times.Never);

        [NUnit.Framework.Test] public void should_save_attachment_meta () =>
            attachmentServiceMock.Verify(x => x.SaveMeta(attachmentId, questionnaireId, attachmentContentId, fileName), Times.Once);

        [NUnit.Framework.Test] public void should_execute_AddOrUpdateAttachment_command () =>
            mockOfCommandService.Verify(x => x.Execute(Moq.It.IsAny<AddOrUpdateAttachment>(), Moq.It.IsAny<string>()), Times.Once);

        private static CommandController controller;
        private static readonly Mock<IAttachmentService> attachmentServiceMock = new Mock<IAttachmentService>();
        private static readonly Mock<ICommandService> mockOfCommandService = new Mock<ICommandService>();
        private static string  serializedUpdateAttachmentCommand = "hello";
        private static readonly string attachmentContentId = "ABECA98D65F866DFCD292BC973BDACF5954B916D";
        private static readonly string name = "Attachment";
        private static readonly string fileName = "Attachment.PNG";
        private static readonly Guid responsibleId = Guid.Parse("DDDD0000000000000000000000000000");
        private static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid oldAttachmentId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static readonly Guid attachmentId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
    }
    
}