using System;
using System.Data;
using Npgsql;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.Infrastructure.Storage.Postgre.Implementation
{
    internal class PostgresPlainKeyValueStorage<TEntity> : PostgresKeyValueStorage<TEntity>,
        IPlainKeyValueStorage<TEntity>, IDisposable
        where TEntity : class
    {
        private readonly PostgresPlainStorageSettings connectionSettings;

        public PostgresPlainKeyValueStorage(IPlainSessionProvider plainSessionProvider, PostgresPlainStorageSettings connectionSettings)
            : base(connectionSettings.ConnectionString)
        {
            this.connectionSettings = connectionSettings;
        }


        protected override object ExecuteScalar(IDbCommand command)
        {
            using (var connection = new NpgsqlConnection(connectionSettings.ConnectionString))
            {
                connection.Open();
                command.Connection = connection;
                return command.ExecuteScalar();
            }
        }

        protected override int ExecuteNonQuery(IDbCommand command)
        {
            using (var connection = new NpgsqlConnection(connectionSettings.ConnectionString))
            {
                connection.Open();
                command.Connection = connection;
                return command.ExecuteNonQuery();
            }
        }
    }
}