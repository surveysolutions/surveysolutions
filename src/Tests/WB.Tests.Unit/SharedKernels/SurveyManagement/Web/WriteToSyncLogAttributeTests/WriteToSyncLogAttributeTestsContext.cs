using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.Core.Infrastructure.PlainStorage;
using WB.Tests.Abc;
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

        protected static HttpActionExecutedContext CreateActionContext()
        {
            return new HttpActionExecutedContext(
                new HttpActionContext(new HttpControllerContext(), new ReflectedHttpActionDescriptor()), null);
        }
    }
}
