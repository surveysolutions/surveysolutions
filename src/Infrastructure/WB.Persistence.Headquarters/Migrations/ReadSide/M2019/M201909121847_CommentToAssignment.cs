using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(201909121847)]
    public class M201909121847_CommentToAssignment : AutoReversingMigration
    {
        public override void Up()
        {
            Create.Column("comments").OnTable("assignments").AsString().Nullable();
        }
    }
}
