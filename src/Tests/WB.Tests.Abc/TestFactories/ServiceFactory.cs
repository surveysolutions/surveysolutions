using System;
using System.Collections.Specialized;
using System.IO;
using Main.Core.Documents;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Domain.Storage;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Storage;
using NHibernate;
using NSubstitute;
using System.Linq;
using MvvmCross.Plugins.Messenger;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using WB.Core.BoundedContexts.Headquarters.DataExport.Denormalizers;
using WB.Core.BoundedContexts.Headquarters.DataExport.Factories;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.EventHandler.WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.Export;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.ChangeStatus;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Implementation.Storage;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.Services.Synchronization;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Implementation.Services;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.CommandBus.Implementation;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.EventBus.Lite.Implementation;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.Implementation.Aggregates;
using WB.Core.Infrastructure.Implementation.EventDispatcher;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Transactions;
using WB.Core.Infrastructure.Versions;
using WB.Core.Infrastructure.WriteSide;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Implementation.Services;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.Enumerator;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Infrastructure.Native.Files.Implementation.FileSystem;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;
using WB.Tests.Abc.Storage;
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
            IAggregateLock aggregateLock = null)
        {
            return new CommandService(
                repository ?? Mock.Of<IEventSourcedAggregateRootRepository>(),
                eventBus ?? Mock.Of<IEventBus>(),
                snapshooter ?? Mock.Of<IAggregateSnapshotter>(),
                serviceLocator ?? Mock.Of<IServiceLocator>(),
                plainRepository ?? Mock.Of<IPlainAggregateRootRepository>(),
                aggregateLock ?? Stub.Lock());
        }

        public IAsyncRunner AsyncRunner() => new SyncAsyncRunner();

        public AttachmentContentService AttachmentContentService(IPlainStorageAccessor<AttachmentContent> attachmentContentPlainStorage)
            => new AttachmentContentService(
                attachmentContentPlainStorage ?? Mock.Of<IPlainStorageAccessor<AttachmentContent>>());

        public CumulativeChartDenormalizer CumulativeChartDenormalizer(
            IReadSideKeyValueStorage<LastInterviewStatus> lastStatusesStorage = null,
            IReadSideRepositoryWriter<CumulativeReportStatusChange> cumulativeReportStatusChangeStorage = null,
            IReadSideKeyValueStorage<InterviewReferences> interviewReferencesStorage = null)
            => new CumulativeChartDenormalizer(
                lastStatusesStorage ?? Mock.Of<IReadSideKeyValueStorage<LastInterviewStatus>>(),
                cumulativeReportStatusChangeStorage ?? Mock.Of<IReadSideRepositoryWriter<CumulativeReportStatusChange>>(),
                interviewReferencesStorage ?? Mock.Of<IReadSideKeyValueStorage<InterviewReferences>>());

        public InterviewerDashboardEventHandler DashboardDenormalizer(
            IPlainStorage<InterviewView> interviewViewRepository = null,
            IQuestionnaireStorage questionnaireStorage = null,
            ILiteEventRegistry liteEventRegistry = null,
            IPlainStorage<PrefilledQuestionView> prefilledQuestions = null
            )
            => new InterviewerDashboardEventHandler(
                interviewViewRepository ?? Mock.Of<IPlainStorage<InterviewView>>(),
                prefilledQuestions ?? new InMemoryPlainStorage<PrefilledQuestionView>(),
                questionnaireStorage ?? Mock.Of<IQuestionnaireStorage>(),
                liteEventRegistry ?? Mock.Of<ILiteEventRegistry>());

        public DomainRepository DomainRepository(IAggregateSnapshotter aggregateSnapshotter = null, IServiceLocator serviceLocator = null)
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
                repository ?? Mock.Of<IDomainRepository>());

        public EventSourcedAggregateRootRepositoryWithExtendedCache EventSourcedAggregateRootRepositoryWithExtendedCache(
            IEventStore eventStore = null, ISnapshotStore snapshotStore = null, IDomainRepository repository = null)
            => new EventSourcedAggregateRootRepositoryWithExtendedCache(
                eventStore ?? Mock.Of<IEventStore>(),
                snapshotStore ?? Mock.Of<ISnapshotStore>(),
                repository ?? Mock.Of<IDomainRepository>());

        public FileSystemIOAccessor FileSystemIOAccessor()
            => new FileSystemIOAccessor();

        public InterviewAnswersCommandValidator InterviewAnswersCommandValidator(IInterviewSummaryViewFactory interviewSummaryViewFactory = null)
            => new InterviewAnswersCommandValidator(
                interviewSummaryViewFactory ?? Mock.Of<IInterviewSummaryViewFactory>());

        public InterviewDetailsViewFactory InterviewDetailsViewFactory(
            IReadSideKeyValueStorage<InterviewData> interviewStore = null,
            IUserViewFactory userStore = null,
            IChangeStatusFactory changeStatusFactory = null,
            IInterviewPackagesService incomingSyncPackagesQueue = null,
            IQuestionnaireStorage questionnaireStorage = null,
            IStatefulInterviewRepository statefulInterviewRepository = null,
            IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaryRepository = null,
            ISubstitutionService substitutionService = null)
        {
            var userView = Create.Entity.UserView();
            return new InterviewDetailsViewFactory(
                userStore ?? Mock.Of<IUserViewFactory>(_=>_.GetUser(It.IsAny<UserViewInputModel>()) == userView),
                changeStatusFactory ?? Mock.Of<IChangeStatusFactory>(),
                incomingSyncPackagesQueue ?? Mock.Of<IInterviewPackagesService>(),
                questionnaireStorage ?? Mock.Of<IQuestionnaireStorage>(),
                statefulInterviewRepository ?? Mock.Of<IStatefulInterviewRepository>(),
                interviewSummaryRepository ?? Mock.Of<IQueryableReadSideRepositoryReader<InterviewSummary>>(),
                interviewStore ?? new TestInMemoryWriter<InterviewData>(),
                substitutionService ?? this.SubstitutionService());
        }

        public InterviewerInterviewAccessor InterviewerInterviewAccessor(
            IPlainStorage<InterviewView> interviewViewRepository = null,
            IInterviewerEventStorage eventStore = null,
            ICommandService commandService = null,
            IPlainStorage<QuestionnaireView> questionnaireRepository = null,
            IInterviewerPrincipal principal = null,
            IJsonAllTypesSerializer synchronizationSerializer = null,
            IEventSourcedAggregateRootRepositoryWithCache aggregateRootRepositoryWithCache = null,
            ISnapshotStoreWithCache snapshotStoreWithCache = null,
            IPlainStorage<InterviewMultimediaView> interviewMultimediaViewRepository = null,
            IPlainStorage<InterviewFileView> interviewFileViewRepository = null)
            => new InterviewerInterviewAccessor(
                questionnaireRepository ?? Mock.Of<IPlainStorage<QuestionnaireView>>(),
                Mock.Of<IPlainStorage<PrefilledQuestionView>>(),
                interviewViewRepository ?? Mock.Of<IPlainStorage<InterviewView>>(),
                interviewMultimediaViewRepository ?? Mock.Of<IPlainStorage<InterviewMultimediaView>>(),
                interviewFileViewRepository ?? Mock.Of<IPlainStorage<InterviewFileView>>(),
                commandService ?? Mock.Of<ICommandService>(),
                principal ?? Mock.Of<IInterviewerPrincipal>(),
                eventStore ?? Mock.Of<IInterviewerEventStorage>(),
                aggregateRootRepositoryWithCache ?? Mock.Of<IEventSourcedAggregateRootRepositoryWithCache>(),
                snapshotStoreWithCache ?? Mock.Of<ISnapshotStoreWithCache>(),
                synchronizationSerializer ?? Mock.Of<IJsonAllTypesSerializer>(),
                Mock.Of<IInterviewEventStreamOptimizer>(),
                Mock.Of<ILogger>());

        public InterviewEventStreamOptimizer InterviewEventStreamOptimizer()
            => new InterviewEventStreamOptimizer();

        public InterviewReferencesDenormalizer InterviewReferencesDenormalizer()
            => new InterviewReferencesDenormalizer(
                Mock.Of<IReadSideKeyValueStorage<InterviewReferences>>());

        public KeywordsProvider KeywordsProvider()
            => new KeywordsProvider(Create.Service.SubstitutionService());

        public LiteEventBus LiteEventBus(ILiteEventRegistry liteEventRegistry = null, IEventStore eventStore = null)
            => new LiteEventBus(
                liteEventRegistry ?? Stub<ILiteEventRegistry>.WithNotEmptyValues,
                eventStore ?? Mock.Of<IEventStore>());

        public LiteEventRegistry LiteEventRegistry()
            => new LiteEventRegistry();

        public MapReportDenormalizer MapReportDenormalizer(
            IReadSideRepositoryWriter<MapReportPoint> mapReportPointStorage = null,
            IReadSideKeyValueStorage<InterviewReferences> interviewReferencesStorage = null,
            QuestionnaireQuestionsInfo questionnaireQuestionsInfo = null,
            QuestionnaireDocument questionnaireDocument = null)
            => new MapReportDenormalizer(
                interviewReferencesStorage ?? new TestInMemoryWriter<InterviewReferences>(),
                mapReportPointStorage ?? new TestInMemoryWriter<MapReportPoint>(),
                Mock.Of<IQuestionnaireStorage>(_ => _.GetQuestionnaireDocument(It.IsAny<Guid>(), It.IsAny<long>()) == questionnaireDocument),
                Mock.Of<IPlainKeyValueStorage<QuestionnaireQuestionsInfo>>(_ => _.GetById(It.IsAny<string>()) == questionnaireQuestionsInfo));

        public NcqrCompatibleEventDispatcher NcqrCompatibleEventDispatcher(EventBusSettings eventBusSettings = null, ILogger logger = null)
            => new NcqrCompatibleEventDispatcher(
                eventStore: Mock.Of<IEventStore>(),
                eventBusSettings: eventBusSettings ?? Create.Entity.EventBusSettings(),
                logger: logger ?? Mock.Of<ILogger>())
            {
                TransactionManager = Mock.Of<ITransactionManagerProvider>(x => x.GetTransactionManager() == Mock.Of<ITransactionManager>())
            };

        public PreloadedDataService PreloadedDataService(QuestionnaireDocument questionnaire)
            => new PreloadedDataService(
                new ExportViewFactory(new FileSystemIOAccessor(), 
                                      new ExportQuestionService(), 
                                      Mock.Of<IQuestionnaireStorage>(_ => _.GetQuestionnaireDocument(Moq.It.IsAny<QuestionnaireIdentity>()) == questionnaire && 
                                                                          _.GetQuestionnaire(Moq.It.IsAny<QuestionnaireIdentity>(), Moq.It.IsAny<string>()) == new PlainQuestionnaire(questionnaire, 1, null)),
                                      new RosterStructureService())
                                    .CreateQuestionnaireExportStructure(new QuestionnaireIdentity(questionnaire.PublicKey, 1)),
                new RosterStructureService().GetRosterScopes(questionnaire), 
                questionnaire,
                new QuestionDataParser(),
                new UserViewFactory(Mock.Of<IUserRepository>()));

        public QuestionnaireKeyValueStorage QuestionnaireKeyValueStorage(IPlainStorage<QuestionnaireDocumentView> questionnaireDocumentViewRepository = null)
            => new QuestionnaireKeyValueStorage(
                questionnaireDocumentViewRepository ?? Mock.Of<IPlainStorage<QuestionnaireDocumentView>>());

        public QuestionnaireNameValidator QuestionnaireNameValidator(
            IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaireBrowseItemStorage = null)
            => new QuestionnaireNameValidator(
                questionnaireBrowseItemStorage ?? Stub<IPlainStorageAccessor<QuestionnaireBrowseItem>>.WithNotEmptyValues);

        public RebuildReadSideCqrsPostgresTransactionManagerWithoutSessions RebuildReadSideCqrsPostgresTransactionManager()
            => new RebuildReadSideCqrsPostgresTransactionManagerWithoutSessions();

        public IStatefulInterviewRepository StatefulInterviewRepository(IEventSourcedAggregateRootRepository aggregateRootRepository, ILiteEventBus liteEventBus = null)
            => new StatefulInterviewRepository(
                aggregateRootRepository: aggregateRootRepository ?? Mock.Of<IEventSourcedAggregateRootRepository>());

        public ISubstitutionService SubstitutionService()
            => new SubstitutionService();

        public TeamViewFactory TeamViewFactory(
            IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaryReader = null)
            => new TeamViewFactory(interviewSummaryReader);

        public ITopologicalSorter<T> TopologicalSorter<T>()
            => new TopologicalSorter<T>();

        public TransactionManagerProvider TransactionManagerProvider(
            Func<ICqrsPostgresTransactionManager> transactionManagerFactory = null,
            Func<ICqrsPostgresTransactionManager> noTransactionTransactionManagerFactory = null,
            ICqrsPostgresTransactionManager rebuildReadSideTransactionManager = null)
            => new TransactionManagerProvider(
                transactionManagerFactory ?? (() => Mock.Of<ICqrsPostgresTransactionManager>()),
                noTransactionTransactionManagerFactory ?? (() => Mock.Of<ICqrsPostgresTransactionManager>()),
                rebuildReadSideTransactionManager ?? Mock.Of<ICqrsPostgresTransactionManager>(),
                rebuildReadSideTransactionManager ?? Mock.Of<ICqrsPostgresTransactionManager>(),
                Create.Entity.ReadSideCacheSettings());

        public VariableToUIStringService VariableToUIStringService()
            => new VariableToUIStringService();

        public IInterviewExpressionStatePrototypeProvider ExpressionStatePrototypeProvider(ILatestInterviewExpressionState expressionState = null)
        {
            var expressionStatePrototypeProvider = new Mock<IInterviewExpressionStatePrototypeProvider>();
            ILatestInterviewExpressionState latestInterviewExpressionState = expressionState ?? new InterviewExpressionStateStub();
            expressionStatePrototypeProvider.SetReturnsDefault(latestInterviewExpressionState);

            return expressionStatePrototypeProvider.Object;
        }

        public IDataExportStatusReader DataExportStatusReader(IDataExportProcessesService dataExportProcessesService = null,
            IFilebasedExportedDataAccessor filebasedExportedDataAccessor = null,
            IParaDataAccessor paraDataAccessor = null,
            IFileSystemAccessor fileSystemAccessor = null,
            IQuestionnaireExportStructureStorage questionnaireExportStructureStorage = null)
        {
            return new DataExportStatusReader(dataExportProcessesService: dataExportProcessesService ?? Substitute.For<IDataExportProcessesService>(),
                filebasedExportedDataAccessor: filebasedExportedDataAccessor ?? Substitute.For<IFilebasedExportedDataAccessor>(),
                paraDataAccessor: paraDataAccessor ?? Substitute.For<IParaDataAccessor>(),
                fileSystemAccessor: fileSystemAccessor ?? Substitute.For<IFileSystemAccessor>(),
                questionnaireExportStructureStorage: questionnaireExportStructureStorage ?? Substitute.For<IQuestionnaireExportStructureStorage>());
        }

        public ISubstitionTextFactory SubstitionTextFactory()
        {
            return new SubstitionTextFactory(Create.Service.SubstitutionService(), Create.Service.VariableToUIStringService());
        }

        public InterviewViewModelFactory InterviewViewModelFactory(IQuestionnaireStorage questionnaireRepository,
            IStatefulInterviewRepository interviewRepository,
            IEnumeratorSettings settings)
        {
            return new InterviewViewModelFactory(questionnaireRepository ?? Mock.Of<IQuestionnaireStorage>(),
                interviewRepository ?? Mock.Of<IStatefulInterviewRepository>(),
                settings ?? Mock.Of<IEnumeratorSettings>());
        }

        public AllInterviewsFactory AllInterviewsFactory(IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummarys = null,
            IQueryableReadSideRepositoryReader<QuestionAnswer> featuredQuestions = null)
        {
            return new AllInterviewsFactory(interviewSummarys ?? Mock.Of<IQueryableReadSideRepositoryReader<InterviewSummary>>(), 
                featuredQuestions ?? Mock.Of<IQueryableReadSideRepositoryReader<QuestionAnswer>>());
        }

        public PlainPostgresTransactionManager PlainPostgresTransactionManager(ISessionFactory sessionFactory = null)
            => new PlainPostgresTransactionManager(sessionFactory ?? Stub<ISessionFactory>.WithNotEmptyValues);

        public CqrsPostgresTransactionManager CqrsPostgresTransactionManager(ISessionFactory sessionFactory = null)
            => new CqrsPostgresTransactionManager(sessionFactory ?? Stub<ISessionFactory>.WithNotEmptyValues);

        public IConfigurationManager ConfigurationManager(NameValueCollection appSettings = null, NameValueCollection membershipSettings = null)
        {
            return new ConfigurationManager(appSettings ?? new NameValueCollection(), membershipSettings ?? new NameValueCollection());
        }

        public WebCacheBasedCaptchaService WebCacheBasedCaptchaService(int? failedLoginsCount = 5, int? timeSpanForLogins = 5, IConfigurationManager configurationManager = null)
        {
            return new WebCacheBasedCaptchaService(configurationManager ?? this.ConfigurationManager(new NameValueCollection
            {
                { "CountOfFailedLoginAttemptsBeforeCaptcha", (failedLoginsCount ?? 5).ToString() },
                { "TimespanInMinutesCaptchaWillBeShownAfterFailedLoginAttempt", (timeSpanForLogins ?? 5).ToString() },
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
            IQueryableReadSideRepositoryReader<InterviewStatuses> interviewStatuses = null,
            QuestionnaireExportStructure questionnaireExportStructure = null,
            IQueryableReadSideRepositoryReader<InterviewCommentaries> interviewCommentaries = null)
            => new ReadSideToTabularFormatExportService(fileSystemAccessor ?? Mock.Of<IFileSystemAccessor>(),
                csvWriter ?? Mock.Of<ICsvWriter>(_
                    => _.OpenCsvWriter(It.IsAny<Stream>(), It.IsAny<string>()) == (csvWriterService ?? Mock.Of<ICsvWriterService>())),
                Mock.Of<ILogger>(),
                Mock.Of<ITransactionManagerProvider>(x => x.GetTransactionManager() == Mock.Of<ITransactionManager>()),
                new TestInMemoryWriter<InterviewSummary>(),
                new InterviewDataExportSettings(),
                Mock.Of<IQuestionnaireExportStructureStorage>(_
                    => _.GetQuestionnaireExportStructure(It.IsAny<QuestionnaireIdentity>()) == questionnaireExportStructure),
                Mock.Of<IProductVersion>());

        public InterviewerPrincipal InterviewerPrincipal(IPlainStorage<InterviewerIdentity> interviewersPlainStorage, IPasswordHasher passwordHasher)
        {
            return new InterviewerPrincipal(
                interviewersPlainStorage ?? Mock.Of<IPlainStorage<InterviewerIdentity>>(),
                passwordHasher ?? Mock.Of<IPasswordHasher>());
        }

        public SynchronizationProcess SynchronizationProcess(IPlainStorage<InterviewView> interviewViewRepository = null,
            IPlainStorage<InterviewerIdentity> interviewersPlainStorage = null,
            IPlainStorage<InterviewMultimediaView> interviewMultimediaViewStorage = null,
            IPlainStorage<InterviewFileView> interviewFileViewStorage = null,
            ISynchronizationService synchronizationService = null,
            ILogger logger = null,
            IUserInteractionService userInteractionService = null,
            IPasswordHasher passwordHasher = null,
            IPrincipal principal = null,
            IMvxMessenger messenger = null,
            IInterviewerQuestionnaireAccessor questionnaireFactory = null,
            IInterviewerInterviewAccessor interviewFactory = null,
            IAttachmentContentStorage attachmentContentStorage = null,
            IHttpStatistican httpStatistican = null)
        {
            var syncServiceMock = synchronizationService ?? Mock.Of<ISynchronizationService>();
            return new SynchronizationProcess(
                syncServiceMock,
                interviewersPlainStorage ?? Mock.Of<IPlainStorage<InterviewerIdentity>>(),
                interviewViewRepository ?? new SqliteInmemoryStorage<InterviewView>(),
                principal ?? Mock.Of<IPrincipal>(),
                logger ?? Mock.Of<ILogger>(),
                userInteractionService ?? Mock.Of<IUserInteractionService>(),
                questionnaireFactory ?? Mock.Of<IInterviewerQuestionnaireAccessor>(),
                attachmentContentStorage ?? Mock.Of<IAttachmentContentStorage>(),
                interviewFactory ?? Mock.Of<IInterviewerInterviewAccessor>(),
                interviewMultimediaViewStorage ?? Mock.Of<IPlainStorage<InterviewMultimediaView>>(),
                interviewFileViewStorage ?? Mock.Of<IPlainStorage<InterviewFileView>>(),
                new CompanyLogoSynchronizer(new InMemoryPlainStorage<CompanyLogo>(), syncServiceMock),
                Mock.Of<AttachmentsCleanupService>(),
                passwordHasher ?? Mock.Of<IPasswordHasher>(),
                httpStatistican ?? Mock.Of<IHttpStatistican>());
        }
    }
}