using Ncqrs.Eventing.Storage;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.Services.Synchronization;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard.DashboardItems;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Implementation.Services;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.Modularity;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.UI.Interviewer.Implementations.Services;
using WB.UI.Interviewer.Services;
using WB.UI.Interviewer.Settings;
using WB.UI.Interviewer.ViewModel;
using WB.UI.Shared.Enumerator.CustomServices;
using WB.UI.Shared.Enumerator.Services.Internals;
using WB.UI.Shared.Enumerator.Services.Internals.MapService;

namespace WB.UI.Interviewer.ServiceLocation
{
    public class InterviewerUIModule : IModule
    {
        public void Load(IIocRegistry registry)
        {
            registry.Bind<IViewModelNavigationService, ViewModelNavigationService>();
            registry.Bind<ITabletDiagnosticService, TabletDiagnosticService>();
            registry.BindToRegisteredInterface<ISnapshotStore, ISnapshotStoreWithCache>();
            registry.BindAsSingleton<ISnapshotStoreWithCache, InMemorySnapshotStoreWithCache>();

            registry.Bind<INetworkService, AndroidNetworkService>();
            registry.Bind<IHttpClientFactory, ModernHttpClientFactory>();
            registry.BindAsSingletonWithConstructorArgument<IRestService, RestService>("restServicePointManager", null);
            registry.Bind<IInterviewUniqueKeyGenerator, InterviewerInterviewUniqueKeyGenerator>();

            registry.Bind<ISynchronizationService, SynchronizationService>();
            registry.Bind<IAssignmentSynchronizationApi, SynchronizationService>();
            registry.Bind<IBattery, AndroidBattery>();
            registry.Bind<IDeviceOrientation, AndroidDeviceOrientation>();
            registry.Bind<IDeviceInformationService, DeviceInformationService>();
            registry.Bind<IArchivePatcherService, ArchivePatcherService>();
            registry.Bind<IInterviewFromAssignmentCreatorService, InterviewFromAssignmentCreatorService>();

            registry.BindAsSingleton<ISyncProtocolVersionProvider, SyncProtocolVersionProvider>();
            registry.BindAsSingleton<IQuestionnaireContentVersionProvider, QuestionnaireContentVersionProvider>();

            registry.Bind<ISynchronizationProcess, SynchronizationProcess>();
            registry.Bind<IQuestionnaireDownloader, QuestionnaireDownloader>();
            registry.Bind<IAssignmentsSynchronizer, AssignmentsSynchronizer>();
            registry.Bind<AttachmentsCleanupService>();
            registry.Bind<CompanyLogoSynchronizer>();
            registry.Bind<IMapSynchronizer, MapSynchronizer>();
            registry.Bind<IMapService, MapService>();
            registry.Bind<IViewModelNavigationService, ViewModelNavigationService>();

            registry.Bind<LoginViewModel>();
            registry.Bind<PrefilledQuestionsViewModel>();
            registry.Bind<InterviewViewModel>();
            registry.Bind<BackupRestoreViewModel>();
            registry.Bind<BackupViewModel>();
            registry.Bind<BandwidthTestViewModel>();
            registry.Bind<CheckNewVersionViewModel>();
            registry.Bind<DiagnosticsViewModel>();
            registry.Bind<FinishInstallationViewModel>();
            registry.Bind<InterviewerCompleteInterviewViewModel>();
            registry.Bind<SynchronizationViewModel>();
            registry.Bind<RelinkDeviceViewModel>();
            registry.Bind<DashboardViewModel>();
            registry.Bind<CompletedInterviewsViewModel>();
            registry.Bind<RejectedInterviewsViewModel>();
            registry.Bind<StartedInterviewsViewModel>();
            registry.Bind<AssignmentDashboardItemViewModel>();
            registry.Bind<CensusQuestionnaireDashboardItemViewModel>();
            registry.Bind<ExpandableQuestionsDashboardItemViewModel>();
            registry.Bind<InterviewDashboardItemViewModel>();
            registry.Bind<CreateNewViewModel>();
            registry.Bind<DashboardSubTitleViewModel>();
            registry.Bind<CompanyLogoSynchronizer>();
            registry.Bind<LoadingViewModel>();

#if EXCLUDEEXTENSIONS
            registry.Bind<IAreaEditService, WB.UI.Shared.Enumerator.CustomServices.AreaEditor.DummyAreaEditService>();
#else
            registry.Bind<WB.UI.Shared.Extensions.CustomServices.AreaEditor.AreaEditorViewModel>();
            registry.Bind<IAreaEditService, WB.UI.Shared.Extensions.CustomServices.AreaEditor.AreaEditService>();
#endif
        }
    }
}