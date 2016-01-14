using System;
using System.Data;
using Npgsql;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.Infrastructure.Storage.Postgre.Implementation
{
    internal class PostgresPlainKeyValueStorage<TEntity> : PostgresKeyValueStorageWithCache<TEntity>,
        IPlainKeyValueStorage<TEntity>, IDisposable
        where TEntity : class
    {
        private readonly PostgresPlainStorageSettings connectionSettings;

        public PostgresPlainKeyValueStorage(IPlainSessionProvider plainSessionProvider, PostgresPlainStorageSettings connectionSettings, ILogger logger)
            : base(connectionSettings.ConnectionString, logger)
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