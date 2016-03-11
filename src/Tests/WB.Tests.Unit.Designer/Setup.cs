using System;
using Main.Core.Documents;
using Microsoft.Practices.ServiceLocation;
using Moq;
using WB.Core.BoundedContexts.Designer.Implementation.Factories;
using WB.Core.BoundedContexts.Designer.Services;

namespace WB.Tests.Unit
{
    internal static class Setup
    {
        public static void InstanceToMockedServiceLocator<TInstance>(TInstance instance)
        {
            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<TInstance>())
                .Returns(instance);
        }

        public static IDesignerEngineVersionService DesignerEngineVersionService(bool isClientVersionSupported = true, bool isQuestionnaireVersionSupported = true, int questionnaireContentVersion = 9)
        {
            var version = new Version(questionnaireContentVersion, 0, 0);

            return Mock.Of<IDesignerEngineVersionService>(_ 
                => _.IsClientVersionSupported(Moq.It.IsAny<Version>()) == isClientVersionSupported
                && _.IsQuestionnaireDocumentSupportedByClientVersion(Moq.It.IsAny<QuestionnaireDocument>(), Moq.It.IsAny<Version>()) == isQuestionnaireVersionSupported
                && _.GetQuestionnaireContentVersion(Moq.It.IsAny<QuestionnaireDocument>()) == version);
        }

        public static Mock<IQuestionnaireEntityFactory> QuestionnaireEntityFactoryWithStaticText(Guid? entityId = null, string text = null, string attachmentName = null)
        {
            var staticText = Create.StaticText(entityId, text, attachmentName);

            var questionnaireEntityFactoryMock = new Mock<IQuestionnaireEntityFactory>();
            if (!entityId.HasValue)
            {
                questionnaireEntityFactoryMock
                   .Setup(x => x.CreateStaticText(Moq.It.IsAny<Guid>(), Moq.It.IsAny<string>(), Moq.It.IsAny<string>()))
                   .Returns((Guid id, string t, string a) => Create.StaticText(id, t, a));
            }
            else if (string.IsNullOrWhiteSpace(attachmentName))
            {
                questionnaireEntityFactoryMock
                    .Setup(x => x.CreateStaticText(entityId.Value, text, Moq.It.IsAny<string>()))
                    .Returns(staticText);
            }
            else
            {
                questionnaireEntityFactoryMock
                   .Setup(x => x.CreateStaticText(entityId.Value, text, attachmentName))
                   .Returns(staticText);
            }

            return questionnaireEntityFactoryMock;
        }
    }
}