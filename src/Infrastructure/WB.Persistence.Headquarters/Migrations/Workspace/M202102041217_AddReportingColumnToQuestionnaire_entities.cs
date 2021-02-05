using System.Data;
using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.Workspace
{
    [Migration(202102041217)]
    public class M202102041217_AddReportingColumnToQuestionnaire_entities : ForwardOnlyMigration
    {
        public override void Up()
        {
            Create.Column("used_in_reporting")
                .OnTable("questionnaire_entities")
                .AsBoolean()
                .Nullable();
        }
    }
}
