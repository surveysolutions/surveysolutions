using System;
using System.Web.Http;
using System.Web.Http.Results;
using FluentAssertions;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.UI.Headquarters.API.PublicApi;

namespace WB.Tests.Unit.Applications.Headquarters.ExportApiTests
{
    public class when_starting_export_process_for_ddi_export_type : ExportControllerTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            controller = CreateExportController();
            BecauseOf();
        }

        public void BecauseOf() => result = controller.StartProcess(new QuestionnaireIdentity(Guid.Parse("11111111111111111111111111111111"), 1).ToString(), DataExportFormat.DDI);

        [NUnit.Framework.Test] public void should_return_http_bad_request_response () =>
            result.Should().BeOfType<BadRequestErrorMessageResult>();

        [NUnit.Framework.Test] public void should_response_has_specified_message () =>
            ((BadRequestErrorMessageResult)result).Message.Should().Be("Not supported export type");

        private static ExportController controller;

        private static IHttpActionResult result;
    }
}
