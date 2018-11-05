using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Tests.Integration.ReportTests.SpeedReportTests
{
    internal class SpeedReportContext : ReportContext
    {
        protected SpeedReportFactory CreateSpeedReport(IQueryableReadSideRepositoryReader<InterviewSummary> summaries)
        {
            return new SpeedReportFactory(summaries);
        }
    }
}