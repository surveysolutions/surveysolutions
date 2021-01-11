using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.Workspaces
{
    [Migration(2020_11_28_1400)]
    public class M202011281400_AddProductVersionHistory : ForwardOnlyMigration
    {
        public override void Up()
        {
            if (!Schema.Table("productversionhistory").Exists())
            {
                Create.Table("productversionhistory")
                    .WithColumn("updatetimeutc").AsDateTime().PrimaryKey()
                    .WithColumn("productversion").AsString().Nullable();
            }
        }
    }
}
