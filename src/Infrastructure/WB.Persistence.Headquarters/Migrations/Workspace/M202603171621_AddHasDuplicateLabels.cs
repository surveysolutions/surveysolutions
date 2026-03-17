using System.ComponentModel;
using System.Data;
using DocumentFormat.OpenXml.Drawing.Charts;
using FluentMigrator;
using FluentMigrator.Postgres;

namespace WB.Persistence.Headquarters.Migrations.Workspace
{
    [Localizable(false)]
    [Migration(202603171621)]
    public class M202603171621_AddHasDuplicateLabels : AutoReversingMigration
    {
        public override void Up()
        {
            Alter.Table("mapbrowseitems")
                .AddColumn("has_duplicate_labels").AsBoolean().Nullable();
        }
    }
}
