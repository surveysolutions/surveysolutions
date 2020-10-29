﻿using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Generators.Postgres;
using FluentMigrator.Runner.Processors.Postgres;

namespace WB.Infrastructure.Native.Storage.Postgre.DbMigrations
{
    public class InSchemaPostgresGenerator : PostgresGenerator
    {
        private readonly string schemaName;

        public InSchemaPostgresGenerator(string schemaName) : base(new PostgresQuoter(new PostgresOptions()))
        {
            this.schemaName = schemaName;
        }

        public override string Generate(AlterColumnExpression expression)
        {
            expression.SchemaName = this.schemaName;
            return base.Generate(expression);
        }

        public override string Generate(AlterDefaultConstraintExpression expression)
        {
            expression.SchemaName = this.schemaName;
            return base.Generate(expression);
        }

        public override string Generate(AlterTableExpression expression)
        {
            expression.SchemaName = this.schemaName;
            return base.Generate(expression);
        }

        public override string Generate(CreateColumnExpression expression)
        {
            expression.SchemaName = this.schemaName;
            return base.Generate(expression);
        }

        public override string Generate(CreateConstraintExpression expression)
        {
            expression.Constraint.SchemaName = this.schemaName;
            return base.Generate(expression);
        }

        public override string Generate(CreateForeignKeyExpression expression)
        {
            expression.ForeignKey.ForeignTableSchema = this.schemaName;
            if (expression.ForeignKey.PrimaryTableSchema == null)
                expression.ForeignKey.PrimaryTableSchema = this.schemaName;
            return base.Generate(expression);
        }

        public override string Generate(CreateIndexExpression expression)
        {
            expression.Index.SchemaName = this.schemaName;
            return base.Generate(expression);
        }

        public override string Generate(CreateSequenceExpression expression)
        {
            expression.Sequence.SchemaName = this.schemaName;
            return base.Generate(expression);
        }

        public override string Generate(CreateTableExpression expression)
        {
            expression.SchemaName = this.schemaName;
            return base.Generate(expression);
        }

        public override string Generate(DeleteColumnExpression expression)
        {
            expression.SchemaName = this.schemaName;
            return base.Generate(expression);
        }

        public override string Generate(DeleteConstraintExpression expression)
        {
            expression.Constraint.SchemaName = this.schemaName;
            return base.Generate(expression);
        }

        public override string Generate(DeleteDataExpression expression)
        {
            expression.SchemaName = this.schemaName;
            return base.Generate(expression);
        }

        public override string Generate(DeleteDefaultConstraintExpression expression)
        {
            expression.SchemaName = this.schemaName;
            return base.Generate(expression);
        }

        public override string Generate(DeleteForeignKeyExpression expression)
        {
            expression.ForeignKey.ForeignTableSchema = this.schemaName;
            if (expression.ForeignKey.PrimaryTableSchema == null)
                expression.ForeignKey.PrimaryTableSchema = this.schemaName;
            return base.Generate(expression);
        }

        public override string Generate(DeleteIndexExpression expression)
        {
            expression.Index.SchemaName = this.schemaName;
            return base.Generate(expression);
        }

        public override string Generate(DeleteSequenceExpression expression)
        {
            expression.SchemaName = this.schemaName;
            return base.Generate(expression);
        }

        public override string Generate(DeleteTableExpression expression)
        {
            expression.SchemaName = this.schemaName;
            return base.Generate(expression);
        }

        public override string Generate(InsertDataExpression expression)
        {
            expression.SchemaName = this.schemaName;
            return base.Generate(expression);
        }

        public override string Generate(RenameColumnExpression expression)
        {
            expression.SchemaName = this.schemaName;
            return base.Generate(expression);
        }

        public override string Generate(RenameTableExpression expression)
        {
            expression.SchemaName = this.schemaName;
            return base.Generate(expression);
        }

        public override string Generate(UpdateDataExpression expression)
        {
            expression.SchemaName = this.schemaName;
            return base.Generate(expression);
        }

        public override string GetUniqueString(CreateIndexExpression column)
        {
            column.Index.SchemaName = this.schemaName;
            return base.GetUniqueString(column);
        }

        public override string GenerateForeignKeyName(ForeignKeyDefinition foreignKey)
        {
            foreignKey.ForeignTableSchema = this.schemaName;
            if (foreignKey.PrimaryTableSchema == null)
                foreignKey.PrimaryTableSchema = this.schemaName;
            return base.GenerateForeignKeyName(foreignKey);
        }

        public override string GetClusterTypeString(CreateIndexExpression column)
        {
            column.Index.SchemaName = this.schemaName;
            return base.GetClusterTypeString(column);
        }
    }
}
