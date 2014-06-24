using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Machine.Specifications;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.UI.Headquarters.Controllers;

namespace WB.UI.Headquarters.Tests.TabletReportControllerTests
{
    internal class when_posting_info_package_is_caller_with_empty_content : TabletReportControllerTestContext
    {
        Establish context = () =>
        {
            tabletReportController = CreateTabletReportController();
        };

        Because of = () => result = tabletReportController.PostInfoPackage() as JsonResult;

        It should_return_JsonResult = () => result.ShouldNotBeNull();

        It should_return_false = () => result.Data.ShouldEqual(false);

        private static TabletReportController tabletReportController;
        private static JsonResult result;
    }
}
