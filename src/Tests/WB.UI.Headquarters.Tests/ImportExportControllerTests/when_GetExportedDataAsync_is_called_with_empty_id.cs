using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Machine.Specifications;
using Main.Core.View;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.UI.Headquarters.Controllers;

namespace WB.UI.Headquarters.Tests.ImportExportControllerTests
{
    internal class when_GetExportedDataAsync_is_called_with_empty_id : ImportExportControllerTestContext
    {
        Establish context = () =>
        {
            controller = CreateImportExportController();
        };

        Because of = () => { expectedException = Catch.Exception(() => controller.GetExportedDataAsync(Guid.Empty, 1)) as HttpException; };

        It should_throw_HttpException = () =>
            expectedException.ShouldNotBeNull();

        It should_throw_404_HttpException = () =>
            expectedException.GetHttpCode().ShouldEqual(404);

        private static ImportExportController controller;
        private static HttpException expectedException;
    }
}
