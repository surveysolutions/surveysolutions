using Machine.Specifications;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Factories;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.TeamsAndStatusesReportTests
{
    [Subject(typeof(HeadquartersTeamsAndStatusesReport))]
    internal class HeadquartersTeamsAndStatusesReportContext
    {
        protected static HeadquartersTeamsAndStatusesReport CreateTeamsAndStatusesReport(IQueryableReadSideRepositoryReader<InterviewSummary> reader = null)
        {
            return new HeadquartersTeamsAndStatusesReport(reader ?? new TestInMemoryWriter<InterviewSummary>());
        }
    }
}