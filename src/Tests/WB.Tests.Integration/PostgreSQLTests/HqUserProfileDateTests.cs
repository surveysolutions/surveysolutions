using System;
using Dapper;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
using Microsoft.Extensions.DependencyInjection;
using NHibernate;
using Npgsql;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Users.Providers;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Infrastructure.Native.Storage.Postgre.DbMigrations;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;
using WB.Persistence.Headquarters.Migrations.Users;

namespace WB.Tests.Integration.PostgreSQLTests;

public class HqUserProfileDateTests : with_postgres_db
{
    private ISessionFactory sessionFactory;

    [OneTimeSetUp]
    public void CreateLegacyProfiles()
    {
        DatabaseManagement.InitDatabase(ConnectionStringBuilder.ConnectionString, "users");
        var connectionString = new NpgsqlConnectionStringBuilder(ConnectionStringBuilder.ConnectionString)
        {
            SearchPath = "users"
        }.ConnectionString;

        using var services = new ServiceCollection()
            .AddLogging()
            .Configure<TypeFilterOptions>(options =>
                options.Namespace = typeof(M001_AddUsersHqIdentityModel).Namespace)
            .AddFluentMigratorCore()
            .ConfigureRunner(builder => builder
                .AddPostgres("users")
                .WithGlobalConnectionString(connectionString)
                .ScanIn(typeof(M001_AddUsersHqIdentityModel).Assembly).For.Migrations())
            .BuildServiceProvider();
        using var scope = services.CreateScope();
        var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
        runner.MigrateUp(202609171200);

        using (var connection = new NpgsqlConnection(connectionString))
        {
            connection.Open();
            connection.Execute(@"
                INSERT INTO users.userprofiles (""Id"", ""DeviceId"", ""DeviceRegistrationDate"", ""AllowRelinkDate"")
                VALUES (98, 'legacy-device', DATE '2026-09-24', TIMESTAMP '2026-09-25 12:34:56'),
                       (99, 'unregistered-device', NULL, NULL);");
        }

        // Keep the existing date column: schema updates would hide the provider compatibility issue.
        sessionFactory = IntegrationCreate.SessionFactory(connectionString,
            "users", new[] { typeof(HqUserProfileMap) }, "users", Array.Empty<Type>(), false);
    }

    [TestCase(98)]
    [TestCase(99)]
    public void ShouldLoadLegacyProfileWithoutSchemaChanges(int id)
    {
        using var session = sessionFactory.OpenSession();
        var profile = session.Load<HqUserProfile>(id);

        Assert.That(profile.DeviceId, Is.EqualTo(id == 98 ? "legacy-device" : "unregistered-device"));
        Assert.That(profile.DeviceRegistrationDate,
            Is.EqualTo(id == 98 ? new DateTime(2026, 9, 24) : (DateTime?)null));
        Assert.That(profile.AllowRelinkDate,
            Is.EqualTo(id == 98 ? new DateTime(2026, 9, 25, 12, 34, 56) : (DateTime?)null));
    }

    [TestCase(true)]
    [TestCase(false)]
    public void ShouldRoundTripRegistrationDate(bool hasRegistrationDate)
    {
        var profile = new HqUserProfile
        {
            DeviceId = "new-device",
            DeviceRegistrationDate = hasRegistrationDate
                ? new DateTime(2026, 9, 24, 13, 45, 56, DateTimeKind.Utc)
                : (DateTime?)null
        };

        using (var session = sessionFactory.OpenSession())
        using (var transaction = session.BeginTransaction())
        {
            session.Save(profile);
            transaction.Commit();
        }

        using (var session = sessionFactory.OpenSession())
        {
            var loaded = session.Get<HqUserProfile>(profile.Id);
            Assert.That(loaded.DeviceRegistrationDate, Is.EqualTo(profile.DeviceRegistrationDate?.Date));
        }
    }

    [Test]
    public void ShouldKeepPostgresDateColumn()
    {
        using var connection = new NpgsqlConnection(ConnectionStringBuilder.ConnectionString);
        connection.Open();
        var dataType = connection.ExecuteScalar<string>(@"
            SELECT data_type FROM information_schema.columns
            WHERE table_schema = 'users' AND table_name = 'userprofiles'
                AND column_name = 'DeviceRegistrationDate';");

        Assert.That(dataType, Is.EqualTo("date"));
    }

    [OneTimeTearDown]
    public void DisposeSessionFactory()
    {
        sessionFactory?.Dispose();
    }
}

