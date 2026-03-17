using System.ComponentModel;
using System.Data;
using DocumentFormat.OpenXml.Drawing.Charts;
using FluentMigrator;
using FluentMigrator.Postgres;

namespace WB.Persistence.Headquarters.Migrations.Workspace
{
    [Localizable(false)]
    [Migration(202603171656)]
    public class M202603171656_MigrateDuplicateLabels : ForwardOnlyMigration
    {
        public override void Up()
        {
            var migrateSql = $@" UPDATE mapbrowseitems m
                SET has_duplicate_labels = EXISTS (
                    SELECT 1
                    FROM duplicatemaplabels d
                    WHERE d.map = m.id
                )
                WHERE m.has_duplicate_labels IS NULL;";
            Execute.Sql(migrateSql);
            
            
            Delete.Table("duplicatemaplabels");
        }
    }
}
