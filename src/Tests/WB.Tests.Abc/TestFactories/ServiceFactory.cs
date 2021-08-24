using System;
using System.Collections.Generic;
using System.IO;
using Moq;
using Ncqrs.Domain.Storage;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Storage;
using NHibernate;
using NSubstitute;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Main.Core.Events;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ncqrs.Eventing;
using NHibernate.Linq;
using Quartz;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Headquarters;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Upgrade;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Verifier;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Assignments.Validators;
using WB.Core.BoundedContexts.Headquarters.CalendarEvents;
using WB.Core.BoundedContexts.Headquarters.DataExport.Factories;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views;
using WB.Core.BoundedContexts.Headquarters.EmailProviders;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Implementation;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.BoundedContexts.Headquarters.Implementation.Synchronization;
using WB.Core.BoundedContexts.Headquarters.Invitations;
using WB.Core.BoundedContexts.Headquarters.Maps;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.Users.UserPreloading;
using WB.Core.BoundedContexts.Headquarters.Users.UserPreloading.Dto;
using WB.Core.BoundedContexts.Headquarters.Users.UserPreloading.Services;
using WB.Core.BoundedContexts.Headquarters.Users.UserPreloading.Tasks;
using WB.Core.BoundedContexts.Headquarters.Users.UserProfile;
using WB.Core.BoundedContexts.Headquarters.Users.UserProfile.InterviewerAuditLog;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.BoundedContexts.Headquarters.Views.Maps;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.BoundedContexts.Headquarters.Workspaces;
using WB.Core.BoundedContexts.Headquarters.Workspaces.Impl;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.Synchronization;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.BoundedContexts.Supervisor.Services;
using WB.Core.BoundedContexts.Supervisor.Services.Implementation;
using WB.Core.BoundedContexts.Supervisor.Services.Implementation.OfflineSyncHandlers;
using WB.Core.BoundedContexts.Supervisor.Views;
using WB.Core.BoundedContexts.Tester.Implementation.Services;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.GenericSubdomains.Portable.Tasks;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.CommandBus.Implementation;
using WB.Core.Infrastructure.DenormalizerStorage;
using WB.Core.Infrastructure.Domain;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.HttpServices.Services;
using WB.Core.Infrastructure.Implementation;
using WB.Core.Infrastructure.Implementation.Aggregates;
using WB.Core.Infrastructure.Implementation.EventDispatcher;
using WB.Core.Infrastructure.Implementation.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Services;
using WB.Core.Infrastructure.TopologicalSorter;
using WB.Core.Infrastructure.WriteSide;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Implementation.Services;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Denormalizer;
using WB.Core.SharedKernels.Enumerator.Implementation.Repositories;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization.Steps;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Messages;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Services;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Services.Implementation;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Services.MapService;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Services.Workspace;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Infrastructure.Native;
using WB.Infrastructure.Native.Files.Implementation.FileSystem;
using WB.Infrastructure.Native.Questionnaire;
using WB.Infrastructure.Native.Questionnaire.Impl;
using WB.Infrastructure.Native.Storage;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Infrastructure.Native.Workspaces;
using WB.Tests.Abc.Storage;
using WB.UI.Shared.Web.Services;
using ILogger = WB.Core.GenericSubdomains.Portable.Services.ILogger;
using AttachmentContent = WB.Core.BoundedContexts.Headquarters.Views.Questionnaire.AttachmentContent;
using IAuditLogService = WB.Core.SharedKernels.Enumerator.Services.IAuditLogService;
using IDenormalizerRegistry = WB.Core.SharedKernels.Enumerator.Services.Infrastructure.IDenormalizerRegistry;

namespace WB.Tests.Abc.TestFactories
{
    internal class ServiceFactory
    {
        public CommandService CommandService(
            IEventSourcedAggregateRootRepository repository = null,
            IPlainAggregateRootRepository plainRepository = null,
            ILiteEventBus eventBus = null,
            IServiceLocator serviceLocator = null,
            IAggregateLock aggregateLock = null,
            IEventStore eventStore = null,
            IAggregateRootCache aggregateRootCacheCleaner = null,
            IAggregateRootPrototypeService prototypeService = null,
            IAggregateRootPrototypePromoterService promoterService = null)
        {
            var locatorMock = 
                serviceLocator != null ?
            Mock.Get(mocked: serviceLocator) : new Mock<IServiceLocator>();

            locatorMock.Setup(expression: x => x.GetInstance<IInScopeExecutor>())
                .Returns(valueFunction: () => new NoScopeInScopeExecutor(rootScope: locatorMock.Object));
            locatorMock.Setup(expression: x => x.GetInstance<ICommandExecutor>())
                .Returns(value: new CommandExecutor(
                    eventSourcedRepository: repository ?? Mock.Of<IEventSourcedAggregateRootRepository>(),
                    eventBus: eventBus ?? Mock.Of<IEventBus>(),
                    serviceLocator: locatorMock.Object,
                    plainRepository: plainRepository ?? Mock.Of<IPlainAggregateRootRepository>(),
                    aggregateRootCache: aggregateRootCacheCleaner ?? Create.Storage.NewAggregateRootCache(),
                    commandsMonitoring: Mock.Of<ICommandsMonitoring>(),
                    promoterService: promoterService ?? Mock.Of<IAggregateRootPrototypePromoterService>(),
                    prototypeService: prototypeService 
                                      ?? Mock.Of<IAggregateRootPrototypeService>(a => a.GetPrototypeType(It.IsAny<Guid>()) == null)));

            return new CommandService(serviceLocator: locatorMock.Object, aggregateLock: aggregateLock ?? Stub.Lock());
        }

        public AttachmentContentService AttachmentContentService(
            IPlainStorageAccessor<AttachmentContent> attachmentContentPlainStorage)
            => new AttachmentContentService(
                attachmentContentPlainStorage ?? Mock.Of<IPlainStorageAccessor<AttachmentContent>>());

        public CumulativeChartDenormalizer CumulativeChartDenormalizer(
            INativeReadSideStorage<CumulativeReportStatusChange> cumulativeReportReader = null,
            IReadSideRepositoryWriter<CumulativeReportStatusChange> cumulativeReportStatusChangeStorage = null,
            IQueryableReadSideRepositoryReader<InterviewSummary> interviewReferencesStorage = null)
            => new CumulativeChartDenormalizer(
                cumulativeReportStatusChangeStorage ?? Mock.Of<IReadSideRepositoryWriter<CumulativeReportStatusChange>>(),
                interviewReferencesStorage ?? new TestInMemoryWriter<InterviewSummary>(),
                cumulativeReportReader ?? Mock.Of<INativeReadSideStorage<CumulativeReportStatusChange>>(),
                Create.Storage.NewMemoryCache());

        public InterviewDashboardEventHandler DashboardDenormalizer(
            IPlainStorage<InterviewView> interviewViewRepository = null,
            IQuestionnaireStorage questionnaireStorage = null,
            IPlainStorage<PrefilledQuestionView> prefilledQuestions = null,
            IAnswerToStringConverter answerToStringConverter = null
        )
        {
            /*var serviceLocator = Create.Fake.ServiceLocator()
                .With(interviewViewRepository ?? Substitute.For<IPlainStorage<InterviewView>>())
                .With(questionnaireStorage ?? Substitute.For<IQuestionnaireStorage>())
                .With(prefilledQuestions ?? Substitute.For<IPlainStorage<PrefilledQuestionView>>())
                .With(answerToStringConverter ?? Substitute.For<IAnswerToStringConverter>())
                .Object;
            return new InterviewDashboardEventHandler(serviceLocator);*/
            return new InterviewDashboardEventHandler(
                interviewViewRepository ?? Substitute.For<IPlainStorage<InterviewView>>(),
                prefilledQuestions ?? Substitute.For<IPlainStorage<PrefilledQuestionView>>(), 
                questionnaireStorage ?? Substitute.For<IQuestionnaireStorage>(), 
                answerToStringConverter ?? Substitute.For<IAnswerToStringConverter>(),
                Mock.Of<IAssignmentDocumentsStorage>(),
                Mock.Of<ICalendarEventStorage>());
        }
        
        CalendarEventEventHandler CalendarEventDenormalizer(ICalendarEventStorage calendarEventStorage = null)
        {
            /*var serviceLocator = Create.Fake.ServiceLocator()
                .With(calendarEventStorage ?? Substitute.For<ICalendarEventStorage>())
                .With(Substitute.For<IPlainStorage<InterviewView>>())
                .With(Substitute.For<IAssignmentDocumentsStorage>())
                .Object;
            return new CalendarEventEventHandler(serviceLocator);*/
            return new CalendarEventEventHandler(
                calendarEventStorage ?? Substitute.For<ICalendarEventStorage>(),
                Mock.Of<IPlainStorage<InterviewView>>(),
                Mock.Of<IAssignmentDocumentsStorage>());
        }


