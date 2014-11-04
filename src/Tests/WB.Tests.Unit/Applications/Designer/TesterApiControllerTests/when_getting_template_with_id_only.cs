using System;
using System.Net;
using System.Net.Http;
using Machine.Specifications;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;
using WB.UI.Designer.Api;

namespace WB.Tests.Unit.Applications.Designer.TesterApiControllerTests
{
    [Ignore("Solve resources referense issue")]
    internal class when_getting_template_with_id_only: TesterApiControllerTestContext
    {
        Establish context = () =>
        {
            controller = CreateQuestionnaireController();
        };

        private Because of = () => result = controller.GetTemplate(id);

        private It should_return_error_response_code = () => 
            result.StatusCode.ShouldEqual(HttpStatusCode.Forbidden);

        /*private It should_return_user_friendly_error_message = () =>
            result.ErrorMessage.ShouldEqual("You have an old version of application. Please update application to continue.");
*/        
        private static TesterController controller;
        private static Guid id = Guid.Parse("11111111111111111111111111111111");
        private static HttpResponseMessage result;
    }
}
