using System.ComponentModel;
using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.Workspace
{
    [Localizable(false)]
    [Migration(202603171621)]
    public class M202603171621_AddHasDuplicateLabels : ForwardOnlyMigration
    {
        public override void Up()
        {
            Alter.Table("mapbrowseitems")
                .AddColumn("has_duplicate_labels").AsBoolean().Nullable();
        }
    }
}
