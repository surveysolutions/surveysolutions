using FluentMigrator;

namespace WB.UI.Designer.Migrations.PlainStore
{
    [Migration(6)]
    public class M006_ReplaceColumnLanguageToTranslationIdInTranslationInstances : Migration
    {
        public override void Up()
        {
            this.Delete.Column("language").FromTable("translationinstances");
            this.Alter.Table("translationinstances").AddColumn("translationid").AsGuid().NotNullable();
        }

        public override void Down()
        {
            this.Delete.Column("translationid").FromTable("translationinstances");
            this.Alter.Table("translationinstances").AddColumn("language").AsString().NotNullable();
        }
    }
}