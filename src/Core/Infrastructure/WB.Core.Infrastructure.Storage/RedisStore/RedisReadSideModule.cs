using Ninject.Modules;
using ServiceStack.Redis;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Storage.Raven.Implementation.ReadSide.RepositoryAccessors;
using WB.Core.Infrastructure.Storage.RedisStore.Implementation;

namespace WB.Core.Infrastructure.Storage.RedisStore
{
    public class RedisReadSideModule : NinjectModule
    {
        private readonly string connectionString;

        public RedisReadSideModule(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public override void Load()
        {
            this.Kernel.Bind<IRedisClientsManager>().ToMethod((kernel) => new RedisManagerPool(connectionString));
            this.Kernel.Bind(typeof(IReadSideKeyValueStorage<>)).To(typeof(RedisReadSideStore<>)).InSingletonScope();
        }
    }
}