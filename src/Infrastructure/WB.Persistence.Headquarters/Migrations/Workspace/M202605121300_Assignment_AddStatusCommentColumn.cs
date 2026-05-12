using System.ComponentModel;
using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.Workspace
{
    [Localizable(false)]
    [Migration(202605121300)]
    public class M202605121300_Assignment_AddStatusCommentColumn : AutoReversingMigration
    {
        public override void Up()
        {
            Create.Column("statuscomment")
                .OnTable("assignments")
                .AsString()
                .Nullable();
        }
    }
}
