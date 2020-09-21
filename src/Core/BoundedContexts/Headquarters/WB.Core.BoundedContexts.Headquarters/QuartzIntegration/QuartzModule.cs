using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;
using Quartz;
using Quartz.Impl.AdoJobStore.Common;
using Quartz.Spi;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.BoundedContexts.Headquarters.Invitations;
using WB.Core.BoundedContexts.Headquarters.Questionnaires.Jobs;
using WB.Core.BoundedContexts.Headquarters.Synchronization.Schedulers.InterviewDetailsDataScheduler;
using WB.Core.BoundedContexts.Headquarters.Users.UserPreloading.Tasks;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Modularity;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Infrastructure.Native.Storage.Postgre.DbMigrations;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;

namespace WB.Core.BoundedContexts.Headquarters.QuartzIntegration
{
    public class QuartzModule : IModule, IInitModule
    {
        private readonly Assembly migrationsAssembly;
        private readonly string nameSpace;
        private readonly string instanceId;
        private readonly bool isClustered;

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
            registry.BindToMethodInSingletonScope<IScheduler>(ctx => ctx.Get<ISchedulerFactory>().GetScheduler().Result);
        }

        public async Task Init(IServiceLocator serviceLocator, UnderConstructionInfo status)
        {
            var connectionString = serviceLocator.GetInstance<UnitOfWorkConnectionSettings>();
            DatabaseManagement.InitDatabase(connectionString.ConnectionString, "quartz");
            var dbUpgradeSettings = new DbUpgradeSettings(migrationsAssembly, nameSpace);
            DbMigrationsRunner.MigrateToLatest(connectionString.ConnectionString, "quartz", dbUpgradeSettings,
                serviceLocator.GetInstance<ILoggerProvider>());

            await serviceLocator.GetInstance<InterviewDetailsBackgroundSchedulerTask>().Configure();
            await serviceLocator.GetInstance<UsersImportTask>().ScheduleRunAsync();
            await serviceLocator.GetInstance<AssignmentsImportTask>().Schedule(repeatIntervalInSeconds: 300);
            await serviceLocator.GetInstance<AssignmentsVerificationTask>().Schedule(repeatIntervalInSeconds: 300);
            await serviceLocator.GetInstance<DeleteQuestionnaireJobScheduler>().Schedule(repeatIntervalInSeconds: 10);
            await serviceLocator.GetInstance<UpgradeAssignmentJobScheduler>().Configure();
            await serviceLocator.GetInstance<SendInvitationsTask>().ScheduleRunAsync();
            await serviceLocator.GetInstance<SendRemindersTask>().Schedule(repeatIntervalInSeconds: 60 * 60);
            await serviceLocator.GetInstance<SendInterviewCompletedTask>().Schedule(repeatIntervalInSeconds: 10);
        }
    }
}
