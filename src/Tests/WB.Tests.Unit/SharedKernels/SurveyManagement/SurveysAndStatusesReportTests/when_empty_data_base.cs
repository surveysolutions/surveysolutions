using Machine.Specifications;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.InputModels;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.SurveysAndStatusesReportTests
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

