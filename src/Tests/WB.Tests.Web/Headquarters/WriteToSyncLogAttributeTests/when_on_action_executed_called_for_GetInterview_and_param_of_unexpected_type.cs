using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Dependencies;
using System.Web.Http.Filters;
using System.Web.Http.Hosting;
using Moq;
using NSubstitute;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.Core.GenericSubdomains.Portable.Services;
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

            WriteToSyncLogAttribute attribute = Create(SynchronizationLogType.GetInterview);
            
            var userMock = new Mock<IAuthorizedUser>();
            userMock.Setup(s => s.IsAuthenticated).Returns(true);

            var logger = new Mock<ILoggerProvider>();
            logger.Setup(x => x.GetFor<WriteToSyncLogAttribute>()).Returns(new Mock<ILogger>().Object);

            var scopeMock = new Mock<IDependencyScope>();
            scopeMock.Setup(x => x.GetService(typeof(IAuthorizedUser))).Returns(userMock.Object);
            scopeMock.Setup(x => x.GetService(typeof(IPlainStorageAccessor<SynchronizationLogItem>))).Returns(synchronizationLogItemPlainStorageAccessorMock.Object);
            scopeMock.Setup(x => x.GetService(typeof(ILoggerProvider))).Returns(logger.Object);

            HttpActionExecutedContext actionContext = CreateActionExecutedContext();
            actionContext.Request.Properties.Add(HttpPropertyKeys.DependencyScope, scopeMock.Object);

            actionContext.ActionContext.ActionArguments.Add("id", "test");
            await attribute.OnActionExecutedAsync(actionContext, new CancellationToken());

            synchronizationLogItemPlainStorageAccessorMock.Verify(
                x => x.Store(Moq.It.IsAny<SynchronizationLogItem>(), Moq.It.IsAny<Guid>()), Times.Once);
        }
    }
}
