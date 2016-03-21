using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Machine.Specifications;
using Moq;
using MultipartDataMediaFormatter.Infrastructure;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Attachments;
using WB.Core.BoundedContexts.Designer.Implementation.Services.AttachmentService;
using WB.Core.BoundedContexts.Designer.Services;
using WB.UI.Designer.Api;
using WB.UI.Shared.Web.CommandDeserialization;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.Applications.CommandApiControllerTests
{
    [Ignore("Slave help, don't know what should check here")]
    internal class when_posting_updated_attachment_with_wrong_content_type : CommandApiControllerTestContext
    {
        Establish context = () =>
        {
            var updateAttachmentCommand = Create.Command.UpdateAttachment(questionnaireId, attachmentId, attachmentContentId, responsibleId, name, fileName);

            attachmentServiceMock.Setup(x => x.SaveAttachmentContent(questionnaireId, attachmentId, attachmentContentId, contentType, fileBytes, fileName)).Verifiable();

            var commandDeserializerMock = new Mock<ICommandDeserializer>();

            commandDeserializerMock
                .Setup(x => x.Deserialize(typeof(UpdateAttachment).Name, serializedUpdateAttachmentCommand))
                .Returns(updateAttachmentCommand);

            controller = CreateCommandController(
                commandDeserializer: commandDeserializerMock.Object,
                attachmentService: attachmentServiceMock.Object);

            Setup.CommandApiControllerToAcceptAttachment(controller, fileBytes, MediaTypeHeaderValue.Parse("text/plain"), serializedUpdateAttachmentCommand);
        };

        Because of = () =>
            message = controller.UpdateAttachment(new CommandController.AttachmentModel { File = new HttpFile { Buffer = fileBytes, FileName = fileName, MediaType = contentType }, Command = serializedUpdateAttachmentCommand });

        It should_return_message_with_NotAcceptable_code = () =>
            message.StatusCode.ShouldEqual(HttpStatusCode.NotAcceptable);
        
        private static CommandController controller;
        private static readonly Mock<IAttachmentService> attachmentServiceMock = new Mock<IAttachmentService>();
        private static HttpResponseMessage message;
        private static byte[] fileBytes = new byte[] { 96, 97, 98, 99, 100 };
        private static string  serializedUpdateAttachmentCommand = "hello";
        private static readonly string attachmentContentId = "ABECA98D65F866DFCD292BC973BDACF5954B916D";
        private static readonly string name = "Attachment";
        private static readonly string fileName = "Attachment.PNG";
        private static readonly Guid responsibleId = Guid.Parse("DDDD0000000000000000000000000000");
        private static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid attachmentId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static string contentType = "image/png";
    }
}