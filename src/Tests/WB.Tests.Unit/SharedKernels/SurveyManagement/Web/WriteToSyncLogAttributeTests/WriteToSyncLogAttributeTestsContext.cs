using Moq;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.Core.Infrastructure.PlainStorage;
using WB.UI.Headquarters.Code;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Web.WriteToSyncLogAttributeTests
{
    public class WriteToSyncLogAttributeTestsContext
    {
        public static void SetupContext(IPlainStorageAccessor<SynchronizationLogItem> accessor = null)
        {
            Setup.InstanceToMockedServiceLocator(accessor?? new Mock<IPlainStorageAccessor<SynchronizationLogItem>>().Object);
        }

        public static WriteToSyncLogAttribute Create(SynchronizationLogType logType)
        {
            return new WriteToSyncLogAttribute(logType);
        }

    }
}
