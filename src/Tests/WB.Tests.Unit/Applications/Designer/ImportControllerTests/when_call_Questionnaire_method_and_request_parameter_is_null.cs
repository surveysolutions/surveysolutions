using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using WB.UI.Designer.Api;

namespace WB.Tests.Unit.Applications.Designer.ImportControllerTests
{
    internal class when_call_Questionnaire_method_and_request_parameter_is_null : ImportControllerTestContext
    {
        Establish context = () =>
        {
            importController = CreateImportController();
        };
        
        Because of = () =>
            exception = Catch.Exception(() =>
                importController.Questionnaire(null));

        It should_throw_ArgumentNullException = () =>
            exception.ShouldBeOfExactType<ArgumentNullException>();

        private static ImportController importController;
        private static Exception exception;
    }
}
