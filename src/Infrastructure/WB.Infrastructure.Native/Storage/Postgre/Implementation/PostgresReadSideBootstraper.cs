using System;
using System.Reflection;
using Npgsql;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Infrastructure.Native.Storage.Postgre.DbMigrations;

namespace WB.Infrastructure.Native.Storage.Postgre.Implementation
{
    internal class PostgresReadSideBootstraper : IPostgresReadSideBootstraper
    {
        private readonly PostgreConnectionSettings connectionSettings;
        private readonly Assembly migrationsAssembly;

        public PostgresReadSideBootstraper(PostgreConnectionSettings connectionSettings, Assembly migrationsAssembly)
        {
            this.connectionSettings = connectionSettings;
            this.migrationsAssembly = migrationsAssembly;
        }

        public void ReCreateViewDatabase()
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(this.connectionSettings.ConnectionString))
            {
                connection.Open();
                var dbCommand = connection.CreateCommand();

                dbCommand.CommandText = "drop schema public cascade;create schema public;";
                dbCommand.ExecuteNonQuery();
            }

            DbMigrationsRunner.MigrateToLatest(this.connectionSettings.ConnectionString, this.migrationsAssembly);
        }

        public bool CheckDatabaseConnection()
        {
            var builder = new NpgsqlConnectionStringBuilder(this.connectionSettings.ConnectionString);
            builder.Database = "postgres"; // System DB name.

            using (var connection = new NpgsqlConnection(builder.ConnectionString))
            {
                connection.Open();
                return connection.State == System.Data.ConnectionState.Open;
            }
        }
    }
}