using FluentMigrator.Expressions;

namespace WB.Infrastructure.Native.Storage.Postgre.DbMigrations
{
    public class InSchemaPostgresGenerator : FluentMigrator.Runner.Generators.Postgres.PostgresGenerator
    {
        public string SchemaName { get; }

        public InSchemaPostgresGenerator(string schemaName)
        {
            this.SchemaName = schemaName;
        }

        public override string Generate(CreateSchemaExpression expression)
        {
            expression.SchemaName = this.SchemaName;
            return base.Generate(expression);
        }

        public override string Generate(DeleteSchemaExpression expression)
        {
            expression.SchemaName = this.SchemaName;
            return base.Generate(expression);
        }

        public override string Generate(CreateTableExpression expression)
        {
            expression.SchemaName = this.SchemaName;
            return base.Generate(expression);
        }

        public override string Generate(AlterColumnExpression expression)
        {
            expression.SchemaName = this.SchemaName;
            return base.Generate(expression);
        }

        public override string Generate(CreateColumnExpression expression)
        {
            expression.SchemaName = this.SchemaName;
            return base.Generate(expression);
        }

        public override string Generate(DeleteTableExpression expression)
        {
            expression.SchemaName = this.SchemaName;
            return base.Generate(expression);
        }

        public override string Generate(DeleteColumnExpression expression)
        {
            expression.SchemaName = this.SchemaName;
            return base.Generate(expression);
        }

        public override string Generate(CreateForeignKeyExpression expression)
        {
            expression.ForeignKey.ForeignTableSchema = this.SchemaName;
            expression.ForeignKey.PrimaryTableSchema = this.SchemaName;
            return base.Generate(expression);
        }

        public override string Generate(DeleteForeignKeyExpression expression)
        {
            expression.ForeignKey.ForeignTableSchema = this.SchemaName;
            expression.ForeignKey.PrimaryTableSchema = this.SchemaName;
            return base.Generate(expression);
        }

        public override string Generate(CreateIndexExpression expression)
        {
            expression.Index.SchemaName = this.SchemaName;
            return base.Generate(expression);
        }

        public override string Generate(DeleteIndexExpression expression)
        {
            expression.Index.SchemaName = this.SchemaName;
            return base.Generate(expression);
        }

        public override string Generate(RenameTableExpression expression)
        {
            expression.SchemaName = this.SchemaName;
            return base.Generate(expression);
        }

        public override string Generate(RenameColumnExpression expression)
        {
            expression.SchemaName = this.SchemaName;
            return base.Generate(expression);
        }

        public override string Generate(InsertDataExpression expression)
        {
            expression.SchemaName = this.SchemaName;
            return base.Generate(expression);
        }

        public override string Generate(AlterDefaultConstraintExpression expression)
        {
            expression.SchemaName = this.SchemaName;
            return base.Generate(expression);
        }

        public override string Generate(DeleteDataExpression expression)
        {
            expression.SchemaName = this.SchemaName;
            return base.Generate(expression);
        }

        public override string Generate(UpdateDataExpression expression)
        {
            expression.SchemaName = this.SchemaName;
            return base.Generate(expression);
        }

        public override string Generate(AlterSchemaExpression expression)
        {
            expression.SourceSchemaName = this.SchemaName;
            return base.Generate(expression);
        }

        public override string Generate(CreateSequenceExpression expression)
        {
            expression.Sequence.SchemaName = this.SchemaName;
            return base.Generate(expression);
        }

        public override string Generate(DeleteSequenceExpression expression)
        {
            expression.SchemaName = this.SchemaName;
            return base.Generate(expression);
        }

        public override string Generate(CreateConstraintExpression expression)
        {
            expression.Constraint.SchemaName = this.SchemaName;
            return base.Generate(expression);
        }

        public override string Generate(DeleteConstraintExpression expression)
        {
            expression.Constraint.SchemaName = this.SchemaName;
            return base.Generate(expression);
        }

        public override string Generate(DeleteDefaultConstraintExpression expression)
        {
            expression.SchemaName = this.SchemaName;
            return base.Generate(expression);
        }

        public override string Generate(AlterTableExpression expression)
        {
            expression.SchemaName = this.SchemaName;
            return base.Generate(expression);
        }
    }
}