        public DomainRepository DomainRepository(
            IServiceLocator serviceLocator = null)
            => new DomainRepository(
                serviceLocator: serviceLocator ?? Mock.Of<IServiceLocator>());

        public EventSourcedAggregateRootRepository EventSourcedAggregateRootRepository(
            IEventStore eventStore = null, IDomainRepository repository = null)
            => new EventSourcedAggregateRootRepository(eventStore, repository);

        public EventSourcedAggregateRootRepositoryWithCache EventSourcedAggregateRootRepositoryWithCache(
            IEventStore eventStore = null, IDomainRepository repository = null)
            => new EventSourcedAggregateRootRepositoryWithCache(
                eventStore ?? Mock.Of<IEventStore>(),
                repository ?? Mock.Of<IDomainRepository>(),
                new AggregateLock());
                
        public EventSourcedAggregateRootRepositoryWithWebCache EventSourcedAggregateRootRepositoryWithWebCache(
            IEventStore eventStore = null, IDomainRepository repository = null)
            => new EventSourcedAggregateRootRepositoryWithWebCache(
                eventStore ?? Mock.Of<IEventStore>(x => x.GetLastEventSequence(It.IsAny<Guid>()) == 0),
                Create.Storage.InMemoryEventStore(),
                Create.Service.MockOfAggregatePrototypeService(), 
                repository ?? Mock.Of<IDomainRepository>(),
                Create.Service.ServiceLocatorService(),
                new AggregateLock(),
                Create.Storage.NewAggregateRootCache(),
                Options.Create(new SchedulerConfig()));

        public FileSystemIOAccessor FileSystemIOAccessor()
            => new FileSystemIOAccessor();

        public InterviewAnswersCommandValidator InterviewAnswersCommandValidator(
            IAllInterviewsFactory interviewSummaryViewFactory = null)
            => new InterviewAnswersCommandValidator(
                interviewSummaryViewFactory ?? Mock.Of<IAllInterviewsFactory>());

        public InterviewerInterviewAccessor InterviewerInterviewAccessor(
            IPlainStorage<InterviewView> interviewViewRepository = null,
            IEnumeratorEventStorage eventStore = null,
            ICommandService commandService = null,
            IPlainStorage<QuestionnaireView> questionnaireRepository = null,
            IPrincipal principal = null,
            IJsonAllTypesSerializer synchronizationSerializer = null,
            IEventSourcedAggregateRootRepositoryWithCache aggregateRootRepositoryWithCache = null,
            IPlainStorage<InterviewMultimediaView> interviewMultimediaViewRepository = null,
            IPlainStorage<InterviewFileView> interviewFileViewRepository = null,
            IPlainStorage<InterviewSequenceView, Guid> interviewSequenceStorage = null,
            IInterviewEventStreamOptimizer eventStreamOptimizer = null,
            ILiteEventBus eventBus = null,
            ILogger logger = null,
            IPlainStorage<PrefilledQuestionView> prefilledQuestions = null)
            => new InterviewerInterviewAccessor(
                questionnaireRepository ?? Mock.Of<IPlainStorage<QuestionnaireView>>(),
                prefilledQuestions ?? Mock.Of<IPlainStorage<PrefilledQuestionView>>() ,
                interviewViewRepository ?? Mock.Of<IPlainStorage<InterviewView>>(),
                interviewMultimediaViewRepository ?? Mock.Of<IPlainStorage<InterviewMultimediaView>>(),
                interviewFileViewRepository ?? Mock.Of<IPlainStorage<InterviewFileView>>(),
                commandService ?? Mock.Of<ICommandService>(),
                principal ?? Mock.Of<IPrincipal>(),
                eventStore ?? Mock.Of<IEnumeratorEventStorage>(),
                aggregateRootRepositoryWithCache ?? Mock.Of<IEventSourcedAggregateRootRepositoryWithCache>(),
                synchronizationSerializer ?? Mock.Of<IJsonAllTypesSerializer>(),
                eventStreamOptimizer ?? Mock.Of<IInterviewEventStreamOptimizer>(),
                Mock.Of<IViewModelEventRegistry>(),
                interviewSequenceStorage ?? Mock.Of<IPlainStorage<InterviewSequenceView, Guid>>(),
                eventBus ?? Mock.Of<ILiteEventBus>(),
                logger?? Mock.Of<ILogger>());

        public InterviewEventStreamOptimizer InterviewEventStreamOptimizer()
            => new InterviewEventStreamOptimizer();

        public KeywordsProvider KeywordsProvider()
            => new KeywordsProvider(Create.Service.SubstitutionService());

        public LiteEventBus LiteEventBus(IViewModelEventRegistry liteEventRegistry = null,
            IEventStore eventStore = null,
            IDenormalizerRegistry denormalizerRegistry = null,
            IAsyncEventQueue viewModelEventQueue = null)
        {
            liteEventRegistry = liteEventRegistry ?? LiteEventRegistry();

            var viewModelEventPublisher = new AsyncEventDispatcher(liteEventRegistry, Mock.Of<ILogger>(),
                Mock.Of<ICurrentViewModelPresenter>());

            var mockOfViewModelEventQueue = new Mock<IAsyncEventQueue>();
            mockOfViewModelEventQueue.Setup(x => x.Enqueue(Moq.It.IsAny<IReadOnlyCollection<CommittedEvent>>()))
                .Callback<IReadOnlyCollection<CommittedEvent>>(@events =>
                    viewModelEventPublisher.ExecuteAsync(@events).WaitAndUnwrapException());

            return new LiteEventBus(eventStore ?? Mock.Of<IEventStore>(),
                denormalizerRegistry ?? Stub<IDenormalizerRegistry>.WithNotEmptyValues,
                viewModelEventQueue ?? mockOfViewModelEventQueue.Object,
                Mock.Of<ILogger>());
        }

        public ViewModelEventRegistry LiteEventRegistry()
            => new ViewModelEventRegistry();

        public EnumeratorDenormalizerRegistry DenormalizerRegistry() =>
            new EnumeratorDenormalizerRegistry(
                Create.Service.ServiceLocatorService(DashboardDenormalizer(),
                CalendarEventDenormalizer()), Mock.Of<ILogger>());

        public WB.Core.Infrastructure.Implementation.EventDispatcher.DenormalizerRegistry DenormalizerRegistryNative() 
            => new WB.Core.Infrastructure.Implementation.EventDispatcher.DenormalizerRegistry(new EventBusSettings());

        public AsyncEventQueue ViewModelEventQueue(IViewModelEventRegistry liteEventRegistry) =>
            new AsyncEventQueue(new AsyncEventDispatcher(liteEventRegistry,
                Mock.Of<ILogger>(), Mock.Of<ICurrentViewModelPresenter>()), Mock.Of<ILogger>());

        public NcqrCompatibleEventDispatcher NcqrCompatibleEventDispatcher(EventBusSettings eventBusSettings = null,
            ILogger logger = null,
            IServiceLocator serviceLocator = null,
            WB.Core.Infrastructure.Implementation.EventDispatcher.IDenormalizerRegistry denormalizerRegistry = null,
            IAggregateRootPrototypeService prototypeService = null)
            => new NcqrCompatibleEventDispatcher(
                eventStore: Mock.Of<IEventStore>(),
                inMemoryEventStore: Mock.Of<IInMemoryEventStore>(),
                serviceLocator: serviceLocator ?? Mock.Of<IServiceLocator>(),
                eventBusSettings: eventBusSettings ?? Create.Entity.EventBusSettings(),
                logger: logger ?? Mock.Of<ILogger>(),
                denormalizerRegistry: denormalizerRegistry ?? Create.Service.DenormalizerRegistryNative(),
                prototypeService: prototypeService ?? Create.Service.MockOfAggregatePrototypeService());

        public IAggregateRootPrototypeService MockOfAggregatePrototypeService(PrototypeType? type = null)
        {
            return Mock.Of<IAggregateRootPrototypeService>(s => s.GetPrototypeType(It.IsAny<Guid>()) == type);
        }

        public QuestionnaireKeyValueStorage QuestionnaireKeyValueStorage(
            IPlainStorage<QuestionnaireDocumentView> questionnaireDocumentViewRepository = null)
            => new QuestionnaireKeyValueStorage(
                questionnaireDocumentViewRepository ?? Mock.Of<IPlainStorage<QuestionnaireDocumentView>>());

        public QuestionnaireValidator QuestionnaireNameValidator(
            IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaireBrowseItemStorage = null)
            => new QuestionnaireValidator(
                questionnaireBrowseItemStorage ??
                Stub<IPlainStorageAccessor<QuestionnaireBrowseItem>>.WithNotEmptyValues);

        public IStatefulInterviewRepository StatefulInterviewRepository(
            IEventSourcedAggregateRootRepository aggregateRootRepository)
            => new StatefulInterviewRepository(
                aggregateRootRepository: aggregateRootRepository ?? Mock.Of<IEventSourcedAggregateRootRepository>());

