using Machine.Specifications;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Factories;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.SupervisorTeamsAndStatusesReportTests
{
    [Subject(typeof(SupervisorTeamsAndStatusesReport))]
    internal class SupervisorTeamsAndStatusesReportContext
    {
        protected static SupervisorTeamsAndStatusesReport CreateTeamsAndStatusesReport(IQueryableReadSideRepositoryReader<InterviewSummary> reader = null)
        {
            return new SupervisorTeamsAndStatusesReport(reader ?? new TestInMemoryWriter<InterviewSummary>());
        }
    }
}