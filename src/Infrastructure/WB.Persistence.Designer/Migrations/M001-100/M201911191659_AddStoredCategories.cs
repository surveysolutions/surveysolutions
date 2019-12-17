using FluentMigrator;

namespace WB.UI.Designer.Migrations.PlainStore
{
    [Migration(201911191659, "Added stored categories", BreakingChange = false)]
    public class M201911191659_AddStoredCategories : AutoReversingMigration
    {
        public override void Up()
        {
            Create.Table("categories")
                .WithColumn("questionnaireid").AsGuid()
                .WithColumn("categoriesid").AsGuid()
                .WithColumn("id").AsInt32().PrimaryKey().Identity()
                .WithColumn("value").AsInt32()
                .WithColumn("parentid").AsInt32().Nullable()
                .WithColumn("text").AsString();

            Create.Index().OnTable("categories").OnColumn("questionnaireid").Ascending().OnColumn("categoriesid")
                .Ascending();
        }
    }
}
