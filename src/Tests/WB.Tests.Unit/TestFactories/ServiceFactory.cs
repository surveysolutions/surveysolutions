using System;
using System.Net.Http;
using Main.Core.Documents;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using System.Collections.Generic;
using Microsoft.Practices.ServiceLocation;
using Ncqrs.Eventing.Storage;
using NSubstitute;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.BoundedContexts.Designer.Implementation.Services.LookupTableService;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Headquarters.DataExport.Denormalizers;
using WB.Core.BoundedContexts.Headquarters.Interviews.Denormalizers;
using WB.Core.BoundedContexts.Headquarters.Questionnaires.Denormalizers;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.BoundedContexts.Tester.Implementation.Services;
using WB.Core.BoundedContexts.Supervisor;
using WB.Core.BoundedContexts.Supervisor.Interviews;
using WB.Core.BoundedContexts.Supervisor.Interviews.Implementation.Views;
using WB.Core.BoundedContexts.Supervisor.Synchronization;
using WB.Core.BoundedContexts.Supervisor.Synchronization.Atom;
using WB.Core.BoundedContexts.Supervisor.Synchronization.Atom.Implementation;
using WB.Core.BoundedContexts.Supervisor.Synchronization.Implementation;
using WB.Core.BoundedContexts.Supervisor.Users;
using WB.Core.BoundedContexts.Supervisor.Users.Implementation;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventBus.Hybrid.Implementation;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.EventBus.Lite.Implementation;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.Implementation.EventDispatcher;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Factories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Interview;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Views.User;
using WB.Infrastructure.Native.Files.Implementation.FileSystem;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;
using WB.Tests.Unit.SharedKernels.SurveyManagement;
using ILogger = WB.Core.GenericSubdomains.Portable.Services.ILogger;
using WB.Core.GenericSubdomains.Portable.Implementation.Services;
using WB.Core.SharedKernels.SurveyManagement.EventHandler.WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using Ncqrs.Domain.Storage;
using WB.Core.BoundedContexts.Designer.Services.CodeGeneration;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.Implementation.Aggregates;
using WB.Core.SharedKernels.Enumerator.Implementation.Repositories;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.UI.Designer.Code;
using WB.UI.Designer.Code.Implementation;
using WB.UI.Shared.Web.Membership;
using WB.UI.Shared.Web.MembershipProvider.Accounts;
using AttachmentContent = WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire.AttachmentContent;

namespace WB.Tests.Unit.TestFactories
{
    internal class ServiceFactory
    {
        public IAnswerToStringService AnswerToStringService()
            => new AnswerToStringService();

        public AtomFeedReader AtomFeedReader(Func<HttpMessageHandler> messageHandler = null, IHeadquartersSettings settings = null)
            => new AtomFeedReader(
                messageHandler ?? Mock.Of<Func<HttpMessageHandler>>(),
                settings ?? Mock.Of<IHeadquartersSettings>());

        public AttachmentContentService AttachmentContentService(IPlainStorageAccessor<AttachmentContent> attachmentContentPlainStorage)
            => new AttachmentContentService(
                attachmentContentPlainStorage ?? Mock.Of<IPlainStorageAccessor<AttachmentContent>>());

        public CodeGenerator CodeGenerator(
            IMacrosSubstitutionService macrosSubstitutionService = null,
            IExpressionProcessor expressionProcessor = null,
            ILookupTableService lookupTableService = null)
            => new CodeGenerator(
                macrosSubstitutionService ?? Create.Other.DefaultMacrosSubstitutionService(),
                expressionProcessor ?? ServiceLocator.Current.GetInstance<IExpressionProcessor>(),
                lookupTableService ?? ServiceLocator.Current.GetInstance<ILookupTableService>(),
                Mock.Of<IFileSystemAccessor>(),
                Mock.Of<ICompilerSettings>());

        public CommandPostprocessor CommandPostprocessor(
            IMembershipUserService membershipUserService,
            IRecipientNotifier recipientNotifier,
            IAccountRepository accountRepository,
            IReadSideKeyValueStorage<QuestionnaireDocument> documentStorage,
            ILogger logger,
            IAttachmentService attachmentService = null,
            ILookupTableService lookupTableService = null)
            => new CommandPostprocessor(
                membershipUserService,
                recipientNotifier,
                accountRepository,
                documentStorage,
                logger,
                attachmentService ?? Mock.Of<IAttachmentService>(),
                lookupTableService ?? Mock.Of<ILookupTableService>());

