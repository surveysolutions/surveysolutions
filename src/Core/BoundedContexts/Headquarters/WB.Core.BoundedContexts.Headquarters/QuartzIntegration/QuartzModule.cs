using System.Reflection;
using System.Threading.Tasks;
using Npgsql;
using NpgsqlTypes;
using Quartz;
using Quartz.Impl.AdoJobStore.Common;
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

        public QuartzModule(Assembly migrationsAssembly, string nameSpace)
        {
            this.migrationsAssembly = migrationsAssembly;
            this.nameSpace = nameSpace;
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

            registry.Bind<IQuartzSettings, QuartzSettings>();
            registry.BindAsSingleton<ISchedulerFactory, AutofacSchedulerFactory>();
            registry.BindToMethodInSingletonScope<IScheduler>(ctx => ctx.Get<ISchedulerFactory>().GetScheduler().Result);
        }

        public Task Init(IServiceLocator serviceLocator, UnderConstructionInfo status)
        {
            var connectionString = serviceLocator.GetInstance<UnitOfWorkConnectionSettings>();

            DatabaseManagement.InitDatabase(connectionString.ConnectionString, "quartz");
            var dbUpgradeSettings = new DbUpgradeSettings(migrationsAssembly, nameSpace);
            DbMigrationsRunner.MigrateToLatest(connectionString.ConnectionString, "quartz", dbUpgradeSettings);

            return Task.CompletedTask;
        }
    }
}
