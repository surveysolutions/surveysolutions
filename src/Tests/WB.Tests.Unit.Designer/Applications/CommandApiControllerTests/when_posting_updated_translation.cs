using System;
using Machine.Specifications;
using Moq;
using MultipartDataMediaFormatter.Infrastructure;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Translations;
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.UI.Designer.Api;
using WB.UI.Shared.Web.CommandDeserialization;


namespace WB.Tests.Unit.Designer.Applications.CommandApiControllerTests
{
    internal class when_posting_updated_translation : CommandApiControllerTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var addOrUpdateTranslation = Create.Command.AddOrUpdateTranslation(questionnaireId, translationId, name, responsibleId);

            var commandDeserializerMock = new Mock<ICommandDeserializer>();
            
            commandDeserializerMock
                .Setup(x => x.Deserialize(typeof (AddOrUpdateTranslation).Name, serializedUpdateTranslationCommand))
                .Returns(addOrUpdateTranslation);
            
            controller = CreateCommandController(
                commandDeserializer: commandDeserializerMock.Object,
                translationsService: translationServiceMock.Object,
                commandService: mockOfCommandService.Object);
            BecauseOf();
        }

        private void BecauseOf() =>
            controller.UpdateTranslation(new CommandController.TranslationModel { File = new HttpFile { Buffer = fileBytes, FileName = fileName }, Command = serializedUpdateTranslationCommand });

        [NUnit.Framework.Test] public void should_save_translation_with_specified_params () =>
            translationServiceMock.Verify(
                x => x.Store(questionnaireId, translationId, fileBytes), Times.Once);

        [NUnit.Framework.Test] public void should_execute_AddOrUpdateTranslation_command () =>
            mockOfCommandService.Verify(
                x => x.Execute(Moq.It.IsAny<AddOrUpdateTranslation>(), Moq.It.IsAny<string>()), Times.Once);

        private static CommandController controller;
        private static readonly Mock<ITranslationsService> translationServiceMock = new Mock<ITranslationsService>();
        private static readonly Mock<ICommandService> mockOfCommandService = new Mock<ICommandService>();
        private static string serializedUpdateTranslationCommand = "hello";
        private static byte[] fileBytes = new byte[] { 96, 97, 98, 99, 100 };
        private static readonly string name = "Translation";
        private static readonly string fileName = "Translation.xls";
        private static readonly Guid responsibleId = Guid.Parse("DDDD0000000000000000000000000000");
        private static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid translationId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
    }
}