using System;
using System.Web.Http;
using System.Web.Http.Results;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.UI.Headquarters.API;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Applications.Headquarters.ExportApiTests
{
    public class when_cancelling_export_process : ExportControllerTestsContext
    {
        Establish context = () =>
        {
            controller = CreateExportController(dataExportProcessesService: mockOfDataExportProcessesService.Object);
        };

        Because of = () => result = controller.CancelProcess(questionnaireIdentity.ToString(), "tabular");

        It should_return_http_ok_response = () =>
            result.ShouldBeOfExactType<OkResult>();

        It should_call_add_export_method_in_data_export_processes_service = () =>
            mockOfDataExportProcessesService.Verify(x=>x.DeleteProcess(questionnaireIdentity, DataExportFormat.Tabular, DataExportType.Data), Times.Once);

        private static ExportController controller;

        private static readonly Mock<IDataExportProcessesService> mockOfDataExportProcessesService =
            new Mock<IDataExportProcessesService>();

        private static IHttpActionResult result;
        private static readonly QuestionnaireIdentity questionnaireIdentity = new QuestionnaireIdentity(Guid.Parse("11111111111111111111111111111111"), 1);
    }
}