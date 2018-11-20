using System;
using System.Data.Common;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Infrastructure.Native.Storage.Postgre.Implementation
{
    internal class PostgresPlainKeyValueStorage<TEntity> : PostgresKeyValueStorageWithCache<TEntity>,
        IPlainKeyValueStorage<TEntity>, IDisposable where TEntity : class
    {
        private readonly IUnitOfWork sessionProvider;

        public PostgresPlainKeyValueStorage(IUnitOfWork sessionProvider, UnitOfWorkConnectionSettings connectionSettings, ILogger logger, IEntitySerializer<TEntity> serializer)
            : base(connectionSettings.ConnectionString, connectionSettings.PlainStorageSchemaName, logger, serializer)
        {
            this.sessionProvider = sessionProvider;
        }

        protected override object ExecuteScalar(DbCommand command)
        {
            var session = this.sessionProvider.Session;
            command.Connection = session.Connection; 
            session.Transaction.Enlist(command);
            return command.ExecuteScalar();
        }

        protected override int ExecuteNonQuery(DbCommand command)
        {
            var session = this.sessionProvider.Session;
            command.Connection = session.Connection;
            session.Transaction.Enlist(command);
            return command.ExecuteNonQuery();
        }
    }
}
