﻿using System;
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
    internal class when_on_action_executed_called_for_GetInterview_and_param_of_unexpected_type : WriteToSyncLogAttributeTestsContext
    {
        Establish context = () =>
        {
            synchronizationLogItemPlainStorageAccessorMock = new Mock<IPlainStorageAccessor<SynchronizationLogItem>>();

            SetupContext(synchronizationLogItemPlainStorageAccessorMock.Object);
            attribute = Create(SynchronizationLogType.GetInterview);

            actionContext = CreateActionContext();
            actionContext.ActionContext.ActionArguments.Add("id", "test");
        };

        Because of = () => attribute.OnActionExecuted(actionContext);

        private It should_store_log_item = () =>
            synchronizationLogItemPlainStorageAccessorMock.Verify(
                x => x.Store(Moq.It.IsAny<SynchronizationLogItem>(), Moq.It.IsAny<Guid>()),Times.Once);

        private static Mock<IPlainStorageAccessor<SynchronizationLogItem>> synchronizationLogItemPlainStorageAccessorMock; 

        private static WriteToSyncLogAttribute attribute;
        private static HttpActionExecutedContext actionContext;
    }
}
