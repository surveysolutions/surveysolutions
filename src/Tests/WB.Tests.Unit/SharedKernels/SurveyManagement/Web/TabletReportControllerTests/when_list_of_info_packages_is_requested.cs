using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Machine.Specifications;
using WB.Core.SharedKernels.SurveyManagement.Views.TabletInformation;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Web.TabletReportControllerTests
{
    internal class when_list_of_info_packages_is_requested : TabletReportControllerTestContext
    {
        Establish context = () =>
        {
            tabletReportController = CreateTabletReportController();
        };

        Because of = () => result = tabletReportController.PackagesInfo() as ViewResult;

        It should_return_ViewResult = () => result.ShouldNotBeNull();

        
        private static TabletReportController tabletReportController;
        private static ViewResult result;
    }
}
