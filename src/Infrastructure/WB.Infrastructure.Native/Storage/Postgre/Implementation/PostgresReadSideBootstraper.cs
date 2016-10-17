using Npgsql;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Infrastructure.Native.Storage.Postgre.DbMigrations;

namespace WB.Infrastructure.Native.Storage.Postgre.Implementation
{
    internal class PostgresReadSideBootstraper : IPostgresReadSideBootstraper
    {
        private readonly PostgreConnectionSettings connectionSettings;
        private readonly DbUpgradeSettings dbUpgradeSettings;

        public PostgresReadSideBootstraper(PostgreConnectionSettings connectionSettings, DbUpgradeSettings dbUpgradeSettings)
        {
            this.connectionSettings = connectionSettings;
            this.dbUpgradeSettings = dbUpgradeSettings;
        }

        public void ReCreateViewDatabase()
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(this.connectionSettings.ConnectionString))
            {
                connection.Open();
                NpgsqlConnectionStringBuilder connectionStringBuilder = new NpgsqlConnectionStringBuilder(this.connectionSettings.ConnectionString);
                string schemaName = connectionStringBuilder.SearchPath ?? "public";

                var dbCommand = connection.CreateCommand();
                dbCommand.CommandText = $"drop schema {schemaName} cascade;create schema {schemaName};";
                dbCommand.ExecuteNonQuery();
            }

            DbMigrationsRunner.MigrateToLatest(this.connectionSettings.ConnectionString, this.connectionSettings.SchemaName, this.dbUpgradeSettings);
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