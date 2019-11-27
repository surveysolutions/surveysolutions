using System.Reflection;
using System.Threading.Tasks;
using Npgsql;
using NpgsqlTypes;
using Quartz;
using Quartz.Impl.AdoJobStore.Common;
using Quartz.Spi;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Modularity;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Infrastructure.Native.Storage.Postgre.DbMigrations;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;

namespace WB.Core.BoundedContexts.Headquarters.QuartzIntegration
{
    public class QuartzModule : IModule
    {
        private readonly Assembly migrationsAssembly;
        private readonly string nameSpace;
        private readonly string instanceId;
        private readonly bool isClustered;

        private Task<IScheduler> Scheduler => SchedulerTask.Task;

        private TaskCompletionSource<IScheduler> SchedulerTask { get; } = new TaskCompletionSource<IScheduler>();
        public QuartzModule(Assembly migrationsAssembly, 
            string nameSpace,
            string instanceId,
            bool isClustered)
        {
            this.migrationsAssembly = migrationsAssembly;
            this.nameSpace = nameSpace;
            this.instanceId = instanceId;
            this.isClustered = isClustered;
        }

        public void Load(IIocRegistry registry)
        {
            DbProvider.RegisterDbMetadata("Npgsql-30", new DbMetadata
            {
                AssemblyName = typeof(NpgsqlConnection).Assembly.FullName,
                BindByName = true,
                ConnectionType = typeof(NpgsqlConnection),
                CommandType = typeof(NpgsqlCommand),
                ParameterType = typeof(NpgsqlParameter),
                ParameterDbType = typeof(NpgsqlDbType),
                ParameterDbTypePropertyName = nameof(NpgsqlDbType),
                ParameterNamePrefix = ":",
                ExceptionType = typeof(NpgsqlException),
                UseParameterNamePrefixInParameterCollection = true
            });

            registry.BindToMethod<IQuartzSettings>(c =>
            {
                var con = c.Get<UnitOfWorkConnectionSettings>();
                return new QuartzSettings(con, instanceId, isClustered);
            });
            registry.BindAsSingleton<ISchedulerFactory, AutofacSchedulerFactory>();
            registry.Bind<IJobFactory, AutofacJobFactory>();
            registry.BindToMethod(() => Scheduler.Result);
        }

        public async Task Init(IServiceLocator serviceLocator, UnderConstructionInfo status)
        {
            var connectionString = serviceLocator.GetInstance<UnitOfWorkConnectionSettings>();
            DatabaseManagement.InitDatabase(connectionString.ConnectionString, "quartz");
            var dbUpgradeSettings = new DbUpgradeSettings(migrationsAssembly, nameSpace);
            DbMigrationsRunner.MigrateToLatest(connectionString.ConnectionString, "quartz", dbUpgradeSettings);
            SchedulerTask.SetResult(await serviceLocator.GetInstance<ISchedulerFactory>().GetScheduler());
        }
    }
}
