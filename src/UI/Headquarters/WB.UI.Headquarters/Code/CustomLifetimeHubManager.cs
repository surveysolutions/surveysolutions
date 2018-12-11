﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Autofac;
using Microsoft.AspNet.SignalR.Hubs;
using WB.Core.Infrastructure.Modularity.Autofac;


namespace WB.UI.Headquarters.Code
{
    public class CustomLifetimeHubManager : IDisposable
    {
        private readonly ConcurrentDictionary<IHub, ILifetimeScope> hubLifetimeScopes = new ConcurrentDictionary<IHub, ILifetimeScope>();

        public T CreateUnitOfWorkScopeAndResolveHub<T>(Type type, ILifetimeScope lifetimeScope) where T : ILifetimeHub
        {
            //return (T) lifetimeScope.Resolve(type);

            ILifetimeScope context = lifetimeScope.BeginLifetimeScope(AutofacServiceLocatorConstants.UnitOfWorkScope);
            
            T obj = (T)context.Resolve(type);
            obj.OnDisposing += new EventHandler(this.HubOnDisposing);
            this.hubLifetimeScopes.TryAdd((IHub)obj, context);
            return obj;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize((object)this);
        }

        protected virtual void Dispose(bool disposing)
        {
            foreach (IDisposable key in (IEnumerable<IHub>)this.hubLifetimeScopes.Keys)
                key.Dispose();
        }

        private void HubOnDisposing(object sender, EventArgs eventArgs)
        {
            IHub key = sender as IHub;

            if (key == null || !this.hubLifetimeScopes.TryRemove(key, out var lifetimeScope) || lifetimeScope == null)
                return;
            
            lifetimeScope.Dispose();
        }
    }
}
