using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using Moq;
using Ncqrs.Domain.Storage;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Storage;
using NHibernate;
using NSubstitute;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Main.Core.Events;
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
using WB.Core.BoundedContexts.Headquarters.DataExport.Factories;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.BoundedContexts.Headquarters.Implementation.Synchronization;
using WB.Core.BoundedContexts.Headquarters.IntreviewerProfiles;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.UserPreloading;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Dto;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Services;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Tasks;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.BoundedContexts.Headquarters.Views.Interviews;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.Synchronization;
using WB.Core.BoundedContexts.Interviewer.Synchronization.Steps;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.BoundedContexts.Supervisor.Services;
using WB.Core.BoundedContexts.Supervisor.Services.Implementation;
using WB.Core.BoundedContexts.Supervisor.Services.Implementation.OfflineSyncHandlers;
using WB.Core.BoundedContexts.Supervisor.Views;
using WB.Core.BoundedContexts.Tester.Implementation.Services;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation.Services;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.CommandBus.Implementation;
using WB.Core.Infrastructure.DenormalizerStorage;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.EventBus.Lite.Implementation;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.Implementation.Aggregates;
using WB.Core.Infrastructure.Implementation.EventDispatcher;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.TopologicalSorter;
using WB.Core.Infrastructure.WriteSide;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
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
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Infrastructure.Native.Files.Implementation.FileSystem;
using WB.Infrastructure.Native.Storage;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Tests.Abc.Storage;
using WB.UI.Headquarters.API.WebInterview.Services;
using WB.UI.Shared.Web.Captcha;
using WB.UI.Shared.Web.Configuration;
using ILogger = WB.Core.GenericSubdomains.Portable.Services.ILogger;
using AttachmentContent = WB.Core.BoundedContexts.Headquarters.Views.Questionnaire.AttachmentContent;

namespace WB.Tests.Abc.TestFactories
{
    internal class ServiceFactory
    {
        public CommandService CommandService(
            IEventSourcedAggregateRootRepository repository = null,
            IPlainAggregateRootRepository plainRepository = null,
            IEventBus eventBus = null,
            IAggregateSnapshotter snapshooter = null,
            IServiceLocator serviceLocator = null,
            IAggregateLock aggregateLock = null,
            IAggregateRootCacheCleaner aggregateRootCacheCleaner = null,
            IEventStore eventStore = null)
        {
            return new CommandService(
                repository ?? Mock.Of<IEventSourcedAggregateRootRepository>(),
                eventBus ?? Mock.Of<IEventBus>(),
                snapshooter ?? Mock.Of<IAggregateSnapshotter>(),
                serviceLocator ?? Mock.Of<IServiceLocator>(),
                plainRepository ?? Mock.Of<IPlainAggregateRootRepository>(),
                aggregateLock ?? Stub.Lock(),
                aggregateRootCacheCleaner ?? Mock.Of<IAggregateRootCacheCleaner>());
        }

        public IAsyncRunner AsyncRunner() => new SyncAsyncRunner();

        public AttachmentContentService AttachmentContentService(
            IPlainStorageAccessor<AttachmentContent> attachmentContentPlainStorage)
            => new AttachmentContentService(
                attachmentContentPlainStorage ?? Mock.Of<IPlainStorageAccessor<AttachmentContent>>());

        public CumulativeChartDenormalizer CumulativeChartDenormalizer(
            INativeReadSideStorage<CumulativeReportStatusChange> cumulativeReportReader = null,
            IReadSideRepositoryWriter<CumulativeReportStatusChange> cumulativeReportStatusChangeStorage = null,
            IQueryableReadSideRepositoryReader<InterviewSummary> interviewReferencesStorage = null)
            => new CumulativeChartDenormalizer(
                cumulativeReportStatusChangeStorage ??
                Mock.Of<IReadSideRepositoryWriter<CumulativeReportStatusChange>>(),
                interviewReferencesStorage ?? new TestInMemoryWriter<InterviewSummary>(),
                cumulativeReportReader ?? Mock.Of<INativeReadSideStorage<CumulativeReportStatusChange>>());

