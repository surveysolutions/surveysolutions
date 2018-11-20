using System;
using Autofac;

namespace WB.Core.Infrastructure.Modularity.Autofac
{
    public class Scope : IDisposable
    {
        private readonly ILifetimeScope lifetimeScope;

        public Scope(ILifetimeScope lifetimeScope)
        {
            this.lifetimeScope = lifetimeScope;
        }

        public void Dispose()
        {
            lifetimeScope.Dispose();
        }
    }
}