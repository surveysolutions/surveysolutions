﻿using System.Net;
using System.Web.Http;
using System.Web.Http.Results;
using Machine.Specifications;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.UI.Headquarters.API.PublicApi;

namespace WB.Tests.Unit.Applications.Headquarters.ExportApiTests
{
    public class when_starting_export_process_and_questionnaire_identity_is_invalid : ExportControllerTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            controller = CreateExportController();
            BecauseOf();
        }

        Because BecauseOf = () => result = controller.StartProcess("invalid questionnaire identity", DataExportFormat.Tabular);

        [NUnit.Framework.Test] public void should_return_http_bad_request_response () =>
            ((NegotiatedContentResult<string>)result).StatusCode.ShouldEqual(HttpStatusCode.BadRequest);

        [NUnit.Framework.Test] public void should_response_has_specified_message () =>
            ((NegotiatedContentResult<string>)result).Content.ShouldEqual("Invalid questionnaire identity");

        private static ExportController controller;

        private static IHttpActionResult result;
    }
}