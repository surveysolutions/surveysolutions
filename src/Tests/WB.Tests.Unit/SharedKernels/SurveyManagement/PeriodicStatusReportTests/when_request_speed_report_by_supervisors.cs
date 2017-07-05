using System.Web.Mvc;
using Machine.Specifications;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Tests.Abc;
using WB.UI.Headquarters.Controllers;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.PeriodicStatusReportTests
{
    internal class when_request_speed_report_by_supervisors
    {
        Establish context = () =>
        {
            reportController = Create.Controller.ReportsController();
        };

        Because of = () =>
            result = reportController.SpeedBySupervisors() as ViewResult;

        It should_active_page_be_SpeedOfCompletedInterviews = () =>
            ((MenuItem)result.ViewBag.ActivePage).ShouldEqual(MenuItem.SpeedOfCompletingInterviews);

        It should_responsible_name_be_a_link = () =>
            ((PeriodicStatusReportModel)result.Model).CanNavigateToQuantityByTeamMember.ShouldEqual(true);

        It should_go_back_to_supervisor_button_be_invisible = () =>
            ((PeriodicStatusReportModel)result.Model).CanNavigateToQuantityBySupervisors.ShouldEqual(false);

        It should_WebApiActionName_be_BySupervisors = () =>
           ((PeriodicStatusReportModel)result.Model).WebApiActionName.ShouldEqual(PeriodicStatusReportWebApiActionName.BySupervisors);

        It should_ReportName_be_Speed = () =>
           ((PeriodicStatusReportModel)result.Model).ReportName.ShouldEqual("Speed");

        private static ReportsController reportController;
        private static ViewResult result;
    }
}