        public CumulativeChartDenormalizer CumulativeChartDenormalizer(
            IReadSideKeyValueStorage<LastInterviewStatus> lastStatusesStorage = null,
            IReadSideRepositoryWriter<CumulativeReportStatusChange> cumulativeReportStatusChangeStorage = null,
            IReadSideKeyValueStorage<InterviewReferences> interviewReferencesStorage = null)
            => new CumulativeChartDenormalizer(
                lastStatusesStorage ?? Mock.Of<IReadSideKeyValueStorage<LastInterviewStatus>>(),
                cumulativeReportStatusChangeStorage ?? Mock.Of<IReadSideRepositoryWriter<CumulativeReportStatusChange>>(),
                interviewReferencesStorage ?? Mock.Of<IReadSideKeyValueStorage<InterviewReferences>>());

        public InterviewEventHandler DashboardDenormalizer(
            IAsyncPlainStorage<InterviewView> interviewViewRepository = null,
            IPlainQuestionnaireRepository plainQuestionnaireRepository = null)
            => new InterviewEventHandler(
                interviewViewRepository ?? Mock.Of<IAsyncPlainStorage<InterviewView>>(),
                plainQuestionnaireRepository ?? Mock.Of<IPlainQuestionnaireRepository>());

        public IDesignerEngineVersionService DesignerEngineVersionService()
            => new DesignerEngineVersionService();

        public IDomainRepository DomainRepository(IAggregateSnapshotter aggregateSnapshotter = null, IServiceLocator serviceLocator = null)
            => new DomainRepository(
                aggregateSnapshotter: aggregateSnapshotter ?? Mock.Of<IAggregateSnapshotter>(),
                serviceLocator: serviceLocator ?? Mock.Of<IServiceLocator>());

        public IEventSourcedAggregateRootRepository EventSourcedAggregateRootRepository(
            IEventStore eventStore = null, ISnapshotStore snapshotStore = null, IDomainRepository repository = null)
            => new EventSourcedAggregateRootRepository(eventStore, snapshotStore, repository);

        public FileSystemIOAccessor FileSystemIOAccessor()
            => new FileSystemIOAccessor();

        public HeadquartersLoginService HeadquartersLoginService(IHeadquartersUserReader headquartersUserReader = null,
            Func<HttpMessageHandler> messageHandler = null,
            ILogger logger = null,
            ICommandService commandService = null,
            IHeadquartersSettings headquartersSettings = null,
            IPasswordHasher passwordHasher = null)
            => new HeadquartersLoginService(
                logger ?? Substitute.For<ILogger>(),
                commandService ?? Substitute.For<ICommandService>(),
                messageHandler ?? Substitute.For<Func<HttpMessageHandler>>(),
                headquartersSettings ?? Create.Other.HeadquartersSettings(),
                headquartersUserReader ?? Substitute.For<IHeadquartersUserReader>(),
                passwordHasher: passwordHasher ?? Substitute.For<IPasswordHasher>());

        public HybridEventBus HybridEventBus(ILiteEventBus liteEventBus = null, IEventBus cqrsEventBus = null)
            => new HybridEventBus(
                liteEventBus ?? Mock.Of<ILiteEventBus>(),
                cqrsEventBus ?? Mock.Of<IEventBus>());

        public InterviewAnswersCommandValidator InterviewAnswersCommandValidator(IInterviewSummaryViewFactory interviewSummaryViewFactory = null)
            => new InterviewAnswersCommandValidator(
                interviewSummaryViewFactory ?? Mock.Of<IInterviewSummaryViewFactory>());

        public InterviewEventStreamOptimizer InterviewEventStreamOptimizer()
            => new InterviewEventStreamOptimizer();

        public InterviewReferencesDenormalizer InterviewReferencesDenormalizer()
            => new InterviewReferencesDenormalizer(
                Mock.Of<IReadSideKeyValueStorage<InterviewReferences>>());

        public InterviewsFeedDenormalizer InterviewsFeedDenormalizer(IReadSideRepositoryWriter<InterviewFeedEntry> feedEntryWriter = null,
            IReadSideKeyValueStorage<InterviewData> interviewsRepository = null, IReadSideRepositoryWriter<InterviewSummary> interviewSummaryRepository = null)
            => new InterviewsFeedDenormalizer(
                feedEntryWriter ?? Substitute.For<IReadSideRepositoryWriter<InterviewFeedEntry>>(),
                interviewsRepository ?? Substitute.For<IReadSideKeyValueStorage<InterviewData>>(), interviewSummaryRepository ?? Substitute.For<IReadSideRepositoryWriter<InterviewSummary>>());

