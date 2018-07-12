using System.Threading.Tasks;
using Ncqrs.Eventing.Storage;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services.OfflineSync;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard.DashboardItems;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Implementation.Services;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.Implementation.Storage;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.CommandBus.Implementation;
using WB.Core.Infrastructure.Modularity;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Implementation.Services.MapSynchronization;
using WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.MapService;
using WB.Core.SharedKernels.Enumerator.Services.MapSynchronization;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewLoading;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.UI.Interviewer.Implementations.Services;
using WB.UI.Interviewer.Services;
using WB.UI.Interviewer.Settings;
using WB.UI.Interviewer.ViewModel;
using WB.UI.Shared.Enumerator.CustomServices;
using WB.UI.Shared.Enumerator.Services;
using WB.UI.Shared.Enumerator.Services.Internals;
using WB.UI.Shared.Enumerator.Settings;

namespace WB.UI.Interviewer.ServiceLocation
{
    public class InterviewerUIModule : IModule
    {
        public void Load(IIocRegistry registry)
        {
            registry.Bind<InterviewerMvxApplication>();
            registry.Bind<InterviewerAppStart>();

            registry.Bind<ISideBarSectionViewModelsFactory, SideBarSectionViewModelFactory>();
            registry.Bind<IViewModelNavigationService, ViewModelNavigationService>();
            registry.Bind<ITabletDiagnosticService, TabletDiagnosticService>();
            registry.BindToRegisteredInterface<ISnapshotStore, ISnapshotStoreWithCache>();
            registry.BindAsSingleton<ISnapshotStoreWithCache, InMemorySnapshotStoreWithCache>();

            registry.Bind<INetworkService, AndroidNetworkService>();
            registry.Bind<IHttpClientFactory, AndroidHttpClientFactory>();
            registry.BindAsSingletonWithConstructorArgument<IRestService, RestService>("restServicePointManager", null);
            registry.Bind<IInterviewUniqueKeyGenerator, InterviewerInterviewUniqueKeyGenerator>();

            registry.Bind<OfflineSynchronizationService, OfflineSynchronizationService>();
            registry.Bind<SynchronizationService, SynchronizationService>();

            registry.BindAsSingleton<ISynchronizationMode, SynchronizationModeSelector>();
            registry.Bind<ISynchronizationService, SyncronizationServiceWrapper>();
            

            registry.Bind<IInterviewerSynchronizationService, SynchronizationService>();
            registry.Bind<IBattery, AndroidBattery>();
            registry.Bind<IDeviceOrientation, AndroidDeviceOrientation>();
            registry.Bind<IDeviceInformationService, DeviceInformationService>();
            registry.Bind<IArchivePatcherService, ArchivePatcherService>();
            registry.Bind<IInterviewFromAssignmentCreatorService, InterviewFromAssignmentCreatorService>();

            registry.BindAsSingleton<IInterviewerSyncProtocolVersionProvider, InterviewerSyncProtocolVersionProvider>();
            registry.BindAsSingleton<ISupervisorSyncProtocolVersionProvider, SupervisorSyncProtocolVersionProvider>();
            registry.BindAsSingleton<IQuestionnaireContentVersionProvider, QuestionnaireContentVersionProvider>();

            registry.Bind<ISynchronizationProcess, SynchronizationProcess>();
            registry.Bind<IQuestionnaireDownloader, QuestionnaireDownloader>();
            
            registry.Bind<IAuditLogSynchronizer, AuditLogSynchronizer>();
            registry.Bind<AttachmentsCleanupService>();
            registry.Bind<CompanyLogoSynchronizer>();
            registry.Bind<IMapSyncProvider, MapSyncProvider>();
            registry.Bind<IMapService, MapService>();
            registry.Bind<IViewModelNavigationService, ViewModelNavigationService>();
            registry.BindAsSingleton<ILastCreatedInterviewStorage, LastCreatedInterviewStorage>();

            registry.BindAsSingleton<IInterviewViewModelFactory, InterviewViewModelFactory>();

            registry.BindAsSingleton<ICommandService, SequentialCommandService>();

            registry.Bind<LoginViewModel>();
            registry.Bind<PrefilledQuestionsViewModel>();
            registry.Bind<InterviewViewModel>();
            registry.Bind<BackupRestoreViewModel>();
            registry.Bind<SendTabletInformationViewModel>();
            registry.Bind<BandwidthTestViewModel>();
            registry.Bind<CheckNewVersionViewModel>();
            registry.Bind<DiagnosticsViewModel>();
            registry.Bind<FinishInstallationViewModel>();
            registry.Bind<InterviewerCompleteInterviewViewModel>();
            registry.Bind<SynchronizationViewModel>();
            registry.Bind<MapSynchronizationViewModel>();
            registry.Bind<RelinkDeviceViewModel>();
            registry.Bind<DashboardViewModel>();
            registry.Bind<DashboardSearchViewModel>();
            registry.Bind<MapsViewModel>();
            registry.Bind<CompletedInterviewsViewModel>();
            registry.Bind<RejectedInterviewsViewModel>();
            registry.Bind<StartedInterviewsViewModel>();
            registry.Bind<InterviewerAssignmentDashboardItemViewModel>();
            registry.Bind<CensusQuestionnaireDashboardItemViewModel>();
            registry.Bind<ExpandableQuestionsDashboardItemViewModel>();
            registry.Bind<InterviewDashboardItemViewModel>();
            registry.Bind<CreateNewViewModel>();
            registry.Bind<DashboardSubTitleViewModel>();
            registry.Bind<CompanyLogoSynchronizer>();
            registry.Bind<LoadingViewModel>();
            registry.Bind<PhotoViewViewModel>();
            registry.Bind<OfflineInterviewerSyncViewModel>();

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