        public ISubstitutionService SubstitutionService()
            => new SubstitutionService();

        public TeamViewFactory TeamViewFactory(
            IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaryReader = null)
            => new TeamViewFactory(interviewSummaryReader, Mock.Of<IUserRepository>(), Mock.Of<IUnitOfWork>());

        public ITopologicalSorter<T> TopologicalSorter<T>()
            => new TopologicalSorter<T>();

        public VariableToUIStringService VariableToUIStringService()
            => new VariableToUIStringService();

        public IInterviewExpressionStorageProvider ExpressionStatePrototypeProvider()
        {
            var expressionStatePrototypeProvider = new Mock<IInterviewExpressionStorageProvider>();
            
            return expressionStatePrototypeProvider.Object;
        }

        
        public ISubstitutionTextFactory SubstitutionTextFactory()
        {
            return new SubstitutionTextFactory(Create.Service.SubstitutionService(),
                Create.Service.VariableToUIStringService());
        }

        public InterviewViewModelFactory InterviewViewModelFactory(IQuestionnaireStorage questionnaireRepository,
            IStatefulInterviewRepository interviewRepository,
            IServiceLocator serviceLocator,
            IEnumeratorSettings settings)
        {
            return new InterviewerInterviewViewModelFactory(questionnaireRepository ?? Mock.Of<IQuestionnaireStorage>(),
                interviewRepository ?? Mock.Of<IStatefulInterviewRepository>(),
                settings ?? Mock.Of<IEnumeratorSettings>(),
                serviceLocator ?? Mock.Of<IServiceLocator>());
        }

        public SupervisorInterviewViewModelFactory SupervisorInterviewViewModelFactory(IQuestionnaireStorage questionnaireRepository = null,
            IStatefulInterviewRepository interviewRepository = null,
            IServiceLocator serviceLocator = null,
            IEnumeratorSettings settings = null)
        {
            return new SupervisorInterviewViewModelFactory(questionnaireRepository ?? Mock.Of<IQuestionnaireStorage>(),
                interviewRepository ?? Mock.Of<IStatefulInterviewRepository>(),
                settings ?? Mock.Of<IEnumeratorSettings>(),
                serviceLocator ?? Mock.Of<IServiceLocator>());
        }

        public AllInterviewsFactory AllInterviewsFactory(
            IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummarys = null)
        {
            return new AllInterviewsFactory(interviewSummarys ??
                                            Mock.Of<IQueryableReadSideRepositoryReader<InterviewSummary>>());
        }
        
        public InterviewHistoryFactory InterviewHistoryFactory(
            IEventStore eventStore = null, 
            IReadSideRepositoryWriter<InterviewSummary> interviewSummaries = null,
            IUserViewFactory userReader = null,
            IQuestionnaireExportStructureStorage questionnaireExportStructureStorage = null,
            IQuestionnaireStorage questionnaireStorage = null)
        {
            return new InterviewHistoryFactory(
                eventStore ?? Mock.Of<IEventStore>(),
                interviewSummaries ?? Mock.Of<IReadSideRepositoryWriter<InterviewSummary>>(),
                userReader ?? Mock.Of<IUserViewFactory>(),
                questionnaireExportStructureStorage ?? Mock.Of<IQuestionnaireExportStructureStorage>(),
                questionnaireStorage ?? Mock.Of<IQuestionnaireStorage>());
        }

        public IRandomValuesSource RandomValuesSource(params int[] sequence)
        {
            var result = Substitute.For<IRandomValuesSource>();
            if (sequence?.Length > 0) result.Next(0).ReturnsForAnyArgs(sequence.First(), sequence.Skip(1).ToArray());
            else result.Next(0).ReturnsForAnyArgs(1, 2, 3, 4, 5, 7, 8, 9, 10);
            return result;
        }

        public ReadSideToTabularFormatExportService ReadSideToTabularFormatExportService(
            IFileSystemAccessor fileSystemAccessor = null,
            ICsvWriterService csvWriterService = null,
            ICsvWriter csvWriter = null,
            IQueryableReadSideRepositoryReader<InterviewSummary> interviewStatuses = null,
            QuestionnaireExportStructure questionnaireExportStructure = null)
            => new ReadSideToTabularFormatExportService(fileSystemAccessor ?? Mock.Of<IFileSystemAccessor>(),
                csvWriter ?? Mock.Of<ICsvWriter>(_
                    => _.OpenCsvWriter(It.IsAny<Stream>(), It.IsAny<string>()) ==
                       (csvWriterService ?? Mock.Of<ICsvWriterService>())),
                Mock.Of<ILogger>(),
                Mock.Of<IQuestionnaireExportStructureStorage>(_
                    => _.GetQuestionnaireExportStructure(It.IsAny<QuestionnaireIdentity>()) ==
                       questionnaireExportStructure));

        public InterviewerPrincipal InterviewerPrincipal(IPlainStorage<InterviewerIdentity> interviewersPlainStorage,
            IPasswordHasher passwordHasher)
        {
            return new InterviewerPrincipal(
                interviewersPlainStorage ?? Mock.Of<IPlainStorage<InterviewerIdentity>>(),
                passwordHasher ?? Mock.Of<IPasswordHasher>(),
                Mock.Of<ILogger>());
        }

        public SupervisorPrincipal SupervisorPrincipal(IPlainStorage<SupervisorIdentity> storage,
            IPasswordHasher passwordHasher)
            => new SupervisorPrincipal(
                storage ?? Mock.Of<IPlainStorage<SupervisorIdentity>>(),
                passwordHasher ?? Mock.Of<IPasswordHasher>(),
                Mock.Of<ILogger>());

        public IInterviewerPrincipal InterviewerPrincipal(Guid userId)
            => Mock.Of<IInterviewerPrincipal>(x => x.IsAuthenticated == true && x.CurrentUserIdentity == Mock.Of<IInterviewerUserIdentity>(u => u.UserId == userId));

        public IPrincipal Principal(Guid userId)
            => Mock.Of<IPrincipal>(x => x.IsAuthenticated == true && x.CurrentUserIdentity == Mock.Of<IUserIdentity>(u => u.UserId == userId));

        public InterviewerOnlineSynchronizationProcess SynchronizationProcess(
            IPlainStorage<InterviewView> interviewViewRepository = null,
            IPlainStorage<InterviewMultimediaView> interviewMultimediaViewStorage = null,
            IPlainStorage<InterviewFileView> interviewFileViewStorage = null,
            IOnlineSynchronizationService synchronizationService = null,
            ILogger logger = null,
            IUserInteractionService userInteractionService = null,
            IPasswordHasher passwordHasher = null,
            IInterviewerPrincipal principal = null,
            IHttpStatistician httpStatistician = null,
            IServiceLocator serviceLocator = null,
            IWorkspaceService workspaceService = null)
        {
            var syncServiceMock = synchronizationService ?? Mock.Of<IOnlineSynchronizationService>();

            return new InterviewerOnlineSynchronizationProcess(
                syncServiceMock,
                interviewViewRepository ?? new InMemoryPlainStorage<InterviewView>(Mock.Of<ILogger>()),
                principal ?? Mock.Of<IInterviewerPrincipal>(),
                logger ?? Mock.Of<ILogger>(),
                passwordHasher ?? Mock.Of<IPasswordHasher>(),
                httpStatistician ?? Mock.Of<IHttpStatistician>(),
                Mock.Of<IAssignmentDocumentsStorage>(),
                Mock.Of<IInterviewerSettings>(),
                Mock.Of<IAuditLogService>(),
                Mock.Of<IDeviceInformationService>(),
                userInteractionService ?? Mock.Of<IUserInteractionService>(),
                serviceLocator ?? Mock.Of<IServiceLocator>(),
                workspaceService ?? Mock.Of<IWorkspaceService>(),
                Mock.Of<IViewModelNavigationService>());
        }

        public InterviewerOfflineSynchronizationProcess OfflineSynchronizationProcess(
            IPlainStorage<InterviewView> interviewViewRepository = null,
            ILogger logger = null,
            IPasswordHasher passwordHasher = null,
            IInterviewerPrincipal principal = null,
            IHttpStatistician httpStatistician = null,
            IOfflineSynchronizationService synchronizationService = null)
        {
            var syncServiceMock = synchronizationService ?? Mock.Of<IOfflineSynchronizationService>();

            return new InterviewerOfflineSynchronizationProcess(
                syncServiceMock,
                interviewViewRepository ?? new InMemoryPlainStorage<InterviewView>(Mock.Of<ILogger>()),
                principal ?? Mock.Of<IInterviewerPrincipal>(),
                logger ?? Mock.Of<ILogger>(),
                httpStatistician ?? Mock.Of<IHttpStatistician>(),
                Mock.Of<IAssignmentDocumentsStorage>(),
                Mock.Of<IAuditLogService>(),
                Mock.Of<IInterviewerSettings>(),
                Mock.Of<IDeviceInformationService>(),
                Mock.Of<IServiceLocator>(),
                Mock.Of<IUserInteractionService>());
        }