        public InterviewDashboardEventHandler DashboardDenormalizer(
            IPlainStorage<InterviewView> interviewViewRepository = null,
            IQuestionnaireStorage questionnaireStorage = null,
            ILiteEventRegistry liteEventRegistry = null,
            IPlainStorage<PrefilledQuestionView> prefilledQuestions = null,
            IAnswerToStringConverter answerToStringConverter = null
        )
            => new InterviewDashboardEventHandler(
                interviewViewRepository ?? Mock.Of<IPlainStorage<InterviewView>>(),
                prefilledQuestions ?? new InMemoryPlainStorage<PrefilledQuestionView>(),
                questionnaireStorage ?? Mock.Of<IQuestionnaireStorage>(),
                liteEventRegistry ?? Mock.Of<ILiteEventRegistry>(),
                answerToStringConverter ?? Mock.Of<IAnswerToStringConverter>());

        public DomainRepository DomainRepository(IAggregateSnapshotter aggregateSnapshotter = null,
            IServiceLocator serviceLocator = null)
            => new DomainRepository(
                aggregateSnapshotter: aggregateSnapshotter ?? Mock.Of<IAggregateSnapshotter>(),
                serviceLocator: serviceLocator ?? Mock.Of<IServiceLocator>());

        public EventSourcedAggregateRootRepository EventSourcedAggregateRootRepository(
            IEventStore eventStore = null, ISnapshotStore snapshotStore = null, IDomainRepository repository = null)
            => new EventSourcedAggregateRootRepository(eventStore, snapshotStore, repository);

        public EventSourcedAggregateRootRepositoryWithCache EventSourcedAggregateRootRepositoryWithCache(
            IEventStore eventStore = null, ISnapshotStore snapshotStore = null, IDomainRepository repository = null)
            => new EventSourcedAggregateRootRepositoryWithCache(
                eventStore ?? Mock.Of<IEventStore>(),
                snapshotStore ?? Mock.Of<ISnapshotStore>(),
                repository ?? Mock.Of<IDomainRepository>(),
                new AggregateLock());

        public EventSourcedAggregateRootRepositoryWithExtendedCache
            EventSourcedAggregateRootRepositoryWithExtendedCache(
                IEventStore eventStore = null, ISnapshotStore snapshotStore = null, IDomainRepository repository = null)
            => new EventSourcedAggregateRootRepositoryWithExtendedCache(
                eventStore ?? Mock.Of<IEventStore>(),
                snapshotStore ?? Mock.Of<ISnapshotStore>(),
                repository ?? Mock.Of<IDomainRepository>());

        public FileSystemIOAccessor FileSystemIOAccessor()
            => new FileSystemIOAccessor();

        public InterviewAnswersCommandValidator InterviewAnswersCommandValidator(
            IInterviewSummaryViewFactory interviewSummaryViewFactory = null)
            => new InterviewAnswersCommandValidator(
                interviewSummaryViewFactory ?? Mock.Of<IInterviewSummaryViewFactory>());

