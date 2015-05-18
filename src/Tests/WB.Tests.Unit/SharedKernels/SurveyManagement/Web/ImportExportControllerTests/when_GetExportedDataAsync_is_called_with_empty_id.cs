using System;
using System.Web;
using Machine.Specifications;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Web.ImportExportControllerTests
{
    internal class when_GetExportedDataAsync_is_called_with_empty_id : ImportExportControllerTestContext
    {
        Establish context = () =>
        {
            controller = CreateImportExportController();
        };

        Because of = () => { expectedException = Catch.Exception(() => controller.GetAllDataAsync(Guid.Empty, 1)) as HttpException; };

        It should_throw_HttpException = () =>
            expectedException.ShouldNotBeNull();

        It should_throw_404_HttpException = () =>
            expectedException.GetHttpCode().ShouldEqual(404);

        private static ImportExportController controller;
        private static HttpException expectedException;
    }
}
