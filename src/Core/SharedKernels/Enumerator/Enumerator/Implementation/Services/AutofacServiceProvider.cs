using System;
using Autofac;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services
{
    public class AutofacServiceProvider: IServiceProvider
    {
        private readonly ILifetimeScope lifetimeScope;

        public AutofacServiceProvider(ILifetimeScope lifetimeScope)
        {
            this.lifetimeScope = lifetimeScope;
        }
        public object GetService(Type serviceType) => lifetimeScope.Resolve(serviceType);
    }
}