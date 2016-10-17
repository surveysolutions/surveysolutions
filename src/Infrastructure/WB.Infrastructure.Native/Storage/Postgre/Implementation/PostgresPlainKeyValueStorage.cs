using System;
using System.Data;
using System.Data.SqlClient;
using NHibernate;
using Npgsql;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;

namespace WB.Infrastructure.Native.Storage.Postgre.Implementation
{
    internal class PostgresPlainKeyValueStorage<TEntity> : PostgresKeyValueStorageWithCache<TEntity>,
        IPlainKeyValueStorage<TEntity>, IDisposable where TEntity : class
    {
        private readonly IPlainSessionProvider sessionProvider;

        public PostgresPlainKeyValueStorage(IPlainSessionProvider sessionProvider, PostgresPlainStorageSettings connectionSettings, ILogger logger)
            : base(connectionSettings.ConnectionString, connectionSettings.SchemaName, logger)
        {
            this.sessionProvider = sessionProvider;
        }

        protected override object ExecuteScalar(IDbCommand command)
        {
            var session = this.sessionProvider.GetSession();
            command.Connection = session.Connection; 
            session.Transaction.Enlist(command);
            return command.ExecuteScalar();
        }

        protected override int ExecuteNonQuery(IDbCommand command)
        {
            var session = this.sessionProvider.GetSession();
            command.Connection = session.Connection;
            session.Transaction.Enlist(command);
            return command.ExecuteNonQuery();
        }
    }
}