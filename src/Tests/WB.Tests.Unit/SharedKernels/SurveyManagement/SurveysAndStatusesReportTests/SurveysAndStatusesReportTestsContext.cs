using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories;
using WB.Infrastructure.Native.Storage;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement
{
    [NUnit.Framework.TestOf(typeof(SurveysAndStatusesReport))]
    internal class SurveysAndStatusesReportTestsContext
    {
        protected static SurveysAndStatusesReport CreateSurveysAndStatusesReport(INativeReadSideStorage<InterviewSummary> summariesRepository = null)
        {
            return new SurveysAndStatusesReport(summariesRepository ?? Stub.ReadSideRepository<InterviewSummary>());
        }
    }
}