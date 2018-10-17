using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Autofac;
using Microsoft.AspNet.SignalR.Hubs;
using SignalR.Extras.Autofac;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;

namespace WB.UI.Headquarters
{
    public class CustomLifetimeHubManager : IDisposable
    {
        private readonly ConcurrentDictionary<IHub, ILifetimeScope> _hubLifetimeScopes = new ConcurrentDictionary<IHub, ILifetimeScope>();

        public T ResolveHub<T>(Type type, ILifetimeScope lifetimeScope) where T : ILifetimeHub
        {
            ILifetimeScope context = lifetimeScope.BeginLifetimeScope(ScopeLifetimeTag.RequestLifetimeScopeTag);
            var serviceLocatorLocal = context.Resolve<IServiceLocator>(new NamedParameter("kernel", context));

            T obj = (T)context.Resolve(type);
            obj.OnDisposing += new EventHandler(this.HubOnDisposing);
            this._hubLifetimeScopes.TryAdd((IHub)obj, context);
            return obj;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize((object)this);
        }

        protected virtual void Dispose(bool disposing)
        {
            foreach (IDisposable key in (IEnumerable<IHub>)this._hubLifetimeScopes.Keys)
                key.Dispose();
        }

        private void HubOnDisposing(object sender, EventArgs eventArgs)
        {
            IHub key = sender as IHub;
            ILifetimeScope lifetimeScope;
            if (key == null || !this._hubLifetimeScopes.TryRemove(key, out lifetimeScope) || lifetimeScope == null)
                return;
            lifetimeScope.Dispose();
        }
    }
}