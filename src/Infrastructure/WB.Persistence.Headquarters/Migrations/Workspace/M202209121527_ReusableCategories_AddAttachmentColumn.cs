using System.ComponentModel;
using FluentMigrator;
using FluentMigrator.Postgres;

namespace WB.Persistence.Headquarters.Migrations.Workspace
{
    [Localizable(false)]
    [Migration(202209121527)]
    public class M202209121527_ReusableCategories_AddAttachmentColumn : AutoReversingMigration
    {
        public override void Up()
        {
            Create.Column("attachmentname")
                .OnTable("reusablecategoricaloptions")
                .AsString()
                .Nullable();
        }
    }
}
