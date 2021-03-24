using System;
using Autofac;
using WB.Core.SharedKernels.Enumerator.Services.Workspace;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services
{
    public class ExecuteInWorkspaceService : IExecuteInWorkspaceService
    {
        private readonly ILifetimeScope lifetimeScope;

        public ExecuteInWorkspaceService(ILifetimeScope lifetimeScope)
        {
            this.lifetimeScope = lifetimeScope;
        }

        public void Execute(WorkspaceView workspace, Action<IServiceProvider> action)
        {
            var workspaceAccessor = new SingleWorkspaceAccessor(workspace);
            using var workspaceLifetimeScope = lifetimeScope.BeginLifetimeScope(cb =>
            {
                cb.Register(c => workspaceAccessor).As<IWorkspaceAccessor>().SingleInstance();
                cb.RegisterType<AutofacServiceProvider>().As<IServiceProvider>();
            });

            var workspaceServiceProvider = workspaceLifetimeScope.Resolve<IServiceProvider>();
            action.Invoke(workspaceServiceProvider);
        }
    }
}