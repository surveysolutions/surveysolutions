using System;
using System.Data.Common;
using Microsoft.Extensions.Caching.Memory;
using NHibernate;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Infrastructure.Native.Storage.Postgre.Implementation
{
    internal class PostgresPlainKeyValueStorage<TEntity> : PostgresKeyValueStorageWithCache<TEntity>,
        IPlainKeyValueStorage<TEntity>, IDisposable where TEntity : class
    {
        private readonly IUnitOfWork sessionProvider;

        public PostgresPlainKeyValueStorage(IUnitOfWork sessionProvider,
            IMemoryCache memoryCache,
            IEntitySerializer<TEntity> serializer)
            : base(sessionProvider, memoryCache, serializer)
        {
            this.sessionProvider = sessionProvider;
        }

        protected override object ExecuteScalar(DbCommand command)
        {
            var session = this.sessionProvider.Session;
            command.Connection = session.Connection;
            session.GetCurrentTransaction().Enlist(command);
            return command.ExecuteScalar();
        }

        protected override int ExecuteNonQuery(DbCommand command)
        {
            var session = this.sessionProvider.Session;
            command.Connection = session.Connection;
            session.GetCurrentTransaction().Enlist(command);
            return command.ExecuteNonQuery();
        }
    }
}
