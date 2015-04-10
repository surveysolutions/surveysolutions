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

            using (var openStatelessSession = sessionFactory.OpenStatelessSession())
            {
                IDbConnection dbConnection = openStatelessSession.Connection;
                var dbCommand = dbConnection.CreateCommand();

                dbCommand.CommandText =
                    @"CREATE OR REPLACE FUNCTION DisableAutovacuum()
                    RETURNS VOID
                    AS $$
                    DECLARE
                    my_row    RECORD;    
                    BEGIN       
                    FOR my_row IN 
                        SELECT table_name
                        FROM   information_schema.tables
                        WHERE  table_schema = 'public'
                    LOOP
                    EXECUTE 'ALTER TABLE ' || my_row.table_name || ' SET (autovacuum_enabled = false, toast.autovacuum_enabled = false)';
                    END LOOP;
                    END;
                    $$ LANGUAGE plpgsql;

                    SELECT DisableAutovacuum();
                    DROP FUNCTION DisableAutovacuum();";
                dbCommand.ExecuteNonQuery();
            }
        }

        public void CreateIndexesAfterRebuildReadSide()
        {
            using (var openStatelessSession = sessionFactory.OpenStatelessSession())
            {
                IDbConnection dbConnection = openStatelessSession.Connection;
                var dbCommand = dbConnection.CreateCommand();

                dbCommand.CommandText =
                    @"CREATE OR REPLACE FUNCTION EnableAutovacuum()
                    RETURNS VOID
                    AS $$
                    DECLARE
                    my_row    RECORD;    
                    BEGIN       
                    FOR my_row IN 
                        SELECT table_name
                        FROM   information_schema.tables
                        WHERE  table_schema = 'public'
                    LOOP
                    EXECUTE 'ALTER TABLE ' || my_row.table_name || ' SET (autovacuum_enabled = true, toast.autovacuum_enabled = true)';
                    END LOOP;
                    END;
                    $$ LANGUAGE plpgsql;

                    SELECT EnableAutovacuum();
                    DROP FUNCTION EnableAutovacuum();";
                dbCommand.ExecuteNonQuery();
            }
        }
    }
}