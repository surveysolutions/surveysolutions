using System;
using System.Collections.Generic;
using System.Configuration;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using NHibernate;
using Npgsql;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer;
using WB.Core.BoundedContexts.Designer.Mappings;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Infrastructure.Native.Logging;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Infrastructure.Native.Storage.Postgre.DbMigrations;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;
using WB.UI.Designer;
using WB.UI.Shared.Web.Kernel;
using WB.UI.Shared.Web.Versions;

namespace WB.Tests.Integration.QuestionnaireSearchStorageTests
{
    public class IntegrationTest
    {
        private string connectionString;
        protected IUnitOfWork UnitOfWork;
        protected ISessionFactory sessionFactory;

        [OneTimeSetUp]
        public async Task InitEnvironment()
        {
            var TestConnectionString = ConfigurationManager.ConnectionStrings["TestConnection"].ConnectionString;
            var databaseName = "testdb_" + Guid.NewGuid().FormatGuid();
            var connectionStringBuilder = new NpgsqlConnectionStringBuilder(TestConnectionString)
            {
                Database = databaseName
            };

            this.connectionString = connectionStringBuilder.ConnectionString;


            using (var connection = new NpgsqlConnection(TestConnectionString))
            {
                connection.Open();
                var command = $"CREATE DATABASE {databaseName} ENCODING = 'UTF8'";
                using (var sqlCommand = connection.CreateCommand())
                {
                    sqlCommand.CommandText = command;
                    sqlCommand.ExecuteNonQuery();
                }

                connection.Close();
            }

            var ormSettings = new UnitOfWorkConnectionSettings()
            {
                ConnectionString = this.connectionString,
                PlainMappingAssemblies = new List<Assembly>
                {
                    typeof(DesignerBoundedContextModule).Assembly,
                    typeof(ProductVersionModule).Assembly,
                },
                PlainStorageSchemaName = "plainstore",
                ReadSideMappingAssemblies = new List<Assembly>(),
                PlainStoreUpgradeSettings = new DbUpgradeSettings(
                    typeof(WB.UI.Designer.Migrations.PlainStore.M001_Init).Assembly,
                    typeof(WB.UI.Designer.Migrations.PlainStore.M001_Init).Namespace)
            };

            var plainStorageSchemaName = ormSettings.PlainStorageSchemaName;

            var autofacKernel = new AutofacWebKernel();
            autofacKernel.Load(
                new OrmModule(ormSettings),
                new NLogLoggingModule(),
                new DesignerWebModule()
            );
            await autofacKernel.InitAsync();

            DatabaseManagement.InitDatabase(ormSettings.ConnectionString, plainStorageSchemaName);
            DbMigrationsRunner.MigrateToLatest(ormSettings.ConnectionString, plainStorageSchemaName,
                ormSettings.PlainStoreUpgradeSettings);

            sessionFactory = IntegrationCreate.SessionFactory(this.connectionString,
                new List<Type>
                {
                    typeof(QuestionnaireListViewItemMap),
                    typeof(QuestionnaireListViewFolderMap),
                },
                true,
                plainStorageSchemaName);

            UnitOfWork = IntegrationCreate.UnitOfWork(sessionFactory);
        }

        [OneTimeTearDown]
        public void DropEnvironment()
        {
            DropDb(connectionString);
        }

        public static void DropDb(string connectionString)
        {
            NpgsqlConnectionStringBuilder builder = new NpgsqlConnectionStringBuilder(connectionString);
            var dbToDelete = builder.Database;
            builder.Database = "postgres";
            using (var connection = new NpgsqlConnection(builder.ConnectionString))
            {
                connection.Open();
                using (var sqlCommand = connection.CreateCommand())
                {
                    sqlCommand.CommandText =
                        string.Format(
                            @"SELECT pg_terminate_backend (pg_stat_activity.pid) FROM pg_stat_activity WHERE pg_stat_activity.datname = '{0}'; DROP DATABASE {0};",
                            dbToDelete);
                    ;
                    sqlCommand.ExecuteNonQuery();
                }
            }
        }

        protected void RunActionInScope(Action<IServiceLocator> action)
        {
            var lifetimeScope = ServiceLocator.Current.GetInstance<ILifetimeScope>();
            using (var scope = lifetimeScope.BeginLifetimeScope())
            {
                var serviceLocatorLocal = scope.Resolve<IServiceLocator>();

                action(serviceLocatorLocal);

                serviceLocatorLocal.GetInstance<IUnitOfWork>().AcceptChanges();
            }
        }
    }
}
