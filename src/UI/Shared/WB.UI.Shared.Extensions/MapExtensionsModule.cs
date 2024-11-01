using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Modularity;
using WB.UI.Shared.Extensions.Services;

namespace WB.UI.Shared.Extensions;

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public class MapExtensionsModule : IModule, IInitModule
{
    public void Load(IIocRegistry registry)
    {
        registry.Bind<IGeofencingListener, GeofencingListener>();
        registry.Bind<IGeoTrackingListener, GeoTrackingListener>();
    }

    public Task Init(IServiceLocator serviceLocator, UnderConstructionInfo status)
    {
        return Task.CompletedTask;
    }
}
