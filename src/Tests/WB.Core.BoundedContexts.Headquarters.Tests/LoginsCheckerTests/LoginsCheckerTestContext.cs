using Moq;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Survey;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Headquarters.Tests.LoginsCheckerTests
{
    internal class LoginsCheckerTestContext
    {
        protected static LoginsChecker CreateLoginsChecker(IQueryableReadSideRepositoryReader<SupervisorLoginView> supervisorLogins = null)
        {
            return new LoginsChecker(supervisorLogins ?? Mock.Of<IQueryableReadSideRepositoryReader<SupervisorLoginView>>());
        }
    }
}