        public InterviewsSynchronizer InterviewsSynchronizer(
            IReadSideRepositoryReader<InterviewSummary> interviewSummaryRepositoryReader = null,
            IQueryableReadSideRepositoryReader<ReadyToSendToHeadquartersInterview> readyToSendInterviewsRepositoryReader = null,
            Func<HttpMessageHandler> httpMessageHandler = null,
            IEventStore eventStore = null,
            ILogger logger = null,
            ISerializer serializer = null,
            ICommandService commandService = null,
            HeadquartersPushContext headquartersPushContext = null,
            IPlainStorageAccessor<UserDocument> userDocumentStorage = null, IPlainStorageAccessor<LocalInterviewFeedEntry> plainStorage = null,
            IHeadquartersInterviewReader headquartersInterviewReader = null,
            IPlainQuestionnaireRepository plainQuestionnaireRepository = null,
            IInterviewSynchronizationFileStorage interviewSynchronizationFileStorage = null,
            IArchiveUtils archiver = null)
            => new InterviewsSynchronizer(
                Mock.Of<IAtomFeedReader>(),
                Create.Other.HeadquartersSettings(),
                logger ?? Mock.Of<ILogger>(),
                commandService ?? Mock.Of<ICommandService>(),
                plainStorage ?? Mock.Of<IPlainStorageAccessor<LocalInterviewFeedEntry>>(),
                userDocumentStorage ?? Mock.Of<IPlainStorageAccessor<UserDocument>>(),
                plainQuestionnaireRepository ??
                Mock.Of<IPlainQuestionnaireRepository>(
                    _ => _.GetQuestionnaireDocument(It.IsAny<Guid>(), It.IsAny<long>()) == new QuestionnaireDocument()),
                headquartersInterviewReader ?? Mock.Of<IHeadquartersInterviewReader>(),
                Create.Other.HeadquartersPullContext(),
                headquartersPushContext ?? Create.Other.HeadquartersPushContext(),
                eventStore ?? Mock.Of<IEventStore>(),
                serializer ?? Mock.Of<ISerializer>(),
                interviewSummaryRepositoryReader ?? Mock.Of<IReadSideRepositoryReader<InterviewSummary>>(),
                readyToSendInterviewsRepositoryReader ?? Stub.ReadSideRepository<ReadyToSendToHeadquartersInterview>(),
                httpMessageHandler ?? Mock.Of<Func<HttpMessageHandler>>(),
                interviewSynchronizationFileStorage ??
                Mock.Of<IInterviewSynchronizationFileStorage>(
                    _ => _.GetImagesByInterviews() == new List<InterviewBinaryDataDescriptor>()),
                archiver ?? Mock.Of<IArchiveUtils>(),
                Mock.Of<IPlainTransactionManager>(),
                Mock.Of<ITransactionManager>());

        public KeywordsProvider KeywordsProvider()
            => new KeywordsProvider(Create.Service.SubstitutionService());

        public LiteEventBus LiteEventBus(ILiteEventRegistry liteEventRegistry = null, IEventStore eventStore = null)
            => new LiteEventBus(
                liteEventRegistry ?? Stub<ILiteEventRegistry>.WithNotEmptyValues,
                eventStore ?? Mock.Of<IEventStore>());

        public ILiteEventRegistry LiteEventRegistry()
            => new LiteEventRegistry();

        public LookupTableService LookupTableService(
            IPlainKeyValueStorage<LookupTableContent> lookupTableContentStorage = null,
            IReadSideKeyValueStorage<QuestionnaireDocument> documentStorage = null)
            => new LookupTableService(
                lookupTableContentStorage ?? Mock.Of<IPlainKeyValueStorage<LookupTableContent>>(),
                documentStorage ?? Mock.Of<IReadSideKeyValueStorage<QuestionnaireDocument>>());

        public MacrosSubstitutionService MacrosSubstitutionService()
            => new MacrosSubstitutionService();

        public MapReportDenormalizer MapReportDenormalizer(
            IReadSideRepositoryWriter<MapReportPoint> mapReportPointStorage = null,
            IReadSideKeyValueStorage<InterviewReferences> interviewReferencesStorage = null,
            QuestionnaireQuestionsInfo questionnaireQuestionsInfo = null,
            QuestionnaireDocument questionnaireDocument = null)
            => new MapReportDenormalizer(
                interviewReferencesStorage ?? new TestInMemoryWriter<InterviewReferences>(),
                mapReportPointStorage ?? new TestInMemoryWriter<MapReportPoint>(),
                Mock.Of<IPlainQuestionnaireRepository>(_ => _.GetQuestionnaireDocument(Moq.It.IsAny<Guid>(), Moq.It.IsAny<long>()) == questionnaireDocument),
                Mock.Of<IPlainKeyValueStorage<QuestionnaireQuestionsInfo>>(_ => _.GetById(Moq.It.IsAny<string>()) == questionnaireQuestionsInfo));

        public NcqrCompatibleEventDispatcher NcqrCompatibleEventDispatcher(EventBusSettings eventBusSettings = null, ILogger logger = null)
            => new NcqrCompatibleEventDispatcher(
                eventStore: Mock.Of<IEventStore>(),
                eventBusSettings: eventBusSettings ?? Create.Other.EventBusSettings(),
                logger: logger ?? Mock.Of<ILogger>())
            {
                TransactionManager = Mock.Of<ITransactionManagerProvider>(x => x.GetTransactionManager() == Mock.Of<ITransactionManager>())
            };

