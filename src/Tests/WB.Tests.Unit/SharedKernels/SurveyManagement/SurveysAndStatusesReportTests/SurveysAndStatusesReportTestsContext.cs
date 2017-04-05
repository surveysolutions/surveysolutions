using Machine.Specifications;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.SurveysAndStatusesReportTests
{
    [Subject(typeof(SurveysAndStatusesReport))]
    internal class SurveysAndStatusesReportTestsContext
    {
        protected static SurveysAndStatusesReport CreateSurveysAndStatusesReport(IQueryableReadSideRepositoryReader<InterviewSummary> summariesRepository = null)
        {
            return new SurveysAndStatusesReport(summariesRepository ?? Stub.ReadSideRepository<InterviewSummary>());
        }
    }
}