using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.PlainStore
{
    [Migration(201910161219)]
    public class M201910161219_CommentToQuestionnaireBrowseItem : AutoReversingMigration
    {
        public override void Up()
        {
            Create.Column("comment").OnTable("questionnairebrowseitems").AsString().Nullable();
        }
    }
}
