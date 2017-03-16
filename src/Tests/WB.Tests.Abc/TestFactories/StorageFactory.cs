using System;
using System.Linq;
using Microsoft.AspNet.Identity;
using Moq;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.Infrastructure.Implementation;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.SurveySolutions;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Abc.TestFactories
{
    public class TestHqUserManager : HqUserManager
    {
        public TestHqUserManager() : base(Mock.Of<IUserStore<HqUser, Guid>>(), Mock.Of<IAuthorizedUser>()) { }
    }
    public class StorageFactory
    {
        public IPlainStorageAccessor<TEntity> InMemoryPlainStorage<TEntity>() where TEntity : class => new InMemoryPlainStorageAccessor<TEntity>();
        public TestInMemoryWriter<TEntity> InMemoryReadeSideStorage<TEntity>() where TEntity : class, IReadSideRepositoryEntity => new TestInMemoryWriter<TEntity>();

        public IUserViewFactory UserViewFactory(params HqUser[] users) => new UserViewFactory(this.UserRepository());

        public IUserRepository UserRepository(params HqUser[] users)
            => Mock.Of<IUserRepository>(x => x.Users == users.AsQueryable());

        public HqUserManager HqUserManager(IUserStore<HqUser, Guid> userStore = null, IAuthorizedUser authorizedUser = null)
            => new HqUserManager(userStore, authorizedUser);
    }
}