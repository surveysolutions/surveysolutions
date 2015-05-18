using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Machine.Specifications;
using WB.UI.Designer.Api;

namespace WB.Tests.Unit.Applications.Designer.ImportControllerTests
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

        private static ImportController importController;
        private static ArgumentNullException exception;
    }
}
