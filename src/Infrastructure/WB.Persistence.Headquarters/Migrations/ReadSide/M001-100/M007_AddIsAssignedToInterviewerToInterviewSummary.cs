using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(7)]
    public class M007_AddIsAssignedToInterviewerToInterviewSummary : Migration
    {
        public override void Up()
        {
            Alter.Table("interviewsummaries").AddColumn("isassignedtointerviewer").AsBoolean().SetExistingRowsTo(true).NotNullable();
        }

        public override void Down()
        {
            Delete.Column("isassignedtointerviewer").FromTable("interviewsummaries");
        }
    }
}