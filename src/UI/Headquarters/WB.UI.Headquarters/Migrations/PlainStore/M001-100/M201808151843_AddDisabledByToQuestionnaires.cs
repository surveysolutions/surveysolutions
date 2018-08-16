using FluentMigrator;

namespace WB.UI.Headquarters.Migrations.PlainStore
{
    [Migration(201808151843)]
    public class M201808151843_AddDisabledByToQuestionnaires : Migration
    {
        public override void Up()
        {
            Create.Column("disabledby").OnTable("questionnairebrowseitems").AsGuid().Nullable();
        }

        public override void Down()
        {
            Delete.Column("disabledby").FromTable("questionnairebrowseitems");
        }
    }
}
