using FluentMigrator;

namespace WB.UI.Designer.Migrations.PlainStore
{
    [Migration(2026_01_12_12_00)]
    public class M202601121200_RemoveCanImportOnHqFromUsers : ForwardOnlyMigration
    {
        public override void Up()
        {
            this.Delete.Column("canimportonhq").FromTable("users");
        }
    }
}

