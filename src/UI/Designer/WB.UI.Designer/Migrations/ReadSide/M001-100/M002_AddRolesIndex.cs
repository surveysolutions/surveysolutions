using FluentMigrator;

namespace WB.UI.Designer.Migrations.ReadSide
{
    [Migration(2)]
    public class M002_AddRolesIndex : Migration
    {
        public override void Up()
        {
            Create.Index("simpleroles_accountid_indx")
                .OnTable("simpleroles")
                .OnColumn("accountid").Ascending();
        }

        public override void Down()
        {
        }
    }
}