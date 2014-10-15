using Main.Core.Documents;
using Microsoft.Practices.ServiceLocation;
using Moq;
using WB.Core.BoundedContexts.Designer.Services;

namespace WB.Core.BoundedContexts.Designer.Tests
{
    internal static class Setup
    {
        public static void InstanceToMockedServiceLocator<TInstance>(TInstance instance)
        {
            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<TInstance>())
                .Returns(instance);
        }

        public static void SimpleQuestionnaireDocumentUpgraderToMockedServiceLocator()
        {
            var questionnaireDocumentUpgrader = Mock.Of<IQuestionnaireDocumentUpgrader>();

            Mock.Get(questionnaireDocumentUpgrader)
                .Setup(upgrader => upgrader.TranslatePropagatePropertiesToRosterProperties(It.IsAny<QuestionnaireDocument>()))
                .Returns<QuestionnaireDocument>(questionnaireDocument => questionnaireDocument);

            Setup.InstanceToMockedServiceLocator(questionnaireDocumentUpgrader);
        }
    }
}