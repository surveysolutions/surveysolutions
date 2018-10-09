using System;
using System.Linq;
using Amazon.S3;
using Amazon.S3.Transfer;
using Microsoft.AspNet.Identity;
using Moq;
using SQLite;
using WB.Core.BoundedContexts.Headquarters.IntreviewerProfiles;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Storage.AmazonS3;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.Implementation;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.Enumerator.Implementation.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.SurveySolutions;
using WB.Tests.Abc.Storage;
using WB.UI.WebTester.Services;
using WB.UI.WebTester.Services.Implementation;

namespace WB.Tests.Abc.TestFactories
{
    public class TestHqUserManager : HqUserManager
    {
        public TestHqUserManager() : base(Mock.Of<IUserStore<HqUser, Guid>>(),
            Mock.Of<IHashCompatibilityProvider>(),
            Mock.Of<IPasswordHasher>(),
            Mock.Of<IIdentityValidator<string>>(),
            Mock.Of<IAuditLog>()) { }
    }

    public class StorageFactory
    {
        public IPlainStorageAccessor<TEntity> InMemoryPlainStorage<TEntity>() where TEntity : class => new InMemoryPlainStorageAccessor<TEntity>();
        public TestInMemoryWriter<TEntity> InMemoryReadeSideStorage<TEntity>() where TEntity : class, IReadSideRepositoryEntity => new TestInMemoryWriter<TEntity>();

        public IUserViewFactory UserViewFactory(params HqUser[] users) => new UserViewFactory(this.UserRepository(users), this.UserProfileFactory());

        public IUserRepository UserRepository(params HqUser[] users)
            => Mock.Of<IUserRepository>(x => x.Users == users.AsQueryable());

        public IInterviewerProfileFactory UserProfileFactory() => Mock.Of<IInterviewerProfileFactory>();

        public HqUserManager HqUserManager(IUserStore<HqUser, Guid> userStore = null,
            IHashCompatibilityProvider hashCompatibilityProvider = null,
            IPasswordHasher passwordHasher = null,
            IIdentityValidator<string> identityValidator = null,
            IAuditLog logger = null)
            => new HqUserManager(userStore ?? Mock.Of<IUserStore<HqUser, Guid>>(),
                hashCompatibilityProvider,
                passwordHasher ?? Mock.Of<IPasswordHasher>(),
                identityValidator ?? Mock.Of<IIdentityValidator<string>>(),
                logger ?? Mock.Of<IAuditLog>());

        public IAssignmentDocumentsStorage AssignmentDocumentsInmemoryStorage()
        {
            var mockOfEncryptionService = new Mock<IEncryptionService>();
            mockOfEncryptionService.Setup(x => x.Encrypt(It.IsAny<string>())).Returns<string>(x=>x);
            mockOfEncryptionService.Setup(x => x.Decrypt(It.IsAny<string>())).Returns<string>(x => x);
            return new AssignmentDocumentsStorage(InMemorySqLiteConnection, Mock.Of<ILogger>(), mockOfEncryptionService.Object);
        }

        public SQLiteConnectionWithLock InMemorySqLiteConnection =>
            new SQLiteConnectionWithLock(new SQLiteConnectionString(":memory:", true),
                openFlags: SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.FullMutex);

        public InMemoryCacheStorage<MultimediaFile, string> MediaStorage()
        {
            return new InMemoryCacheStorage<MultimediaFile, string>();
        }

        public S3FileStorage S3FileStorage(AmazonS3Settings s3Settings, IAmazonS3 client, ITransferUtility transferUtility, ILoggerProvider loggerProvider)
        {
            return new S3FileStorage(s3Settings, client, transferUtility, loggerProvider);
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
    }
}
