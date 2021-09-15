using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Classifications;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Search;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.DependencyInjection;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Infrastructure.Native.Storage.Postgre.DbMigrations;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;
using WB.UI.Designer.Migrations.PlainStore;

namespace WB.Tests.Integration.Core
{
    public class IntegrationTest
    {
        private IServiceLocator serviceLocator;

        [SetUp]
        public void InitEnvironment()
        {
            var connectionString = $"Server=127.0.0.1;Port=5432;User Id=postgres;Password=P@$$w0rd;Database={"testdb_" + Guid.NewGuid().ToString("N")};";

            var migration = typeof(M001_Init);
            var schemaName = "plainstore";

            DatabaseManagement.InitDatabase(connectionString, schemaName);
            var dbUpgradeSettings = new DbUpgradeSettings(migration.Assembly, migration.Namespace);
            DbMigrationsRunner.MigrateToLatest(connectionString, schemaName, dbUpgradeSettings);

            var serviceProvider = new ServiceCollection()
                .AddTransient<IQuestionnaireSearchStorage, QuestionnaireSearchStorage>()
                .AddTransient<IClassificationsStorage, ClassificationsStorage>()
                .AddDbContext<DesignerDbContext>(options =>
                    options.UseNpgsql(connectionString))
                .BuildServiceProvider();
            this.serviceLocator = new DotNetCoreServiceLocatorAdapter(serviceProvider);
        }

        [TearDown]
        public void DropEnvironment() => 
            this.serviceLocator.GetInstance<DesignerDbContext>().Database.EnsureDeleted();

        protected void RunActionInScope(Action<IServiceLocator> action)
        {
            var dbContext = serviceLocator.GetInstance<DesignerDbContext>();
            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    action(serviceLocator);
                    transaction.Commit();
                }
                catch(Exception e)
                {
                    transaction.Rollback();
                }
            }
        }
            

        protected async Task RunActionInScopeAsync(Func<IServiceLocator, Task> action)
        {
            var dbContext = serviceLocator.GetInstance<DesignerDbContext>();
            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    await action(serviceLocator).ConfigureAwait(false);
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                }
            }
        }
    }
}
