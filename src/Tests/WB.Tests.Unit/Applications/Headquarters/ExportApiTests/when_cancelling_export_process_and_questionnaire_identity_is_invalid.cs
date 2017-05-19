using System.Net;
using System.Web.Http;
using System.Web.Http.Results;
using Machine.Specifications;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.UI.Headquarters.API;
using WB.UI.Headquarters.API.PublicApi;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Applications.Headquarters.ExportApiTests
{
    public class when_cancelling_export_process_and_questionnaire_identity_is_invalid : ExportControllerTestsContext
    {
        Establish context = () =>
        {
            controller = CreateExportController();
        };

        Because of = () => result = controller.CancelProcess("invalid questionnaire identity", DataExportFormat.Tabular);

        It should_return_http_bad_request_response = () =>
            ((NegotiatedContentResult<string>)result).StatusCode.ShouldEqual(HttpStatusCode.BadRequest);

        It should_response_has_specified_message = () =>
            ((NegotiatedContentResult<string>)result).Content.ShouldEqual("Invalid questionnaire identity");

        private static ExportController controller;

        private static IHttpActionResult result;
    }
}