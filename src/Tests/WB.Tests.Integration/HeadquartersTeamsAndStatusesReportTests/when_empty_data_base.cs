using Machine.Specifications;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.InputModels;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.Infrastructure.Transactions;

namespace WB.Tests.Integration.HeadquartersTeamsAndStatusesReportTests
{
    internal class when_empty_data_base : HeadquartersTeamsAndStatusesReportContext
    {
        Establish context = () =>
        {
            reportFactory = CreateTeamsAndStatusesReport();
        };

        Because of = () => report = postgresTransactionManager.ExecuteInQueryTransaction(() => reportFactory.Load(new TeamsAndStatusesInputModel()));

        It should_return_0_records = () => report.TotalCount.ShouldEqual(0);

        private static HeadquartersTeamsAndStatusesReport reportFactory;
        private static TeamsAndStatusesReportView report;
    }
}

