using System.Threading.Tasks;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.GenericSubdomains.Portable.Implementation.Services;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.CommandBus.Implementation;
using WB.Core.Infrastructure.Modularity;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Services;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.Enumerator.Denormalizer;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.MapService;
using WB.Core.SharedKernels.Enumerator.Services.MapSynchronization;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;
using WB.UI.Interviewer.Implementations.Services;
using WB.UI.Interviewer.Services;
using WB.UI.Shared.Enumerator.CustomServices;
using WB.UI.Shared.Enumerator.Services;
using WB.UI.Shared.Enumerator.Services.Internals;
using WB.UI.Shared.Enumerator.Settings;
using WB.UI.Shared.Extensions.CustomServices;

namespace WB.UI.Interviewer.ServiceLocation
{
    public class InterviewerUIModule : IModule, IInitModule
    {
        public void Load(IIocRegistry registry)
        {
            registry.Bind<ISideBarSectionViewModelsFactory, SideBarSectionViewModelFactory>();
            registry.Bind<IViewModelNavigationService, ViewModelNavigationService>();
            registry.Bind<ITabletDiagnosticService, TabletDiagnosticService>();

            registry.Bind<INetworkService, AndroidNetworkService>();
            registry.Bind<IHttpClientFactory, AndroidHttpClientFactory>();
            registry.BindAsSingletonWithConstructorArgument<IRestService, RestService>("restServicePointManager", null);
            registry.Bind<IFastBinaryFilesHttpHandler, FastBinaryFilesHttpHandler>();
            registry.Bind<IInterviewUniqueKeyGenerator, InterviewerInterviewUniqueKeyGenerator>();
            registry.Bind<IGroupStateCalculationStrategy, EnumeratorGroupGroupStateCalculationStrategy>();
            registry.Bind<IInterviewStateCalculationStrategy, EnumeratorInterviewStateCalculationStrategy>();
            
            registry.Bind<IOfflineSynchronizationService, OfflineSynchronizationService>();
            registry.Bind<IOnlineSynchronizationService, OnlineSynchronizationService>();
            registry.BindToMethod(x =>
                x.Get<IInterviewerSettings>().AllowSyncWithHq
                    ? x.Get<IOnlineSynchronizationService>()
                    : (IInterviewerSynchronizationService)x.Get<IOfflineSynchronizationService>());
            registry.BindToRegisteredInterface<ISynchronizationService, IInterviewerSynchronizationService>();

            registry.Bind<IBattery, AndroidBattery>();
            registry.Bind<IDeviceOrientation, AndroidDeviceOrientation>();
            registry.Bind<IDeviceInformationService, DeviceInformationService>();
            registry.Bind<IArchivePatcherService, ArchivePatcherService>();

            registry.BindAsSingleton<IInterviewerSyncProtocolVersionProvider, InterviewerSyncProtocolVersionProvider>();
            registry.BindAsSingleton<ISupervisorSyncProtocolVersionProvider, SupervisorSyncProtocolVersionProvider>();
            registry.BindAsSingleton<IQuestionnaireContentVersionProvider, QuestionnaireContentVersionProvider>();

            registry.Bind<InterviewerOnlineSynchronizationProcess>();
            registry.Bind<InterviewerOfflineSynchronizationProcess>();
            registry.BindToMethod(x =>
                x.Get<IInterviewerSettings>().AllowSyncWithHq
                    ? x.Get<InterviewerOnlineSynchronizationProcess>()
                    : (ISynchronizationProcess) x.Get<InterviewerOfflineSynchronizationProcess>());

            registry.Bind<IQuestionnaireDownloader, QuestionnaireDownloader>();
            
            registry.Bind<IAuditLogSynchronizer, AuditLogSynchronizer>();
            registry.Bind<IMapSyncProvider, MapSyncProvider>();
            registry.Bind<IMapService, MapService>();
            registry.Bind<IViewModelNavigationService, ViewModelNavigationService>();
            registry.BindAsSingleton<ILastCreatedInterviewStorage, LastCreatedInterviewStorage>();

            registry.BindAsSingleton<IInterviewViewModelFactory, InterviewerInterviewViewModelFactory>();

            registry.BindAsSingleton<ICommandService, SequentialCommandService>();
#if EXCLUDEEXTENSIONS
            registry.Bind<IMapInteractionService, WB.UI.Shared.Enumerator.CustomServices.AreaEditor.DummyMapInteractionService>();
            registry.Bind<ICheckVersionUriProvider, CheckForVersionUriProvider>();
#else
            registry.Bind<WB.UI.Shared.Extensions.CustomServices.AreaEditor.AreaEditorViewModel>();
            registry.Bind<WB.UI.Shared.Extensions.CustomServices.MapDashboard.MapDashboardViewModel>();
            registry.Bind<ICheckVersionUriProvider, CheckForExtendedVersionUriProvider>();
            registry.Bind<IMapInteractionService, MapInteractionService>();
#endif
            registry.BindAsSingleton<InterviewDashboardEventHandler, InterviewDashboardEventHandler>();
            
            RegisterViewModels(registry);
        }

        private static void RegisterViewModels(IIocRegistry registry)
        {
            registry.BindToMethod(context => new CheckNewVersionViewModel(
                context.Get<IOnlineSynchronizationService>(),
                context.Get<IDeviceSettings>(),
                context.Get<ITabletDiagnosticService>(),
                context.Get<ILogger>()));
        }

        public Task Init(IServiceLocator serviceLocator, UnderConstructionInfo status)
        {
#if !EXCLUDEEXTENSIONS
            MapInteractionService.RegisterLicense();
#endif

            return Task.CompletedTask;
        }
    }
}
