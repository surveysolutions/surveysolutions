using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Machine.Specifications;
using Moq;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Web.TabletReportControllerTests
{
    internal class when_info_package_requested: TabletReportControllerTestContext
    {
        Establish context = () =>
        {
            var tabletInformationService = new Mock<ITabletInformationService>();
            tabletInformationService.Setup(x => x.GetPackageNameWithoutRegistrationId(packageName)).Returns(packageName);
            tabletInformationService.Setup(x => x.GetFullPathToContentFile(packageName)).Returns(packageName);

            tabletReportController = CreateTabletReportController(tabletInformationService.Object);
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