        public InterviewerInterviewAccessor InterviewerInterviewAccessor(
            IPlainStorage<InterviewView> interviewViewRepository = null,
            IEnumeratorEventStorage eventStore = null,
            ICommandService commandService = null,
            IPlainStorage<QuestionnaireView> questionnaireRepository = null,
            IPrincipal principal = null,
            IJsonAllTypesSerializer synchronizationSerializer = null,
            IEventSourcedAggregateRootRepositoryWithCache aggregateRootRepositoryWithCache = null,
            ISnapshotStoreWithCache snapshotStoreWithCache = null,
            IPlainStorage<InterviewMultimediaView> interviewMultimediaViewRepository = null,
            IPlainStorage<InterviewFileView> interviewFileViewRepository = null,
            IPlainStorage<InterviewSequenceView, Guid> interviewSequenceStorage = null,
            IInterviewEventStreamOptimizer eventStreamOptimizer = null)
            => new InterviewerInterviewAccessor(
                questionnaireRepository ?? Mock.Of<IPlainStorage<QuestionnaireView>>(),
                Mock.Of<IPlainStorage<PrefilledQuestionView>>(),
                interviewViewRepository ?? Mock.Of<IPlainStorage<InterviewView>>(),
                interviewMultimediaViewRepository ?? Mock.Of<IPlainStorage<InterviewMultimediaView>>(),
                interviewFileViewRepository ?? Mock.Of<IPlainStorage<InterviewFileView>>(),
                commandService ?? Mock.Of<ICommandService>(),
                principal ?? Mock.Of<IPrincipal>(),
                eventStore ?? Mock.Of<IEnumeratorEventStorage>(),
                aggregateRootRepositoryWithCache ?? Mock.Of<IEventSourcedAggregateRootRepositoryWithCache>(),
                snapshotStoreWithCache ?? Mock.Of<ISnapshotStoreWithCache>(),
                synchronizationSerializer ?? Mock.Of<IJsonAllTypesSerializer>(),
                eventStreamOptimizer ?? Mock.Of<IInterviewEventStreamOptimizer>(),
                Mock.Of<ILiteEventRegistry>(),
                interviewSequenceStorage ?? Mock.Of<IPlainStorage<InterviewSequenceView, Guid>>());

        public InterviewEventStreamOptimizer InterviewEventStreamOptimizer()
            => new InterviewEventStreamOptimizer();

        public KeywordsProvider KeywordsProvider()
            => new KeywordsProvider(Create.Service.SubstitutionService());

        public LiteEventBus LiteEventBus(ILiteEventRegistry liteEventRegistry = null, IEventStore eventStore = null)
            => new LiteEventBus(
                liteEventRegistry ?? Stub<ILiteEventRegistry>.WithNotEmptyValues,
                eventStore ?? Mock.Of<IEventStore>());

        public LiteEventRegistry LiteEventRegistry()
            => new LiteEventRegistry();

        public NcqrCompatibleEventDispatcher NcqrCompatibleEventDispatcher(EventBusSettings eventBusSettings = null,
            ILogger logger = null,
            IServiceLocator serviceLocator = null,
            params IEventHandler[] handlers)
            => new NcqrCompatibleEventDispatcher(
                eventStore: Mock.Of<IEventStore>(),
                inMemoryEventStore: Mock.Of<IInMemoryEventStore>(),
                serviceLocator: serviceLocator ?? Mock.Of<IServiceLocator>(),
                eventBusSettings: eventBusSettings ?? Create.Entity.EventBusSettings(),
                logger: logger ?? Mock.Of<ILogger>(),
                eventHandlers: handlers);

        public QuestionnaireKeyValueStorage QuestionnaireKeyValueStorage(
            IPlainStorage<QuestionnaireDocumentView> questionnaireDocumentViewRepository = null)
            => new QuestionnaireKeyValueStorage(
                questionnaireDocumentViewRepository ?? Mock.Of<IPlainStorage<QuestionnaireDocumentView>>());

        public QuestionnaireImportValidator QuestionnaireNameValidator(
            IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaireBrowseItemStorage = null)
            => new QuestionnaireImportValidator(
                questionnaireBrowseItemStorage ??
                Stub<IPlainStorageAccessor<QuestionnaireBrowseItem>>.WithNotEmptyValues);

        public IStatefulInterviewRepository StatefulInterviewRepository(
            IEventSourcedAggregateRootRepository aggregateRootRepository, ILiteEventBus liteEventBus = null)
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

        public IInterviewExpressionStatePrototypeProvider ExpressionStatePrototypeProvider(
            ILatestInterviewExpressionState expressionState = null)
        {
            var expressionStatePrototypeProvider = new Mock<IInterviewExpressionStatePrototypeProvider>();
            ILatestInterviewExpressionState latestInterviewExpressionState =
                expressionState ?? new InterviewExpressionStateStub();
            expressionStatePrototypeProvider.SetReturnsDefault(latestInterviewExpressionState);

            return expressionStatePrototypeProvider.Object;
        }

        public ISubstitutionTextFactory SubstitutionTextFactory()
        {
            return new SubstitutionTextFactory(Create.Service.SubstitutionService(),
                Create.Service.VariableToUIStringService());
        }

