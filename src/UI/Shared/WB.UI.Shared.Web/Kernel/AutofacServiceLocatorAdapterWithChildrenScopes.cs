using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using Autofac;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.UI.Shared.Enumerator.Services.Internals;

namespace WB.UI.Shared.Web.Kernel
{
    public class AutofacServiceLocatorAdapterWithChildrenScopes : ServiceLocatorImplBase
    {
        protected AsyncLocal<List<ILifetimeScope>> containers = new AsyncLocal<List<ILifetimeScope>>(); 

        protected readonly ILifetimeScope rootScope;

        public AutofacServiceLocatorAdapterWithChildrenScopes(ILifetimeScope rootScope)
        {
            this.rootScope = rootScope;
            this.containers.Value = new List<ILifetimeScope>();
        }

        protected override object DoGetInstance(Type serviceType, string key)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }

            var container = GetCurrentScope();
            return key != null ? container.ResolveNamed(key, serviceType) : container.Resolve(serviceType);
        }

        protected override IEnumerable<object> DoGetAllInstances(Type serviceType)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }

            var container = GetCurrentScope();
            var enumerableType = typeof(IEnumerable<>).MakeGenericType(serviceType);
            object instance = container.Resolve(enumerableType);
            return ((IEnumerable)instance).Cast<object>();
        }

//        public ILifetimeScope BeginLifetimeScope(Action<ContainerBuilder> @override)
//        {
//            var container = GetCurrentScope();
//            var childContainer = container.BeginLifetimeScope(@override);
//            if (containers.Value == null)
//                containers.Value = new List<ILifetimeScope>();
//            containers.Value.Add(childContainer);
//            return childContainer;
//        }

        public ILifetimeScope BeginLifetimeScope()
        {
            var container = GetCurrentScope();
            var childLifetimeScope = container.BeginLifetimeScope();
            if (containers.Value == null)
                containers.Value = new List<ILifetimeScope>();
            childLifetimeScope.Resolve<IServiceLocator>(new NamedParameter("kernel", childLifetimeScope));
            containers.Value.Add(childLifetimeScope);
            return childLifetimeScope;
        }

        public void CloseScopeAndChildrenScopes(ILifetimeScope scope)
        {
            var index = containers.Value.FindIndex(s => s == scope);

            if (index >= 0)
            {
                var count = containers.Value.Count;
                containers.Value.Skip(index).ForEach(l => l.Dispose());
                containers.Value.RemoveRange(index, count - index);
                return;
            }

            throw new ArgumentException("Cant close scope: Not found scope");
        }

        public void CloseAllChildrenScopes()
        {
            containers.Value?.ForEach(l => l.Dispose());
            containers.Value?.Clear();
        }

        private ILifetimeScope GetCurrentScope()
        {
            return containers.Value?.LastOrDefault() ?? rootScope;
        }

    }

    public class ScopeManager
    {
        private static AutofacServiceLocatorAdapterWithChildrenScopes scopeAdapter;

        public static void SetScopeAdapter(AutofacServiceLocatorAdapterWithChildrenScopes adapter)
        {
            scopeAdapter = adapter;
        }

        public static Scope BeginScope()
        {
            return new Scope(scopeAdapter);
        }

        public static void EndScope(Scope scope)
        {
            scope.Dispose();
        }

        public static void EndAllScope()
        {
            scopeAdapter.CloseAllChildrenScopes();
        }
    }

    public class Scope : IDisposable
    {
        private readonly AutofacServiceLocatorAdapterWithChildrenScopes scopeAdapter;
        private readonly ILifetimeScope lifetimeScope;

        public Scope(AutofacServiceLocatorAdapterWithChildrenScopes scopeAdapter)
        {
            this.scopeAdapter = scopeAdapter;
            this.lifetimeScope = scopeAdapter.BeginLifetimeScope();
        }

        public void Dispose()
        {
            scopeAdapter.CloseScopeAndChildrenScopes(lifetimeScope);
        }
    }
}
