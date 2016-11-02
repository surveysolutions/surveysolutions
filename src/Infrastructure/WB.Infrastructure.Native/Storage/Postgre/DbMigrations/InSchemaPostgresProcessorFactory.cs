using FluentMigrator;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Generators.Postgres;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.Postgres;

namespace WB.Infrastructure.Native.Storage.Postgre.DbMigrations
{
    public class InSchemaPostgresProcessorFactory : MigrationProcessorFactory
    {
        public string SchemaName { get; }

        public InSchemaPostgresProcessorFactory(string schemaName)
        {
            this.SchemaName = schemaName;
        }

        public override IMigrationProcessor Create(string connectionString, IAnnouncer announcer, IMigrationProcessorOptions options)
        {
            PostgresDbFactory postgresDbFactory = new PostgresDbFactory();
            return new InSchemaPostgresProcessor(
                postgresDbFactory.CreateConnection(connectionString),
                SchemaName, 
                new PostgresGenerator(), 
                announcer, 
                options, 
                postgresDbFactory);
        }
    }
}