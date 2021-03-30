using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.Workspaces
{
    [Migration(202103301900)]
    public class M202103301900_AllowNullForResponsibleName : AutoReversingMigration
    {
        public override void Up()
        {
            Alter.Table("auditlogrecords")
                .AlterColumn("responsiblename").AsString().Nullable();
        }
    }
}