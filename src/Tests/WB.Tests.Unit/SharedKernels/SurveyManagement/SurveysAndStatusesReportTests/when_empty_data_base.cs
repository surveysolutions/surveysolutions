using Machine.Specifications;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.InputModels;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement
{
    internal class when_empty_data_base : SurveysAndStatusesReportTestsContext
    {
        Establish context = () =>
        {
            reportFactory = CreateSurveysAndStatusesReport(Stub.ReadSideRepository<InterviewSummary>());
        };

        Because of = () => report = reportFactory.Load(new SurveysAndStatusesReportInputModel());

        It should_return_0_in_counters = () => report.TotalCount.ShouldEqual(0);

        static SurveysAndStatusesReport reportFactory;
        static SurveysAndStatusesReportView report;
    }
}

