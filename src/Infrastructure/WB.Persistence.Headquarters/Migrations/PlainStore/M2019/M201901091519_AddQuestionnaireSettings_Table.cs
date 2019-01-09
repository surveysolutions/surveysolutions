using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.PlainStore
{
    [Migration(201901091519)]
    public class M201901091519_AddQuestionnaireSettingsTable : Migration
    {
        public override void Up()
        {
            Alter.Table("questionnairebrowseitems")
                .AddColumn("isaudiorecordingenabled").AsBoolean().Nullable()
                .AddColumn("questionnairesettingsversion").AsInt32().NotNullable().WithDefaultValue(0);
        }

        public override void Down()
        {
            Delete.Column("isaudiorecordingenabled").FromTable("questionnairesettings");
        }
    }
}
