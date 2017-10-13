using System;
using System.Linq;
using Microsoft.AspNet.Identity;
using Moq;
using SQLite;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Services.Synchronization;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.Implementation;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.SurveySolutions;
using WB.Tests.Abc.Storage;

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

        public IUserViewFactory UserViewFactory(params HqUser[] users) => new UserViewFactory(this.UserRepository(users));

        public IUserRepository UserRepository(params HqUser[] users)
            => Mock.Of<IUserRepository>(x => x.Users == users.AsQueryable());

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
            return new AssignmentDocumentsStorage(InMemorySqLiteConnection, Mock.Of<ILogger>());
        }

        public SQLiteConnectionWithLock InMemorySqLiteConnection =>
            new SQLiteConnectionWithLock(new SQLiteConnectionString(":memory:", true),
                openFlags: SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.FullMutex);
    }
}