        public InterviewViewModelFactory InterviewViewModelFactory(IQuestionnaireStorage questionnaireRepository,
            IStatefulInterviewRepository interviewRepository,
            IEnumeratorSettings settings)
        {
            return new InterviewerInterviewViewModelFactory(questionnaireRepository ?? Mock.Of<IQuestionnaireStorage>(),
                interviewRepository ?? Mock.Of<IStatefulInterviewRepository>(),
                settings ?? Mock.Of<IEnumeratorSettings>());
        }

        public SupervisorInterviewViewModelFactory SupervisorInterviewViewModelFactory(IQuestionnaireStorage questionnaireRepository = null,
            IStatefulInterviewRepository interviewRepository = null,
            IEnumeratorSettings settings = null)
        {
            return new SupervisorInterviewViewModelFactory(questionnaireRepository ?? Mock.Of<IQuestionnaireStorage>(),
                interviewRepository ?? Mock.Of<IStatefulInterviewRepository>(),
                settings ?? Mock.Of<IEnumeratorSettings>());
        }

        public AllInterviewsFactory AllInterviewsFactory(
            IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummarys = null)
        {
            return new AllInterviewsFactory(interviewSummarys ??
                                            Mock.Of<IQueryableReadSideRepositoryReader<InterviewSummary>>());
        }

        public ITeamInterviewsFactory TeamInterviewsFactory(
            IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummarys = null)
        {
            return new TeamInterviewsFactory(interviewSummarys ??
                                             Mock.Of<IQueryableReadSideRepositoryReader<InterviewSummary>>());
        }

        public IConfigurationManager ConfigurationManager(NameValueCollection appSettings = null,
            NameValueCollection membershipSettings = null)
        {
            return new ConfigurationManager(appSettings ?? new NameValueCollection(),
                membershipSettings ?? new NameValueCollection());
        }

