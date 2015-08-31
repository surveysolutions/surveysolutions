using System;
using System.Data;
using NHibernate;
using NHibernate.Tool.hbm2ddl;
using Ninject;
using Npgsql;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.Infrastructure.Storage.Postgre.Implementation
{
    internal class PostgresReadSideCleaner : IReadSideCleaner
    {
        private readonly PostgreConnectionSettings connectionSettings;
        private readonly SchemaUpdate schemaUpdate;

        public PostgresReadSideCleaner(PostgreConnectionSettings connectionSettings, SchemaUpdate schemaUpdate)
        {
            this.connectionSettings = connectionSettings;
            this.schemaUpdate = schemaUpdate;
        }

        public void ReCreateViewDatabase()
        {
            using (NpgsqlConnection connection = new Npgsql.NpgsqlConnection(this.connectionSettings.ConnectionString))
            {
                connection.Open();
                var dbCommand = connection.CreateCommand();

                dbCommand.CommandText = "drop schema public cascade;create schema public;";
                dbCommand.ExecuteNonQuery();
            }

            schemaUpdate.Execute(true, true);
        }

        public void CreateIndexesAfterRebuildReadSide()
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(this.connectionSettings.ConnectionString))
            {
                connection.Open();
                NpgsqlCommand dbCommand = connection.CreateCommand();
                dbCommand.AllResultTypesAreUnknown = true;

                dbCommand.CommandText = "SELECT to_regclass('public.userdocuments')";
                var tableExists = dbCommand.ExecuteScalar();

                if (tableExists != DBNull.Value)
                {
                    dbCommand.CommandText = "CREATE UNIQUE INDEX ON userdocuments ((lower(username)));";
                    dbCommand.ExecuteNonQuery();
                }
            }
        }
    }
}