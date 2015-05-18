using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Machine.Specifications;
using Moq;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Services.Export;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Web.ImportExportControllerTests
{
    internal class when_GetExportedFilesAsync_is_called : ImportExportControllerTestContext
    {
        Establish context = () =>
        {
            dataExportServiceMock = new Mock<IFilebasedExportedDataAccessor>();
            questionnaireId = Guid.NewGuid();
            dataExportServiceMock.Setup(x => x.GetFilePathToExportedBinaryData(questionnaireId, 1)).Returns("hello.txt");
            controller = CreateImportExportController(dataExportServiceMock.Object);
        };

        Because of = () => ExecuteAsync(controller, () => controller.GetFilesAsync(questionnaireId, 1), () =>
        {
            result = controller.GetFilesCompleted(controller.AsyncManager.Parameters["result"].ToString()) as FilePathResult;
        });

        It should_DataExportService_be_called_once = () =>
            dataExportServiceMock.Verify(x => x.GetFilePathToExportedBinaryData(questionnaireId, 1), Times.Once());

        It should_return_FilePathResult = () =>
            result.ShouldNotBeNull();

        private static ImportExportController controller;
        private static Guid questionnaireId;
        private static Mock<IFilebasedExportedDataAccessor> dataExportServiceMock;
        private static FilePathResult result;
    }
}
