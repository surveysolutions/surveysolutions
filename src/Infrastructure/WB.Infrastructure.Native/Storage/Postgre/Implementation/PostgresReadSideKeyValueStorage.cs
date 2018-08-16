using System;
using System.Data.Common;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Infrastructure.Native.Storage.Postgre.Implementation
{
    internal class PostgresReadSideKeyValueStorage<TEntity> : PostgresKeyValueStorageWithCache<TEntity>,
        IReadSideKeyValueStorage<TEntity>, IDisposable
        where TEntity : class, IReadSideRepositoryEntity
    {
        private readonly IUnitOfWork sessionProvider;

        public PostgresReadSideKeyValueStorage(IUnitOfWork unitOfWork, 
            UnitOfWorkConnectionSettings connectionSettings, ILogger logger, IEntitySerializer<TEntity> entitySerializer) 
            : base(connectionSettings.ConnectionString, connectionSettings.ReadSideSchemaName, logger, entitySerializer)
        {
            this.sessionProvider = unitOfWork;

            this.EnshureTableExists();
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
            session.Transaction.Enlist(command);
        }

        public void Flush()
        {
            this.sessionProvider.Session.Flush();
        }
    }
}
