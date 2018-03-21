using System;
using System.Web.Http;
using System.Web.Http.Results;
using FluentAssertions;
using Moq;
using WB.Core.BoundedContexts.Headquarters.DataExport.DataExportDetails;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.UI.Headquarters.API.PublicApi;

namespace WB.Tests.Unit.Applications.Headquarters.ExportApiTests
{
    public class when_starting_export_process_for_tabular_export_type : ExportControllerTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            controller = CreateExportController(dataExportProcessesService: mockOfDataExportProcessesService.Object);
            BecauseOf();
        }

        public void BecauseOf
            () => result = controller.StartProcess(questionnaireIdentity.ToString(), DataExportFormat.Tabular);

        [NUnit.Framework.Test] public void should_return_http_ok_response () =>
            result.Should().BeOfType<OkResult>();

        [NUnit.Framework.Test] public void should_call_add_export_method_in_data_export_processes_service () =>
            mockOfDataExportProcessesService.Verify(x=>x.AddDataExport(Moq.It.IsAny<DataExportProcessDetails>()), Times.Once);

        private static ExportController controller;

        private static readonly Mock<IDataExportProcessesService> mockOfDataExportProcessesService =
            new Mock<IDataExportProcessesService>();

        private static IHttpActionResult result;
        private static readonly QuestionnaireIdentity questionnaireIdentity = new QuestionnaireIdentity(Guid.Parse("11111111111111111111111111111111"), 1);
    }
}
