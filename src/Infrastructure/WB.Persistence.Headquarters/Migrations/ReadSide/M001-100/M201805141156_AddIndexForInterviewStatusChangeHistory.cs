using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(201805141156)]
    public class M201805141156_AddIndexForInterviewStatusChangeHistory : Migration
    {
        public override void Up()
        {
            Create.Index("UniqueIdIndex").OnTable("interviewcommentedstatuses").OnColumn("id").Unique();
        }

        public override void Down()
        {
            Delete.Index("UniqueIdIndex").OnTable("interviewcommentedstatuses");
        }
    }
}