        public PreloadedDataService PreloadedDataService(QuestionnaireDocument questionnaire)
            => new PreloadedDataService(
                new ExportViewFactory(new QuestionnaireRosterStructureFactory(), new FileSystemIOAccessor())
                    .CreateQuestionnaireExportStructure(questionnaire, 1),
                new QuestionnaireRosterStructureFactory()
                    .CreateQuestionnaireRosterStructure(questionnaire, 1),
                questionnaire,
                new QuestionDataParser(),
                new UserViewFactory(new TestPlainStorage<UserDocument>()));

        public QuestionnaireExpressionStateModelFactory QuestionnaireExecutorTemplateModelFactory(
            IMacrosSubstitutionService macrosSubstitutionService = null,
            IExpressionProcessor expressionProcessor = null,
            ILookupTableService lookupTableService = null)
            => new QuestionnaireExpressionStateModelFactory(
                macrosSubstitutionService ?? Create.Other.DefaultMacrosSubstitutionService(),
                expressionProcessor ?? ServiceLocator.Current.GetInstance<IExpressionProcessor>(),
                lookupTableService ?? ServiceLocator.Current.GetInstance<ILookupTableService>());

        public QuestionnaireFeedDenormalizer QuestionnaireFeedDenormalizer(IReadSideRepositoryWriter<QuestionnaireFeedEntry> questionnaireFeedWriter)
            => new QuestionnaireFeedDenormalizer(questionnaireFeedWriter);

        public QuestionnaireImportService QuestionnaireImportService(IPlainQuestionnaireRepository plainKeyValueStorage = null)
            => new QuestionnaireImportService(
                Mock.Of<IPlainQuestionnaireRepository>(),
                Mock.Of<IQuestionnaireAssemblyFileAccessor>(),
                Mock.Of<IOptionsRepository>());

        public QuestionnaireKeyValueStorage QuestionnaireKeyValueStorage(IAsyncPlainStorage<QuestionnaireDocumentView> questionnaireDocumentViewRepository = null)
            => new QuestionnaireKeyValueStorage(
                questionnaireDocumentViewRepository ?? Mock.Of<IAsyncPlainStorage<QuestionnaireDocumentView>>());

        public QuestionnaireNameValidator QuestionnaireNameValidator(
            IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaireBrowseItemStorage = null)
            => new QuestionnaireNameValidator(
                questionnaireBrowseItemStorage ?? Stub<IPlainStorageAccessor<QuestionnaireBrowseItem>>.WithNotEmptyValues);

        public RebuildReadSideCqrsPostgresTransactionManagerWithoutSessions RebuildReadSideCqrsPostgresTransactionManager()
            => new RebuildReadSideCqrsPostgresTransactionManagerWithoutSessions();

        public RoslynExpressionProcessor RoslynExpressionProcessor()
            => new RoslynExpressionProcessor();

        public IStatefulInterviewRepository StatefulInterviewRepository(IEventSourcedAggregateRootRepository aggregateRootRepository, ILiteEventBus liteEventBus = null)
            => new StatefulInterviewRepository(
                aggregateRootRepository: aggregateRootRepository ?? Mock.Of<IEventSourcedAggregateRootRepository>(),
                eventBus: liteEventBus ?? Mock.Of<ILiteEventBus>());

        public ISubstitutionService SubstitutionService()
            => new SubstitutionService();

        public Synchronizer Synchronizer(IInterviewsSynchronizer interviewsSynchronizer = null)
            => new Synchronizer(
                Mock.Of<ILocalFeedStorage>(),
                Mock.Of<IUserChangedFeedReader>(),
                Mock.Of<ILocalUserFeedProcessor>(),
                interviewsSynchronizer ?? Mock.Of<IInterviewsSynchronizer>(),
                Mock.Of<IQuestionnaireSynchronizer>(),
                Mock.Of<IPlainTransactionManager>(),
                Create.Other.HeadquartersPullContext(),
                Create.Other.HeadquartersPushContext(),
                Mock.Of<ILogger>());

        public TeamViewFactory TeamViewFactory(
            IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaryReader = null,
            IPlainStorageAccessor<UserDocument> usersReader = null)
            => new TeamViewFactory(
                interviewSummaryReader, usersReader);

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
                Create.Other.ReadSideCacheSettings());

        public UserChangedFeedReader UserChangedFeedReader(IHeadquartersSettings settings = null,
            Func<HttpMessageHandler> messageHandler = null)
            => new UserChangedFeedReader(
                settings ?? Create.Other.HeadquartersSettings(),
                messageHandler ?? Substitute.For<Func<HttpMessageHandler>>(),
                Create.Other.HeadquartersPullContext());
    }
}