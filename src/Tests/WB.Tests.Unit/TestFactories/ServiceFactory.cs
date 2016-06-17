using System;
using Main.Core.Documents;
using Moq;
using Microsoft.Practices.ServiceLocation;
using Ncqrs.Eventing.Storage;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Headquarters.DataExport.Denormalizers;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.EventBus.Lite.Implementation;
using WB.Core.Infrastructure.Implementation.EventDispatcher;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Implementation.Factories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Infrastructure.Native.Files.Implementation.FileSystem;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;
using WB.Tests.Unit.SharedKernels.SurveyManagement;
using ILogger = WB.Core.GenericSubdomains.Portable.Services.ILogger;
using WB.Core.GenericSubdomains.Portable.Implementation.Services;
using Ncqrs.Domain.Storage;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.EventHandler.WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.ChangeStatus;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Implementation.Repositories;
using WB.Core.SharedKernels.Enumerator.Repositories;
using AttachmentContent = WB.Core.BoundedContexts.Headquarters.Views.Questionnaire.AttachmentContent;

namespace WB.Tests.Unit.TestFactories
{
    internal class ServiceFactory
    {
        public IAnswerToStringService AnswerToStringService()
            => new AnswerToStringService();

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
            IAsyncPlainStorage<InterviewView> interviewViewRepository = null,
            IPlainQuestionnaireRepository plainQuestionnaireRepository = null,
            ILiteEventRegistry liteEventRegistry = null)
            => new InterviewerDashboardEventHandler(
                interviewViewRepository ?? Mock.Of<IAsyncPlainStorage<InterviewView>>(),
                plainQuestionnaireRepository ?? Mock.Of<IPlainQuestionnaireRepository>(),
                liteEventRegistry ?? Mock.Of<ILiteEventRegistry>());

        public IDomainRepository DomainRepository(IAggregateSnapshotter aggregateSnapshotter = null, IServiceLocator serviceLocator = null)
            => new DomainRepository(
                aggregateSnapshotter: aggregateSnapshotter ?? Mock.Of<IAggregateSnapshotter>(),
                serviceLocator: serviceLocator ?? Mock.Of<IServiceLocator>());

        public IEventSourcedAggregateRootRepository EventSourcedAggregateRootRepository(
            IEventStore eventStore = null, ISnapshotStore snapshotStore = null, IDomainRepository repository = null)
            => new EventSourcedAggregateRootRepository(eventStore, snapshotStore, repository);

        public FileSystemIOAccessor FileSystemIOAccessor()
            => new FileSystemIOAccessor();

        public InterviewAnswersCommandValidator InterviewAnswersCommandValidator(IInterviewSummaryViewFactory interviewSummaryViewFactory = null)
            => new InterviewAnswersCommandValidator(
                interviewSummaryViewFactory ?? Mock.Of<IInterviewSummaryViewFactory>());

        public InterviewDetailsViewFactory InterviewDetailsViewFactory(IReadSideKeyValueStorage<InterviewData> interviewStore = null,
            IPlainStorageAccessor<UserDocument> userStore = null,
            IInterviewDataAndQuestionnaireMerger merger = null,
            IChangeStatusFactory changeStatusFactory = null,
            IInterviewPackagesService incomingSyncPackagesQueue = null,
            IPlainQuestionnaireRepository plainQuestionnaireRepository = null,
            IEventSourcedAggregateRootRepository eventSourcedRepository = null,
            IReadSideKeyValueStorage<InterviewLinkedQuestionOptions> interviewLinkedQuestionOptionsStore = null,
            IAttachmentContentService attachmentContentService = null)
            => new InterviewDetailsViewFactory(interviewStore ?? new TestInMemoryWriter<InterviewData>(),
                userStore ?? Mock.Of<IPlainStorageAccessor<UserDocument>>(_
                    => _.GetById(It.IsAny<object>()) == Create.Entity.UserDocument()),
                merger ?? Mock.Of<IInterviewDataAndQuestionnaireMerger>(),
                changeStatusFactory ?? Mock.Of<IChangeStatusFactory>(),
                incomingSyncPackagesQueue ?? Mock.Of<IInterviewPackagesService>(),
                plainQuestionnaireRepository ?? Mock.Of<IPlainQuestionnaireRepository>(),
                eventSourcedRepository ?? Mock.Of<IEventSourcedAggregateRootRepository>(),
                interviewLinkedQuestionOptionsStore ?? new TestInMemoryWriter<InterviewLinkedQuestionOptions>(),
                attachmentContentService ?? Mock.Of<IAttachmentContentService>());

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
                Mock.Of<IPlainQuestionnaireRepository>(_ => _.GetQuestionnaireDocument(It.IsAny<Guid>(), It.IsAny<long>()) == questionnaireDocument),
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
                new ExportViewFactory(new QuestionnaireRosterStructureFactory(), new FileSystemIOAccessor())
                    .CreateQuestionnaireExportStructure(questionnaire, 1),
                new QuestionnaireRosterStructureFactory()
                    .CreateQuestionnaireRosterStructure(questionnaire, 1),
                questionnaire,
                new QuestionDataParser(),
                new UserViewFactory(new TestPlainStorage<UserDocument>()));

        public QuestionnaireKeyValueStorage QuestionnaireKeyValueStorage(IAsyncPlainStorage<QuestionnaireDocumentView> questionnaireDocumentViewRepository = null)
            => new QuestionnaireKeyValueStorage(
                questionnaireDocumentViewRepository ?? Mock.Of<IAsyncPlainStorage<QuestionnaireDocumentView>>());

        public QuestionnaireNameValidator QuestionnaireNameValidator(
            IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaireBrowseItemStorage = null)
            => new QuestionnaireNameValidator(
                questionnaireBrowseItemStorage ?? Stub<IPlainStorageAccessor<QuestionnaireBrowseItem>>.WithNotEmptyValues);

        public RebuildReadSideCqrsPostgresTransactionManagerWithoutSessions RebuildReadSideCqrsPostgresTransactionManager()
            => new RebuildReadSideCqrsPostgresTransactionManagerWithoutSessions();

        public IStatefulInterviewRepository StatefulInterviewRepository(IEventSourcedAggregateRootRepository aggregateRootRepository, ILiteEventBus liteEventBus = null)
            => new StatefulInterviewRepository(
                aggregateRootRepository: aggregateRootRepository ?? Mock.Of<IEventSourcedAggregateRootRepository>(),
                eventBus: liteEventBus ?? Mock.Of<ILiteEventBus>());

        public ISubstitutionService SubstitutionService()
            => new SubstitutionService();

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
                Create.Entity.ReadSideCacheSettings());

        public VariableToUIStringService VariableToUIStringService()
            => new VariableToUIStringService();
    }
}