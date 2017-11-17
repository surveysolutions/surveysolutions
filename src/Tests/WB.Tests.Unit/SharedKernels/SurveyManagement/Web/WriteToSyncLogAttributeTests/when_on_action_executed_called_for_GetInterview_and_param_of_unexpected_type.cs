using System;
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
        [OneTimeSetUp]
        public void context()
        {
            synchronizationLogItemPlainStorageAccessorMock = new Mock<IPlainStorageAccessor<SynchronizationLogItem>>();

            SetupContext(synchronizationLogItemPlainStorageAccessorMock.Object);
            attribute = Create(SynchronizationLogType.GetInterview);

            actionContext = CreateActionContext();
            actionContext.ActionContext.ActionArguments.Add("id", "test");
            Becauseof();
        }

        public void Becauseof() => attribute.OnActionExecuted(actionContext);

        [Test]
        public void hould_store_log_item() =>
            synchronizationLogItemPlainStorageAccessorMock.Verify(
                x => x.Store(Moq.It.IsAny<SynchronizationLogItem>(), Moq.It.IsAny<Guid>()),Times.Once);

        private static Mock<IPlainStorageAccessor<SynchronizationLogItem>> synchronizationLogItemPlainStorageAccessorMock; 

        private static WriteToSyncLogAttribute attribute;
        private static HttpActionExecutedContext actionContext;
    }
}
