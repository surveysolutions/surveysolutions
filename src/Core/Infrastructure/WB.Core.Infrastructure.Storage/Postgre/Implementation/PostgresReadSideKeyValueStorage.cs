using System;
using System.Data;
using NHibernate;
using Ninject;
using Npgsql;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.Infrastructure.Storage.Postgre.Implementation
{
    internal class PostgresReadSideKeyValueStorage<TEntity> : PostgresKeyValueStorage<TEntity>,
        IReadSideKeyValueStorage<TEntity>, IReadSideRepositoryCleaner, IDisposable
        where TEntity : class, IReadSideRepositoryEntity
    {
        private readonly PostgreConnectionSettings connectionSettings;

        public PostgresReadSideKeyValueStorage([Named(PostgresReadSideModule.SessionProviderName)]ISessionProvider sessionProvider, 
            PostgreConnectionSettings connectionSettings)
            : base(sessionProvider, connectionSettings.ConnectionString)
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