using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.PlainStore
{
    [Migration(201912231142)]
    public class M201912231142_RemoveAssignmentsTable : Migration
    {
        public override void Up()
        {
            Delete.Table("assignmentsidentifyinganswers");
            Delete.Table("assignments");
        }

        public override void Down()
        {
            
        }
    }
}
