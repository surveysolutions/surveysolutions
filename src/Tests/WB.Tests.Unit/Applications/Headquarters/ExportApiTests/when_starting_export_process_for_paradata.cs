using System.Web.Http;
using System.Web.Http.Results;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.UI.Headquarters.API;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Applications.Headquarters.ExportApiTests
{
    public class when_starting_export_process_for_paradata : ExportControllerTestsContext
    {
        Establish context = () =>
        {
            controller = CreateExportController(dataExportProcessesService: mockOfDataExportProcessesService.Object);
        };

        Because of = () => result = controller.StartProcess(null, "paradata");

        It should_return_http_ok_response = () =>
            result.ShouldBeOfExactType<OkResult>();

        It should_call_add_paradata_export_method_in_data_export_processes_service = () =>
            mockOfDataExportProcessesService.Verify(x=>x.AddParaDataExport(DataExportFormat.Paradata), Times.Once);

        private static ExportController controller;

        private static readonly Mock<IDataExportProcessesService> mockOfDataExportProcessesService =
            new Mock<IDataExportProcessesService>();

        private static IHttpActionResult result;
    }
}