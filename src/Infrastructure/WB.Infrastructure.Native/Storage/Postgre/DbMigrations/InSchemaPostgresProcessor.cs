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
        public string DefaultSchemaName { get; }
         
        public InSchemaPostgresProcessor(string defaultSchemaName, 
            PostgresDbFactory factory, 
            PostgresGenerator generator,
            ILogger<PostgresProcessor> logger, 
            IOptionsSnapshot<ProcessorOptions> options,
            IConnectionStringAccessor connectionStringAccessor)
            : base(factory, generator, logger, options, connectionStringAccessor, new PostgresOptions())
        {
            this.DefaultSchemaName = defaultSchemaName;
        }

        public override DataSet ReadTableData(string schemaName, string tableName) =>
            base.ReadTableData(schemaName ?? this.DefaultSchemaName, tableName);

        public override bool ColumnExists(string schemaName, string tableName, string columnName) =>
            base.ColumnExists(schemaName ?? this.DefaultSchemaName, tableName, columnName);

        public override bool TableExists(string schemaName, string tableName) =>
            base.TableExists(schemaName ?? this.DefaultSchemaName, tableName);

        public override bool SequenceExists(string schemaName, string sequenceName) =>
            base.SequenceExists(schemaName ?? this.DefaultSchemaName, sequenceName);

        public override bool ConstraintExists(string schemaName, string tableName, string constraintName) =>
            base.ConstraintExists(schemaName ?? this.DefaultSchemaName, tableName, constraintName);

        public override bool IndexExists(string schemaName, string tableName, string indexName) =>
            base.IndexExists(schemaName ?? this.DefaultSchemaName, tableName, indexName);

        public override bool DefaultValueExists(string schemaName, string tableName, string columnName, object defaultValue) => 
            base.DefaultValueExists(schemaName ?? this.DefaultSchemaName, tableName, columnName, defaultValue);
    }
}
