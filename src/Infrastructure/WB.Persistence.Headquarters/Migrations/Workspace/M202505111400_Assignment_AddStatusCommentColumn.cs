using System.ComponentModel;
using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.Workspace
{
    [Localizable(false)]
    [Migration(202505111400)]
    public class M202505111400_Assignment_AddStatusCommentColumn : AutoReversingMigration
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
