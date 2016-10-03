using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Controllers;
using Main.Core.Documents;
using Microsoft.Practices.ServiceLocation;
using Moq;
using NSubstitute;
using WB.Core.BoundedContexts.Designer.Implementation.Factories;
using WB.Core.BoundedContexts.Designer.Implementation.Services.AttachmentService;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Tests.Unit.Designer
{
    internal static class Setup
    {
        public static IAttachmentService AttachmentsServiceForOneQuestionnaire(Guid questionnaireId, params AttachmentView[] attachments)
        {
            var attachmentServiceMock = new Mock<IAttachmentService>();

            attachmentServiceMock.Setup(x => x.GetAttachmentSizesByQuestionnaire(questionnaireId))
                .Returns(attachments.Select(y => new AttachmentSize {Size = y.Content.Size}).ToList());
            
            return attachmentServiceMock.Object;
        }


        public static void InstanceToMockedServiceLocator<TInstance>(TInstance instance)
        {
            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<TInstance>())
                .Returns(instance);
        }

        public static IDesignerEngineVersionService DesignerEngineVersionService(bool isClientVersionSupported = true, bool isQuestionnaireVersionSupported = true, int questionnaireContentVersion = 9)
        {
            return Mock.Of<IDesignerEngineVersionService>(_ 
                => _.IsClientVersionSupported(Moq.It.IsAny<int>()) == isClientVersionSupported
                && _.GetListOfNewFeaturesForClient(Moq.It.IsAny<QuestionnaireDocument>(), Moq.It.IsAny<int>()) == (isQuestionnaireVersionSupported ? new string[0] : new []{"New questionnaire feature"})
                && _.GetQuestionnaireContentVersion(Moq.It.IsAny<QuestionnaireDocument>()) == questionnaireContentVersion);
        }

        public static Mock<IQuestionnaireEntityFactory> QuestionnaireEntityFactoryWithStaticText(Guid? entityId = null, string text = null, string attachmentName = null)
        {
            var staticText = Create.StaticText(entityId, text, attachmentName);

            var questionnaireEntityFactoryMock = new Mock<IQuestionnaireEntityFactory>();
            if (!entityId.HasValue)
            {
                questionnaireEntityFactoryMock
                   .Setup(x => x.CreateStaticText(Moq.It.IsAny<Guid>(), Moq.It.IsAny<string>(), Moq.It.IsAny<string>(), Moq.It.IsAny<string>(), Moq.It.IsAny<bool>(), Moq.It.IsAny<IList<ValidationCondition>>()))
                   .Returns((Guid id, string t, string a, string c, bool h, IList<ValidationCondition> v) => Create.StaticText(id, t, a));
            }
            else if (string.IsNullOrWhiteSpace(attachmentName))
            {
                questionnaireEntityFactoryMock
                    .Setup(x => x.CreateStaticText(entityId.Value, text, Moq.It.IsAny<string>(), Moq.It.IsAny<string>(), Moq.It.IsAny<bool>(), Moq.It.IsAny<IList<ValidationCondition>>()))
                    .Returns(staticText);
            }
            else
            {
                questionnaireEntityFactoryMock
                   .Setup(x => x.CreateStaticText(entityId.Value, text, attachmentName, Moq.It.IsAny<string>(), Moq.It.IsAny<bool>(), Moq.It.IsAny<IList<ValidationCondition>>()))
                   .Returns(staticText);
            }

            return questionnaireEntityFactoryMock;
        }

        public static void CommandApiControllerToAcceptAttachment(
            ApiController controller, 
            byte[] fileContent, 
            MediaTypeHeaderValue contentType, 
            string serializedCommand,
            string fileParamName = "file",
            string commandParamName = "command")
        {
            var binaryContent = new ByteArrayContent(fileContent);

            binaryContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                Name = fileParamName
            };
            binaryContent.Headers.ContentType = contentType;

            var commandContent = new StringContent(serializedCommand);
            commandContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                Name = commandParamName
            };

            var multipartContent = new MultipartContent { binaryContent, commandContent };

            var request = new HttpRequestMessage
            {
                Content = multipartContent
            };
            request.SetConfiguration(new HttpConfiguration());

            var controllerContext = new HttpControllerContext
            {
                Request = request
            };

            controller.ControllerContext = controllerContext;
        }

        public static void ServiceLocatorForAttachmentService(IPlainStorageAccessor<AttachmentContent> attachmentContentStorage, IPlainStorageAccessor<AttachmentMeta> attachmentMetaStorage)
        {
            var serviceLocatorMock = new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock };

            serviceLocatorMock
                .Setup(locator => locator.GetInstance<IPlainStorageAccessor<AttachmentContent>>())
                .Returns(attachmentContentStorage);

            serviceLocatorMock
                .Setup(locator => locator.GetInstance<IPlainStorageAccessor<AttachmentMeta>>())
                .Returns(attachmentMetaStorage);

            ServiceLocator.SetLocatorProvider(() => serviceLocatorMock.Object);
        }

        public static IPlainKeyValueStorage<QuestionnaireStateTracker> QuestionnaireStateTrackerForOneQuestionnaire()
        {
            return Mock.Of<IPlainKeyValueStorage<QuestionnaireStateTracker>>(_ => 
                _.GetById(It.IsAny<string>()) == Create.QuestionnaireStateTacker());
        }

        public static IQuestionnaireTranslator QuestionnaireTranslator(QuestionnaireDocument questionnaireDocument, ITranslation translation, QuestionnaireDocument translatedQuestionnaireDocument)
        {
            var serviceMock = new Mock<IQuestionnaireTranslator>();
            serviceMock.Setup(x => x.Translate(questionnaireDocument, translation))
                 .Returns(translatedQuestionnaireDocument);

            return serviceMock.Object;
        }
    }
}