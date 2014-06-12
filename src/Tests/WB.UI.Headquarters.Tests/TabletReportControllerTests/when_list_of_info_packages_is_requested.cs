using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Machine.Specifications;
using WB.Core.SharedKernels.SurveyManagement.Views.TabletInformation;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.UI.Headquarters.Controllers;

namespace WB.UI.Headquarters.Tests.TabletReportControllerTests
{
    internal class when_list_of_info_packages_is_requested : TabletReportControllerTestContext
    {
        Establish context = () =>
        {
            tabletReportController = CreateTabletReportController();
        };

        Because of = () => result = tabletReportController.Packages() as ViewResult;

        It should_return_ViewResult = () => result.ShouldNotBeNull();

        It should_return_model_of_type_List_ofTabletInformationView = () =>
            result.Model.ShouldBeOfExactType<List<TabletInformationView>>();

        private static TabletReportController tabletReportController;
        private static ViewResult result;
    }
}
