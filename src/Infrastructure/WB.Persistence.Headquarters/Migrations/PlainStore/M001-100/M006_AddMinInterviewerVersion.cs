using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.PlainStore
{
    [Migration(6)]
    public class M006_AddMinInterviewerVersion : Migration
    {
        public override void Up()
        {
            Create.Table("supportedquestionnaireversion")
                .WithColumn("id").AsString(15).PrimaryKey()
                .WithColumn("minquestionnaireversionsupportedbyinterviewer").AsInt32().NotNullable();
        }

        public override void Down()
        {
            Delete.Table("supportedquestionnaireversion");
        }
    }
}