using System;
using Machine.Specifications;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;
using WB.UI.Designer.Api;

namespace WB.Tests.Unit.Applications.Designer.TesterApiControllerTests
{
    internal class when_getting_template_with_id_only: TesterApiControllerTestContext
    {
        Establish context = () =>
        {
            controller = CreateQuestionnaireController();
        };

        private Because of = () => result = controller.GetTemplate(id);

        private It should_package_contains_error = () => 
            result.IsErrorOccured.ShouldBeTrue();

        private It should_package_contains_error_message = () =>
            result.ErrorMessage.ShouldEqual("You have an old version of application. Please update application to continue.");
        
        private static TesterController controller;
        private static Guid id = Guid.Parse("11111111111111111111111111111111");
        private static QuestionnaireCommunicationPackage result;
    }
}
