using Machine.Specifications;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.InputModels;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.Infrastructure.Transactions;

namespace WB.Tests.Integration.ReportTests.TeamsAndStatusesTests.Hq
{
    internal class when_empty_data_base : TeamsAndStatusesReportContext
    {
        Establish context = () =>
        {
            reportFactory = CreateHqTeamsAndStatusesReport();
        };

        Because of = () => report = postgresTransactionManager.ExecuteInQueryTransaction(() => reportFactory.GetBySupervisors(new TeamsAndStatusesInputModel()));

        It should_return_0_records = () => report.TotalCount.ShouldEqual(0);

        private static TeamsAndStatusesReport reportFactory;
        private static TeamsAndStatusesReportView report;
    }
}

