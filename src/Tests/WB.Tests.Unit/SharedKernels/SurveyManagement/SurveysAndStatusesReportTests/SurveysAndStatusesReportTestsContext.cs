using Machine.Specifications;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Infrastructure.Native.Storage;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.SurveysAndStatusesReportTests
{
    [Subject(typeof(SurveysAndStatusesReport))]
    internal class SurveysAndStatusesReportTestsContext
    {
        protected static SurveysAndStatusesReport CreateSurveysAndStatusesReport(INativeReadSideStorage<InterviewSummary> summariesRepository = null)
        {
            return new SurveysAndStatusesReport(summariesRepository ?? Stub.ReadSideRepository<InterviewSummary>());
        }
    }
}