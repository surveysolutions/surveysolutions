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
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.UI.Headquarters.Controllers;
using It = Machine.Specifications.It;

namespace WB.UI.Headquarters.Tests.ImportExportControllerTests
{
    internal class when_GetExportedDataAsync_is_called_with_not_empty_id : ImportExportControllerTestContext
    {
        Establish context = () =>
        {
            dataExportServiceMock=new Mock<IDataExportService>();
            questionnaireId = Guid.NewGuid();
            dataExportServiceMock.Setup(x => x.GetFilePathToExportedCompressedData(questionnaireId, 1)).Returns("hello.txt");
            controller = CreateImportExportController(dataExportServiceMock.Object);
        };

        Because of = () => ExecuteAsync(controller, () => controller.GetExportedDataAsync(questionnaireId, 1), () =>
        {
            result = controller.GetExportedDataCompleted(controller.AsyncManager.Parameters["result"].ToString()) as FilePathResult;
        });

        It should_DataExportService_be_called_once = () =>
            dataExportServiceMock.Verify(x => x.GetFilePathToExportedCompressedData(questionnaireId, 1), Times.Once());

        It should_return_FilePathResult = () =>
            result.ShouldNotBeNull();

        private static ImportExportController controller;
        private static Guid questionnaireId;
        private static Mock<IDataExportService> dataExportServiceMock;
        private static FilePathResult result;
    }
}
