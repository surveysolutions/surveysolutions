using Machine.Specifications;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.InputModels;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.SupervisorTeamsAndStatusesReportTests
{
    internal class when_empty_data_base : SupervisorTeamsAndStatusesReportContext
    {
        Establish context = () =>
        {
            reportFactory = CreateTeamsAndStatusesReport();
        };

        private Because of = () => report = reportFactory.Load(new TeamsAndStatusesInputModel());

        private It should_return_0_records = () => report.TotalCount.ShouldEqual(0);

        private static SupervisorTeamsAndStatusesReport reportFactory;
        private static TeamsAndStatusesReportView report;
    }
}

