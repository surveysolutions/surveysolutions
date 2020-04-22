using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(2020_04_10_13_38)]
    public class M202004101338_AddLowerCaseAnswer : Migration
    {
        public override void Up()
        {
            Create.Column("answer_lower_case").OnTable("answerstofeaturedquestions")
                .AsString().Nullable();
            Create.Index("answerstofeaturedquestions_answer_lower_case_idx").OnTable("answerstofeaturedquestions")
                .OnColumn("answer_lower_case");
            
            Execute.Sql("UPDATE readside.answerstofeaturedquestions SET answer_lower_case = LOWER(answervalue)");
        }

        public override void Down()
        {
            Delete.Column("answer_lower_case").FromTable("answerstofeaturedquestions");
        }
    }
}
