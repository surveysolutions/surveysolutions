using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Modularity;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.UI.Shared.Extensions.Services;
using WB.UI.Shared.Extensions.Synchronization;

namespace WB.UI.Shared.Extensions;

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public class SupervisorMapExtensionsModule : IModule, IInitModule
{
    public void Load(IIocRegistry registry)
    {
        registry.Bind<IAssignmentMapSettings, SupervisorAssignmentMapSettings>();
    }

    public Task Init(IServiceLocator serviceLocator, UnderConstructionInfo status)
    {
        return Task.CompletedTask;
    }
}
