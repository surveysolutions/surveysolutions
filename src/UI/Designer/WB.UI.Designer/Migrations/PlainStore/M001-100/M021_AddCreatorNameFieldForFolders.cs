using FluentMigrator;

namespace WB.UI.Designer.Migrations.PlainStore
{
    [Migration(21)]
    public class M021_AddCreatorNameFieldForFolders : Migration
    {
        public override void Up()
        {
            this.Create.Column("creatorname").OnTable("questionnairelistviewfolders")
                .AsString()
                .Nullable();
        }

        public override void Down()
        {
            this.Delete.Column("creatorname").FromTable("questionnairelistviewfolders");
        }
    }
}
