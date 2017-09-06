using FluentMigrator;

namespace WB.UI.Headquarters.Migrations.ReadSide
{
    [Migration(18)]
    public class M018_RemoveInterviewReferencesTable : Migration
    {
        public override void Up()
        {
            Delete.Table("interviewreferences");
        }

        public override void Down()
        {
            // Table is going to be created on first use
        }
    }
}