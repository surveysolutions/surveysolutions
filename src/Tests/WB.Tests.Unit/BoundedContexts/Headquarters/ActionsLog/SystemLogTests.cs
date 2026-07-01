using System;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Services;
using ServiceSystemLog = WB.Core.BoundedContexts.Headquarters.Services.Internal.SystemLog;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.Domain;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.BoundedContexts.Headquarters.Views.SystemLog;
using WB.Infrastructure.Native.Workspaces;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.ActionsLog
{
    [TestFixture]
    [TestOf(typeof(ServiceSystemLog))]
    public class SystemLogTests
    {
        [Test]
        public void when_logging_workspace_enabled_should_write_to_global_audit_log()
        {
            var executor = new Mock<IInScopeExecutor<IPlainStorageAccessor<SystemLogEntry>>>();

            CreateSystemLog(executor.Object)
                .WorkspaceEnabled("workspace");

            executor.Verify(x => x.Execute(It.IsAny<Action<IPlainStorageAccessor<SystemLogEntry>>>(),
                WorkspaceConstants.WorkspaceNames.AdminWorkspaceName), Times.Once);
            executor.Verify(x => x.Execute(It.IsAny<Action<IPlainStorageAccessor<SystemLogEntry>>>(), "workspace"), Times.Never);
            executor.Verify(x => x.Execute(It.IsAny<Action<IPlainStorageAccessor<SystemLogEntry>>>(), null), Times.Never);
        }

        [Test]
        public void when_logging_workspace_disabled_should_write_to_global_audit_log()
        {
            var executor = new Mock<IInScopeExecutor<IPlainStorageAccessor<SystemLogEntry>>>();

            CreateSystemLog(executor.Object)
                .WorkspaceDisabled("workspace");

            executor.Verify(x => x.Execute(It.IsAny<Action<IPlainStorageAccessor<SystemLogEntry>>>(),
                WorkspaceConstants.WorkspaceNames.AdminWorkspaceName), Times.Once);
            executor.Verify(x => x.Execute(It.IsAny<Action<IPlainStorageAccessor<SystemLogEntry>>>(), "workspace"), Times.Never);
            executor.Verify(x => x.Execute(It.IsAny<Action<IPlainStorageAccessor<SystemLogEntry>>>(), null), Times.Never);
        }

        private static ServiceSystemLog CreateSystemLog(IInScopeExecutor<IPlainStorageAccessor<SystemLogEntry>> inScopeExecutor)
        {
            return new ServiceSystemLog(
                Mock.Of<IAuthorizedUser>(x => x.Id == Guid.Empty && x.UserName == "admin"),
                inScopeExecutor,
                Mock.Of<ILogger>());
        }
    }
}
