using System;
using System.Collections.Generic;
using System.Data.Common;
using Microsoft.Extensions.Caching.Memory;
using NHibernate;
using Npgsql;
using NpgsqlTypes;
using Polly.Bulkhead;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Infrastructure.Native.Storage.Postgre.Implementation
{
    internal class PostgresReadSideKeyValueStorage<TEntity> 
        : PostgresKeyValueStorageWithCache<TEntity>, IReadSideKeyValueStorage<TEntity>, IDisposable
        where TEntity : class, IReadSideRepositoryEntity
    {
        private readonly IUnitOfWork sessionProvider;

        public PostgresReadSideKeyValueStorage(IUnitOfWork unitOfWork, IMemoryCache memoryCache, IEntitySerializer<TEntity> entitySerializer) 
            : base(unitOfWork, memoryCache, entitySerializer)
        {
            this.sessionProvider = unitOfWork;

            this.EnsureTableExists();
        }

        protected override object ExecuteScalar(DbCommand command)
        {
            this.EnlistInTransaction(command);
            return command.ExecuteScalar();
        }

        protected override int ExecuteNonQuery(DbCommand command)
        {
            this.EnlistInTransaction(command);
            return command.ExecuteNonQuery();
        }

        private void EnlistInTransaction(DbCommand command)
        {
            var session = this.sessionProvider.Session;
            command.Connection = session.Connection;
            session.GetCurrentTransaction().Enlist(command);
        }

        public void BulkStore(List<Tuple<TEntity, string>> bulk)
        {
            base.BulkStore(bulk, this.sessionProvider.Session.Connection as NpgsqlConnection);
        }

        public void Flush()
        {
            this.sessionProvider.Session.Flush();
        }
    }
}
