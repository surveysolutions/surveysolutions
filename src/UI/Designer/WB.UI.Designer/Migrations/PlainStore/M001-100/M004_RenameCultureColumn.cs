using FluentMigrator;

namespace WB.UI.Designer.Migrations.PlainStore
{
    [Migration(4)]
    public class M004_RenameCultureColumn : Migration
    {
        public override void Up()
        {
            Rename.Column("culture").OnTable("translationinstances").To("language");
        }

        public override void Down()
        {
            Rename.Column("language").OnTable("translationinstances").To("culture");
        }
    }
}