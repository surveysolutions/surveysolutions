using System.Net;
using System.Web.Http;
using System.Web.Http.Results;
using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.UI.Headquarters.API;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Applications.Headquarters.ExportApiTests
{
    public class when_cancelling_export_process_with_unexpected_export_type : ExportControllerTestsContext
    {
        Establish context = () =>
        {
            controller = CreateExportController();
        };

        Because of = () => result = controller.CancelProcess(new QuestionnaireIdentity().ToString(), "unexpected export type");

        It should_return_http_not_found_response = () =>
            ((NegotiatedContentResult<string>)result).StatusCode.ShouldEqual(HttpStatusCode.NotFound);

        It should_response_has_specified_message = () =>
            ((NegotiatedContentResult<string>)result).Content.ShouldEqual("Unknown export type");

        private static ExportController controller;

        private static IHttpActionResult result;
    }
}