        public WebCacheBasedCaptchaService WebCacheBasedCaptchaService(int? failedLoginsCount = 5,
            int? timeSpanForLogins = 5, IConfigurationManager configurationManager = null)
        {
            return new WebCacheBasedCaptchaService(configurationManager ?? this.ConfigurationManager(
                                                       new NameValueCollection
                                                       {
                                                           {
                                                               "CountOfFailedLoginAttemptsBeforeCaptcha",
                                                               (failedLoginsCount ?? 5).ToString()
                                                           },
                                                           {
                                                               "TimespanInMinutesCaptchaWillBeShownAfterFailedLoginAttempt",
                                                               (timeSpanForLogins ?? 5).ToString()
                                                           },
                                                       }));
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
            QuestionnaireExportStructure questionnaireExportStructure = null,
            IQueryableReadSideRepositoryReader<InterviewCommentaries> interviewCommentaries = null)
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
                passwordHasher ?? Mock.Of<IPasswordHasher>());
        }

        public SupervisorPrincipal SupervisorPrincipal(IPlainStorage<SupervisorIdentity> storage,
            IPasswordHasher passwordHasher)
            => new SupervisorPrincipal(
                storage ?? Mock.Of<IPlainStorage<SupervisorIdentity>>(),
                passwordHasher ?? Mock.Of<IPasswordHasher>());

        public IInterviewerPrincipal InterviewerPrincipal(Guid userId)
            => Mock.Of<IInterviewerPrincipal>(x => x.IsAuthenticated == true && x.CurrentUserIdentity == Mock.Of<IInterviewerUserIdentity>(u => u.UserId == userId));

        public IPrincipal Principal(Guid userId)
            => Mock.Of<IPrincipal>(x => x.IsAuthenticated == true && x.CurrentUserIdentity == Mock.Of<IUserIdentity>(u => u.UserId == userId));

        public InterviewerOnlineSynchronizationProcess SynchronizationProcess(
            IPlainStorage<InterviewView> interviewViewRepository = null,
            IPlainStorage<InterviewerIdentity> interviewersPlainStorage = null,
            IPlainStorage<InterviewMultimediaView> interviewMultimediaViewStorage = null,
            IPlainStorage<InterviewFileView> interviewFileViewStorage = null,
            IOnlineSynchronizationService synchronizationService = null,
            ILogger logger = null,
            IUserInteractionService userInteractionService = null,
            IPasswordHasher passwordHasher = null,
            IInterviewerPrincipal principal = null,
            IHttpStatistician httpStatistician = null)
        {
            var syncServiceMock = synchronizationService ?? Mock.Of<IOnlineSynchronizationService>();

            return new InterviewerOnlineSynchronizationProcess(
                syncServiceMock,
                interviewersPlainStorage ?? Mock.Of<IPlainStorage<InterviewerIdentity>>(),
                interviewViewRepository ?? new InMemoryPlainStorage<InterviewView>(),
                principal ?? Mock.Of<IInterviewerPrincipal>(),
                logger ?? Mock.Of<ILogger>(),
                passwordHasher ?? Mock.Of<IPasswordHasher>(),
                httpStatistician ?? Mock.Of<IHttpStatistician>(),
                Mock.Of<IAssignmentDocumentsStorage>(),
                Mock.Of<IInterviewerSettings>(),
                Mock.Of<IAuditLogService>(),
                userInteractionService ?? Mock.Of<IUserInteractionService>(),
                Mock.Of<IServiceLocator>());
        }

        public InterviewerOfflineSynchronizationProcess OfflineSynchronizationProcess(
            IPlainStorage<InterviewView> interviewViewRepository = null,
            IPlainStorage<InterviewerIdentity> interviewersPlainStorage = null,
            ILogger logger = null,
            IPasswordHasher passwordHasher = null,
            IInterviewerPrincipal principal = null,
            IHttpStatistician httpStatistician = null,
            IOfflineSynchronizationService synchronizationService = null)
        {
            var syncServiceMock = synchronizationService ?? Mock.Of<IOfflineSynchronizationService>();

            return new InterviewerOfflineSynchronizationProcess(
                syncServiceMock,
                interviewViewRepository ?? new InMemoryPlainStorage<InterviewView>(),
                interviewersPlainStorage ?? Mock.Of<IPlainStorage<InterviewerIdentity>>(),
                principal ?? Mock.Of<IInterviewerPrincipal>(),
                logger ?? Mock.Of<ILogger>(),
                httpStatistician ?? Mock.Of<IHttpStatistician>(),
                Mock.Of<IAssignmentDocumentsStorage>(),
                Mock.Of<IAuditLogService>(),
                Mock.Of<IInterviewerSettings>(),
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
                synchronizationService ?? Mock.Of<ISynchronizationService>());
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
                interviewerViewRepository ?? Mock.Of<IPlainStorage<InterviewerDocument>>());
        }

        public IAnswerToStringConverter AnswerToStringConverter()
        {
            return new AnswerToStringConverter();
        }

        public ExpressionsPlayOrderProvider ExpressionsPlayOrderProvider(
            IExpressionProcessor expressionProcessor = null,
            IMacrosSubstitutionService macrosSubstitutionService = null)
        {
            if (expressionProcessor == null && !ServiceLocator.IsLocationProviderSet)
            {
                var serviceLocator = Stub<IServiceLocator>.WithNotEmptyValues;

                ServiceLocator.SetLocatorProvider(() => serviceLocator);
                Setup.StubToMockedServiceLocator<IExpressionProcessor>();
            }

            return new ExpressionsPlayOrderProvider(
                new ExpressionsGraphProvider(
                    expressionProcessor ?? ServiceLocator.Current.GetInstance<IExpressionProcessor>(),
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
            IPlainStorageAccessor<Assignment> accessor = new TestPlainStorage<Assignment>();
            foreach (var assignment in assignments)
            {
                accessor.Store(assignment, assignment.Id);
            }

            var service = new AssignmentsService(accessor, Mock.Of<IInterviewAnswerSerializer>());

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
            UsersImportTask usersImportTask = null)
        {
            usersImportTask = usersImportTask ?? new UsersImportTask(Mock.Of<IScheduler>(x =>
                                  x.GetCurrentlyExecutingJobs() == Array.Empty<IJobExecutionContext>()));

            userPreloadingSettings = userPreloadingSettings ?? Create.Entity.UserPreloadingSettings();
            return new UserImportService(
                userPreloadingSettings,
                csvReader ?? Stub<ICsvReader>.WithNotEmptyValues,
                importUsersProcessRepository ?? Stub<IPlainStorageAccessor<UsersImportProcess>>.WithNotEmptyValues,
                importUsersRepository ?? Stub<IPlainStorageAccessor<UserToImport>>.WithNotEmptyValues,
                userStorage ?? Stub<IUserRepository>.WithNotEmptyValues,
                userImportVerifier ?? new UserImportVerifier(userPreloadingSettings),
                authorizedUser ?? Stub<IAuthorizedUser>.WithNotEmptyValues,
                sessionProvider ?? Stub<IUnitOfWork>.WithNotEmptyValues,
                usersImportTask ?? Stub<UsersImportTask>.WithNotEmptyValues);
        }

        public ICsvReader CsvReader<T>(string[] headers, params T[] rows)
        {
            return Mock.Of<ICsvReader>(
                x => x.ReadAll<T>(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<bool>()) == rows &&
                     x.ReadHeader(It.IsAny<Stream>(), It.IsAny<string>()) == headers);
        }

        public InterviewerProfileFactory InterviewerProfileFactory(TestHqUserManager userManager = null,
            IQueryableReadSideRepositoryReader<InterviewSummary> interviewRepository = null,
            IDeviceSyncInfoRepository deviceSyncInfoRepository = null,
            IInterviewerVersionReader interviewerVersionReader = null,
            IInterviewFactory interviewFactory = null)
        {
            return new InterviewerProfileFactory(
                userManager ?? Mock.Of<HqUserManager>(),
                interviewRepository ?? Mock.Of<IQueryableReadSideRepositoryReader<InterviewSummary>>(),
                deviceSyncInfoRepository ?? Mock.Of<IDeviceSyncInfoRepository>(),
                interviewerVersionReader ?? Mock.Of<IInterviewerVersionReader>(),
                interviewFactory ?? Mock.Of<IInterviewFactory>(),
                Mock.Of<IAuthorizedUser>(),
                Mock.Of<IQRCodeHelper>());
        }

        public StatefullInterviewSearcher StatefullInterviewSearcher()
        {
            return new StatefullInterviewSearcher(Mock.Of<IInterviewFactory>(x =>
                x.GetFlaggedQuestionIds(It.IsAny<Guid>()) == new Identity[] { }));
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
            IUserRepository userRepository = null)
        {
            InterviewKey generatedInterviewKey = new InterviewKey(5533);

            var userRepositoryMock = new Mock<IUserRepository>();

            var hqUserProfile = Mock.Of<HqUserProfile>(_ => _.SupervisorId == Id.gB);

            var hqUser = Mock.Of<HqUser>(_ => _.Id == Id.gA
                                           && _.Profile == hqUserProfile);
            userRepositoryMock
                .Setup(arg => arg.FindByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(hqUser);

            return new InterviewPackagesService(
                syncSettings: syncSettings ?? Mock.Of<SyncSettings>(),
                logger: logger ?? Mock.Of<ILogger>(),
                interviewPackageStorage: interviewPackageStorage ?? Mock.Of<IPlainStorageAccessor<InterviewPackage>>(),
                brokenInterviewPackageStorage: brokenInterviewPackageStorage ?? Mock.Of<IPlainStorageAccessor<BrokenInterviewPackage>>(),
                packagesTracker: new TestPlainStorage<ReceivedPackageLogEntry>());
        }

        public ImportDataVerifier ImportDataVerifier(IFileSystemAccessor fileSystem = null,
            IInterviewTreeBuilder interviewTreeBuilder = null,
            IUserViewFactory userViewFactory = null,
            IQuestionOptionsRepository optionsRepository = null)
            => new ImportDataVerifier(fileSystem ?? new FileSystemIOAccessor(),
                interviewTreeBuilder ?? Mock.Of<IInterviewTreeBuilder>(),
                userViewFactory ?? Mock.Of<IUserViewFactory>(),
                optionsRepository ?? Mock.Of<IQuestionOptionsRepository>());

        public IAssignmentsUpgrader AssignmentsUpgrader(IPreloadedDataVerifier importService = null,
            IQuestionnaireStorage questionnaireStorage = null,
            IPlainStorageAccessor<Assignment> assignments = null,
            IAssignmentsUpgradeService upgradeService = null)
        {
            return new AssignmentsUpgrader(assignments ?? new TestPlainStorage<Assignment>(),
                importService ?? Mock.Of<IPreloadedDataVerifier>(s => s.VerifyWithInterviewTree(It.IsAny<List<InterviewAnswer>>(), It.IsAny<Guid?>(), It.IsAny<IQuestionnaire>()) == null),
                questionnaireStorage ?? Mock.Of<IQuestionnaireStorage>(),
                upgradeService ?? Mock.Of<IAssignmentsUpgradeService>());
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
            IInterviewCreatorFromAssignment interviewCreatorFromAssignment = null,
            IPlainStorageAccessor<Assignment> assignmentsStorage = null,
            IAssignmentsImportFileConverter assignmentsImportFileConverter = null)
        {
            var session = Mock.Of<ISession>(x =>
                x.Query<AssignmentsImportProcess>() == GetNhQueryable<AssignmentsImportProcess>() &&
                x.Query<AssignmentToImport>() == GetNhQueryable<AssignmentToImport>());

            sessionProvider = sessionProvider ?? Mock.Of<IUnitOfWork>(x => x.Session == session);
            userViewFactory = userViewFactory ?? Mock.Of<IUserViewFactory>();

            return new AssignmentsImportService(userViewFactory,
                verifier ?? ImportDataVerifier(),
                authorizedUser ?? Mock.Of<IAuthorizedUser>(),
                sessionProvider,
                importAssignmentsProcessRepository ?? Mock.Of<IPlainStorageAccessor<AssignmentsImportProcess>>(),
                importAssignmentsRepository ?? Mock.Of<IPlainStorageAccessor<AssignmentToImport>>(),
                interviewCreatorFromAssignment ?? Mock.Of<IInterviewCreatorFromAssignment>(),
                assignmentsStorage ?? Mock.Of<IPlainStorageAccessor<Assignment>>(),
                assignmentsImportFileConverter ?? AssignmentsImportFileConverter(userViewFactory: userViewFactory));
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
            IInterviewerQuestionnaireAccessor questionnaireAccessor = null,
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
            IAssignmentDocumentsStorage assignments = null)
        {
            return new SupervisorInterviewsHandler(
                eventBus ?? Mock.Of<ILiteEventBus>(),
                eventStorage ?? Mock.Of<IEnumeratorEventStorage>(),
                interviews ?? new InMemoryPlainStorage<InterviewView>(),
                serializer ?? Mock.Of<IJsonAllTypesSerializer>(s => s.Deserialize<AggregateRootEvent[]>(It.IsAny<string>()) == new AggregateRootEvent[] { }),// new JsonAllTypesSerializer(),
                commandService ?? Mock.Of<ICommandService>(),
                Mock.Of<ILogger>(),
                brokenInterviewStorage ?? Mock.Of<IPlainStorage<BrokenInterviewPackageView, int?>>(),
                receivedPackagesLog ?? new SqliteInmemoryStorage<SuperivsorReceivedPackageLogEntry, int>(),
                principal ?? Mock.Of<IPrincipal>(),
                assignments ?? Create.Storage.AssignmentDocumentsInmemoryStorage());
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
                interviewSequenceViewRepository ?? new InMemoryPlainStorage<InterviewSequenceView, Guid>(),
                interviewViewRepository ?? new InMemoryPlainStorage<InterviewView>(),
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

        public CensusQuestionnairesSynchronization CensusQuestionnairesSynchronization(
            IInterviewerSynchronizationService synchronizationService = null,
            IInterviewerQuestionnaireAccessor questionnairesAccessor = null,
            IQuestionnaireDownloader questionnaireDownloader = null)
        {
            var censusQuestionnairesSynchronization = new CensusQuestionnairesSynchronization(
                synchronizationService ?? Mock.Of<IInterviewerSynchronizationService>(),
                questionnairesAccessor ?? Mock.Of<IInterviewerQuestionnaireAccessor>(),
                questionnaireDownloader ?? Mock.Of<IQuestionnaireDownloader>(),
                Mock.Of<ILogger>(),
                10
            );
            censusQuestionnairesSynchronization.Context = new EnumeratorSynchonizationContext
            {
                CancellationToken = CancellationToken.None,
                Progress = new Progress<SyncProgressInfo>(),
                Statistics = new SynchronizationStatistics()
            };

            return censusQuestionnairesSynchronization;
        }

        public RemoveObsoleteQuestionnaires RemoveObsoleteQuestionnaires(ISynchronizationService synchronizationService = null,
            IInterviewerQuestionnaireAccessor questionnairesAccessor = null,
            IPlainStorage<InterviewView> interviewViewRepository = null,
            IAttachmentsCleanupService attachmentsCleanupService = null,
            IInterviewsRemover interviewsRemover = null)
        {
            var result = new RemoveObsoleteQuestionnaires(
                synchronizationService ?? Mock.Of<ISynchronizationService>(),
                questionnairesAccessor ?? Mock.Of<IInterviewerQuestionnaireAccessor>(),
                interviewViewRepository ?? new InMemoryPlainStorage<InterviewView>(),
                attachmentsCleanupService ?? Mock.Of<IAttachmentsCleanupService>(),
                interviewsRemover ?? Mock.Of<IInterviewsRemover>(),
                Mock.Of<ILogger>(),
                10
                );
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
            return new InterviewsToExportViewFactory(interviewSummaries ??
                                                     new InMemoryReadSideRepositoryAccessor<InterviewSummary>(),
                new InMemoryReadSideRepositoryAccessor<InterviewCommentaries>());
        }
        
        public Core.BoundedContexts.Interviewer.Implementation.Services.MapSyncProvider MapSyncProvider(
            IMapService mapService = null,
            IOnlineSynchronizationService synchronizationService = null,
            ILogger logger = null,
            IHttpStatistician httpStatistician = null,
            IUserInteractionService userInteractionService = null,
            IPrincipal principal = null,
            IPasswordHasher passwordHasher = null,
            IPlainStorage<InterviewerIdentity> interviewers = null,
            IPlainStorage<InterviewView> interviews = null,
            IAuditLogService auditLogService = null,
            IEnumeratorSettings enumeratorSettings = null)
        {
            return new Core.BoundedContexts.Interviewer.Implementation.Services.MapSyncProvider(
                mapService ?? Mock.Of<IMapService>(),
                synchronizationService ?? Mock.Of<IOnlineSynchronizationService>(),
                logger ?? Mock.Of<ILogger>(),
                httpStatistician ?? Mock.Of<IHttpStatistician>(),
                principal ?? Mock.Of<IPrincipal>(),
                passwordHasher ?? Mock.Of<IPasswordHasher>(),
                interviewers ?? Mock.Of<IPlainStorage<InterviewerIdentity>>(),
                interviews ?? Mock.Of<IPlainStorage<InterviewView>>(),
                auditLogService ?? Mock.Of<IAuditLogService>(),
                enumeratorSettings ?? Mock.Of<IEnumeratorSettings>(),
                userInteractionService ?? Mock.Of<IUserInteractionService>(),
                Mock.Of<IServiceLocator>(),
                Mock.Of<IAssignmentDocumentsStorage>());
        }

        public InterviewFactory InterviewFactory(
            IQueryableReadSideRepositoryReader<InterviewSummary> summaryRepository = null,
            IUnitOfWork sessionProvider = null)
        {
            return new InterviewFactory(
                summaryRepository ?? Mock.Of<IQueryableReadSideRepositoryReader<InterviewSummary>>(),
                sessionProvider ?? Mock.Of<IUnitOfWork>());
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
