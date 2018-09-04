using FluentMigrator;

namespace WB.UI.Designer.Migrations.PlainStore
{
    [Migration(201809041548)]
    public class M201809041548_AddDiffWithPreviousVersionColumn : Migration
    {
        public override void Up()
        {
            Create.Column("diffwithprevisousversion").OnTable("questionnairechangerecords").AsString().Nullable();
        }

        public override void Down()
        {
            Delete.Column("diffwithprevisousversion").FromTable("questionnairechangerecords");
        }
    }
}
