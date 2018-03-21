using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using FluentAssertions;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Web.TabletReportControllerTests
{
    internal class when_list_of_info_packages_is_requested : TabletReportControllerTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            tabletReportController = CreateTabletReportController();
            BecauseOf();
        }

        public void BecauseOf() => result = tabletReportController.PackagesInfo() as ViewResult;

        [NUnit.Framework.Test] public void should_return_ViewResult () => result.Should().NotBeNull();

        
        private static TabletReportController tabletReportController;
        private static ViewResult result;
    }
}
