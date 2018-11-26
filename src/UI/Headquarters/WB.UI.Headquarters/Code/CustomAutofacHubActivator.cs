using Autofac;
using Microsoft.AspNet.SignalR.Hubs;

namespace WB.UI.Headquarters.Code
{
    class CustomAutofacHubActivator : IHubActivator
    {
        private readonly ILifetimeScope lifetimeScope;
        private readonly CustomLifetimeHubManager lifetimeHubManager;

        public CustomAutofacHubActivator(CustomLifetimeHubManager lifetimeHubManager, ILifetimeScope lifetimeScope)
        {
            this.lifetimeScope = lifetimeScope;
            this.lifetimeHubManager = lifetimeHubManager;
        }

        public IHub Create(HubDescriptor descriptor)
        {
            return typeof(ILifetimeHub).IsAssignableFrom(descriptor.HubType) 
                ? (IHub)this.lifetimeHubManager.CreateUnitOwWorkScopeAndResolveHub<ILifetimeHub>(descriptor.HubType, this.lifetimeScope) 
                : this.lifetimeScope.Resolve(descriptor.HubType) as IHub;
        }
    }
}
