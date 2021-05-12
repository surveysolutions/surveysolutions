using System;
using System.Linq;
using Amazon.S3;
using Amazon.S3.Transfer;
using Main.Core.Documents;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using Moq;
using Ncqrs.Eventing.Storage;
using SQLite;
using WB.Core.BoundedContexts.Headquarters.Storage.AmazonS3;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.Users.UserProfile;
using WB.Core.BoundedContexts.Headquarters.Views.Device;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.Implementation;
using WB.Core.Infrastructure.Implementation.Aggregates;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.Enumerator.Implementation.Repositories;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Core.SharedKernels.SurveySolutions;
using WB.Enumerator.Native.Questionnaire.Impl;
using WB.Tests.Abc.Storage;
using ILogger = WB.Core.GenericSubdomains.Portable.Services.ILogger;
using ILoggerProvider = WB.Core.GenericSubdomains.Portable.Services.ILoggerProvider;

namespace WB.Tests.Abc.TestFactories
{
    // TODO: Core migration https://issues.mysurvey.solutions/youtrack/issue/KP-13523
    //public class TestHqUserManager : HqUserManager
    //{
    //    public TestHqUserManager() : base(Mock.Of<IUserRepository>(),
    //        Mock.Of<IHashCompatibilityProvider>(),
    //        Mock.Of<IIdentityPasswordHasher>(),
    //        Mock.Of<IPasswordValidator>(),
    //        Mock.Of<IIdentityValidator>(),
    //        Mock.Of<ISystemLog>()) { }
    //}

    public class StorageFactory
    {
        public InMemoryPlainStorage<TEntity, TKey> InMemorySqlitePlainStorage<TEntity, TKey>(ILogger logger = null) where TEntity : class, IPlainStorageEntity<TKey>, new()
            => new InMemoryPlainStorage<TEntity, TKey>(logger ?? Mock.Of<ILogger>());
        public InMemoryPlainStorage<TEntity> InMemorySqlitePlainStorage<TEntity>(ILogger logger = null) where TEntity : class, IPlainStorageEntity, new()
            => new InMemoryPlainStorage<TEntity>(logger ?? Mock.Of<ILogger>());
        public IEnumeratorEventStorage InMemorySqliteMultiFilesEventStorage(IEnumeratorSettings enumeratorSettings = null) 
            => new InMemorySqliteMultiFilesEventStorage(enumeratorSettings ?? Mock.Of<IEnumeratorSettings>(s => s.EventChunkSize == 100));

        public IPlainStorageAccessor<TEntity> InMemoryPlainStorage<TEntity>() where TEntity : class => new InMemoryPlainStorageAccessor<TEntity>();
        public TestInMemoryWriter<TEntity> InMemoryReadSideStorage<TEntity>() where TEntity : class, IReadSideRepositoryEntity => new TestInMemoryWriter<TEntity>();
        public TestInMemoryWriter<TEntity, TKey> InMemoryReadSideStorage<TEntity, TKey>() where TEntity : class, IReadSideRepositoryEntity => new TestInMemoryWriter<TEntity, TKey>();

        public IUserViewFactory UserViewFactory(params HqUser[] users) => new UserViewFactory(
            this.UserRepository(users),
            NewMemoryCache(),
            Create.Storage.InMemoryPlainStorage<DeviceSyncInfo>(),
            Create.Service.WorkspaceContextAccessor());

        public InMemoryEventStore InMemoryEventStore() => new InMemoryEventStore(NewAggregateRootCache());
        
        public IUserRepository UserRepository(params HqUser[] users)
            => Mock.Of<IUserRepository>(x => x.Users == users.AsQueryable());

        public IInterviewerProfileFactory UserProfileFactory() => Mock.Of<IInterviewerProfileFactory>();

        // public UserManager<HqUser> HqUserManager(IUserRepository userStore = null,
        //     IHashCompatibilityProvider hashCompatibilityProvider = null,
        //     IIdentityPasswordHasher passwordHasher = null,
        //     IIdentityValidator identityValidator = null,
        //     ISystemLog logger = null)
        //     => new UserManager<HqUser>(userStore ?? Mock.Of<IUserRepository>(),
        //         hashCompatibilityProvider,
        //         passwordHasher ?? Mock.Of<IIdentityPasswordHasher>(),
        //         passwordValidator ?? Mock.Of<IPasswordValidator>(),
        //         identityValidator ?? Mock.Of<IIdentityValidator>(),
        //         logger ?? Mock.Of<ISystemLog>());

        public IAssignmentDocumentsStorage AssignmentDocumentsInmemoryStorage()
        {
            var mockOfEncryptionService = new Mock<IEncryptionService>();
            mockOfEncryptionService.Setup(x => x.Encrypt(It.IsAny<string>())).Returns<string>(x=>x);
            mockOfEncryptionService.Setup(x => x.Decrypt(It.IsAny<string>())).Returns<string>(x => x);
            return new AssignmentDocumentsStorage(InMemorySqLiteConnection, Mock.Of<ILogger>(), mockOfEncryptionService.Object);
        }

