using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(201802121510)]
    public class M201802121510_AddInterviewsIdMap : Migration
    {
        public override void Up()
        {
            Create.Table("interviews_id")
                .WithColumn("id").AsCustom("serial").PrimaryKey()
                .WithColumn("interviewid").AsGuid().NotNullable();

            Execute.Sql(@"insert into readside.interviews_id (interviewid) select interviewid from readside.interviewsummaries");
            Execute.Sql(@"create unique index if not exists interviewsid_interviewid_un on readside.interviews_id (interviewid)");
        }

        public override void Down()
        {
            
        }
    }
}
