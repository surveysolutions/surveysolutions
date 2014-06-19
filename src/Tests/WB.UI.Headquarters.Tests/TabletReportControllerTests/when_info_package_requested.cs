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
    internal class when_info_package_requested: TabletReportControllerTestContext
    {
        Establish context = () =>
        {
            tabletReportController = CreateTabletReportController();
        };

        Because of = () => result = tabletReportController.DownloadPackages(packageName) as FilePathResult;

        It should_return_FilePathResult = () =>
         result.ShouldNotBeNull();

        It should_file_name_be_equal_to_passed = () =>
            result.FileDownloadName.ShouldEqual(packageName);

        private static TabletReportController tabletReportController;
        private const string packageName = "package name";
        private static FilePathResult result;
    }
}
