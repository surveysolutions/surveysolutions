using FluentMigrator;

namespace WB.UI.Designer.Migrations.PlainStore
{
    [Migration(201806041250)]
    public class M201806041250_RemoveObsoleteTables : Migration
    {
        public override void Up()
        {
            if (Schema.Table("zz_archived_sharedpersons").Exists())
            {
                Delete.Table("zz_archived_sharedpersons");
            }

            if (Schema.Table("zz_archived_questionnairesharedpersons").Exists())
            {
                Delete.Table("zz_archived_questionnairesharedpersons");
            }
        }

        public override void Down()
        {
        }
    }
}
