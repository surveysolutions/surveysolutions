using FluentMigrator;

namespace WB.UI.Designer.Migrations.PlainStore
{
    [Migration(13)]
    public class M013_AddTargetItemDateTimeToHistory : Migration
    {
        public override void Up()
        {
            Create.Column("targetitemdatetime")
                .OnTable("questionnairechangerecords")
                .AsDateTime()
                .Nullable();
        }

        public override void Down()
        {
            Delete.Column("targetitemdatetime").FromTable("questionnairechangerecords");
        }
    }
}