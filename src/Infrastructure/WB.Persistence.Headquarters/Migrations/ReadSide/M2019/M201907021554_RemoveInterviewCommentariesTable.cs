using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(201907021554)]
    public class M201907021554_RemoveInterviewCommentariesTable : Migration
    {
        public override void Up()
        {
            // this check need to don't allow second execute, because we change number of migration
            // from 2019070215544 to 201907021554, to correct migration run order 
            if (Schema.Table("commentaries").Column("summary_id").Exists())
                return;

            Create.Column("summary_id").OnTable("commentaries").AsInt32().Nullable();

            Execute.Sql(@"update readside.commentaries s
                            set
                            summary_id = sr.id
                            from readside.interviewsummaries sr 
                            where sr.interviewid = s.interviewid::uuid");

            Execute.Sql(@"delete from readside.commentaries where summary_id is null");

            Delete.Column("interviewid").FromTable("commentaries");
            Delete.Column("position").FromTable("commentaries");

            Alter.Column("summary_id").OnTable("commentaries").AsInt32().NotNullable();
            Create.Index().OnTable("commentaries").OnColumn("summary_id");
            Delete.Table("interviewcommentaries");
            Create.Column("id").OnTable("commentaries").AsInt32().Identity().NotNullable().Indexed();
        }

        public override void Down()
        {
        }
    }
}
