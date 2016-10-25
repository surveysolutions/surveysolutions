using System.Data;
using FluentMigrator;
using FluentMigrator.Builders.Execute;
using FluentMigrator.Expressions;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.Postgres;

namespace WB.Infrastructure.Native.Storage.Postgre.DbMigrations
{
    public class InSchemaPostgresProcessor : PostgresProcessor
    {
        public string SchemaName { get; }

        public InSchemaPostgresProcessor(IDbConnection connection, string schemaName, IMigrationGenerator generator, IAnnouncer announcer, IMigrationProcessorOptions options, IDbFactory factory)
            : base(connection, generator, announcer, options, factory)
        {
            this.SchemaName = schemaName;
        }

        public override void Process(CreateSchemaExpression expression)
        {
            expression.SchemaName = this.SchemaName;
            base.Process(expression);
        }

        public override void Process(DeleteSchemaExpression expression)
        {
            expression.SchemaName = this.SchemaName;
            base.Process(expression);
        }

        public override void Process(CreateTableExpression expression)
        {
            expression.SchemaName = this.SchemaName;
            base.Process(expression);
        }

        public override void Process(AlterTableExpression expression)
        {
            expression.SchemaName = this.SchemaName;
            base.Process(expression);
        }

        public override void Process(AlterColumnExpression expression)
        {
            expression.SchemaName = this.SchemaName;
            base.Process(expression);
        }

        public override void Process(CreateColumnExpression expression)
        {
            expression.SchemaName = this.SchemaName;
            base.Process(expression);
        }

        public override void Process(DeleteTableExpression expression)
        {
            expression.SchemaName = this.SchemaName;
            base.Process(expression);
        }

        public override void Process(DeleteColumnExpression expression)
        {
            expression.SchemaName = this.SchemaName;
            base.Process(expression);
        }

        public override void Process(CreateForeignKeyExpression expression)
        {
            expression.ForeignKey.ForeignTableSchema = this.SchemaName;
            expression.ForeignKey.PrimaryTableSchema = this.SchemaName;
            base.Process(expression);
        }

        public override void Process(DeleteForeignKeyExpression expression)
        {
            expression.ForeignKey.ForeignTableSchema = this.SchemaName;
            expression.ForeignKey.PrimaryTableSchema = this.SchemaName;
            base.Process(expression);
        }

        public override void Process(CreateIndexExpression expression)
        {
            expression.Index.SchemaName = this.SchemaName;
            base.Process(expression);
        }

        public override void Process(DeleteIndexExpression expression)
        {
            expression.Index.SchemaName = this.SchemaName;
            base.Process(expression);
        }

        public override void Process(RenameTableExpression expression)
        {
            expression.SchemaName = this.SchemaName;
            base.Process(expression);
        }

        public override void Process(RenameColumnExpression expression)
        {
            expression.SchemaName = this.SchemaName;
            base.Process(expression);
        }

        public override void Process(InsertDataExpression expression)
        {
            expression.SchemaName = this.SchemaName;
            base.Process(expression);
        }

        public override void Process(DeleteDataExpression expression)
        {
            expression.SchemaName = this.SchemaName;
            base.Process(expression);
        }

        public override void Process(AlterDefaultConstraintExpression expression)
        {
            expression.SchemaName = this.SchemaName;
            base.Process(expression);
        }

        public override void Process(UpdateDataExpression expression)
        {
            expression.SchemaName = this.SchemaName;
            base.Process(expression);
        }

        public override void Process(AlterSchemaExpression expression)
        {
            expression.SourceSchemaName = this.SchemaName;
            base.Process(expression);
        }

        public override void Process(CreateSequenceExpression expression)
        {
            expression.Sequence.SchemaName = this.SchemaName;
            base.Process(expression);
        }

        public override void Process(DeleteSequenceExpression expression)
        {
            expression.SchemaName = this.SchemaName;
            base.Process(expression);
        }

        public override void Process(CreateConstraintExpression expression)
        {
            expression.Constraint.SchemaName = this.SchemaName;
            base.Process(expression);
        }

        public override void Process(DeleteConstraintExpression expression)
        {
            expression.Constraint.SchemaName = this.SchemaName;
            base.Process(expression);
        }

        public override void Process(DeleteDefaultConstraintExpression expression)
        {
            expression.SchemaName = this.SchemaName;
            base.Process(expression);
        }

        public override DataSet ReadTableData(string schemaName, string tableName)
        {
            return base.ReadTableData(this.SchemaName, tableName);
        }

        public override bool ColumnExists(string schemaName, string tableName, string columnName)
        {
            return base.ColumnExists(this.SchemaName, tableName, columnName);
        }

        public override bool TableExists(string schemaName, string tableName)
        {
            return base.TableExists(this.SchemaName, tableName);
        }

        public override bool SequenceExists(string schemaName, string sequenceName)
        {
            return base.SequenceExists(this.SchemaName, sequenceName);
        }

        public override bool ConstraintExists(string schemaName, string tableName, string constraintName)
        {
            return base.ConstraintExists(this.SchemaName, tableName, constraintName);
        }

        public override bool IndexExists(string schemaName, string tableName, string indexName)
        {
            return base.IndexExists(this.SchemaName, tableName, indexName);
        }

        public override bool DefaultValueExists(string schemaName, string tableName, string columnName, object defaultValue)
        {
            return base.DefaultValueExists(this.SchemaName, tableName, columnName, defaultValue);
        }
    }
}