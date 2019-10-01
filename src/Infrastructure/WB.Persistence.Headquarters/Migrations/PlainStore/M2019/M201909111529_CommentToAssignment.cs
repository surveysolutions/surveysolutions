using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.PlainStore
{
    [Migration(201909111529)]
    public class M201909111529_CommentToAssignment : AutoReversingMigration
    {
        public override void Up()
        {
            Create.Column("comments").OnTable("assignmenttoimport").AsString().Nullable();
        }
    }
}
