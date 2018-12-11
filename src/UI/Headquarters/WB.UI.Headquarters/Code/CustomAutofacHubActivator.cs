using Autofac;
using Microsoft.AspNet.SignalR.Hubs;

namespace WB.UI.Headquarters.Code
{
    class CustomAutofacHubActivator : IHubActivator
    {
        private readonly ILifetimeScope lifetimeScope;
        
        public CustomAutofacHubActivator(ILifetimeScope lifetimeScope)
        {
            this.lifetimeScope = lifetimeScope;
        }

        public IHub Create(HubDescriptor descriptor)
        {
            return this.lifetimeScope.Resolve(descriptor.HubType) as IHub;
        }
    }
}
