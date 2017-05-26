using FluentMigrator;

namespace WB.UI.Headquarters.Migrations.PlainStore
{
    [Migration(13)]
    public class M013_AddCompletedCountToAssignments : Migration
    {
        const string assignments = "assignments";

        public override void Up()
        {
            this.Alter.Table(assignments)
                .AddColumn("completed").AsInt64().Nullable();
        }

        public override void Down()
        {
            this.Delete.Column("completed").FromTable(assignments);
        }
    }
}