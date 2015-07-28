using System.Data;
using Microsoft.Isam.Esent.Interop;
using NHibernate;
using NHibernate.Tool.hbm2ddl;
using Ninject;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.Infrastructure.Storage.Postgre.Implementation
{
    internal class PostgresReadSideCleaner : IReadSideCleaner
    {
        private readonly ISessionFactory sessionFactory;
        private readonly SchemaUpdate schemaUpdate;

        public PostgresReadSideCleaner([Named(PostgresReadSideModule.ReadSideSessionFactoryName)] ISessionFactory sessionFactory,
            SchemaUpdate schemaUpdate)
        {
            this.sessionFactory = sessionFactory;
            this.schemaUpdate = schemaUpdate;
        }

        public void ReCreateViewDatabase()
        {
            using (var openStatelessSession = sessionFactory.OpenStatelessSession())
            {
                IDbConnection dbConnection = openStatelessSession.Connection;
                var dbCommand = dbConnection.CreateCommand();

                dbCommand.CommandText = "drop schema public cascade;create schema public;";
                dbCommand.ExecuteNonQuery();
            }

            schemaUpdate.Execute(true, true);
        }

        public void CreateIndexesAfterRebuildReadSide()
        {
        }
    }
}