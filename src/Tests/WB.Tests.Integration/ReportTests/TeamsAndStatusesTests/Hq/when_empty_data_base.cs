using FluentAssertions;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.InputModels;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.Infrastructure.Transactions;

namespace WB.Tests.Integration.ReportTests.TeamsAndStatusesTests.Hq
{
    internal class when_empty_data_base : TeamsAndStatusesReportContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            reportFactory = CreateHqTeamsAndStatusesReport();
            BecauseOf();
        }

        public void BecauseOf() => report = postgresTransactionManager.ExecuteInQueryTransaction(() => reportFactory.GetBySupervisors(new TeamsAndStatusesByHqInputModel()));

        [NUnit.Framework.Test] public void should_return_0_records () => report.TotalCount.Should().Be(0);

        private static TeamsAndStatusesReport reportFactory;
        private static TeamsAndStatusesReportView report;
    }
}

