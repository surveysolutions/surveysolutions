using System;
using System.Threading.Tasks;
using Autofac;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Domain;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Infrastructure.Native.Workspaces;

namespace WB.Core.BoundedContexts.Headquarters.Implementation
{
    public class InRootScopeExecutor : IRootScopeExecutor
    {
        private readonly IWorkspaceContextAccessor workspaceContextAccessor;
        public static ILifetimeScope RootScope { get; set; }

        public InRootScopeExecutor(IWorkspaceContextAccessor workspaceContextAccessor)
        {
            this.workspaceContextAccessor = workspaceContextAccessor;
        }

        public async Task<T> ExecuteAsync<T>(Func<IServiceLocator, Task<T>> func)
        {
            if (RootScope == null)
                throw new ArgumentException("Need set root scope firstly");
            
            using var scope = RootScope.BeginLifetimeScope();
            var workspace = workspaceContextAccessor.CurrentWorkspace();

            if (workspace != null)
            {
                scope.Resolve<IWorkspaceContextSetter>().Set(workspace);
            }

            var serviceLocatorLocal = scope.Resolve<IServiceLocator>();

            var result = await func(serviceLocatorLocal);

            scope.Resolve<IUnitOfWork>().AcceptChanges();

            return result;
        }
    }
}
