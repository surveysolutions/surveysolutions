using System;
using System.Web.Http;
using System.Web.Http.Results;
using FluentAssertions;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.UI.Headquarters.API;
using WB.UI.Headquarters.API.PublicApi;

namespace WB.Tests.Unit.Applications.Headquarters.ExportApiTests
{
    public class when_cancelling_export_process : ExportControllerTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            controller = CreateExportController(dataExportProcessesService: mockOfDataExportProcessesService.Object);
            BecauseOf();
        }

        private void BecauseOf() => result = controller.CancelProcess(questionnaireIdentity.ToString(), DataExportFormat.Tabular);

        [NUnit.Framework.Test] public void should_return_http_ok_response () =>
            result.Should().BeOfType<OkResult>();

        [NUnit.Framework.Test] public void should_call_add_export_method_in_data_export_processes_service () =>
            mockOfDataExportProcessesService.Verify(x=>x.DeleteDataExport(Moq.It.IsAny<string>()), Times.Once);

        private static ExportController controller;

        private static readonly Mock<IDataExportProcessesService> mockOfDataExportProcessesService =
            new Mock<IDataExportProcessesService>();

        private static IHttpActionResult result;
        private static readonly QuestionnaireIdentity questionnaireIdentity = new QuestionnaireIdentity(Guid.Parse("11111111111111111111111111111111"), 1);
    }
}
