using System;
using Machine.Specifications;
using WB.UI.Designer.Api.Headquarters;

namespace WB.Tests.Unit.Designer.Applications.ImportControllerTests
{
    internal class when_getting_Questionaire_and_request_parameter_is_null : ImportControllerTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            importController = CreateImportController();
            BecauseOf();
        }

        private void BecauseOf() =>
            exception = Catch.Only<ArgumentNullException>(() =>
                importController.Questionnaire(null));

        [NUnit.Framework.Test] public void should_throw_ArgumentNullException () =>
            exception.ShouldNotBeNull();

        private static ImportV2Controller importController;
        private static ArgumentNullException exception;
    }
}
