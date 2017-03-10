using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Tests.Abc.Storage;

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
