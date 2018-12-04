using System.Data;
using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.PlainStore
{
    [Migration(13)]
    public class M013_AddCascadingForeignKeyToIdentifyingDataOfAssignments : Migration
    {
        public override void Up()
        {
            Create.ForeignKey("assignments_assignmentsidentifyinganswers")
                .FromTable("assignmentsidentifyinganswers").ForeignColumn("assignmentid")
                .ToTable("assignments").PrimaryColumn("id")
                .OnDelete(Rule.Cascade);
        }

        public override void Down()
        {
            Delete.ForeignKey("assignments_assignmentsidentifyinganswers");
        }
    }
}