using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(202008111558)]
    public class M202008111558_AddNotAnsweredQuestionsCount : AutoReversingMigration
    {
        public override void Up()
        {
            Create.Column("not_answered_count").OnTable("interviewsummaries").AsInt32().Nullable();
        }
    }
}
