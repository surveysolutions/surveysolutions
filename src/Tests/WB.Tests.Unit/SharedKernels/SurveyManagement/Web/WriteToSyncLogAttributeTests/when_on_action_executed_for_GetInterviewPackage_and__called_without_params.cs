using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.Core.Infrastructure.PlainStorage;
using WB.UI.Headquarters.Code;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Web.WriteToSyncLogAttributeTests
{
    internal class when_on_action_executed_for_GetInterviewPackage_and__called_without_params : WriteToSyncLogAttributeTestsContext
    {
        [Test]
        public async Task should_store_log_item()
        {
            Mock<IPlainStorageAccessor<SynchronizationLogItem>> synchronizationLogItemPlainStorageAccessorMock =
                new Mock<IPlainStorageAccessor<SynchronizationLogItem>>();

            SetupContext(synchronizationLogItemPlainStorageAccessorMock.Object);
            WriteToSyncLogAttribute attribute = Create(SynchronizationLogType.GetInterviewPackage);

            HttpActionExecutedContext actionContext = CreateActionContext();

            await attribute.OnActionExecutedAsync(actionContext, new CancellationToken());


            synchronizationLogItemPlainStorageAccessorMock.Verify(
                x => x.Store(Moq.It.IsAny<SynchronizationLogItem>(), Moq.It.IsAny<Guid>()), Times.Once);
        }
    }
}
