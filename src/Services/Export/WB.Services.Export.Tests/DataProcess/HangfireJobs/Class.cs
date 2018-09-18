using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.PostgreSql;
using Hangfire.Storage;
using Npgsql;
using NUnit.Framework;

namespace WB.Services.Export.Tests.DataProcess.HangfireJobs
{
    public class WithDb
    {
        public static string ConnectionString = "Server=127.0.0.1;Port=5432;User Id=postgres;Password=P@$$w0rd;Database=postgres;CommandTimeout=60";
        public string TestDbConnection;
        public const string TestDbName = "exportservice_tests";
        public string SchemaName;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            CreateDatabaseIfNeeded();

            var builder = new NpgsqlConnectionStringBuilder(ConnectionString)
            {
                Database = TestDbName
            };

            SchemaName = "schema_" + Guid.NewGuid().ToString().Replace("-", "").Substring(0, 6);

            
            TestDbConnection = builder.ConnectionString;
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            ExecuteCommand(TestDbConnection, $"DROP SCHEMA {SchemaName} CASCADE");
        }

        public static void CreateDatabaseIfNeeded()
        {
            var result = ExecuteCommand(ConnectionString, $"SELECT 1 FROM pg_database WHERE datname = '{TestDbName}'");
            if (result == null)
            {
                ExecuteCommand(ConnectionString, $"CREATE DATABASE {TestDbName} ENCODING = 'UTF8'");
            };
        }

        public static object ExecuteCommand(string connectionString, string cmd)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                using (NpgsqlCommand sqlCommand = connection.CreateCommand())
                {
                    sqlCommand.CommandText = cmd;
                    return sqlCommand.ExecuteScalar();
                }
            }
        }
    }

    public class WithHangfireConnection : WithDb
    {
        protected IMonitoringApi MonitoringApi;
        protected IBackgroundJobClient BackgroundJobClient;

        [OneTimeSetUp]
        public void OneTime()
        {
            var options = new PostgreSqlStorageOptions { SchemaName = SchemaName };
            GlobalConfiguration.Configuration.UsePostgreSqlStorage(TestDbConnection, options);
            MonitoringApi = JobStorage.Current.GetMonitoringApi();
            BackgroundJobClient = new BackgroundJobClient(JobStorage.Current);
        }
    }

    public class BasicWithHangfireConnetionTest : WithHangfireConnection
    {
        [Test]
        public void Can_eneque_and_query_job()
        {
            this.BackgroundJobClient.Enqueue(() => Console.WriteLine("Test"));
            var res = MonitoringApi.EnqueuedCount("default");

            Assert.That(res, Is.EqualTo(1));
        }
    }
}
