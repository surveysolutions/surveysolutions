using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(2019070215544)]
    public class M2019070215544_RemoveInterviewCommentariesTable : Migration
    {
        public override void Up()
        {
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
