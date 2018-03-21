using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using FluentAssertions;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;


namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Web.TabletReportControllerTests
{
    internal class when_info_package_requested: TabletReportControllerTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var tabletInformationService = new Mock<ITabletInformationService>();
            tabletInformationService.Setup(x => x.GetFileName(packageName, Moq.It.IsAny<string>())).Returns(packageName);
            tabletInformationService.Setup(x => x.GetFullPathToContentFile(packageName)).Returns(packageName);

            tabletReportController = CreateTabletReportController(tabletInformationService.Object);
            BecauseOf();
        }

        public void BecauseOf() => result = tabletReportController.DownloadPackages(packageName) as FilePathResult;

        [NUnit.Framework.Test] public void should_return_FilePathResult () =>
         result.Should().NotBeNull();

        [NUnit.Framework.Test] public void should_file_name_be_equal_to_passed () =>
            result.FileDownloadName.Should().Be(packageName);

        private static TabletReportController tabletReportController;
        private const string packageName = "package name";
        private static FilePathResult result;
    }
}