        public SQLiteConnectionWithLock InMemorySqLiteConnection =>
            new SQLiteConnectionWithLock(new SQLiteConnectionString(":memory:", SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.FullMutex, true));

        public AmazonS3ExternalFileStorage AmazonS3ExternalFileStorage(
            IAmazonS3Configuration s3Settings, 
            IAmazonS3 client, 
            ITransferUtility transferUtility)
        { 
            return new AmazonS3ExternalFileStorage(s3Settings, client, transferUtility, new NullLogger<AmazonS3ExternalFileStorage>());
        }

        public IPlainStorage<TEntity> SqliteInmemoryStorage<TEntity>(params TEntity[] items)
            where TEntity : class, IPlainStorageEntity, IPlainStorageEntity<string>, new()
        {
            var storage = new SqliteInmemoryStorage<TEntity>();
            foreach (var entity in items)
                storage.Store(entity);

            return storage;
        }

        public IPlainStorage<TEntity, TKey> SqliteInmemoryStorage<TEntity, TKey>(params TEntity[] items)
            where TEntity : class, IPlainStorageEntity<TKey>, new()
        {
            var storage = new SqliteInmemoryStorage<TEntity, TKey>();
            foreach (var entity in items)
                storage.Store(entity);

            return storage;
        }

        public QuestionnaireQuestionOptionsRepository QuestionnaireQuestionOptionsRepository()
        {
            var optionsRepository = new QuestionnaireQuestionOptionsRepository(
                );

            return optionsRepository;
        }

        public IStatefulInterviewRepository InterviewRepository(IStatefulInterview interview)
        {
            return SetUp.StatefulInterviewRepository(interview);
        }

        private static IMemoryCache cache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
        public IMemoryCache NewMemoryCache() => cache;

        public AggregateRootCache NewAggregateRootCache() 
            => new AggregateRootCache(NewMemoryCache());

        public IQuestionnaireStorage QuestionnaireStorage(QuestionnaireDocument questionnaire)
        {
            var result = new Mock<IQuestionnaireStorage>();
            result.Setup(x => x.GetQuestionnaire(It.IsAny<QuestionnaireIdentity>(), It.IsAny<string>()))
                .Returns(Create.Entity.PlainQuestionnaire(questionnaire));
            result.Setup(x => x.GetQuestionnaireOrThrow(It.IsAny<QuestionnaireIdentity>(), It.IsAny<string>()))
                .Returns(Create.Entity.PlainQuestionnaire(questionnaire));
            result.Setup(x => x.GetQuestionnaireDocument(It.IsAny<QuestionnaireIdentity>()))
                .Returns(questionnaire);
            result.Setup(x => x.GetQuestionnaireDocument(It.IsAny<Guid>(), It.IsAny<long>()))
                .Returns(questionnaire);

            return result.Object;
        }

        public IQuestionnaireStorage QuestionnaireStorage(
            IPlainKeyValueStorage<QuestionnaireDocument> repository = null,
            ITranslationStorage translationStorage = null,
            IQuestionnaireTranslator translator = null,
            IQuestionOptionsRepository questionOptionsRepository = null,
            ISubstitutionService substitutionService = null,
            IInterviewExpressionStorageProvider expressionStorageProvider = null,
            IMemoryCache memoryCache = null)
        {
            return new QuestionnaireStorage(
                repository ?? new TestInMemoryKeyValueStorage<QuestionnaireDocument>(),
                translationStorage ?? new TranslationsStorage(new SqliteInmemoryStorage<TranslationInstance>()),
                translator ?? Create.Service.QuestionnaireTranslator(),
                questionOptionsRepository ?? QuestionOptionsRepository(new SqliteInmemoryStorage<OptionView, int?>()),
                substitutionService ?? Create.Service.SubstitutionService(),
                expressionStorageProvider ?? Mock.Of<IInterviewExpressionStorageProvider>(),
                memoryCache ?? new MemoryCache(Options.Create(new MemoryCacheOptions()))
                );            
        }

        public IQuestionOptionsRepository QuestionOptionsRepository(IOptionsRepository optionsRepository)
        {
            return new QuestionOptionsRepository(optionsRepository);
        }

        public IQuestionOptionsRepository QuestionOptionsRepository(IPlainStorage<OptionView, int?> plainStore)
        {
            return new QuestionOptionsRepository(OptionsRepository(plainStore));
        }

        public IOptionsRepository OptionsRepository(IPlainStorage<OptionView, int?> plainStore)
        {
            return new OptionsRepository(plainStore);
        }

        public IQuestionnaireStorage QuestionnaireStorage(IQuestionnaire questionnaire)
        {
            var result = new Mock<IQuestionnaireStorage>();
            result.Setup(x => x.GetQuestionnaire(It.IsAny<QuestionnaireIdentity>(), It.IsAny<string>()))
                .Returns(questionnaire);
            result.Setup(x => x.GetQuestionnaireOrThrow(It.IsAny<QuestionnaireIdentity>(), It.IsAny<string>()))
                .Returns(questionnaire);

            return result.Object;
        }
    }
}
