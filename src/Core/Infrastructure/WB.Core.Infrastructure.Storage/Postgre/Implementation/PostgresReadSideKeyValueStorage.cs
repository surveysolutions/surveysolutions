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
        private readonly ISessionProvider sessionProvider;

        public PostgresReadSideKeyValueStorage([Named(PostgresReadSideModule.SessionProviderName)]ISessionProvider sessionProvider, 
            PostgreConnectionSettings connectionSettings)
            : base(sessionProvider, connectionSettings.ConnectionString)
        {
            this.sessionProvider = sessionProvider;
        }

        protected override object ExecuteScalar(IDbCommand command)
        {
            var session = sessionProvider.GetSession();
            command.Connection = session.Connection;
            session.Transaction.Enlist(command);
            return command.ExecuteScalar();
        }

        protected override int ExecuteNonQuery(IDbCommand command)
        {
            var session = sessionProvider.GetSession();
            command.Connection = session.Connection;
            session.Transaction.Enlist(command);
            return command.ExecuteNonQuery();
        }
    }
}