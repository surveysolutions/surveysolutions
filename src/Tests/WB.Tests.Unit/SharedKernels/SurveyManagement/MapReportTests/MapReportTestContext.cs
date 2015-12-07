using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Factories;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.MapReportTests
{
    internal class MapReportTestContext
    {
        protected static MapReport CreateMapReport(IQueryableReadSideRepositoryReader<MapReportPoint> answersByVariableStorage = null)
        {
            return new MapReport(answersByVariableStorage ?? new TestInMemoryWriter<MapReportPoint>());
        }
    }
}
