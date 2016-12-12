using System;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.Core.Infrastructure.PlainStorage;
using WB.UI.Headquarters.Code;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Web.WriteToSyncLogAttributeTests
{
    internal class when_on_action_executed_called_for_GetQuestionnaire_and_params_provided : WriteToSyncLogAttributeTestsContext
    {
        Establish context = () =>
        {
            synchronizationLogItemPlainStorageAccessorMock = new Mock<IPlainStorageAccessor<SynchronizationLogItem>>();

            SetupContext(synchronizationLogItemPlainStorageAccessorMock.Object);
            attribute = Create(SynchronizationLogType.GetQuestionnaire);

            actionContext = CreateActionContext();
            actionContext.ActionContext.ActionArguments.Add("id", Guid.NewGuid());
            actionContext.ActionContext.ActionArguments.Add("version", (int)4);
        };

        Because of = () => attribute.OnActionExecuted(actionContext);

        private It should_store_log_item = () =>
            synchronizationLogItemPlainStorageAccessorMock.Verify(
                x => x.Store(Moq.It.IsAny<SynchronizationLogItem>(), Moq.It.IsAny<Guid>()),Times.Once);

        private static Mock<IPlainStorageAccessor<SynchronizationLogItem>> synchronizationLogItemPlainStorageAccessorMock; 

        private static WriteToSyncLogAttribute attribute;
        private static HttpActionExecutedContext actionContext;

        protected static HttpActionExecutedContext CreateActionContext()
        {
            return new HttpActionExecutedContext(
                new HttpActionContext(new HttpControllerContext(), new ReflectedHttpActionDescriptor()), null); 
        }
    }
}
