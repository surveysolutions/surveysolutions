using System.Net;
using System.Net.Http;
using System.Web.Mvc;
using Machine.Specifications;
using Ncqrs.Commanding;
using WB.UI.Designer.Api;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Designer.NCalcToSharpControllerTests
{
    internal class when_getting_status
    {
        Establish context = () =>
        {
            controller = Create.NCalcToSharpController();
        };

        Because of = () =>
            result = controller.Get();

        It should_return_http_response_OK = () =>
            result.StatusCode.ShouldEqual(HttpStatusCode.OK);

        It should_return_message_containing__no_operations_were_performed__ = () =>
            result.Content.ReadAsStringAsync().Result.ShouldContain("no operations were performed");

        private static NCalcToSharpController controller;
        private static HttpResponseMessage result;
    }
}