using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Storage.Postgre.Implementation;

namespace WB.Core.Infrastructure.Storage.Postgre
{
    public class PostgresPlainStorageModule : Ninject.Modules.NinjectModule
    {
        public override void Load()
        {
            this.Bind(typeof(IPlainStorageAccessor<>)).To(typeof(PostgresPlainStorageRepository<>));
            this.Bind(typeof(IQueryablePlainStorageAccessor<>)).To(typeof(PostgresPlainStorageRepository<>));
        }
    }
}