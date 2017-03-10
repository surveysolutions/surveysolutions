using System.Linq;
using System.Reflection.Emit;
using Moq;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.Infrastructure.Implementation;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.SurveySolutions;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Abc.TestFactories
{
    public class StorageFactory
    {
        public IPlainStorageAccessor<TEntity> InMemoryPlainStorage<TEntity>() where TEntity : class => new InMemoryPlainStorageAccessor<TEntity>();
        public TestInMemoryWriter<TEntity> InMemoryReadeSideStorage<TEntity>() where TEntity : class, IReadSideRepositoryEntity => new TestInMemoryWriter<TEntity>();

        public IUserViewFactory UserViewFactory(params ApplicationUser[] users) => new UserViewFactory(this.UserRepository());

        public IUserRepository UserRepository(params ApplicationUser[] users)
            => Mock.Of<IUserRepository>(x => x.Users == users.AsQueryable());
    }
}