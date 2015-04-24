using Machine.Specifications;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Factories;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.SurveysAndStatusesReportTests
{
    [Subject(typeof(SurveysAndStatusesReport))]
    internal class SurveysAndStatusesReportTestsContext
    {
        protected static SurveysAndStatusesReport CreateSurveysAndStatusesReport(IQueryableReadSideRepositoryReader<InterviewSummary> summariesRepository = null)
        {
            return new SurveysAndStatusesReport(summariesRepository ?? new TestInMemoryWriter<InterviewSummary>());
        }
    }
}