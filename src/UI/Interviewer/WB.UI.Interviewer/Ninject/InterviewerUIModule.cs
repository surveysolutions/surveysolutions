using Ncqrs.Eventing.Storage;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Implementation.Services;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.Modularity;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.UI.Interviewer.Implementations.Services;
using WB.UI.Interviewer.Services;
using WB.UI.Shared.Enumerator.CustomServices;
using WB.UI.Shared.Enumerator.Services.Internals;

namespace WB.UI.Interviewer.Ninject
{
    public class InterviewerUIModule : IModule
    {
        public void Load(IIocRegistry registry)
        {
            registry.Bind<IViewModelNavigationService, ViewModelNavigationService>();
            registry.Bind<ITabletDiagnosticService, TabletDiagnosticService>();
            //registry.Bind<ISnapshotStore, ISnapshotStoreWithCache>().To<InMemorySnapshotStoreWithCache>().InSingletonScope();

            registry.Bind<INetworkService, AndroidNetworkService>();
            registry.Bind<IHttpClientFactory, ModernHttpClientFactory>();
            registry.BindAsSingletonWithConstructorArgument<IRestService, RestService>("restServicePointManager", null);
            registry.Bind<IInterviewUniqueKeyGenerator, InterviewerInterviewUniqueKeyGenerator>();

#if EXCLUDEEXTENSIONS
            registry.Bind<IAreaEditService, WB.UI.Shared.Enumerator.CustomServices.AreaEditor.DummyAreaEditService>();
#else
            registry.Bind<IAreaEditService, WB.UI.Shared.Extensions.CustomServices.AreaEditor.AreaEditService>();
#endif
        }
    }
}