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
    [TestFixture]
    internal class when_on_action_executed_called_for_GetInterview_and_param_of_unexpected_type : WriteToSyncLogAttributeTestsContext
    {
        [Test]
        public async Task should_store_log_item()
        {
            Mock<IPlainStorageAccessor<SynchronizationLogItem>> synchronizationLogItemPlainStorageAccessorMock =
                new Mock<IPlainStorageAccessor<SynchronizationLogItem>>();

            SetupContext(synchronizationLogItemPlainStorageAccessorMock.Object);
            WriteToSyncLogAttribute attribute = Create(SynchronizationLogType.GetInterview);

            HttpActionExecutedContext actionContext = CreateActionContext();
            actionContext.ActionContext.ActionArguments.Add("id", "test");
            await attribute.OnActionExecutedAsync(actionContext, new CancellationToken());

            synchronizationLogItemPlainStorageAccessorMock.Verify(
                x => x.Store(Moq.It.IsAny<SynchronizationLogItem>(), Moq.It.IsAny<Guid>()), Times.Once);

        }
    }
}
