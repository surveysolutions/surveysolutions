using System;
using Machine.Specifications;
using Moq;
using WB.Core.SharedKernels.SurveyManagement.Services.Export;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Web.ImportExportControllerTests
{
    internal class when_GetExportedDataAsync_is_called_and_exception_occured : ImportExportControllerTestContext
    {
        Establish context = () =>
        {
            questionnaireId = Guid.NewGuid();
            dataExportServiceMock = new Mock<IFilebasedExportedDataAccessor>();
            dataExportServiceMock.Setup(x => x.GetFilePathToExportedCompressedData(questionnaireId, 1, ExportDataType.Tab)).Throws<NullReferenceException>();
            
            controller = CreateImportExportController(dataExportServiceMock.Object);
        };

        Because of = () => ExecuteAsync(controller, () => controller.GetAllDataAsync(questionnaireId, 1), () =>
        {
            Result = controller.AsyncManager.Parameters["result"];
        });

        It should_have_null_result = () =>
            Result.ShouldBeNull();

        private static ImportExportController controller;
        private static Guid questionnaireId;
        private static object Result;
        private static Mock<IFilebasedExportedDataAccessor> dataExportServiceMock;

    }
}
