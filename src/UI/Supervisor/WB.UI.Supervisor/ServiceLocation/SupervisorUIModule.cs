using System.Threading.Tasks;
using WB.Core.BoundedContexts.Supervisor.Services;
using WB.Core.BoundedContexts.Supervisor.Services.Implementation;
using WB.Core.BoundedContexts.Supervisor.Services.Implementation.OfflineSyncHandlers;
using WB.Core.BoundedContexts.Supervisor.ViewModel;
using WB.Core.BoundedContexts.Supervisor.ViewModel.Dashboard.Services;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.CommandBus.Implementation;
using WB.Core.Infrastructure.HttpServices.Services;
using WB.Core.Infrastructure.Modularity;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Services;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.Enumerator.Denormalizer;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.MapService;
using WB.Core.SharedKernels.Enumerator.Services.MapSynchronization;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.Markers;
using WB.UI.Shared.Enumerator.CustomServices;
using WB.UI.Shared.Enumerator.Services;
using WB.UI.Shared.Enumerator.Services.Internals;
using WB.UI.Shared.Enumerator.Settings;
using WB.UI.Supervisor.Services.Implementation;

namespace WB.UI.Supervisor.ServiceLocation
{
    internal class SupervisorUiModule : IModule, IInitModule
    {
        public void Load(IIocRegistry registry)
        {
            registry.Bind<ISideBarSectionViewModelsFactory, SupervisorSideBarSectionViewModelFactory>();
            registry.Bind<IDashboardViewModelFactory, SupervisorDashboardViewModelFactory>();
            
            registry.Bind<IViewModelNavigationService, ViewModelNavigationService>();
            registry.Bind<ITabletDiagnosticService, TabletDiagnosticService>();

            registry.Bind<INetworkService, AndroidNetworkService>();
            registry.BindInPerLifetimeScope<IHttpClientFactory, AndroidHttpClientFactory>();
            registry.BindAsSingletonWithConstructorArgument<IRestService, RestService>("restServicePointManager", null);
            registry.Bind<IFastBinaryFilesHttpHandler, BinaryFilesHttpHandler>();
            registry.Bind<IGroupStateCalculationStrategy, SupervisorGroupStateCalculationStrategy>();
            registry.Bind<IInterviewStateCalculationStrategy, SupervisorInterviewStateCalculationStrategy>();

            registry.Bind<ISupervisorSynchronizationService, SynchronizationService>();
            registry.BindToRegisteredInterface<ISynchronizationService, ISupervisorSynchronizationService>();

            registry.Bind<IBattery, AndroidBattery>();
            registry.Bind<IDeviceOrientation, AndroidDeviceOrientation>();
            registry.Bind<IDeviceInformationService, DeviceInformationService>();
            registry.Bind<IArchivePatcherService, ArchivePatcherService>();

            registry.BindAsSingleton<IInterviewerSyncProtocolVersionProvider, InterviewerSyncProtocolVersionProvider>();
            registry.BindAsSingleton<ISupervisorSyncProtocolVersionProvider, SupervisorSyncProtocolVersionProvider>();
            registry.BindAsSingleton<IQuestionnaireContentVersionProvider, QuestionnaireContentVersionProvider>();
            registry.BindAsSingleton<ICommandService, SequentialCommandService>();

            registry.BindToRegisteredInterface<ISynchronizationProcess, SupervisorSynchronizationProcess>();

            registry.Bind<IQuestionnaireDownloader, QuestionnaireDownloader>();
            registry.Bind<IAuditLogSynchronizer, AuditLogSynchronizer>();
            registry.Bind<ITechInfoSynchronizer, TechInfoSynchronizer>();
            registry.Bind<IMapSyncProvider, MapSyncProvider>();
            registry.Bind<IMapService, MapService>();

            registry.Bind<IDashboardItemsAccessor, DashboardItemsAccessor>();
            BindOfflineServices(registry);

            registry.Bind<IInterviewViewModelFactory, SupervisorInterviewViewModelFactory>();

           
#if EXCLUDEEXTENSIONS
            registry.Bind<IMapInteractionService, WB.UI.Shared.Enumerator.CustomServices.AreaEditor.DummyMapInteractionService>();
            registry.Bind<ICheckVersionUriProvider, CheckForExtendedVersionUriProvider>();
#else
            registry.Bind<WB.UI.Shared.Extensions.ViewModels.GeographyEditorViewModel>();
            registry.Bind<WB.UI.Shared.Extensions.ViewModels.SupervisorMapDashboardViewModel>();
            registry.Bind<ICheckVersionUriProvider, CheckForExtendedVersionUriProvider>();
            registry.Bind<IMapInteractionService,  WB.UI.Shared.Extensions.Services.MapInteractionService>();
            registry.Bind<WB.UI.Shared.Extensions.Services.IMapUtilityService, WB.UI.Shared.Extensions.Services.MapUtilityService>();
#endif

            registry.BindAsSingleton<InterviewDashboardEventHandler, InterviewDashboardEventHandler>();
            registry.BindAsSingleton<CalendarEventEventHandler, CalendarEventEventHandler>();
        }

        private void BindOfflineServices(IIocRegistry registry)
        {
            registry.Bind<IHandleCommunicationMessage, SupervisorInterviewsHandler>();
            registry.Bind<IHandleCommunicationMessage, SupervisorInterviewUploadStateHandler>();
            registry.Bind<IHandleCommunicationMessage, SupervisorQuestionnairesHandler>();
            registry.Bind<IHandleCommunicationMessage, SupervisorBinaryHandler>();
            registry.Bind<IHandleCommunicationMessage, SupervisorAuditLogHandler>();
            registry.Bind<IHandleCommunicationMessage, SupervisorSyncStatisticsHandler>();
            registry.Bind<IHandleCommunicationMessage, SupervisorAssignmentsHandler>();
            registry.Bind<IHandleCommunicationMessage, SupervisorTabletInfoHandler>();
            registry.Bind<IHandleCommunicationMessage, SupervisorSynchronizeHandler>();
            registry.Bind<IHandleCommunicationMessage, InterviewerUpdateHandler>();
            registry.Bind<IHandleCommunicationMessage, SupervisorCalendarEventsHandler>();
            registry.Bind<IHandleCommunicationMessage, SupervisorInterviewerSettingsHandler>();
            registry.Bind<IHandleCommunicationMessage, SupervisorGeoTrackingHandler>();
        }

        public Task Init(IServiceLocator serviceLocator, UnderConstructionInfo status)
        {
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
