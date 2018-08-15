using System.Threading.Tasks;
using Ncqrs.Eventing.Storage;
using WB.Core.BoundedContexts.Supervisor.Services;
using WB.Core.BoundedContexts.Supervisor.Services.Implementation;
using WB.Core.BoundedContexts.Supervisor.Services.Implementation.OfflineSyncHandlers;
using WB.Core.BoundedContexts.Supervisor.ViewModel;
using WB.Core.BoundedContexts.Supervisor.ViewModel.Dashboard.Services;
using WB.Core.BoundedContexts.Supervisor.ViewModel.InterviewerSelector;
using WB.Core.GenericSubdomains.Portable.Implementation.Services;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.CommandBus.Implementation;
using WB.Core.Infrastructure.Implementation.Storage;
using WB.Core.Infrastructure.Modularity;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Services;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.MapService;
using WB.Core.SharedKernels.Enumerator.Services.MapSynchronization;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;
using WB.UI.Shared.Enumerator.CustomServices;
using WB.UI.Shared.Enumerator.Services;
using WB.UI.Shared.Enumerator.Settings;
using WB.UI.Supervisor.CustomControls;
using WB.UI.Supervisor.Services.Implementation;

namespace WB.UI.Supervisor.ServiceLocation
{
    internal class SupervisorUiModule : IModule
    {
        public void Load(IIocRegistry registry)
        {
            registry.Bind<ISideBarSectionViewModelsFactory, SupervisorSideBarSectionViewModelFactory>();
            
            registry.Bind<IViewModelNavigationService, ViewModelNavigationService>();
            registry.Bind<ITabletDiagnosticService, TabletDiagnosticService>();
            registry.BindToRegisteredInterface<ISnapshotStore, ISnapshotStoreWithCache>();
            registry.BindAsSingleton<ISnapshotStoreWithCache, InMemorySnapshotStoreWithCache>();

            registry.Bind<INetworkService, AndroidNetworkService>();
            registry.BindAsSingletonWithConstructorArgument<IRestService, RestService>("restServicePointManager", null);
            registry.Bind<IGroupStateCalculationStrategy, SupervisorGroupStateCalculationStrategy>();
            registry.Bind<IInterviewStateCalculationStrategy, SupervisorInterviewStateCalculationStrategy>();

            registry.Bind<ISynchronizationService, SynchronizationService>();
            registry.Bind<ISupervisorSynchronizationService, SynchronizationService>();
            registry.Bind<IBattery, AndroidBattery>();
            registry.Bind<IDeviceOrientation, AndroidDeviceOrientation>();
            registry.Bind<IDeviceInformationService, DeviceInformationService>();
            registry.Bind<IArchivePatcherService, ArchivePatcherService>();

            registry.BindAsSingleton<IInterviewerSyncProtocolVersionProvider, InterviewerSyncProtocolVersionProvider>();
            registry.BindAsSingleton<ISupervisorSyncProtocolVersionProvider, SupervisorSyncProtocolVersionProvider>();
            registry.BindAsSingleton<IQuestionnaireContentVersionProvider, QuestionnaireContentVersionProvider>();
            registry.BindAsSingleton<ICommandService, SequentialCommandService>();

            registry.Bind<ISynchronizationProcess, SupervisorSynchronizationProcess>();
            registry.Bind<IQuestionnaireDownloader, QuestionnaireDownloader>();
            registry.Bind<IAuditLogSynchronizer, AuditLogSynchronizer>();
            registry.Bind<ITechInfoSynchronizer, TechInfoSynchronizer>();
            registry.Bind<IMapSyncProvider, MapSyncProvider>();
            registry.Bind<IMapService, MapService>();

            registry.Bind<IDashboardItemsAccessor, DashboardItemsAccessor>();
            BindOfflineServices(registry);
            registry.Bind<IInterviewerSelectorDialog, InterviewerSelectorDialog>();
            registry.Bind<IInterviewersListAccessor, InterviewersListAccessor>();

            registry.BindAsSingleton<IInterviewViewModelFactory, SupervisorInterviewViewModelFactory>();

           
#if EXCLUDEEXTENSIONS
            registry.Bind<IAreaEditService, WB.UI.Shared.Enumerator.CustomServices.AreaEditor.DummyAreaEditService>();
            registry.Bind<ICheckVersionUriProvider, CheckForExtendedVersionUriProvider>();
#else
            registry.Bind<WB.UI.Shared.Extensions.CustomServices.AreaEditor.AreaEditorViewModel>();
            registry.Bind<ICheckVersionUriProvider, CheckForExtendedVersionUriProvider>();
            registry.Bind<IAreaEditService, WB.UI.Shared.Extensions.CustomServices.AreaEditor.AreaEditService>();
#endif
        }

        private void BindOfflineServices(IIocRegistry registry)
        {
            registry.Bind<IHandleCommunicationMessage, SupervisorInterviewsHandler>();
            registry.Bind<IHandleCommunicationMessage, SupervisorQuestionnairesHandler>();
            registry.Bind<IHandleCommunicationMessage, SupervisorBinaryHandler>();
            registry.Bind<IHandleCommunicationMessage, SupervisorAuditLogHandler>();
            registry.Bind<IHandleCommunicationMessage, SupervisorSyncStatisticsHandler>();
            registry.Bind<IHandleCommunicationMessage, SupervisorAssignmentsHandler>();
            registry.Bind<IHandleCommunicationMessage, SupervisorTabletInfoHandler>();
            registry.Bind<IHandleCommunicationMessage, SupervisorSynchronizeHandler>();
        }

        public Task Init(IServiceLocator serviceLocator, UnderConstructionInfo status)
        {
#if !EXCLUDEEXTENSIONS
            WB.UI.Shared.Extensions.CustomServices.AreaEditor.AreaEditService.RegisterLicence();
#endif
            CommandRegistry
                .Setup<StatefulInterview>()
                .InitializesWith<SynchronizeInterviewEventsCommand>(command => command.InterviewId,
                    aggregate => aggregate.SynchronizeInterviewEvents);

            CommandRegistry.Configure<StatefulInterview, QuestionCommand>(configuration =>
                configuration
                    .ValidatedBy<SupervisorAnsweringValidator>()
                    .SkipValidationFor<SetFlagToAnswerCommand>()
                    .SkipValidationFor<RemoveFlagFromAnswerCommand>()
                    .SkipValidationFor<CommentAnswerCommand>()
            );
            return Task.CompletedTask;
        }
    }
}
