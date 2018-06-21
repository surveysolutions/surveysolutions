using System.Threading.Tasks;
using Ncqrs.Eventing.Storage;
using WB.Core.BoundedContexts.Supervisor.Services;
using WB.Core.BoundedContexts.Supervisor.Services.Implementation;
using WB.Core.BoundedContexts.Supervisor.ViewModel;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Implementation.Services;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.CommandBus.Implementation;
using WB.Core.Infrastructure.Implementation.Storage;
using WB.Core.Infrastructure.Modularity;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.MapService;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.UI.Shared.Enumerator.CustomServices;
using WB.UI.Shared.Enumerator.Services;
using WB.UI.Shared.Enumerator.Services.Internals;
using WB.UI.Shared.Enumerator.Settings;
using WB.UI.Supervisor.Services;
using WB.UI.Supervisor.Services.Implementation;

namespace WB.UI.Supervisor.ServiceLocation
{
    internal class SupervisorUIModule : IModule
    {
        public void Load(IIocRegistry registry)
        {
            registry.Bind<IViewModelNavigationService, ViewModelNavigationService>();
            registry.Bind<ITabletDiagnosticService, TabletDiagnosticService>();
            registry.BindToRegisteredInterface<ISnapshotStore, ISnapshotStoreWithCache>();
            registry.BindAsSingleton<ISnapshotStoreWithCache, InMemorySnapshotStoreWithCache>();

            registry.Bind<INetworkService, AndroidNetworkService>();
            registry.Bind<IHttpClientFactory, AndroidHttpClientFactory>();
            registry.BindAsSingletonWithConstructorArgument<IRestService, RestService>("restServicePointManager", null);
            //registry.Bind<IInterviewUniqueKeyGenerator, InterviewerInterviewUniqueKeyGenerator>();

            registry.Bind<ISynchronizationService, SynchronizationService>();
            registry.Bind<ISupervisorSynchronizationService, SynchronizationService>();
            registry.Bind<IAssignmentSynchronizationApi, SynchronizationService>();
            registry.Bind<IBattery, AndroidBattery>();
            registry.Bind<IDeviceOrientation, AndroidDeviceOrientation>();
            registry.Bind<IDeviceInformationService, DeviceInformationService>();
            registry.Bind<IArchivePatcherService, ArchivePatcherService>();
            //registry.Bind<IInterviewFromAssignmentCreatorService, InterviewFromAssignmentCreatorService>();

            registry.BindAsSingleton<ISyncProtocolVersionProvider, SyncProtocolVersionProvider>();
            registry.BindAsSingleton<IQuestionnaireContentVersionProvider, QuestionnaireContentVersionProvider>();
            registry.BindAsSingleton<ICommandService, SequentialCommandService>();

            registry.Bind<ISynchronizationProcess, SynchronizationProcess>();
            registry.Bind<IQuestionnaireDownloader, QuestionnaireDownloader>();
            registry.Bind<IAssignmentsSynchronizer, AssignmentsSynchronizer>();
            registry.Bind<IAuditLogSynchronizer, AuditLogSynchronizer>();
            registry.Bind<AttachmentsCleanupService>();
            registry.Bind<CompanyLogoSynchronizer>();
            //registry.Bind<IMapSyncProvider, MapSyncProvider>();
            registry.Bind<IMapService, MapService>();
            //registry.BindAsSingleton<ILastCreatedInterviewStorage, LastCreatedInterviewStorage>();

           ]
#if EXCLUDEEXTENSIONS
            registry.Bind<IAreaEditService, WB.UI.Shared.Enumerator.CustomServices.AreaEditor.DummyAreaEditService>();
            registry.Bind<ICheckVersionUriProvider, CheckForVersionUriProvider>();
#else
            registry.Bind<WB.UI.Shared.Extensions.CustomServices.AreaEditor.AreaEditorViewModel>();
            registry.Bind<ICheckVersionUriProvider, CheckForExtendedVersionUriProvider>();
            registry.Bind<IAreaEditService, WB.UI.Shared.Extensions.CustomServices.AreaEditor.AreaEditService>();
#endif
        }

        public Task Init(IServiceLocator serviceLocator)
        {
#if !EXCLUDEEXTENSIONS
            WB.UI.Shared.Extensions.CustomServices.AreaEditor.AreaEditService.RegisterLicence();
#endif

            return Task.CompletedTask;
        }
    }
}
