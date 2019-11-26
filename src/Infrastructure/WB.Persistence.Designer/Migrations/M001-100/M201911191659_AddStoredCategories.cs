using FluentMigrator;

namespace WB.UI.Designer.Migrations.PlainStore
{
    [Migration(201911191659, "Added stored categories", BreakingChange = false)]
    public class M201911191659_AddStoredCategories : AutoReversingMigration
    {
        public override void Up()
        {
            Create.Table("categories")
                .WithColumn("questionnaireid").AsGuid().NotNullable().PrimaryKey()
                .WithColumn("categoriesid").AsGuid().NotNullable().PrimaryKey()
                .WithColumn("id").AsInt32().NotNullable().PrimaryKey()
                .WithColumn("parentid").AsInt32().Nullable().PrimaryKey()
                .WithColumn("text").AsString().NotNullable();
        }
    }
}
