using FluentAssertions;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.InputModels;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;

namespace WB.Tests.Integration.ReportTests.TeamsAndStatusesTests.Sv
{
    internal class when_empty_data_base : TeamsAndStatusesReportContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            reportFactory = CreateSvTeamsAndStatusesReport();
            BecauseOf();
        }

        private void BecauseOf() => 
            report = reportFactory.GetBySupervisorAndDependentInterviewers(new TeamsAndStatusesInputModel());

        [NUnit.Framework.Test] public void should_return_0_records () => report.TotalCount.Should().Be(0);

        private static TeamsAndStatusesReport reportFactory;
        private static TeamsAndStatusesReportView report;
    }
}