        public OnlineSynchronizationService SynchronizationService(IPrincipal principal = null,
            IRestService restService = null,
            IInterviewerSettings interviewerSettings = null,
            IInterviewerSyncProtocolVersionProvider syncProtocolVersionProvider = null,
            IFileSystemAccessor fileSystemAccessor = null,
            ILogger logger = null)
        {
            return new OnlineSynchronizationService(
                principal ?? Mock.Of<IPrincipal>(),
                restService ?? Mock.Of<IRestService>(),
                interviewerSettings ?? Mock.Of<IInterviewerSettings>(),
                syncProtocolVersionProvider ?? Mock.Of<IInterviewerSyncProtocolVersionProvider>(),
                fileSystemAccessor ?? Mock.Of<IFileSystemAccessor>(),
                Mock.Of<ICheckVersionUriProvider>(),
                logger ?? Mock.Of<ILogger>()
            );
        }

        public TesterImageFileStorage TesterPlainInterviewFileStorage(IFileSystemAccessor fileSystemAccessor,
            string rootDirectory)
        {
            return new TesterImageFileStorage(fileSystemAccessor, rootDirectory);
        }

        public IQuestionnaireDownloader QuestionnaireDownloader(
            IAttachmentContentStorage attachmentContentStorage = null,
            IInterviewerQuestionnaireAccessor questionnairesAccessor = null,
            ISynchronizationService synchronizationService = null)
        {
            return new QuestionnaireDownloader(
                attachmentContentStorage ?? Mock.Of<IAttachmentContentStorage>(),
                questionnairesAccessor ?? Mock.Of<IInterviewerQuestionnaireAccessor>(),
                synchronizationService ?? Mock.Of<ISynchronizationService>(),
                Mock.Of<ILogger>());
        }

        public IAssignmentsSynchronizer AssignmentsSynchronizer(
            ISynchronizationService synchronizationService = null,
            IAssignmentDocumentsStorage assignmentsRepository = null,
            IQuestionnaireDownloader questionnaireDownloader = null,
            IQuestionnaireStorage questionnaireStorage = null,
            IPlainStorage<InterviewView> interviewViewRepository = null,
            IPlainStorage<InterviewerDocument> interviewerViewRepository = null)
        {
            return new AssignmentsSynchronizer(
                synchronizationService ?? Mock.Of<ISynchronizationService>(),
                assignmentsRepository ?? Create.Storage.AssignmentDocumentsInmemoryStorage(),
                questionnaireDownloader ?? Mock.Of<IQuestionnaireDownloader>(),
                questionnaireStorage ?? Mock.Of<IQuestionnaireStorage>(),
                new AssignmentDocumentFromDtoBuilder(
                Mock.Of<IAnswerToStringConverter>(),
                Mock.Of<IInterviewAnswerSerializer>()),
                interviewViewRepository ?? Mock.Of<IPlainStorage<InterviewView>>(),
                interviewerViewRepository ?? Mock.Of<IPlainStorage<InterviewerDocument>>(),
                Mock.Of<ILogger>());
        }

        public IAnswerToStringConverter AnswerToStringConverter()
        {
            return new AnswerToStringConverter();
        }

        public ExpressionsPlayOrderProvider ExpressionsPlayOrderProvider(
            IExpressionProcessor expressionProcessor = null,
            IMacrosSubstitutionService macrosSubstitutionService = null)
        {
            return new ExpressionsPlayOrderProvider(
                new ExpressionsGraphProvider(
                    expressionProcessor ?? new RoslynExpressionProcessor(),
                    macrosSubstitutionService ?? Create.Service.DefaultMacrosSubstitutionService()));
        }

        public IMacrosSubstitutionService DefaultMacrosSubstitutionService()
        {
            var macrosSubstitutionServiceMock = new Mock<IMacrosSubstitutionService>();
            macrosSubstitutionServiceMock.Setup(x => x.InlineMacros(It.IsAny<string>(), It.IsAny<IEnumerable<Macro>>()))
                .Returns((string e, IEnumerable<Macro> macros) => e);
            return macrosSubstitutionServiceMock.Object;
        }

        public IAssignmentsService AssignmentService(params Assignment[] assignments)
        {
            var accessor = new TestInMemoryWriter<Assignment, Guid>();
            foreach (var assignment in assignments)
            {
                accessor.Store(assignment, assignment.PublicKey);
            }

            var service = new AssignmentsService(accessor, Mock.Of<IInterviewAnswerSerializer>(),
                Mock.Of<IUnitOfWork>(), Mock.Of<IAuthorizedUser>());

            return service;
        }

        public IInterviewTreeBuilder InterviewTreeBuilder()
        {
            return new InterviewTreeBuilder(Create.Service.SubstitutionTextFactory());
        }

        public InterviewStatusTimeSpanDenormalizer InterviewStatusTimeSpanDenormalizer()
        {
            return new InterviewStatusTimeSpanDenormalizer();
        }
        
        public ICsvWriter CsvWriter(List<CsvData> writeTo)
        {
            var csvWriterMock = new Mock<ICsvWriter>();
            csvWriterMock
                .Setup(x => x.WriteData(It.IsAny<string>(), It.IsAny<IEnumerable<string[]>>(), It.IsAny<string>()))
                .Callback((string s, IEnumerable<string[]> p, string t) =>
                {
                    writeTo.Add(new CsvData
                    {
                        File = s,
                        Data = p.ToList()
                    });
                });
            return csvWriterMock.Object;
        }

        public UserImportService UserImportService(UserPreloadingSettings userPreloadingSettings = null,
            ICsvReader csvReader = null,
            IPlainStorageAccessor<UsersImportProcess> importUsersProcessRepository = null,
            IPlainStorageAccessor<UserToImport> importUsersRepository = null,
            IUserRepository userStorage = null,
            IUserImportVerifier userImportVerifier = null,
            IAuthorizedUser authorizedUser = null,
            IUnitOfWork sessionProvider = null,
            PasswordOptions passwordOptions = null,
            IWorkspacesService workspacesService = null)
        {
            PasswordOptions defaultPasswordOptions = passwordOptions ?? new PasswordOptions
            {
                RequireDigit = true,
                RequiredLength = 10,
                RequireLowercase = true,
                RequireUppercase = true,
                RequiredUniqueChars = 5,
                RequireNonAlphanumeric = true
            };

            userPreloadingSettings ??= Create.Entity.UserPreloadingSettings();
            return new UserImportService(
                userPreloadingSettings,
                csvReader ?? Stub<ICsvReader>.WithNotEmptyValues,
                importUsersProcessRepository ?? Stub<IPlainStorageAccessor<UsersImportProcess>>.WithNotEmptyValues,
                importUsersRepository ?? Stub<IPlainStorageAccessor<UserToImport>>.WithNotEmptyValues,
                userStorage ?? Stub<IUserRepository>.WithNotEmptyValues,
                userImportVerifier ?? new UserImportVerifier(userPreloadingSettings,
                    Mock.Of<IOptions<IdentityOptions>>(x => x.Value == new IdentityOptions {Password = defaultPasswordOptions} )),
                authorizedUser ?? Stub<IAuthorizedUser>.WithNotEmptyValues,
                sessionProvider ?? Stub<IUnitOfWork>.WithNotEmptyValues,
                workspacesService ?? Create.Service.WorkspacesService(Mock.Of<IPlainStorageAccessor<Workspace>>()));
        }

        public ICsvReader CsvReader<T>(string[] headers, params T[] rows)
        {
            return Mock.Of<ICsvReader>(
                x => x.ReadAll<T>(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<bool>()) == rows &&
                     x.ReadHeader(It.IsAny<Stream>(), It.IsAny<string>()) == headers);
        }

        public InterviewerProfileFactory InterviewerProfileFactory(IUserRepository userManager = null,
            IQueryableReadSideRepositoryReader<InterviewSummary> interviewRepository = null,
            IDeviceSyncInfoRepository deviceSyncInfoRepository = null,
            IInterviewerVersionReader interviewerVersionReader = null,
            IInterviewFactory interviewFactory = null,
            IAuthorizedUser currentUser = null)
        {
            var defaultUserManager = Mock.Of<IUserRepository>(x => x.Users == (new HqUser[0]).AsQueryable());
            return new InterviewerProfileFactory(
                userManager ?? defaultUserManager,
                interviewRepository ?? Mock.Of<IQueryableReadSideRepositoryReader<InterviewSummary>>(),
                deviceSyncInfoRepository ?? Mock.Of<IDeviceSyncInfoRepository>(),
                interviewerVersionReader ?? Mock.Of<IInterviewerVersionReader>(),
                interviewFactory ?? Mock.Of<IInterviewFactory>(),
                currentUser ?? Mock.Of<IAuthorizedUser>(),
                Mock.Of<IQRCodeHelper>(),
                Mock.Of<IPlainKeyValueStorage<ProfileSettings>>(),
                Mock.Of<IVirtualPathService>(s => s.GetAbsolutePath(It.IsAny<string>()) == "path")
                );
        }

