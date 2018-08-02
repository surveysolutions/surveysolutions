using System.Data;
using FluentMigrator.Runner.Generators.Postgres;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.Postgres;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace WB.Infrastructure.Native.Storage.Postgre.DbMigrations
{
    public class InSchemaPostgresProcessor : PostgresProcessor
    {
        public string SchemaName { get; }

        public InSchemaPostgresProcessor(string schemaName, 
            PostgresDbFactory factory, 
            PostgresGenerator generator,
            ILogger<PostgresProcessor> logger, 
            IOptionsSnapshot<ProcessorOptions> options,
            IConnectionStringAccessor connectionStringAccessor)
            : base(factory, generator, logger, options, connectionStringAccessor)
        {
            this.SchemaName = schemaName;
        }

        public override DataSet ReadTableData(string schemaName, string tableName) =>
            base.ReadTableData(this.SchemaName, tableName);

        public override bool ColumnExists(string schemaName, string tableName, string columnName) =>
            base.ColumnExists(this.SchemaName, tableName, columnName);

        public override bool TableExists(string schemaName, string tableName) =>
            base.TableExists(this.SchemaName, tableName);

        public override bool SequenceExists(string schemaName, string sequenceName) =>
            base.SequenceExists(this.SchemaName, sequenceName);

        public override bool ConstraintExists(string schemaName, string tableName, string constraintName) =>
            base.ConstraintExists(this.SchemaName, tableName, constraintName);

        public override bool IndexExists(string schemaName, string tableName, string indexName) =>
            base.IndexExists(this.SchemaName, tableName, indexName);

        public override bool DefaultValueExists(string schemaName, string tableName, string columnName,
            object defaultValue) => base.DefaultValueExists(this.SchemaName, tableName, columnName, defaultValue);
    }
}
