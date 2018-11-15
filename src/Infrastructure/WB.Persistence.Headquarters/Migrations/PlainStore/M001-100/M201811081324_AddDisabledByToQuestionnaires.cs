using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.PlainStore
{
    [Migration(201811081324)]
    public class M201811081324_AddDisabledByToQuestionnaires : Migration
    {
        public override void Up()
        {
            if (!Schema.Table("questionnairebrowseitems").Column("disabledby").Exists())
            {
                Create.Column("disabledby").OnTable("questionnairebrowseitems").AsGuid().Nullable();
            }
        }

        public override void Down()
        {
            Delete.Column("disabledby").FromTable("questionnairebrowseitems");
        }
    }
}
