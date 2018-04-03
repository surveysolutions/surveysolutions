using FluentAssertions;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.InputModels;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement
{
    internal class when_empty_data_base : SurveysAndStatusesReportTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            reportFactory = CreateSurveysAndStatusesReport(Stub.ReadSideRepository<InterviewSummary>());
            BecauseOf();
        }

        public void BecauseOf() => report = reportFactory.Load(new SurveysAndStatusesReportInputModel());

        [NUnit.Framework.Test] public void should_return_0_in_counters () => report.TotalCount.Should().Be(0);

        static SurveysAndStatusesReport reportFactory;
        static SurveysAndStatusesReportView report;
    }
}

