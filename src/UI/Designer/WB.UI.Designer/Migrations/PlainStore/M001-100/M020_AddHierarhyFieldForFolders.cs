using FluentMigrator;

namespace WB.UI.Designer.Migrations.PlainStore
{
    [Migration(20)]
    public class M020_AddHierarhyFieldForFolders : Migration
    {
        public override void Up()
        {
            this.Create.Column("depth").OnTable("questionnairelistviewfolders")
                .AsInt32()
                .NotNullable()
                .SetExistingRowsTo(0);
            this.Create.Column("path").OnTable("questionnairelistviewfolders")
                .AsString()
                .NotNullable()
                .SetExistingRowsTo("/");

            this.Create.Index("path_questionnairelistviewfolders_idx")
                .OnTable("questionnairelistviewfolders")
                .OnColumn("path");
        }

        public override void Down()
        {
            this.Delete.Index("path_questionnairelistviewfolders_idx").OnTable("questionnairelistviewfolders");
            this.Delete.Column("path").FromTable("questionnairelistviewfolders");
            this.Delete.Column("depth").FromTable("questionnairelistviewfolders");
        }
    }
}
