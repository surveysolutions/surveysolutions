using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(18)]
    public class M018_RemoveInterviewReferencesTable : Migration
    {
        public override void Up()
        {
            if (Schema.Table("interviewreferences").Exists())
                this.Delete.Table("interviewreferences");
        }

        public override void Down()
        {
            // Table is going to be created on first use
        }
    }
}