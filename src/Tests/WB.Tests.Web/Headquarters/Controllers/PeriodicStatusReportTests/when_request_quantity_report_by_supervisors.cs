using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Tests.Web;
using WB.UI.Headquarters.Controllers;


namespace WB.Tests.Unit.SharedKernels.SurveyManagement.PeriodicStatusReportTests
{
    internal class when_request_quantity_report_by_supervisors
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            reportController = Create.Controller.ReportsController();
            BecauseOf();
        }

        public void BecauseOf() =>
            result = reportController.QuantityBySupervisors(PeriodiceReportType.NumberOfCompletedInterviews) as ViewResult;

        [NUnit.Framework.Test] public void should_responsible_name_be_a_link () =>
            ((PeriodicStatusReportModel)result.Model).CanNavigateToQuantityByTeamMember.Should().Be(true);

        [NUnit.Framework.Test] public void should_go_back_to_supervisor_button_be_invisible () =>
            ((PeriodicStatusReportModel)result.Model).CanNavigateToQuantityBySupervisors.Should().Be(false);

        [NUnit.Framework.Test] public void should_ReportName_be_Quantity () =>
           ((PeriodicStatusReportModel)result.Model).ReportName.Should().Be("Quantity");

        private static ReportsController reportController;
        private static ViewResult result;
    }
}
