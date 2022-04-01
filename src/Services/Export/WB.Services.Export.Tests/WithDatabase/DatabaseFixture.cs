using System;
using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using NUnit.Framework;

namespace WB.Services.Export.Tests.WithDatabase
{
    [SetUpFixture]
    public class DatabaseFixture
    {
        public const string DataBaseName = "export_service_tests";
        public static string TenantName = "schema_" + Guid.NewGuid().ToString().Replace("-", "");

        [OneTimeSetUp]
        public void Setup()
        {
            //legacy
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            
            var builder = new NpgsqlConnectionStringBuilder(TestConfig.GetConnectionString());
            builder.Database = "postgres";

            using var connection = new NpgsqlConnection(builder.ConnectionString);
            connection.Open();
            var checkDbExistsCommand = connection.CreateCommand();
            checkDbExistsCommand.CommandText = 
                "SELECT 1 FROM pg_catalog.pg_database WHERE lower(datname) = lower(:dbName);";
            checkDbExistsCommand.Parameters.AddWithValue("dbName", DataBaseName);
            var dbExists = checkDbExistsCommand.ExecuteScalar();

            if (dbExists == null)
            {
                var createCommand = connection.CreateCommand();
                createCommand.CommandText = $@"CREATE DATABASE ""{DataBaseName}"" ENCODING = 'UTF8'";
                // unfortunately there is no way to use parameters based syntax here 
                createCommand.ExecuteNonQuery();
            }
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            using var db = new NpgsqlConnection(TestConfig.GetConnectionString());
            db.Open();
            db.Execute($"DROP SCHEMA if exists " + TenantName + " CASCADE");
        }

    }
}