        public InterviewPackagesService InterviewPackagesService(
            IPlainStorageAccessor<InterviewPackage> interviewPackageStorage = null,
            IPlainStorageAccessor<BrokenInterviewPackage> brokenInterviewPackageStorage = null,
            ILogger logger = null,
            IJsonAllTypesSerializer serializer = null,
            ICommandService commandService = null,
            IInterviewUniqueKeyGenerator uniqueKeyGenerator = null,
            SyncSettings syncSettings = null,
            IQueryableReadSideRepositoryReader<InterviewSummary> interviews = null,
            IUserRepository userRepository = null,
            ISessionFactory sessionFactory = null,
            IInScopeExecutor inScopeExecutor = null)
        {
            InterviewKey generatedInterviewKey = new InterviewKey(5533);

            var userRepositoryMock = new Mock<IUserRepository>();

            var hqUserProfile = Mock.Of<WorkspaceUserProfile>(_ => _.SupervisorId == Id.gB);

            var hqUser = Mock.Of<HqUser>(_ => _.Id == Id.gA
                                           && _.WorkspaceProfile == hqUserProfile);
            userRepositoryMock
                .Setup(arg => arg.FindByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(hqUser);

            return new InterviewPackagesService(
                syncSettings: syncSettings ?? Mock.Of<SyncSettings>(),
                logger: logger ?? Mock.Of<ILogger>(),
                interviewPackageStorage: interviewPackageStorage ?? Mock.Of<IPlainStorageAccessor<InterviewPackage>>(),
                brokenInterviewPackageStorage: brokenInterviewPackageStorage ?? Mock.Of<IPlainStorageAccessor<BrokenInterviewPackage>>(),
                packagesTracker: new TestPlainStorage<ReceivedPackageLogEntry>(),
                inScopeExecutor: inScopeExecutor ?? Create.Service.InScopeExecutor(Create.Service.ServiceLocatorService()));
        }

        public ImportDataVerifier ImportDataVerifier(IFileSystemAccessor fileSystem = null,
            IInterviewTreeBuilder interviewTreeBuilder = null,
            IUserViewFactory userViewFactory = null,
            IQuestionOptionsRepository optionsRepository = null,
            IQueryableReadSideRepositoryReader<Assignment, Guid> assignmentsRepository = null)
            => new ImportDataVerifier(fileSystem ?? new FileSystemIOAccessor(),
                interviewTreeBuilder ?? Create.Service.InterviewTreeBuilder(),
                userViewFactory ?? Mock.Of<IUserViewFactory>(),
                optionsRepository ?? Mock.Of<IQuestionOptionsRepository>(),
                assignmentsRepository ?? Mock.Of<IQueryableReadSideRepositoryReader<Assignment, Guid>>());

        public IAssignmentsUpgrader AssignmentsUpgrader(IPreloadedDataVerifier importService = null,
            IQuestionnaireStorage questionnaireStorage = null,
            IAssignmentsService assignments = null,
            IAssignmentsUpgradeService upgradeService = null,
            ICommandService commandService = null)
        {
            var commands = commandService ?? Mock.Of<ICommandService>();
            IAssignmentsService assignmentsService = assignments;
            if (assignmentsService == null)
            {
                var assignment = Create.Entity.Assignment();
                assignmentsService = Mock.Of<IAssignmentsService>(s =>
                    s.GetAssignmentByAggregateRootId(It.IsAny<Guid>()) == assignment);
            }

            var questionnaires = questionnaireStorage ?? Mock.Of<IQuestionnaireStorage>();
            var upgService = upgradeService ?? Mock.Of<IAssignmentsUpgradeService>();
            var localVerifier = importService ?? Mock.Of<IPreloadedDataVerifier>();

            var upgrader = new SingleAssignmentUpgrader(Create.Service.AssignmentFactory(commands, assignmentsService, Mock.Of<IAssignmentIdGenerator>()),
                Mock.Of<IInvitationService>(),
                commands,
                localVerifier,
                assignmentsService);
            
            var sl = Mock.Of<IServiceLocator>(locator =>
                locator.GetInstance<ISingleAssignmentUpgrader>() == upgrader
            );

            return new AssignmentsUpgrader(
                assignmentsService,
                questionnaires,
                upgService,
                Create.Service.InScopeExecutor(sl),
                Mock.Of<ILogger<AssignmentsUpgrader>>());
        }

        public IAssignmentFactory AssignmentFactory(
            ICommandService commandService = null, 
            IAssignmentsService assignmentsService = null,
            IAssignmentIdGenerator assignmentIdGenerator = null)
        {
            var result = new AssignmentFactory(new InMemoryPlainStorageAccessor<QuestionnaireBrowseItem>(),
                commandService ?? Mock.Of<ICommandService>(),
                assignmentsService ?? Mock.Of<IAssignmentsService>(),
                assignmentIdGenerator ?? Mock.Of<IAssignmentIdGenerator>());
            return result;
        }

        public AssignmentsImportFileConverter AssignmentsImportFileConverter(IFileSystemAccessor fs = null, IUserViewFactory userViewFactory = null)
            => new AssignmentsImportFileConverter(fs ?? Create.Service.FileSystemIOAccessor(), userViewFactory ?? Mock.Of<IUserViewFactory>());

        public AssignmentsImportReader AssignmentsImportReader(ICsvReader csvReader = null,
            IArchiveUtils archiveUtils = null)
            => new AssignmentsImportReader(csvReader ?? Create.Service.CsvReader(),
                archiveUtils ?? Create.Service.ArchiveUtils());

        public CsvReader CsvReader() => new CsvReader();
        public ZipArchiveUtils ArchiveUtils() => new ZipArchiveUtils();

        public AssignmentsImportService AssignmentsImportService(IUserViewFactory userViewFactory = null,
            IPreloadedDataVerifier verifier = null,
            IAuthorizedUser authorizedUser = null,
            IUnitOfWork sessionProvider = null,
            IPlainStorageAccessor<AssignmentsImportProcess> importAssignmentsProcessRepository = null,
            IPlainStorageAccessor<AssignmentToImport> importAssignmentsRepository = null,
            IQueryableReadSideRepositoryReader<Assignment, Guid> assignmentsStorage = null,
            IAssignmentsImportFileConverter assignmentsImportFileConverter = null,
            IInvitationService invitationService = null,
            IAssignmentFactory assignmentFactory = null)
        {
            var session = Mock.Of<ISession>(x =>
                x.Query<AssignmentsImportProcess>() == GetNhQueryable<AssignmentsImportProcess>() &&
                x.Query<AssignmentToImport>() == GetNhQueryable<AssignmentToImport>());

            sessionProvider ??= Mock.Of<IUnitOfWork>(x => x.Session == session);
            userViewFactory ??= Mock.Of<IUserViewFactory>();

            return new AssignmentsImportService(verifier ?? ImportDataVerifier(),
                authorizedUser ?? Mock.Of<IAuthorizedUser>(),
                sessionProvider,
                importAssignmentsProcessRepository ?? Mock.Of<IPlainStorageAccessor<AssignmentsImportProcess>>(),
                importAssignmentsRepository ?? Mock.Of<IPlainStorageAccessor<AssignmentToImport>>(),
                assignmentsImportFileConverter ?? AssignmentsImportFileConverter(userViewFactory: userViewFactory),
                assignmentFactory ?? Create.Service.AssignmentFactory(),
                invitationService ?? Mock.Of<IInvitationService>(),
                Mock.Of<IAssignmentPasswordGenerator>());
        }

        public NearbyCommunicator NearbyConnectionManager(IRequestHandler requestHandler = null, int maxBytesLength = 0)
        {
            return new NearbyCommunicator(requestHandler ?? Mock.Of<IRequestHandler>(),
                Create.Fake.PayloadProvider(),
                new PayloadSerializer(new JsonAllTypesSerializer()), Mock.Of<IConnectionsApiLimits>(c => c.MaxBytesLength == maxBytesLength), Mock.Of<ILogger>());
        }

        public NearbyConnectionsRequestHandler GoogleConnectionsRequestHandler()
        {
            return new NearbyConnectionsRequestHandler(Mock.Of<ILogger>());
        }

        private static IQueryable<TEntity> GetNhQueryable<TEntity>() => Mock.Of<IQueryable<TEntity>>(x => x.Provider == Mock.Of<INhQueryProvider>());

        public OfflineSynchronizationService OfflineSynchronizationService(
            IOfflineSyncClient offlineSyncClient = null,
            IInterviewerPrincipal interviewerPrincipal = null,
            IDeviceSettings deviceSettings = null)
        {
            return new OfflineSynchronizationService(
                offlineSyncClient ?? Mock.Of<IOfflineSyncClient>(),
                interviewerPrincipal ?? Mock.Of<IInterviewerPrincipal>(),
                Mock.Of<IInterviewerQuestionnaireAccessor>(),
                Mock.Of<IPlainStorage<InterviewView>>(),
                Mock.Of<IEnumeratorSettings>(),
                deviceSettings: deviceSettings ?? Mock.Of<IDeviceSettings>());
        }

        public SupervisorSynchronizeHandler SupervisorSynchronizeHandler(
            IPlainStorage<InterviewerDocument> interviewerViewRepository = null,
            ISupervisorSettings settings = null,
            IFileSystemAccessor fileSystemAccessor = null)
        {
            return new SupervisorSynchronizeHandler(
                interviewerViewRepository ?? Mock.Of<IPlainStorage<InterviewerDocument>>(),
                settings ?? Mock.Of<ISupervisorSettings>(),
                fileSystemAccessor: fileSystemAccessor ?? Mock.Of<IFileSystemAccessor>(),
                secureStorage: Mock.Of<ISecureStorage>());
        }

        public SupervisorInterviewsHandler SupervisorInterviewsHandler(ILiteEventBus eventBus = null,
            IEnumeratorEventStorage eventStorage = null,
            IPlainStorage<InterviewView> interviews = null,
            ICommandService commandService = null,
            IJsonAllTypesSerializer serializer = null,
            IPlainStorage<BrokenInterviewPackageView, int?> brokenInterviewStorage = null,
            IPrincipal principal = null,
            IPlainStorage<InterviewerDocument> interviewerViewRepository = null,
            IPlainStorage<SuperivsorReceivedPackageLogEntry, int> receivedPackagesLog = null,
            IAssignmentDocumentsStorage assignments = null,
            ISupervisorSettings settings = null)
        {
            return new SupervisorInterviewsHandler(
                eventBus ?? Mock.Of<ILiteEventBus>(),
                eventStorage ?? Mock.Of<IEnumeratorEventStorage>(),
                interviews ?? new InMemoryPlainStorage<InterviewView>(Mock.Of<ILogger>()),
                serializer ?? Mock.Of<IJsonAllTypesSerializer>(s => s.Deserialize<AggregateRootEvent[]>(It.IsAny<string>()) == new AggregateRootEvent[] { }),// new JsonAllTypesSerializer(),
                commandService ?? Mock.Of<ICommandService>(),
                Mock.Of<ILogger>(),
                brokenInterviewStorage ?? Mock.Of<IPlainStorage<BrokenInterviewPackageView, int?>>(),
                receivedPackagesLog ?? new SqliteInmemoryStorage<SuperivsorReceivedPackageLogEntry, int>(),
                principal ?? Mock.Of<IPrincipal>(),
                assignments ?? Create.Storage.AssignmentDocumentsInmemoryStorage(),
                settings ?? Mock.Of<ISupervisorSettings>());
        }

        public SupervisorGroupStateCalculationStrategy SupervisorGroupStateCalculationStrategy()
        {
            return new SupervisorGroupStateCalculationStrategy();
        }

        public SupervisorAssignmentsHandler SupervisorAssignmentsHandler(
            IAssignmentDocumentsStorage assignmentDocumentsStorage = null)
        {
            return new SupervisorAssignmentsHandler(assignmentDocumentsStorage ??
                                                    Create.Storage.AssignmentDocumentsInmemoryStorage());
        }

        public InterviewerUpdateHandler InterviewerUpdateHandler(IFileSystemAccessor fileSystemAccessor, ISupervisorSettings settings)
        {
            return new InterviewerUpdateHandler(fileSystemAccessor, settings);
        }

        public InterviewerDownloadInterviews InterviewerDownloadInterviews(
            ISynchronizationService synchronizationService = null,
            IQuestionnaireDownloader questionnaireDownloader = null,
            IPlainStorage<InterviewSequenceView, Guid> interviewSequenceViewRepository = null,
            IPlainStorage<InterviewView> interviewViewRepository = null,
            ILiteEventBus eventBus = null,
            IEnumeratorEventStorage eventStore = null,
            ILogger logger = null,
            IInterviewsRemover interviewsRemover = null)
        {
            var interviewerDownloadInterviews = new InterviewerDownloadInterviews(
                synchronizationService ?? Mock.Of<ISynchronizationService>(),
                questionnaireDownloader ?? Mock.Of<IQuestionnaireDownloader>(),
                interviewSequenceViewRepository ?? new InMemoryPlainStorage<InterviewSequenceView, Guid>(logger),
                interviewViewRepository ?? new InMemoryPlainStorage<InterviewView>(logger),
                eventBus ?? Create.Service.LiteEventBus(),
                eventStore ?? Mock.Of<IEnumeratorEventStorage>(),
                logger ?? Mock.Of<ILogger>(),
                interviewsRemover ?? Mock.Of<IInterviewsRemover>(),
                0
            );
            interviewerDownloadInterviews.Context = new EnumeratorSynchonizationContext
            {
                Progress = new Progress<SyncProgressInfo>(),
                Statistics = new SynchronizationStatistics()
            };

            return interviewerDownloadInterviews;
        }

        public UpdateQuestionnaires UpdateQuestionnaires(ISynchronizationService synchronizationService = null,
            IInterviewerQuestionnaireAccessor questionnairesAccessor = null,
            IPlainStorage<InterviewView> interviewViewRepository = null,
            IAttachmentsCleanupService attachmentsCleanupService = null,
            IInterviewsRemover interviewsRemover = null)
        {
            var result = new UpdateQuestionnaires(
                synchronizationService ?? Mock.Of<ISynchronizationService>(),
                questionnairesAccessor ?? Mock.Of<IInterviewerQuestionnaireAccessor>(),
                interviewViewRepository ?? new InMemoryPlainStorage<InterviewView>(Mock.Of<ILogger>()),
                attachmentsCleanupService ?? Mock.Of<IAttachmentsCleanupService>(),
                interviewsRemover ?? Mock.Of<IInterviewsRemover>(),
                Mock.Of<ILogger>(),
                10);

            result.Context = new EnumeratorSynchonizationContext
            {
                CancellationToken = CancellationToken.None,
                Progress = new Progress<SyncProgressInfo>(),
                Statistics = new SynchronizationStatistics()
            };

            return result;
        }

        public InterviewsToExportViewFactory InterviewsToExportViewFactory(
            IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaries)
        {
            return new InterviewsToExportViewFactory(new InMemoryReadSideRepositoryAccessor<InterviewComment>());
        }

        public AttachmentContentStorage AttachmentContentStorage(
            IPlainStorage<AttachmentContentMetadata> attachmentContentMetadataRepository = null,
            IPlainStorage<AttachmentContentData> attachmentContentDataRepository = null,
            IPathUtils pathUtils = null,
            IFileSystemAccessor files = null)
        {
            return new AttachmentContentStorage(
                attachmentContentMetadataRepository ?? Mock.Of<IPlainStorage<AttachmentContentMetadata>>(),
                attachmentContentDataRepository ?? Mock.Of<IPlainStorage<AttachmentContentData>>(),
                pathUtils ?? Mock.Of<IPathUtils>(p => p.GetRootDirectory() == @"c:\tmp"),
                Mock.Of<IPermissionsService>(),
                files ?? Mock.Of<IFileSystemAccessor>());
        }

        public Core.BoundedContexts.Interviewer.Implementation.Services.MapSyncProvider MapSyncProvider(
            IMapService mapService = null,
            IOnlineSynchronizationService synchronizationService = null,
            ILogger logger = null,
            IHttpStatistician httpStatistician = null,
            IUserInteractionService userInteractionService = null,
            IInterviewerPrincipal principal = null,
            IPasswordHasher passwordHasher = null,
            IPlainStorage<InterviewView> interviews = null,
            IAuditLogService auditLogService = null,
            IEnumeratorSettings enumeratorSettings = null)
        {
            return new Core.BoundedContexts.Interviewer.Implementation.Services.MapSyncProvider(
                mapService ?? Mock.Of<IMapService>(),
                synchronizationService ?? Mock.Of<IOnlineSynchronizationService>(),
                logger ?? Mock.Of<ILogger>(),
                httpStatistician ?? Mock.Of<IHttpStatistician>(),
                principal ?? Mock.Of<IInterviewerPrincipal>(),
                passwordHasher ?? Mock.Of<IPasswordHasher>(),
                interviews ?? Mock.Of<IPlainStorage<InterviewView>>(),
                auditLogService ?? Mock.Of<IAuditLogService>(),
                enumeratorSettings ?? Mock.Of<IEnumeratorSettings>(),
                userInteractionService ?? Mock.Of<IUserInteractionService>(),
                Mock.Of<IServiceLocator>(),
                Mock.Of<IDeviceInformationService>(),
                Mock.Of<IAssignmentDocumentsStorage>());
        }

        public MapReport MapReport(IInterviewFactory interviewFactory = null,
            IPlainStorageAccessor<QuestionnaireBrowseItem> questionnairesAccessor = null,
            IPlainStorageAccessor<QuestionnaireCompositeItem> questionnaireItems = null,
            IAuthorizedUser authorizedUser = null)
        {
            return new MapReport(
                interviewFactory ?? Mock.Of<IInterviewFactory>(),
                questionnairesAccessor ?? Mock.Of<IPlainStorageAccessor<QuestionnaireBrowseItem>>(),
                authorizedUser ?? Mock.Of<IAuthorizedUser>(),
                Create.Storage.NewMemoryCache(),
                questionnaireItems ?? Mock.Of<IPlainStorageAccessor<QuestionnaireCompositeItem>>());
        }

        public IInScopeExecutor InScopeExecutor(IServiceLocator serviceLocatorMock)
        {
            return new NoScopeInScopeExecutor(serviceLocatorMock);
        }
        
        public RestService RestService(IRestServiceSettings restServiceSettings = null,
            INetworkService networkService = null,
            IJsonAllTypesSerializer synchronizationSerializer = null,
            IStringCompressor stringCompressor = null,
            IRestServicePointManager restServicePointManager = null,
            IHttpStatistician httpStatistician = null,
            IHttpClientFactory httpClientFactory = null)
        {
            return new RestService(
                restServiceSettings ?? Mock.Of<IRestServiceSettings>(x => x.Endpoint == "http://localhost"),
                networkService ?? Mock.Of<INetworkService>(x => x.IsHostReachable(It.IsAny<string>()) == true && x.IsNetworkEnabled() == true),
                synchronizationSerializer ?? new JsonAllTypesSerializer(),
                stringCompressor ?? Mock.Of<IStringCompressor>(),
                restServicePointManager ?? Mock.Of<IRestServicePointManager>(),
                httpStatistician ?? Mock.Of<IHttpStatistician>(),
                httpClientFactory ?? Mock.Of<IHttpClientFactory>(),
                new SimpleFileHandler(),
                Mock.Of<ILogger>()
            );
        }

        public EnumeratorGroupGroupStateCalculationStrategy EnumeratorGroupGroupStateCalculationStrategy()
        {
            return new EnumeratorGroupGroupStateCalculationStrategy();
        }

        public IInvitationService InvitationService(
            IPlainStorageAccessor<Invitation> invitations = null,
            IPlainKeyValueStorage<InvitationDistributionStatus> invitationDistributionStatuses = null,
            IAggregateRootPrototypePromoterService promoter = null
            )
        {
            var service = new InvitationService(invitations ?? new TestPlainStorage<Invitation>(),
                invitationDistributionStatuses ?? new InMemoryKeyValueStorage<InvitationDistributionStatus>(),
                promoter ?? Mock.Of<IAggregateRootPrototypePromoterService>(),
                Create.Service.TokenGenerator());
            return service;
        }

        public IInvitationService InvitationService(params Invitation[] invitations)
        {
            IPlainStorageAccessor<Invitation> accessor = new TestPlainStorage<Invitation>();
            foreach (var invitation in invitations)
            {
                accessor.Store(invitation, invitation.Id);
            }

            var service = new InvitationService(accessor,
                Mock.Of<IPlainKeyValueStorage<InvitationDistributionStatus>>(),
                Mock.Of<IAggregateRootPrototypePromoterService>(),
                Mock.Of<ITokenGenerator>());
            return service;
        }

        public SendRemindersJob SendRemindersJob(
            IInvitationService invitationService = null, 
            IEmailService emailService = null,
            IWebInterviewConfigProvider webInterviewConfigProvider = null,
            IPlainKeyValueStorage<EmailParameters> emailParamsStorage = null, 
            IWebInterviewEmailRenderer webInterviewEmailRenderer = null)
        {
            var emailServiceMock = new Mock<IEmailService>();
            emailServiceMock.Setup(x => x.IsConfigured()).Returns(true);

            var settingsMock = new Mock<IWebInterviewConfigProvider>();
            settingsMock.Setup(x => x.Get(It.IsAny<QuestionnaireIdentity>())).Returns(Mock.Of<WebInterviewConfig>(_ 
                => _.ReminderAfterDaysIfNoResponse == 2
                && _.ReminderAfterDaysIfPartialResponse == 2));
            
            var invService = invitationService ?? Mock.Of<IInvitationService>();

            var webInterviewConfigProvider1 = webInterviewConfigProvider ?? settingsMock.Object;

            var linkProvider = new WebInterviewLinkProvider(
                new VirtualPathService(Options.Create(new HeadquartersConfig
                {
                    BaseUrl = "http://localhost",
                    BaseAppUrl = "http://localhost"
                }), Mock.Of<IWorkspaceContextAccessor>()));

            return new SendRemindersJob(
                Mock.Of<ILogger<SendRemindersJob>>(),
                invService,
                emailService ?? emailServiceMock.Object,
                webInterviewConfigProvider1,
                emailParamsStorage ?? Mock.Of<IPlainKeyValueStorage<EmailParameters>>(),
                webInterviewEmailRenderer ?? Mock.Of<IWebInterviewEmailRenderer>(),
                Create.Service.InScopeExecutor(Mock.Of<IServiceLocator>(sl => sl.GetInstance<IInvitationService>() ==
                                                                              invService)),
                linkProvider);
        }

        public SendInvitationsJob SendInvitationsJob(
            ILogger<SendInvitationsJob> logger = null, 
            IInvitationService invitationService = null, 
            IEmailService emailService = null, 
            IInvitationMailingService invitationMailingService = null)
        {
            var emailServiceMock = new Mock<IEmailService>();
            emailServiceMock.Setup(x => x.IsConfigured()).Returns(true);

            return new SendInvitationsJob(
                logger ?? Mock.Of<ILogger<SendInvitationsJob>>(),
                invitationService ?? Mock.Of<IInvitationService>(),
                emailService ?? emailServiceMock.Object,
                invitationMailingService ?? Mock.Of<IInvitationMailingService>());
        }

        public TokenGenerator TokenGenerator(int tokenLength = 8, 
            IPlainStorageAccessor<Invitation> invitationStorage = null,
            IPlainStorageAccessor<ServerSettings> tenantSettingsStorage = null)
        {
            return new TokenGenerator(
                invitationStorage ?? new InMemoryPlainStorageAccessor<Invitation>(),
                tenantSettingsStorage ?? new InMemoryPlainStorageAccessor<ServerSettings>())
            {
                tokenLength = tokenLength
            };
        }

        public AssignmentPasswordGenerator AssignmentPasswordGenerator(
            IQueryableReadSideRepositoryReader<Assignment, Guid> assignments = null, 
            IPlainStorageAccessor<AssignmentToImport> importAssignments = null)
        {
            return new AssignmentPasswordGenerator(
                assignments ?? new InMemoryReadSideRepositoryAccessor<Assignment, Guid>(),
                importAssignments ?? new InMemoryPlainStorageAccessor<AssignmentToImport>());
        }

        public AsyncEventDispatcher InterviewViewModelEventsPublisher(IViewModelEventRegistry viewModelEventRegistry = null,
            ILogger logger = null,
            ICurrentViewModelPresenter currentViewModelPresenter = null) =>
            new AsyncEventDispatcher(viewModelEventRegistry ?? Mock.Of<IViewModelEventRegistry>(),
                logger ?? Mock.Of<ILogger>(), currentViewModelPresenter ?? Mock.Of<ICurrentViewModelPresenter>());
        
        public IServiceLocator ServiceLocatorService(params object[] instances)
        {
            var result = new Mock<IServiceLocator>();

            foreach (var instance in instances)
            {
                result.Setup(x => x.GetInstance(instance.GetType()))
                    .Returns(instance);
            }

            return result.Object;
        }
        
        public  Core.BoundedContexts.Headquarters.Users.UserProfile.InterviewerAuditLog.IAuditLogService AuditLogService(
            IAuditLogFactory auditLogFactory,
            IAuthorizedUser authorizedUser = null)
        {
            return new AuditLogService(auditLogFactory, authorizedUser ?? Mock.Of<IAuthorizedUser>());
        }

        public IAssignmentsService AssignmentsService(IQueryableReadSideRepositoryReader<Assignment, Guid> assignments,
            IInterviewAnswerSerializer interviewAnswerSerializer = null)
        {
            return new AssignmentsService(assignments, 
                interviewAnswerSerializer ?? Mock.Of<IInterviewAnswerSerializer>(),
                Mock.Of<IUnitOfWork>(),
                Mock.Of<IAuthorizedUser>());
        }

        public IReusableCategoriesFillerIntoQuestionnaire ReusableCategoriesFillerIntoQuestionnaire(IReusableCategoriesStorage reusableCategoriesStorage)
        {
            return new ReusableCategoriesFillerIntoQuestionnaire(reusableCategoriesStorage);
        }

        public UserArchiveService UserArchiveService(IUserRepository userRepository)
        {
            return new UserArchiveService(userRepository, Mock.Of<ISystemLog>());
        }
        
        public ISupportedVersionProvider SupportedVersionProvider()
        {
            return new SupportedVersionProvider(new InMemoryKeyValueStorage<QuestionnaireVersion>());
        }
        
        public ISerializer NewtonJsonSerializer()
            => new NewtonJsonSerializer();

        public MapFileStorageService MapFileStorageService(
            IFileSystemAccessor fileSystemAccessor = null, 
            IOptions<FileStorageConfig> fileStorageConfig = null,
            IArchiveUtils archiveUtils = null,
            IPlainStorageAccessor<MapBrowseItem> mapsStorage = null,
            IPlainStorageAccessor<UserMap> userMapsStorage = null,
            ISerializer serializer = null,
            IUserRepository userStorage = null,
            IExternalFileStorage externalFileStorage = null,
            IAuthorizedUser authorizedUser = null,
            IOptions<GeospatialConfig> geospatialConfig = null)
        {
           return new MapFileStorageService(
             fileSystemAccessor ?? Create.Service.FileSystemIOAccessor(), 
             fileStorageConfig ?? Options.Create(new FileStorageConfig()),
             archiveUtils ?? Create.Service.ArchiveUtils(),
             mapsStorage ?? new TestPlainStorage<MapBrowseItem>(),
             userMapsStorage ?? new TestPlainStorage<UserMap>(),
             serializer ?? Create.Service.NewtonJsonSerializer(),
             userStorage ?? Create.Storage.UserRepository(),
             externalFileStorage ?? Mock.Of<IExternalFileStorage>(),
             geospatialConfig ?? Mock.Of<IOptions<GeospatialConfig>>(),
             authorizedUser ?? Mock.Of<IAuthorizedUser>(),
             Mock.Of<ILogger<MapFileStorageService>>()); 
        }

        public ResponsibleAssignmentValidator WebModeResponsibleAssignmentValidator(IUserViewFactory userViewFactory = null)
        {
            return new ResponsibleAssignmentValidator(userViewFactory ?? Create.Storage.UserViewFactory());
        }

        public QuestionnaireStateForAssignmentValidator QuestionnaireStateForAssignmentValidator(IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaireBrowseItemStorage = null)
        {
            return new QuestionnaireStateForAssignmentValidator(questionnaireBrowseItemStorage ?? 
                                                                Stub<IPlainStorageAccessor<QuestionnaireBrowseItem>>.WithNotEmptyValues);
        }

        public QuestionnaireTranslator QuestionnaireTranslator()
            => new QuestionnaireTranslator();

        public IWorkspaceContextAccessor WorkspaceContextAccessor(string workspaceName = "primary")
        {
            return !string.IsNullOrEmpty(workspaceName) 
                ? Mock.Of<IWorkspaceContextAccessor>(x => x.CurrentWorkspace() == new WorkspaceContext(workspaceName, String.Empty, null)) 
                : new WorkspaceContextAccessor(new WorkspaceContextHolder());
        }
        
        public ICalendarEventPackageService CalendarEventPackageService(
            ICalendarEventService calendarEventService,
            ICommandService commandService,
            IQueryableReadSideRepositoryReader<InterviewSummary> interviews,
            IAssignmentsService assignments,
            ISerializer serializer,
            IUserViewFactory userViewFactory = null)
        {
            return new CalendarEventPackageService(
                Mock.Of<ILogger<CalendarEventPackageService>>(),
                calendarEventService ?? Mock.Of<ICalendarEventService>(),
                commandService ?? Mock.Of<ICommandService>(),
                interviews ?? Mock.Of<IQueryableReadSideRepositoryReader<InterviewSummary>>(),
                assignments ?? Mock.Of<IAssignmentsService>(),
                serializer ?? Mock.Of<ISerializer>(),
                userViewFactory ?? Mock.Of<IUserViewFactory>());
        }

        
        public ICalendarEventService CalendarEventService(params Core.BoundedContexts.Headquarters.CalendarEvents.CalendarEvent[] calendarEvents)
        {
            var accessor = new TestInMemoryWriter<Core.BoundedContexts.Headquarters.CalendarEvents.CalendarEvent, Guid>();
            foreach (var calendarEvent in calendarEvents)
            {
                accessor.Store(calendarEvent, calendarEvent.PublicKey);
            }
            return this.CalendarEventService(accessor);
        }
        public ICalendarEventService CalendarEventService(Core.BoundedContexts.Headquarters.CalendarEvents.CalendarEvent[] calendarEvents,
            Assignment[] assignments, IInterviewInformationFactory interviewerInterviewsFactory = null)
        {
            var accessor = new TestInMemoryWriter<Core.BoundedContexts.Headquarters.CalendarEvents.CalendarEvent, Guid>();
            foreach (var calendarEvent in calendarEvents)
            {
                accessor.Store(calendarEvent, calendarEvent.PublicKey);
            }
            var assignmentsAccessor = new TestInMemoryWriter<Assignment, Guid>();
            foreach (var assignment in assignments)
            {
                assignmentsAccessor.Store(assignment, assignment.PublicKey);
            }
            
            return this.CalendarEventService(accessor, assignmentsAccessor, interviewerInterviewsFactory);
        }
        
        public ICalendarEventService CalendarEventService(
            IQueryableReadSideRepositoryReader<Core.BoundedContexts.Headquarters.CalendarEvents.CalendarEvent, Guid> calendarEventsAccessor = null,
            IQueryableReadSideRepositoryReader<Assignment, Guid> assignmentsAccessor = null,  
            IInterviewInformationFactory interviewerInterviewsFactory = null)
        {
            return new CalendarEventService(
                calendarEventsAccessor ??
                Mock.Of<IQueryableReadSideRepositoryReader<
                    Core.BoundedContexts.Headquarters.CalendarEvents.CalendarEvent, Guid>>(),
                assignmentsAccessor ?? Mock.Of<IQueryableReadSideRepositoryReader<Assignment, Guid>>(),
                interviewerInterviewsFactory ?? Mock.Of<IInterviewInformationFactory>());
        }

        public WorkspacesService WorkspacesService(IPlainStorageAccessor<Workspace> workspaces,
            IServiceLocator serviceLocator = null)
        {
            return new WorkspacesService(
                new UnitOfWorkConnectionSettings(),
                Mock.Of<Microsoft.Extensions.Logging.ILoggerProvider>(),
                Mock.Of<IAuthorizedUser>(),
                workspaces,
                new TestPlainStorage<WorkspacesUsers>(),
                Mock.Of<IUserRepository>(),
                Mock.Of<ILogger<WorkspacesService>>(),
                Mock.Of<ISystemLog>(),
                Mock.Of<IWorkspacesUsersCache>(),
                new NoScopeInScopeExecutor(serviceLocator ?? Create.Service.ServiceLocatorService())
            );
        }
    }

