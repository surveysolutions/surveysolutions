using FluentMigrator;

namespace WB.UI.Designer.Migrations.PlainStore
{
    [Migration(6)]
    public class M006_ReplacedColumnLanguageToTranslationIdAndChangedIdColumnTypeInTranslationInstances : Migration
    {
        public override void Up()
        {
            this.Delete.Column("language").FromTable("translationinstances");
            this.Delete.Column("id").FromTable("translationinstances");
            this.Alter.Table("translationinstances").AddColumn("translationid").AsGuid().NotNullable();
            this.Alter.Table("translationinstances").AddColumn("id").AsGuid().PrimaryKey();
        }

        public override void Down()
        {
            this.Delete.Column("id").FromTable("translationinstances");
            this.Delete.Column("translationid").FromTable("translationinstances");
            this.Alter.Table("translationinstances").AddColumn("language").AsString().NotNullable();
            this.Alter.Table("translationinstances").AddColumn("id").AsInt32().PrimaryKey();
        }
    }
}