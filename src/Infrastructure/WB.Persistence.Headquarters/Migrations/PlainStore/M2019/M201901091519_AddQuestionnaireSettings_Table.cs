using System.ComponentModel;
using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.PlainStore
{
    [Migration(201901091519)]
    [Localizable(false)]
    public class M201901091519_AddQuestionnaireSettingsTable : Migration
    {
        public override void Up()
        {
            Alter.Table("questionnairebrowseitems")
                .AddColumn("isaudiorecordingenabled").AsBoolean().NotNullable().WithDefaultValue(false);
        }

        public override void Down()
        {
            Delete.Column("isaudiorecordingenabled").FromTable("questionnairesettings");
        }
    }
}
