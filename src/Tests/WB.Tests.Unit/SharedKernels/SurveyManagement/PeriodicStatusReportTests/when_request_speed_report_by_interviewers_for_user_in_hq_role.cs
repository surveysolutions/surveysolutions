using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Machine.Specifications;
using Moq;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.PeriodicStatusReportTests
{
    internal class when_request_speed_report_by_interviewers_for_user_in_hq_role : PeriodicStatusReportControllerTestContext
    {
        Establish context = () =>
        {
            IGlobalInfoProvider globalInfoProvider = Mock.Of<IGlobalInfoProvider>(_ => _.IsHeadquarter == true);
            periodicStatusReportController = CreatePeriodicStatusReportController(globalInfoProvider: globalInfoProvider);
        };

        Because of = () =>
            result = periodicStatusReportController.SpeedByInterviewers(null) as ViewResult;

        It should_active_page_be_SpeedOfCompletingInterviews = () =>
            ((MenuItem)result.ViewBag.ActivePage).ShouldEqual(MenuItem.SpeedOfCompletingInterviews);

        It should_responsible_name_be_not_a_link = () =>
            ((PeriodicStatusReportModel)result.Model).CanNavigateToQuantityByTeamMember.ShouldEqual(false);

        It should_go_back_to_supervisor_button_be_visible = () =>
            ((PeriodicStatusReportModel)result.Model).CanNavigateToQuantityBySupervisors.ShouldEqual(true);

        It should_WebApiActionName_be_ByInterviewers = () =>
          ((PeriodicStatusReportModel)result.Model).WebApiActionName.ShouldEqual(PeriodicStatusReportWebApiActionName.ByInterviewers);

        It should_ReportName_be_Speed = () =>
           ((PeriodicStatusReportModel)result.Model).ReportName.ShouldEqual("Speed");

        private static PeriodicStatusReportController periodicStatusReportController;
        private static ViewResult result;
    }
}