    internal class SimpleFileHandler : IFastBinaryFilesHttpHandler
    {
        public Task<byte[]> DownloadBinaryDataAsync(HttpClient http, HttpResponseMessage response, IProgress<TransferProgress> transferProgress,
            CancellationToken token)
        {
            return response.Content.ReadAsByteArrayAsync();
        }
    }

    internal static class GoogleConnectionsRequestHandlerExtensions
    {
        public static IRequestHandler WithSampleEchoHandler(this IRequestHandler requestHandler)
        {
            requestHandler.RegisterHandler<PingMessage, PongMessage>(ping => Task.FromResult(new PongMessage() { Id = ping.Id })); // pre
            return requestHandler;
        }

        public static IRequestHandler WithHandler<TRes, TReq>(this IRequestHandler requestHandler, Func<TRes, Task<TReq>> ahandler)
            where TReq : ICommunicationMessage
            where TRes : ICommunicationMessage
        {
            requestHandler.RegisterHandler(ahandler);
            return requestHandler;
        }
    }

    public class PingPongMessage : ICommunicationMessage
    {
        public string Content { get; set; }

        public Guid Id { get; set; }

        public PingPongMessage(int size = 0)
        {
            Content = new string('*', size);
        }
    }

    public class PingMessage : PingPongMessage
    {
        public PingMessage(int size = 0) : base(size)
        {
        }
    }

    public class PongMessage : PingPongMessage
    {
        public PongMessage(int size = 0) : base(size)
        {
        }
    }
}
