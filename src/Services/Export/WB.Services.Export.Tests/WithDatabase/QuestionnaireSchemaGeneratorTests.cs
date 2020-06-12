using System;
using System.Threading.Tasks;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Npgsql;
using NUnit.Framework;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.InterviewDataStorage;
using WB.Services.Export.InterviewDataStorage.Services;
using WB.Services.Infrastructure;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.Tests.WithDatabase
{
    public class QuestionnaireSchemaGeneratorTests
    {
        [Test]
        public async Task DropTenantSchemaAsyncTest()
        {
            var connectionOptions = Options.Create(new DbConnectionSettings
            {
                DefaultConnection = DatabaseFixture.ConnectionString
            });

            var tenant = new TenantInfo("http://example", Guid.NewGuid().FormatGuid(), "testTenant");

            var ctx = new TenantContext(null, tenant);

            var optionsBuilder = new DbContextOptionsBuilder<TenantDbContext>();
            optionsBuilder.UseNpgsql(connectionOptions.Value.DefaultConnection,
                b => { b.MigrationsHistoryTable("__migrations", tenant.SchemaName());});
            var db = new TenantDbContext(ctx, connectionOptions, optionsBuilder.Options, Mock.Of<ILogger<TenantDbContext>>());
            var generator = new QuestionnaireSchemaGenerator(ctx, db, new DatabaseSchemaCommandBuilder(),
                  new NullLogger<DatabaseSchemaService>());

            using (var tr = db.Database.BeginTransaction())
            {
                generator.CreateQuestionnaireDbStructure(Create.QuestionnaireDocument());
                tr.Commit();
            }

            using (var tr = db.Database.BeginTransaction())
            {
                await generator.DropTenantSchemaAsync(tenant.Name);
                var schemas = await db.Database.GetDbConnection().QueryAsync<string>("select nspname from pg_catalog.pg_namespace");

                Assert.That(schemas, Is.Not.Contain(tenant.SchemaName()));

                tr.Rollback();
            }
        }
    }
}
