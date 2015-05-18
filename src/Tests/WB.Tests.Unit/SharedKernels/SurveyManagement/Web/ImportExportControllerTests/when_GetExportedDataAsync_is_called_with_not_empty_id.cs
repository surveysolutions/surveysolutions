using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Machine.Specifications;
using Moq;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Services.Export;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Web.ImportExportControllerTests
{
    internal class when_GetExportedDataAsync_is_called_with_not_empty_id : ImportExportControllerTestContext
    {
        Establish context = () =>
        {
            dataExportServiceMock = new Mock<IFilebasedExportedDataAccessor>();
            questionnaireId = Guid.NewGuid();
            dataExportServiceMock.Setup(x => x.GetFilePathToExportedCompressedData(questionnaireId, 1, ExportDataType.Tab)).Returns("hello.txt");
            controller = CreateImportExportController(dataExportServiceMock.Object);
        };

        Because of = () => ExecuteAsync(controller, () => controller.GetAllDataAsync(questionnaireId, 1), () =>
        {
            result = controller.GetAllDataCompleted(controller.AsyncManager.Parameters["result"].ToString()) as FilePathResult;
        });

        It should_DataExportService_be_called_once = () =>
            dataExportServiceMock.Verify(x => x.GetFilePathToExportedCompressedData(questionnaireId, 1, ExportDataType.Tab), Times.Once());

        It should_return_FilePathResult = () =>
            result.ShouldNotBeNull();

        private static ImportExportController controller;
        private static Guid questionnaireId;
        private static Mock<IFilebasedExportedDataAccessor> dataExportServiceMock;
        private static FilePathResult result;
    }
}
