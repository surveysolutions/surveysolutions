using System;
using Machine.Specifications;
using WB.UI.Designer.Api;

namespace WB.Tests.Unit.Designer.Applications.ImportControllerTests
{
    internal class when_getting_Questionaire_and_request_parameter_is_null : ImportControllerTestContext
    {
        Establish context = () =>
        {
            importController = CreateImportController();
        };

        Because of = () =>
            exception = Catch.Only<ArgumentNullException>(() =>
                importController.Questionnaire(null));

        It should_throw_ArgumentNullException = () =>
            exception.ShouldNotBeNull();

        private static ImportV2Controller importController;
        private static ArgumentNullException exception;
    }
}
