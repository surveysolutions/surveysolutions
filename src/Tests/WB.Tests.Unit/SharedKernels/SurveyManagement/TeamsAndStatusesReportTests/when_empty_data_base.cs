using Machine.Specifications;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.InputModels;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.TeamsAndStatusesReportTests
{
    internal class when_empty_data_base : TeamsAndStatusesReportContext
    {
        Establish context = () =>
        {
            reportFactory = CreateTeamsAndStatusesReport();
        };

        private Because of = () => report = reportFactory.Load(new TeamsAndStatusesInputModel());

        private It should_return_0_records = () => report.TotalCount.ShouldEqual(0);

        private static TeamsAndStatusesReport reportFactory;
        private static TeamsAndStatusesReportView report;
    }
}

