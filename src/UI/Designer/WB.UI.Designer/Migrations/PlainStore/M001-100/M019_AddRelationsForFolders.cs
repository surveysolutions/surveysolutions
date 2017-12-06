using System.Data;
using FluentMigrator;

namespace WB.UI.Designer.Migrations.PlainStore
{
    [Migration(19)]
    public class M019_AddRelationsForFolders : Migration
    {
        public override void Up()
        {
            this.Create.ForeignKey("folder_relation_fk")
                .FromTable("questionnairelistviewfolders").ForeignColumn("parent")
                .ToTable("questionnairelistviewfolders").PrimaryColumn("id")
                .OnDelete(Rule.Cascade);

            this.Create.ForeignKey("questionnaire_folder_relation_fk")
                .FromTable("questionnairelistviewitems").ForeignColumn("folderid")
                .ToTable("questionnairelistviewfolders").PrimaryColumn("id")
                .OnDelete(Rule.SetNull);
        }

        public override void Down()
        {
            this.Delete.ForeignKey("questionnaire_folder_relation_fk");
            this.Delete.ForeignKey("folder_relation_fk");
        }
    }
}