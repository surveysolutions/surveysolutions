using Machine.Specifications;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Factories;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.TeamsAndStatusesReportTests
{
    [Subject(typeof(TeamsAndStatusesReport))]
    internal class TeamsAndStatusesReportContext
    {
        protected static TeamsAndStatusesReport CreateTeamsAndStatusesReport(IQueryableReadSideRepositoryReader<InterviewSummary> reader = null)
        {
            return new TeamsAndStatusesReport(reader ?? new TestInMemoryWriter<InterviewSummary>());
        }
    }
}